using Android.Content;

namespace Xamarin.FFmpeg.Android
{
    public class Init
    {
        public static void Initialize(Context context)
        {
            FFmpeg.Init.Initialize(new FFmpegLibrary
            {
                Context = context
            });
        }
    }
}