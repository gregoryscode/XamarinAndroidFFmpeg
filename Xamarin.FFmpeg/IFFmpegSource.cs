using System;

namespace Xamarin.FFmpeg
{
    public interface IFFmpegSource
    {
        string FFmpegVersion { get; }
        string Url { get; set; }
        string Arch { get; }
        string Hash { get; }
        Func<string, bool> IsArch { get; }

        void SetUrl(string url);
        bool IsHashMatch(byte[] data);
    }
}
