using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using System.Threading.Tasks;
using Xamarin.FFmpeg.Exceptions;
using Xamarin.FFmpeg;
using Java.IO;

namespace Xamarin.FFmpeg.Android
{
    public class FFmpegLibrary : IFFmpegLibrary
    {
        public string EndOfFFMPEGLine { get; } = "final ratefactor:";
        private string Url;
        private string SourceFolder;
        private string DownloadTitle;
        private bool Initialized = false;
        private File FFmpegFile;
        internal Context Context;

        /// <summary>
        /// Initializes the FFmpeg library and download it if necessary
        /// </summary>
        /// <param name="obj">Context</param>
        /// <exception cref="FFmpegNotInitializedException"></exception>
        /// <exception cref="FFmpegNotDownloadedException"></exception>
        /// <returns></returns>
        public async Task Init()
        {
            if (Initialized)
            {
                return;
            }
            FFmpegFile = new File((SourceFolder ?? Context.FilesDir.AbsolutePath) + "/ffmpeg");

            FFmpegSource source = FFmpegSource.Get();                        

            if (source == null)
            {
                throw new FFmpegNotInitializedException();
            }

            if (Url != null)
            {
                source.SetUrl(Url);
            }

            await Task.Run(() =>
            {
                if (FFmpegFile.Exists())
                {
                    try
                    {
                        if (source.IsHashMatch(System.IO.File.ReadAllBytes(FFmpegFile.CanonicalPath)))
                        {
                            if (!FFmpegFile.CanExecute())
                            {
                                FFmpegFile.SetExecutable(true);
                            }

                            Initialized = true;

                            return;
                        }
                    }
                    catch (Exception)
                    {
                        // Não implementado
                    }

                    if (FFmpegFile.CanExecute())
                    {
                        FFmpegFile.SetExecutable(false);
                    }

                    FFmpegFile.Delete();
                }
            });

            if (Initialized)
            {
                // Ffmpeg file exists...
                return;
            }

            if (FFmpegFile.Exists())
            {
                FFmpegFile.Delete();
            }

            await Download();

            if (!FFmpegFile.CanExecute())
            {
                FFmpegFile.SetExecutable(true);
            }

            Initialized = true;
        }

        /// <summary>
        /// Run a command in FFmpeg (must be executed in the UI thread)
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="downloadTitle"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public async Task<int> Run(string cmd, Action<string> logger = null)
        {
            try
            {
                TaskCompletionSource<int> source = new TaskCompletionSource<int>();

                await Init();

                await Task.Run(() =>
                {
                    try
                    {
                        int n = _Run(cmd, logger);
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

        private int _Run(string cmd, Action<string> logger = null)
        {
            TaskCompletionSource<int> task = new TaskCompletionSource<int>();

            var startInfo = new System.Diagnostics.ProcessStartInfo(FFmpegFile.CanonicalPath, cmd)
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var process = new System.Diagnostics.Process
            {
                StartInfo = startInfo
            };

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

        /// <summary>
        /// Download the FFmpeg library
        /// </summary>
        /// <exception cref="FFmpegNotInitializedException"></exception>
        /// <exception cref="FFmpegNotDownloadedException"></exception>
        /// <returns></returns>
        public async Task<bool> Download()
        {
            File ffmpegFile;

            if (SourceFolder != null)
            {
                ffmpegFile = new File(SourceFolder + "/ffmpeg");
            }
            else
            {
                var filesDir = Context.FilesDir;

                ffmpegFile = new File(filesDir + "/ffmpeg");
            }

            FFmpegSource source = FFmpegSource.Get();

            if (source == null)
            {
                throw new FFmpegNotInitializedException();
            }

            if (Url != null)
            {
                source.SetUrl(Url);
            }

            try
            {
                using (var c = new System.Net.Http.HttpClient())
                {
                    using (var fout = System.IO.File.OpenWrite(ffmpegFile.AbsolutePath))
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

                        while ((count = await s.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fout.WriteAsync(buffer, 0, count);

                            progress += count;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw new FFmpegNotDownloadedException();
            }

            return true;
        }

        /// <summary>
        /// Set the download url for ffmpeg library
        /// </summary>
        /// <param name="url"></param>
        public void SetDownloadUrl(string url)
        {
            Url = url;
        }

        /// <summary>
        /// Set the source folder containing the ffmpeg library
        /// </summary>
        /// <param name="path"></param>
        public void SetSourceFolder(string path)
        {
            SourceFolder = path;
        }

        /// <summary>
        /// Set the source folder containing the ffmpeg library
        /// </summary>
        /// <param name="path"></param>
        public void SetDownloadTitle(string title)
        {
            DownloadTitle = title;
        }
    }
}