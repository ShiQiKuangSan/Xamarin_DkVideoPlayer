using System;
using System.Collections.Generic;
using System.Threading;
using Android.Content;
using Android.Content.Res;
using Android.Media;
using Android.OS;
using Android.Text;
using Android.Views;
using DkVideoPlayer.VideoPlayer.Player;
using TV.Danmaku.Ijk.Media.Player;

namespace DkVideoPlayer.Ijk
{
    public class IjkPlayer : AbstractPlayer
    {
        protected  IjkMediaPlayer MediaPlayer;
        public int BufferedPercent;
        public Context AppContext;

        public IjkPlayer(Context context)
        {
            AppContext = context;
        }

        public override void InitPlayer()
        {
            MediaPlayer = new IjkMediaPlayer();
            //native日志
            IjkMediaPlayer.Native_setLogLevel(VideoViewManager.Config.mIsEnableLog
                ? IjkMediaPlayer.IjkLogInfo
                : IjkMediaPlayer.IjkLogSilent);
            _onErrorListener = new OnErrorListenerAnonymousInnerClass(this);
            _onInfoListener = new OnInfoListenerAnonymousInnerClass(this);
            SetOptions();
            MediaPlayer.SetAudioStreamType((int) Stream.Music);
            MediaPlayer.SetOnErrorListener(_onErrorListener);
            MediaPlayer.Completion += OnCompletion;
            MediaPlayer.SetOnInfoListener(_onInfoListener);
            MediaPlayer.BufferingUpdate += OnBufferingUpdate;
            MediaPlayer.Prepared += OnPrepared;
            MediaPlayer.VideoSizeChanged += OnVideoSizeChanged;
            MediaPlayer.SetOnNativeInvokeListener(new OnNativeInvokeListenerAnonymousInnerClass());
        }

        private class OnNativeInvokeListenerAnonymousInnerClass : Java.Lang.Object,
            IjkMediaPlayer.IOnNativeInvokeListener
        {
            public bool OnNativeInvoke(int what, Bundle args)
            {
                return true;
            }
        }


        public override void SetOptions()
        {
        }

        public override void SetDataSource(string path, IDictionary<string, string> headers)
        {
            try
            {
                var uri = Android.Net.Uri.Parse(path);
                if (ContentResolver.SchemeAndroidResource.Equals(uri.Scheme))
                {
                    var rawDataSourceProvider = RawDataSourceProvider.Create(AppContext, uri);
                    MediaPlayer.SetDataSource(rawDataSourceProvider);
                }
                else
                {
                    //处理UA问题
                    if (headers != null)
                    {
                        var userAgent = headers["User-Agent"];
                        if (!TextUtils.IsEmpty(userAgent))
                        {
                            MediaPlayer.SetOption(IjkMediaPlayer.OptCategoryFormat, "user_agent", userAgent);
                        }
                    }

                    MediaPlayer.SetDataSource(AppContext, uri, headers);
                }
            }
            catch (Exception)
            {
                PlayerEventListener.OnError();
            }
        }

        public override AssetFileDescriptor DataSource
        {
            set
            {
                try
                {
                    MediaPlayer.SetDataSource(new RawDataSourceProvider(value));
                }
                catch (Exception)
                {
                    PlayerEventListener.OnError();
                }
            }
        }

        public override void Pause()
        {
            try
            {
                MediaPlayer.Pause();
            }
            catch (InvalidOperationException)
            {
                PlayerEventListener.OnError();
            }
        }

        public override void Start()
        {
            try
            {
                MediaPlayer.Start();
            }
            catch (InvalidOperationException)
            {
                PlayerEventListener.OnError();
            }
        }

        public override void Stop()
        {
            try
            {
                MediaPlayer.Stop();
            }
            catch (InvalidOperationException)
            {
                PlayerEventListener.OnError();
            }
        }

