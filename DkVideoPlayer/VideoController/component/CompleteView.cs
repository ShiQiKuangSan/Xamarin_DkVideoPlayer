using Android.Content;
using Android.Content.PM;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using DkVideoPlayer.VideoPlayer.Controller;
using DkVideoPlayer.VideoPlayer.Util;
using VideoView = DkVideoPlayer.VideoPlayer.Player.VideoView;

namespace DkVideoPlayer.VideoController.component
{
    /// <summary>
    /// 自动播放完成界面
    /// </summary>
    public  class CompleteView : FrameLayout, IControlComponent
    {
        private ControlWrapper _controlWrapper;

        private readonly ImageView _stopFullscreen;

        public CompleteView(Context context) : this(context, null)
        {
        }

        public CompleteView(Context context, IAttributeSet attrs) : this(context, attrs, 0)
        {
        }

        public CompleteView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Visibility = ViewStates.Gone;
            LayoutInflater.From(Context)?.Inflate(Resource.Layout.dkplayer_layout_complete_view, this, true);
            var rep = FindViewById(Resource.Id.iv_replay);
            if (rep != null) rep.Click += (sender, args) => { _controlWrapper.Replay(true); };

            _stopFullscreen = FindViewById<ImageView>(Resource.Id.stop_fullscreen);
            if (_stopFullscreen != null)
                _stopFullscreen.Click += (sender, args) =>
                {
                    if (_controlWrapper.FullScreen)
                    {
                        var activity = PlayerUtils.ScanForActivity(Context);
                        if (activity != null && !activity.IsFinishing)
                        {
                            activity.RequestedOrientation = ScreenOrientation.Portrait;
                            _controlWrapper.StopFullScreen();
                        }
                    }
                };

            Clickable = true;
        }

        public void Attach(ControlWrapper controlWrapper)
        {
            _controlWrapper = controlWrapper;
        }

        public View View => this;

        public void OnVisibilityChanged(bool isVisible, Animation anim)
        {
        }

        public void OnPlayStateChanged(int playState)
        {
            if (playState == VideoView.STATE_PLAYBACK_COMPLETED)
            {
                Visibility = ViewStates.Visible;
                _stopFullscreen.Visibility = _controlWrapper.FullScreen ? ViewStates.Visible : ViewStates.Gone;
                BringToFront();
            }
            else
            {
                Visibility = ViewStates.Gone;
            }
        }

        public void OnPlayerStateChanged(int playerState)
        {
            if (playerState == VideoView.PLAYER_FULL_SCREEN)
            {
                _stopFullscreen.Visibility = ViewStates.Visible;
            }
            else if (playerState == VideoView.PLAYER_NORMAL)
            {
                _stopFullscreen.Visibility = ViewStates.Gone;
            }

            var activity = PlayerUtils.ScanForActivity(Context);
            if (activity != null && _controlWrapper.HasCutout())
            {
                var orientation = activity.RequestedOrientation;
                var cutoutHeight = _controlWrapper.CutoutHeight;
                var sflp = (LayoutParams) _stopFullscreen.LayoutParameters;
                if (orientation == ScreenOrientation.Portrait)
                {
                    sflp?.SetMargins(0, 0, 0, 0);
                }
                else if (orientation == ScreenOrientation.Landscape)
                {
                    sflp?.SetMargins(cutoutHeight, 0, 0, 0);
                }
                else if (orientation == ScreenOrientation.ReverseLandscape)
                {
                    sflp?.SetMargins(0, 0, 0, 0);
                }
            }
        }

        public void SetProgress(int duration, int position)
        {
        }

        public void OnLockStateChanged(bool isLock)
        {
        }
    }
}