using System;

namespace FFMpeg.Xamarin
{
    public class FFmpegSource
    {
        private static string FFmpegVersion { get; } = "3.0.1.1";
        public string Url { get; set; }
        public string Arch { get; }
        public string Hash { get; }
        public Func<string, bool> IsArch { get; }

        public FFmpegSource(string arch, Func<string, bool> isArch, string hash)
        {
            Arch = arch;
            IsArch = isArch;
            Hash = hash;
            Url = $"https://raw.githubusercontent.com/gperozzo/XamarinAndroidFFmpeg/master/binary/{FFmpegVersion}/{Arch}/ffmpeg";
        }

        public static FFmpegSource[] Sources = new FFmpegSource[] {
            new FFmpegSource("arm", x=> !x.EndsWith("86"), "yRVoeaZATQdZIR/lZxMsIa/io9U="),
            new FFmpegSource("x86", x=> x.EndsWith("86"), "mU4QKhrLEO0aROb9N7JOCJ/rVTA==")
        };

        public static FFmpegSource Get()
        {
            string osArchitecture = Java.Lang.JavaSystem.GetProperty("os.arch");

            foreach (var source in Sources)
            {
                if (source.IsArch(osArchitecture))
                    return source;
            }

            return null;
        }

        public void SetUrl(string url)
        {
            Url = url;
        }

        public bool IsHashMatch(byte[] data)
        {
            var sha = System.Security.Cryptography.SHA1.Create();
            string h = Convert.ToBase64String(sha.ComputeHash(data));
            return h == Hash;
        }
    }
}