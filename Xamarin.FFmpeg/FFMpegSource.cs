using System;

namespace FFMpeg.Xamarin
{
    public class FFmpegSource
    {
        public static string FFmpegVersion { get; } = "1.0.0";

        public FFmpegSource(string arch, Func<string, bool> isArch, string hash)
        {
            this.Arch = arch;
            this.IsArch = isArch;
            this.Hash = hash;
        }

        public static FFmpegSource[] Sources = new FFmpegSource[] {
            new FFmpegSource("arm", x=> !x.EndsWith("86"), "yRVoeaZATQdZIR/lZxMsIa/io9U="),
            new FFmpegSource("x86", x=> x.EndsWith("86"), "mU4QKhrLEO0aROb9N7JOCJ/rVTA==")
        };

        public string Arch { get; }
        public string Hash { get; }       
        public string Url => $"https://{FFmpegLibrary.Instance.CDNHost}/gperozzo/XamarinAndroidFFmpeg/v1.0.7/binary/{FFmpegVersion}/{Arch}/ffmpeg";

        public Func<string, bool> IsArch { get; }

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

        public bool IsHashMatch(byte[] data)
        {
            var sha = System.Security.Cryptography.SHA1.Create();
            string h = Convert.ToBase64String(sha.ComputeHash(data));
            return h == Hash;
        }
    }
}