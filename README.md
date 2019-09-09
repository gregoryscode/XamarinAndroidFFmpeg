# XamarinAndroidFFmpeg (Xamarin.FFmpeg)
FFmpeg library to run ffmeg commands over Xamarin.Forms

## About
* Created to support a solution that needed ffmpeg to control camera devices.
* Edited to allow usage on Xamarin.Forms

## Big thanks
* FFmpeg library adapted from https://github.com/neurospeech/xamarin-android-ffmpeg
* Original library in java https://github.com/WritingMinds/ffmpeg-android-java

## License
* MIT
* Besides that, to use this library you must accept the licensing terms mentioned in the source project at https://github.com/WritingMinds/ffmpeg-android-java

## Nuget package
You can download Xamarin.FFmpeg package from Nuget Package Manager or run following command in Nuget Package Console.
```
Install-Package Xamarin.FFmpeg
```
## Usage

### Android

In MainActivity.cs before ``Forms.Init(this, bundle)``:
```cs
Xamarin.FFmpeg.Android.Init.Initialize(this.BaseContext);
```
