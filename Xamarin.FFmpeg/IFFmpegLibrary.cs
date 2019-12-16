using System;
using System.Threading.Tasks;

namespace Xamarin.FFmpeg
{
    public interface IFFmpegLibrary
    {
        string EndOfFFMPEGLine { get; }

        Task Init();

        Task<int> Run(string cmd, Action<string> logger = null);

        Task<bool> Download();

        void SetDownloadUrl(string url);

        void SetSourceFolder(string path);

        void SetDownloadTitle(string title);
    }
}
