
using System;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views; 
using WeakReference = Java.Lang.Ref.WeakReference;

namespace DkVideoPlayer.VideoPlayer.Player
{


    /// <summary>
    /// 音频焦点改变监听
    /// </summary>
    public sealed class AudioFocusHelper : Java.Lang.Object, AudioManager.IOnAudioFocusChangeListener
    {

        private readonly Handler _mHandler = new Handler(Looper.MainLooper ?? throw new InvalidOperationException());

        private readonly WeakReference _mWeakVideoView;

        private readonly AudioManager _mAudioManager;

        private bool _startRequested = false;
        private bool _pausedForLoss = false;
        private AudioFocus _currentFocus;

        internal AudioFocusHelper(View videoView)
        {
            _mWeakVideoView = new WeakReference(videoView);
            _mAudioManager = videoView.Context.ApplicationContext.GetSystemService(Context.AudioService).JavaCast<AudioManager>();
        }

        public void OnAudioFocusChange(AudioFocus focusChange)
        {
            if (_currentFocus == focusChange)
            {
                return;
            }

            //由于onAudioFocusChange有可能在子线程调用，
            //故通过此方式切换到主线程去执行
            _mHandler.Post(() =>
            {
                HandleAudioFocusChange(focusChange);
            });

            _currentFocus = focusChange;
        }

        private void HandleAudioFocusChange(AudioFocus focusChange)
        {
            if (!(_mWeakVideoView.Get() is VideoView videoView))
            {
                return;
            }
            switch (focusChange)
            {
                case AudioFocus.Gain: //获得焦点
                case AudioFocus.GainTransient: //暂时获得焦点
                    if (_startRequested || _pausedForLoss)
                    {
                        videoView.Start();
                        _startRequested = false;
                        _pausedForLoss = false;
                    }
                    if (!videoView.Mute) //恢复音量
                    {
                        videoView.SetVolume(1.0f, 1.0f);
                    }
                    break;
                case AudioFocus.Loss: //焦点丢失
                case AudioFocus.LossTransient: //焦点暂时丢失
                    if (videoView.Playing)
                    {
                        _pausedForLoss = true;
                        videoView.Pause();
                    }
                    break;
                case AudioFocus.LossTransientCanDuck: //此时需降低音量
                    if (videoView.Playing && !videoView.Mute)
                    {
                        videoView.SetVolume(0.1f, 0.1f);
                    }
                    break;
            }
        }

        /// <summary>
        /// Requests to obtain the audio focus
        /// </summary>
        internal void RequestFocus()
        {
            if (_currentFocus == AudioFocus.Gain)
            {
                return;
            }

            if (_mAudioManager == null)
            {
                return;
            }

            var status = _mAudioManager.RequestAudioFocus(this, Stream.Music, AudioFocus.Gain);
            if (status == AudioFocusRequest.Granted)
            {
                _currentFocus = AudioFocus.Gain;
                return;
            }

            _startRequested = true;
        }

        /// <summary>
        /// Requests the system to drop the audio focus
        /// </summary>
        internal void AbandonFocus()
        {

            if (_mAudioManager == null)
            {
                return;
            }

            _startRequested = false;
            _mAudioManager.AbandonAudioFocus(this);
        }
    }
}