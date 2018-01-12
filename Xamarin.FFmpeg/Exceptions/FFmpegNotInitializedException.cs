using System;

namespace Xamarin.FFmpeg.Exceptions
{
    public class FFmpegNotInitializedException : Exception
    {
        /// <summary>
        /// Just create the exception
        /// </summary>
        public FFmpegNotInitializedException() : base()
        {

        }

        /// <summary>
        /// Create the exception with description
        /// </summary>
        /// <param name="message">Exception description</param>
        public FFmpegNotInitializedException(string message) : base(message)
        {

        }

        /// <summary>
        /// Create the exception with description and inner cause
        /// </summary>
        /// <param name="message">Exception description</param>
        /// <param name="innerException">Exception inner cause</param>
        public FFmpegNotInitializedException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}