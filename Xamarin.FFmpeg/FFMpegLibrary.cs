using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using System.Threading.Tasks;
using Xamarin.FFmpeg.Exceptions;

namespace FFMpeg.Xamarin
{
    public class FFmpegLibrary
    {
        public static string EndOfFFMPEGLine = "final ratefactor:";

        public string CDNHost { get; set; } = "raw.githubusercontent.com";

        private static string Url { get; set; }

        public readonly static FFmpegLibrary Instance = new FFmpegLibrary();

        private bool _initialized = false;

        private Java.IO.File _ffmpegFile;

        public static void SetDownloadUrl(string url)
        {
            Url = url;
        }

        /// <summary>
        /// Initializes the FFmpeg library and download it if necessary
        /// </summary>
        /// <param name="context"></param>
        /// <param name="downloadTitle"></param>
        /// <exception cref="FFmpegNotInitializedException"></exception>
        /// <returns></returns>
        public async Task Init(Context context, string downloadUrl = null, string sourcePath= null, string downloadTitle = null)
        {
            if (_initialized)
                return;

            if (Url != null)
            {
                CDNHost = Url;
            }

            var filesDir = context.FilesDir;

            _ffmpegFile = new Java.IO.File(filesDir + "/ffmpeg");

            FFmpegSource source = FFmpegSource.Get();

            if(source == null)
            {
                throw new FFmpegNotInitializedException();
            }

            await Task.Run(() =>
            {
                if (_ffmpegFile.Exists())
                {
                    try
                    {
                        if (source.IsHashMatch(System.IO.File.ReadAllBytes(_ffmpegFile.CanonicalPath)))
                        {
                            if (!_ffmpegFile.CanExecute())
                            {
                                _ffmpegFile.SetExecutable(true);
                            }

                            _initialized = true;

                            return;
                        }
                    }
                    catch (Exception)
                    {
                        // Não implementado
                    }

                    if (_ffmpegFile.CanExecute())
                    {
                        _ffmpegFile.SetExecutable(false);
                    }
                        
                    _ffmpegFile.Delete();
                }
            });

            if (_initialized)
            {
                // Ffmpeg file exists...
                return;
            }

            if (_ffmpegFile.Exists())
            {
                _ffmpegFile.Delete();
            }

            // Download
            var dialog = new ProgressDialog(context);
            dialog.SetTitle(downloadTitle ?? "Realizando download da biblioteca FFmpeg");
            dialog.Indeterminate = false;
            dialog.SetProgressStyle(ProgressDialogStyle.Horizontal);
            dialog.SetCancelable(false);
            dialog.SetCanceledOnTouchOutside(false);
            dialog.Show();

            using (var c = new System.Net.Http.HttpClient())
            {
                using (var fout = System.IO.File.OpenWrite(_ffmpegFile.AbsolutePath))
                {
                    string url = source.Url;

                    var g = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);

                    var h = await c.SendAsync(g, System.Net.Http.HttpCompletionOption.ResponseHeadersRead);

                    var buffer = new byte[51200];

                    var s = await h.Content.ReadAsStreamAsync();
                    long total = h.Content.Headers.ContentLength.GetValueOrDefault();

                    IEnumerable<string> sl;

                    if (h.Headers.TryGetValues("Content-Length", out sl))
                    {
                        if (total == 0 && sl.Any())
                        {
                            long.TryParse(sl.FirstOrDefault(), out total);
                        }
                    }

                    int count = 0;
                    int progress = 0;

                    dialog.Max = (int)total;

                    while ((count = await s.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fout.WriteAsync(buffer, 0, count);

                        progress += count;

                        dialog.Progress = progress;
                    }

                    dialog.Hide();
                }
            }

            if (!_ffmpegFile.CanExecute())
            {
                _ffmpegFile.SetExecutable(true);
            }

            _initialized = true;
        }

        /// <summary>
        /// Run a command in FFmpeg (must be executed in the UI thread)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cmd"></param>
        /// <param name="downloadTitle"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static async Task<int> Run(Context context, string cmd, string downloadTitle = null, Action<string> logger = null)
        {
            try
            {
                TaskCompletionSource<int> source = new TaskCompletionSource<int>();

                await Instance.Init(context, null, downloadTitle);

                await Task.Run(() =>
                {
                    try
                    {
                        int n = _Run(context, cmd, logger);
                        source.SetResult(n);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                        source.SetException(ex);
                    }
                });

                return await source.Task;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static int _Run(Context context, string cmd, Action<string> logger = null)
        {
            TaskCompletionSource<int> task = new TaskCompletionSource<int>();

            var startInfo = new System.Diagnostics.ProcessStartInfo(Instance._ffmpegFile.CanonicalPath, cmd);

            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;

            var process = new System.Diagnostics.Process();

            process.StartInfo = startInfo;

            bool finished = false;

            string error = null;

            process.Start();

            Task.Run(() =>
            {
                try
                {
                    using (var reader = process.StandardError)
                    {
                        StringBuilder processOutput = new StringBuilder();
                        while (!finished)
                        {
                            var line = reader.ReadLine();
                            if (line == null)
                                break;
                            logger?.Invoke(line);
                            processOutput.Append(line);

                            if (line.StartsWith(EndOfFFMPEGLine))
                            {
                                Task.Run(async () =>
                                {
                                    await Task.Delay(TimeSpan.FromMinutes(1));
                                    finished = true;
                                });
                            }
                        }
                        error = processOutput.ToString();
                    }
                }
                catch (Exception)
                {
                    // Não implementado
                }
            });

            while (!finished)
            {
                process.WaitForExit(10000);
                if (process.HasExited)
                {
                    break;
                }
            }

            return process.ExitCode;
        }
    }
}