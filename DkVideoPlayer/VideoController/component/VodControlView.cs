using Android.Content;
using Android.Content.PM;
using Android.OS;
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
    /// 点播底部控制栏
    /// </summary>
    public class VodControlView : FrameLayout, IControlComponent, View.IOnClickListener,SeekBar.IOnSeekBarChangeListener
    {
        protected ControlWrapper ControlWrapper;

        private TextView mTotalTime, mCurrTime;
        private LinearLayout mBottomContainer;
        private SeekBar mVideoProgress;
        private ProgressBar mBottomProgress;
        private ImageView mPlayButton;

        private bool mIsDragging;

        private bool mIsShowBottomProgress = true;

        public VodControlView(Context context) : this(context, null)
        {
        }

        public VodControlView(Context context, IAttributeSet attrs) : this(context, attrs, 0)
        {
        }

        public VodControlView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs,
            defStyleAttr)
        {
            Visibility = ViewStates.Gone;
            LayoutInflater.From(Context)?.Inflate(LayoutId, this, true);
            mBottomContainer = FindViewById<LinearLayout>(Resource.Id.bottom_container);
            mVideoProgress = FindViewById<SeekBar>(Resource.Id.seekBar);
            mVideoProgress?.SetOnSeekBarChangeListener(this);
            mTotalTime = FindViewById<TextView>(Resource.Id.total_time);
            mCurrTime = FindViewById<TextView>(Resource.Id.curr_time);
            mPlayButton = FindViewById<ImageView>(Resource.Id.iv_play);
            mPlayButton?.SetOnClickListener(this);
            mBottomProgress = FindViewById<ProgressBar>(Resource.Id.bottom_progress);

            //5.1以下系统SeekBar高度需要设置成WRAP_CONTENT
            if (Build.VERSION.SdkInt <= BuildVersionCodes.LollipopMr1)
            {
                if (mVideoProgress.LayoutParameters != null)
                    mVideoProgress.LayoutParameters.Height = ViewGroup.LayoutParams.WrapContent;
            }
        }


        protected virtual int LayoutId => Resource.Layout.dkplayer_layout_vod_control_view;


        /// <summary>
        /// 是否显示底部进度条，默认显示
        /// </summary>
        public virtual void ShowBottomProgress(bool isShow)
        {
            mIsShowBottomProgress = isShow;
        }

        public void Attach(ControlWrapper controlWrapper)
        {
            ControlWrapper = controlWrapper;
        }

        public View View => this;

        public void OnVisibilityChanged(bool isVisible, Animation anim)
        {
            if (isVisible)
            {
                mBottomContainer.Visibility = ViewStates.Visible;
                if (anim != null)
                {
                    mBottomContainer.StartAnimation(anim);
                }

                if (mIsShowBottomProgress)
                {
                    mBottomProgress.Visibility = ViewStates.Gone;
                }
            }
            else
            {
                mBottomContainer.Visibility = ViewStates.Gone;
                if (anim != null)
                {
                    mBottomContainer.StartAnimation(anim);
                }

                if (mIsShowBottomProgress)
                {
                    mBottomProgress.Visibility = ViewStates.Visible;
                    var animation = new AlphaAnimation(0f, 1f) {Duration = 300};
                    mBottomProgress.StartAnimation(animation);
                }
            }
        }

        public void OnPlayStateChanged(int playState)
        {
            switch (playState)
            {
                case VideoView.STATE_IDLE:
                case VideoView.STATE_PLAYBACK_COMPLETED:
                    Visibility = ViewStates.Gone;
                    mBottomProgress.Progress = 0;
                    mBottomProgress.SecondaryProgress = 0;
                    mVideoProgress.Progress = 0;
                    mVideoProgress.SecondaryProgress = 0;
                    break;
                case VideoView.STATE_START_ABORT:
                case VideoView.STATE_PREPARING:
                case VideoView.STATE_PREPARED:
                case VideoView.STATE_ERROR:
                    Visibility = ViewStates.Gone;
                    break;
                case VideoView.STATE_PLAYING:
                    mPlayButton.Selected = true;
                    if (mIsShowBottomProgress)
                    {
                        if (ControlWrapper.Showing)
                        {
                            mBottomProgress.Visibility = ViewStates.Gone;
                            mBottomContainer.Visibility = ViewStates.Visible;
                        }
                        else
                        {
                            mBottomContainer.Visibility = ViewStates.Gone;
                            mBottomProgress.Visibility = ViewStates.Visible;
                        }
                    }
                    else
                    {
                        mBottomContainer.Visibility = ViewStates.Gone;
                    }

                    Visibility = ViewStates.Visible;
                    //开始刷新进度
                    ControlWrapper.StartProgress();
                    break;
                case VideoView.STATE_PAUSED:
                    mPlayButton.Selected = false;
                    break;
                case VideoView.STATE_BUFFERING:
                case VideoView.STATE_BUFFERED:
                    mPlayButton.Selected = ControlWrapper.Playing;
                    break;
            }
        }

        public void OnPlayerStateChanged(int playerState)
        {
            var activity = PlayerUtils.ScanForActivity(Context);
            if (activity != null && ControlWrapper.HasCutout())
            {
                var orientation = activity.RequestedOrientation;
                var cutoutHeight = ControlWrapper.CutoutHeight;
                if (orientation == ScreenOrientation.Portrait)
                {
                    mBottomContainer.SetPadding(0, 0, 0, 0);
                    mBottomProgress.SetPadding(0, 0, 0, 0);
                }
                else if (orientation == ScreenOrientation.Landscape)
                {
                    mBottomContainer.SetPadding(cutoutHeight, 0, 0, 0);
                    mBottomProgress.SetPadding(cutoutHeight, 0, 0, 0);
                }
                else if (orientation == ScreenOrientation.ReverseLandscape)
                {
                    mBottomContainer.SetPadding(0, 0, cutoutHeight, 0);
                    mBottomProgress.SetPadding(0, 0, cutoutHeight, 0);
                }
            }
        }

        public void SetProgress(int duration, int position)
        {
            if (mIsDragging)
            {
                return;
            }

            if (mVideoProgress != null)
            {
                if (duration > 0)
                {
                    mVideoProgress.Enabled = true;
                    int pos = (int) (position * 1.0 / duration * mVideoProgress.Max);
                    mVideoProgress.Progress = pos;
                    mBottomProgress.Progress = pos;
                }
                else
                {
                    mVideoProgress.Enabled = false;
                }

                int percent = ControlWrapper.BufferedPercentage;
                if (percent >= 95)
                {
                    //解决缓冲进度不能100%问题
                    mVideoProgress.SecondaryProgress = mVideoProgress.Max;
                    mBottomProgress.SecondaryProgress = mBottomProgress.Max;
                }
                else
                {
                    mVideoProgress.SecondaryProgress = percent * 10;
                    mBottomProgress.SecondaryProgress = percent * 10;
                }
            }

            if (mTotalTime != null)
            {
                mTotalTime.Text = PlayerUtils.StringForTime(duration);
            }

            if (mCurrTime != null)
            {
                mCurrTime.Text = PlayerUtils.StringForTime(position);
            }
        }

        public void OnLockStateChanged(bool isLocked)
        {
            OnVisibilityChanged(!isLocked, null);
        }

        public void OnClick(View v)
        {
            int id = v.Id; 
            if (id == Resource.Id.iv_play)
            {
                ControlWrapper.TogglePlay();
            }
        }
        
        /// <summary>
        /// 横竖屏切换
        /// </summary>
        private void ToggleFullScreen()
        {
            var activity = PlayerUtils.ScanForActivity(Context);
            ControlWrapper.ToggleFullScreen(activity);
        }

        public void OnStartTrackingTouch(SeekBar seekBar)
        {
            mIsDragging = true;
            ControlWrapper.StopProgress();
            ControlWrapper.StopFadeOut();
        }

        public void OnStopTrackingTouch(SeekBar seekBar)
        {
            var duration = ControlWrapper.Duration;
            var newPosition = (duration * seekBar.Progress) / mVideoProgress.Max;
            ControlWrapper.SeekTo((int) newPosition);
            mIsDragging = false;
            ControlWrapper.StartProgress();
            ControlWrapper.StartFadeOut();
        }

        public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
        {
            if (!fromUser)
            {
                return;
            }

            var duration = ControlWrapper.Duration;
            var newPosition = (duration * progress) / mVideoProgress.Max;
            if (mCurrTime != null)
            {
                mCurrTime.Text = PlayerUtils.StringForTime((int) newPosition);
            }
        }
    }
}