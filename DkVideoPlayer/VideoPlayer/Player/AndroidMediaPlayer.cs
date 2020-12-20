using System;
using System.Collections.Generic;
using System.Threading;
using Android.Content;
using Android.Content.Res;
using Android.Media;
using Android.OS;
using Android.Views;

namespace DkVideoPlayer.VideoPlayer.Player
{
    /// <summary>
    /// 封装系统的MediaPlayer，不推荐，系统的MediaPlayer兼容性较差，建议使用IjkPlayer或者ExoPlayer
    /// </summary>
    public class AndroidMediaPlayer : AbstractPlayer
    {
        protected MediaPlayer MediaPlayer;
        private int _mBufferedPercent;
        private readonly Context _mAppContext;
        private bool _mIsPreparing;

        public AndroidMediaPlayer(Context context)
        {
            _mAppContext = context.ApplicationContext;
        }

        public override void InitPlayer()
        {
            MediaPlayer = new MediaPlayer();
            SetOptions();
            _onErrorListener = new OnErrorListenerAnonymousInnerClass(this);
            MediaPlayer.SetAudioStreamType(Stream.Music);
            MediaPlayer.SetOnErrorListener(_onErrorListener);
            MediaPlayer.Completion += OnCompletion;
            MediaPlayer.Info += OnInfo;
            MediaPlayer.BufferingUpdate += OnBufferingUpdate;
            MediaPlayer.Prepared += OnPrepared;
            MediaPlayer.VideoSizeChanged += OnVideoSizeChanged;
        }


        public override void SetDataSource(string path, IDictionary<string, string> headers)
        {
            try
            {
                MediaPlayer.SetDataSource(_mAppContext,
                    Android.Net.Uri.Parse(path) ?? throw new InvalidOperationException(), headers);
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
                    MediaPlayer.SetDataSource(value.FileDescriptor, value.StartOffset, value.Length);
                }
                catch (Exception)
                {
                    PlayerEventListener.OnError();
                }
            }
        }

        public override void Start()
        {
            try
            {
                MediaPlayer.Start();
            }
            catch (System.InvalidOperationException)
            {
                PlayerEventListener.OnError();
            }
        }

        public override void Pause()
        {
            try
            {
                MediaPlayer.Pause();
            }
            catch (System.InvalidOperationException)
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
            catch (System.InvalidOperationException)
            {
                PlayerEventListener.OnError();
            }
        }

        public override void PrepareAsync()
        {
            try
            {
                _mIsPreparing = true;
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
            MediaPlayer.SetSurface(null);
            MediaPlayer.SetDisplay(null);
            MediaPlayer.SetVolume(1, 1);
        }

        public override bool Playing => MediaPlayer.IsPlaying;

        public override void SeekTo(long time)
        {
            try
            {
                MediaPlayer.SeekTo((int) time);
            }
            catch (InvalidOperationException)
            {
                PlayerEventListener.OnError();
            }
        }

        public override void Release()
        {
            MediaPlayer.SetOnErrorListener(null);
            MediaPlayer.Completion -= OnCompletion;
            MediaPlayer.Info -= OnInfo;
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

        public override int BufferedPercentage => _mBufferedPercent;

        public override Surface Surface
        {
            set
            {
                try
                {
                    MediaPlayer.SetSurface(value);
                }
                catch (Exception)
                {
                    PlayerEventListener.OnError();
                }
            }
        }

        public override ISurfaceHolder Display
        {
            set
            {
                try
                {
                    MediaPlayer.SetDisplay(value);
                }
                catch (Exception)
                {
                    PlayerEventListener.OnError();
                }
            }
        }

        public override void SetVolume(float v1, float v2)
        {
            MediaPlayer.SetVolume(v1, v2);
        }

        public override bool Looping
        {
            set => MediaPlayer.Looping = value;
        }

        public override void SetOptions()
        {
        }

        public override float Speed
        {
            set
            {
                // only support above Android M
                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    try
                    {
                        MediaPlayer.PlaybackParams = MediaPlayer.PlaybackParams.SetSpeed(value) ??
                                                     throw new InvalidOperationException();
                    }
                    catch (Exception)
                    {
                        PlayerEventListener.OnError();
                    }
                }
            }
            get
            {
                // only support above Android M
                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    try
                    {
                        return MediaPlayer.PlaybackParams.Speed;
                    }
                    catch (Exception)
                    {
                        PlayerEventListener.OnError();
                    }
                }

                return 1f;
            }
        }


        public override long TcpSpeed => 0;

        private MediaPlayer.IOnErrorListener _onErrorListener;

        private class OnErrorListenerAnonymousInnerClass : Java.Lang.Object, MediaPlayer.IOnErrorListener
        {
            private readonly AndroidMediaPlayer _outerInstance;

            public OnErrorListenerAnonymousInnerClass(AndroidMediaPlayer outerInstance)
            {
                this._outerInstance = outerInstance;
            }

            public bool OnError(MediaPlayer mp, MediaError what, int extra)
            {
                _outerInstance.PlayerEventListener.OnError();
                return true;
            }
        }


        public void OnCompletion(object sender, EventArgs e)
        {
            PlayerEventListener.OnCompletion();
        }


        public void OnInfo(object sender, MediaPlayer.InfoEventArgs e)
        {
            //解决MEDIA_INFO_VIDEO_RENDERING_START多次回调问题
            if (e.What == MediaInfo.VideoRenderingStart)
            {
                if (_mIsPreparing)
                {
                    PlayerEventListener.OnInfo((int) e.What, e.Extra);
                    _mIsPreparing = false;
                }
            }
            else
            {
                PlayerEventListener.OnInfo((int) e.What, e.Extra);
            }
        }


        public void OnBufferingUpdate(object sender, MediaPlayer.BufferingUpdateEventArgs e)
        {
            _mBufferedPercent = e.Percent;
        }


        public void OnPrepared(object sender, EventArgs e)
        {
            PlayerEventListener.OnPrepared();
            Start();
        }


        public void OnVideoSizeChanged(object sender, MediaPlayer.VideoSizeChangedEventArgs e)
        {
            var videoWidth = e.Width;
            var videoHeight = e.Height;
            if (videoWidth != 0 && videoHeight != 0)
            {
                PlayerEventListener.OnVideoSizeChanged(videoWidth, videoHeight);
            }
        }
    }
}