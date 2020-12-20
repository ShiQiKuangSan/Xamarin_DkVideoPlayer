using System;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using DkVideoPlayer.VideoPlayer.Controller;
using DkVideoPlayer.VideoPlayer.Player;
using VideoView = DkVideoPlayer.VideoPlayer.Player.VideoView;

namespace DkVideoPlayer.VideoController.component
{
    /// <summary>
    /// 准备播放界面
    /// </summary>
    public class PrepareView : FrameLayout, IControlComponent
    {
        private ControlWrapper mControlWrapper;

        private ImageView mThumb;
        private ImageView mStartPlay;
        private ProgressBar mLoading;
        private FrameLayout mNetWarning;

        public PrepareView(Context context) : this(context, null)
        {
        }

        public PrepareView(Context context, IAttributeSet attrs) : this(context, attrs, 0)
        {
        }

        public PrepareView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            LayoutInflater.From(Context)?.Inflate(Resource.Layout.dkplayer_layout_prepare_view, this, true);
            mThumb = FindViewById<ImageView>(Resource.Id.thumb);
            mStartPlay = FindViewById<ImageView>(Resource.Id.start_play);
            mLoading = FindViewById<ProgressBar>(Resource.Id.loading);
            mNetWarning = FindViewById<FrameLayout>(Resource.Id.net_warning_layout);
            var btn = FindViewById(Resource.Id.status_btn);
            if (btn != null)
                btn.Click += (sender, args) =>
                {
                    mNetWarning.Visibility = ViewStates.Gone;
                    VideoViewManager.Instance().PlayOnMobileNetwork = true;
                    mControlWrapper.Start();
                };
        }

        /// <summary>
        /// 设置点击此界面开始播放
        /// </summary>
        public virtual void SetClickStart()
        {
            Click += OnClick;
        }

        private void OnClick(object sender, EventArgs e)
        {
            mControlWrapper.Start();
        }

        public void Attach(ControlWrapper controlWrapper)
        {
            mControlWrapper = controlWrapper;
        }

        public View View => this;

        public void OnVisibilityChanged(bool isVisible, Animation anim)
        {
        }

        public void OnPlayStateChanged(int playState)
        {
            switch (playState)
            {
                case VideoView.STATE_PREPARING:
                    BringToFront();
                    Visibility = ViewStates.Visible;
                    mStartPlay.Visibility = ViewStates.Gone;
                    mNetWarning.Visibility = ViewStates.Gone;
                    mLoading.Visibility = ViewStates.Visible;
                    break;
                case VideoView.STATE_PLAYING:
                case VideoView.STATE_PAUSED:
                case VideoView.STATE_ERROR:
                case VideoView.STATE_BUFFERING:
                case VideoView.STATE_BUFFERED:
                case VideoView.STATE_PLAYBACK_COMPLETED:
                    Visibility = ViewStates.Gone;
                    break;
                case VideoView.STATE_IDLE:
                    Visibility = ViewStates.Visible;
                    BringToFront();
                    mLoading.Visibility = ViewStates.Gone;
                    mNetWarning.Visibility = ViewStates.Gone;
                    mStartPlay.Visibility = ViewStates.Visible;
                    mThumb.Visibility = ViewStates.Visible;
                    break;
                case VideoView.STATE_START_ABORT:
                    Visibility = ViewStates.Visible;
                    mNetWarning.Visibility = ViewStates.Visible;
                    mNetWarning.BringToFront();
                    break;
            }
        }

        public void OnPlayerStateChanged(int playerState)
        {
        }

        public void SetProgress(int duration, int position)
        {
        }

        public void OnLockStateChanged(bool isLocked)
        {
        }
    }
}