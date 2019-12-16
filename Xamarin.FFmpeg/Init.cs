namespace Xamarin.FFmpeg
{
    public class Init
    {
        public static void Initialize<T1>(T1 lib)
            where T1 : IFFmpegLibrary
        {
            FFmpegLibrary.Instance = lib;
        }

        public static void Initialize<T1, T2>(T1 lib, T2 source) 
            where T1 : IFFmpegLibrary
            where T2 : IFFmpegSource
        {
            FFmpegLibrary.Instance = lib;
            FFmpegSource.Instance = source;
        }
    }
}
