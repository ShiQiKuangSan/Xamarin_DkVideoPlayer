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
    /// 直播底部控制栏
    /// </summary>
    public  class LiveControlView : FrameLayout, IControlComponent, View.IOnClickListener
    {
        private ControlWrapper _controlWrapper;

        private readonly LinearLayout _bottomContainer;
        private readonly ImageView _playButton;


        public LiveControlView(Context context) : this(context, null)
        {
        }

        public LiveControlView(Context context, IAttributeSet attrs) : this(context, attrs, 0)
        {
        }

        public LiveControlView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs,
            defStyleAttr)
        {
            Visibility = ViewStates.Gone;
            LayoutInflater.From(Context)?.Inflate(Resource.Layout.dkplayer_layout_live_control_view, this, true);
            _bottomContainer = FindViewById<LinearLayout>(Resource.Id.bottom_container);
            _playButton = FindViewById<ImageView>(Resource.Id.iv_play);
            _playButton?.SetOnClickListener(this);
            var refresh = FindViewById<ImageView>(Resource.Id.iv_refresh);
            refresh?.SetOnClickListener(this);
        }


        public void Attach(ControlWrapper controlWrapper)
        {
            _controlWrapper = controlWrapper;
        }

        public View View => this;

        public void OnVisibilityChanged(bool isVisible, Animation anim)
        {
            if (isVisible)
            {
                if (Visibility == ViewStates.Gone)
                {
                    Visibility = ViewStates.Visible;
                    if (anim != null)
                    {
                        StartAnimation(anim);
                    }
                }
            }
            else
            {
                if (Visibility == ViewStates.Visible)
                {
                    Visibility = ViewStates.Gone;
                    if (anim != null)
                    {
                        StartAnimation(anim);
                    }
                }
            }
        }

        public void OnPlayStateChanged(int playState)
        {
            switch (playState)
            {
                case VideoView.STATE_IDLE:
                case VideoView.STATE_START_ABORT:
                case VideoView.STATE_PREPARING:
                case VideoView.STATE_PREPARED:
                case VideoView.STATE_ERROR:
                case VideoView.STATE_PLAYBACK_COMPLETED:
                    Visibility = ViewStates.Gone;
                    break;
                case VideoView.STATE_PLAYING:
                    _playButton.Selected = true;
                    break;
                case VideoView.STATE_PAUSED:
                    _playButton.Selected = false;
                    break;
                case VideoView.STATE_BUFFERING:
                case VideoView.STATE_BUFFERED:
                    _playButton.Selected = _controlWrapper.Playing;
                    break;
            }
        }

        public void OnPlayerStateChanged(int playerState)
        {
            switch (playerState)
            {
                case VideoView.PLAYER_NORMAL:
                    break;
                case VideoView.PLAYER_FULL_SCREEN:
                    break;
            }

            var activity = PlayerUtils.ScanForActivity(Context);
            if (activity != null && _controlWrapper.HasCutout())
            {
                var orientation = activity.RequestedOrientation;
                var cutoutHeight = _controlWrapper.CutoutHeight;
                if (orientation == ScreenOrientation.Portrait)
                {
                    _bottomContainer.SetPadding(0, 0, 0, 0);
                }
                else if (orientation == ScreenOrientation.Landscape)
                {
                    _bottomContainer.SetPadding(cutoutHeight, 0, 0, 0);
                }
                else if (orientation == ScreenOrientation.ReverseLandscape)
                {
                    _bottomContainer.SetPadding(0, 0, cutoutHeight, 0);
                }
            }
        }

        public void SetProgress(int duration, int position)
        {
        }

        public void OnLockStateChanged(bool isLocked)
        {
            OnVisibilityChanged(!isLocked, null);
        }

        public void OnClick(View v)
        {
            var id = v.Id; 
            if (id == Resource.Id.iv_play)
            {
                _controlWrapper.TogglePlay();
            }
            else if (id == Resource.Id.iv_refresh)
            {
                _controlWrapper.Replay(true);
            }
        }

        /// <summary>
        /// 横竖屏切换
        /// </summary>
        private void ToggleFullScreen()
        {
            var activity = PlayerUtils.ScanForActivity(Context);
            _controlWrapper.ToggleFullScreen(activity);
        }
    }
}