        public override void PrepareAsync()
        {
            try
            {
                MediaPlayer.PrepareAsync();
            }
            catch (System.InvalidOperationException)
            {
                PlayerEventListener.OnError();
            }
        }

        public override void Reset()
        {
            MediaPlayer.Reset();
            MediaPlayer.VideoSizeChanged += OnVideoSizeChanged;
            SetOptions();
        }

        public override bool Playing => MediaPlayer.IsPlaying;

        public override void SeekTo(long time)
        {
            try
            {
                MediaPlayer.SeekTo((int) time);
            }
            catch (System.InvalidOperationException)
            {
                PlayerEventListener.OnError();
            }
        }

        public override void Release()
        {
            MediaPlayer.SetOnErrorListener(null);
            MediaPlayer.Completion -= OnCompletion;
            MediaPlayer.SetOnInfoListener(null);
            MediaPlayer.BufferingUpdate -= OnBufferingUpdate;
            MediaPlayer.Prepared -= OnPrepared;
            MediaPlayer.VideoSizeChanged -= OnVideoSizeChanged;

            new Thread(() =>
            {
                try
                {
                    MediaPlayer.Release();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                }
            }).Start();
        }

        public override long CurrentPosition => MediaPlayer.CurrentPosition;

        public override long Duration => MediaPlayer.Duration;

        public override int BufferedPercentage => BufferedPercent;

        public override Surface Surface
        {
            set => MediaPlayer.SetSurface(value);
        }

        public override ISurfaceHolder Display
        {
            set => MediaPlayer.SetDisplay(value);
        }

        public override void SetVolume(float v1, float v2)
        {
            MediaPlayer.SetVolume(v1, v2);
        }

        public override bool Looping
        {
            set => MediaPlayer.Looping = value;
        }

        public override float Speed
        {
            set => MediaPlayer.SetSpeed(value);
            get => MediaPlayer.GetSpeed(0);
        }

        public override long TcpSpeed => MediaPlayer.TcpSpeed;

        private IMediaPlayerOnErrorListener _onErrorListener;

        private class OnErrorListenerAnonymousInnerClass : Java.Lang.Object, IMediaPlayerOnErrorListener
        {
            private readonly IjkPlayer _ijkPlayer;

            public OnErrorListenerAnonymousInnerClass(IjkPlayer ijkPlayer)
            {
                _ijkPlayer = ijkPlayer;
            }

            public bool OnError(IMediaPlayer mp, int what, int extra)
            {
                _ijkPlayer.PlayerEventListener.OnError();
                return true;
            }
        }

        private void OnCompletion(object sender, MediaPlayerOnCompletionEventArgs e)
        {
            PlayerEventListener.OnCompletion();
        }

        private IMediaPlayerOnInfoListener _onInfoListener;

        private class OnInfoListenerAnonymousInnerClass : Java.Lang.Object, IMediaPlayerOnInfoListener
        {
            private readonly IjkPlayer _ijkPlayer;

            public OnInfoListenerAnonymousInnerClass(IjkPlayer ijkPlayer)
            {
                _ijkPlayer = ijkPlayer;
            }

            public bool OnInfo(IMediaPlayer mp, int what, int extra)
            {
                _ijkPlayer.PlayerEventListener.OnInfo(what, extra);
                return true;
            }
        }

        private void OnBufferingUpdate(object sender, MediaPlayerOnBufferingUpdateEventArgs e)
        {
            BufferedPercent = e.Percent;
        }

        private void OnPrepared(object sender, MediaPlayerOnPreparedEventArgs e)
        {
            PlayerEventListener.OnPrepared();
        }

        private void OnVideoSizeChanged(object sender, MediaPlayerOnVideoSizeChangedEventArgs e)
        {
            var videoWidth = e.Mp.VideoWidth;
            var videoHeight = e.Mp.VideoHeight;
            if (videoWidth != 0 && videoHeight != 0)
            {
                PlayerEventListener.OnVideoSizeChanged(videoWidth, videoHeight);
            }
        }
    }
}