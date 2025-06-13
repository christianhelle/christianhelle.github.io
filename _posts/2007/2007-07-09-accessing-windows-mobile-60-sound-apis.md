---
layout: post
title: Accessing Windows Mobile 6.0 Sound API's through .NETCF
date: '2007-07-09T10:34:00.001+02:00'
author: Christian Resma Helle
tags:
- ".NET Compact Framework"
- Windows Mobile 6.0
modified_time: '2009-05-06T12:19:36.882+02:00'
blogger_id: tag:blogger.com,1999:blog-4995334164049002857.post-2335941440338487316
blogger_orig_url: https://christian-helle.blogspot.com/2007/06/accessing-windows-mobile-60-sound-apis.html
---

A new set of APIs were introduced in Windows Mobile 6 to make it easier to manage and play sound. The new API's support playing sounds in a variety of formats that Windows Media Player supports  
  
These API's are really easy to use. You can play a sound file with a single function call. Let's try to do that through .NETCF by P/Invoking `SndPlaySync(LPCTSTR pszSoundFile, DWORD dwFlags)`.
  
```csharp
[DllImport("aygshell.dll")]
static extern uint SndPlaySync(string pszSoundFile, uint dwFlags);

void PlaySound() 
{
  SndPlaySync("\\Storage Card\\Intro.mp3", 0);
}
```
  
  
In the previous sample, we are playing a sound file synchronously. Now, this is interesting in a way that it's very very easy to play an audio file. But what really gets interesting is that the new Sound API provides methods for playing sound files asynchronously.  
  
To play audio files asynchronously, we will need to call 4 methods from the Sound API.  
  
  SndOpen(LPCTSTR pszSoundFile, HSOUND\* phSound)  
  SndPlayAsync(HSOUND hSound, DWORD dwFlags)  
  SndClose(HSOUND hSound)  
  SndStop(SND\_SCOPE SoundScope, HSOUND hSound)  
  
Let's start by declare our P/Invokes  
  
```csharp
[DllImport("aygshell.dll")]
static extern uint SndOpen(string pszSoundFile, ref IntPtr phSound);

[DllImport("aygshell.dll")]
static extern uint SndPlayAsync(IntPtr hSound, uint dwFlags);

[DllImport("aygshell.dll")]
static extern uint SndClose(IntPtr hSound);

[DllImport("aygshell.dll")]
static extern uint SndStop(int SoundScope, IntPtr hSound);
```  

Now that we have our P/Invokes ready. Let's start playing with the Sound API in .NETCF. In the sample below, the application will play the audio file Intro.mp3 located in the Storage Card. To play an Audio file asynchronously, we will first need a handle to the audio file. We use `SndOpen(string, IntPtr)` to accomplish that. Once we have the handle to the audio file, we can call `SndPlayAsync(IntPtr, int)` to start playing the audio file. To stop playing the audio we just have to close the handle and call `SndStop(SND_SCOPE_PROCESS, IntPtr.Zero)` to stop the playback of the sound.
  
  
```csharp
IntPtr hSound = IntPtr.Zero;
const string AUDIO_FILE = "\\Storage Card\\Intro.mp3";
const int SND_SCOPE_PROCESS = 0x1;

void Play() 
{
    SndOpen(AUDIO_FILE, ref hSound);
    SndPlayAsync(hSound, 0);
}

void Stop() 
{
    SndClose(hSound);
    SndStop(SND_SCOPE_PROCESS, IntPtr.Zero);
}
```

How cool is that? You can now easily add some cool sound effects to your application. Maybe even use the Sound API for one of those annoying startup sounds!