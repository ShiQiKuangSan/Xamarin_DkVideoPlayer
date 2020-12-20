using Android.Content;
using Android.Content.PM;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;

using DkVideoPlayer.VideoController.component;
using DkVideoPlayer.VideoPlayer.Controller;
using DkVideoPlayer.VideoPlayer.Util;

using VideoView = DkVideoPlayer.VideoPlayer.Player.VideoView;

namespace DkVideoPlayer.VideoController
{
    /// <summary>
    /// 直播/点播控制器
    /// 注意：此控制器仅做一个参考，如果想定制ui，你可以直接继承GestureVideoController或者BaseVideoController实现
    /// 你自己的控制器
    /// Created by dueeeke on 2017/4/7.
    /// </summary>
    public class StandardVideoController : GestureVideoController, View.IOnClickListener
    {
        protected ImageView mLockButton;

        protected ProgressBar mLoadingProgress;

        public StandardVideoController(Context context) : base(context)
        {
        }

        public StandardVideoController(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public StandardVideoController(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs,
            defStyleAttr)
        {
        }

        protected override int LayoutId => Resource.Layout.dkplayer_layout_standard_controller;

        protected override void InitView()
        {
            base.InitView();
            mLockButton = FindViewById<ImageView>(Resource.Id.@lock);
            mLockButton?.SetOnClickListener(this);
            mLoadingProgress = FindViewById<ProgressBar>(Resource.Id.loading);
        }

        /// <summary>
        /// 快速添加各个组件 </summary>
        /// <param name="title">  标题 </param>
        /// <param name="isLive"> 是否为直播 </param>
        public virtual void AddDefaultControlComponent(string title, bool isLive)
        {
            var completeView = new CompleteView(Context);
            var errorView = new ErrorView(Context);
            var prepareView = new PrepareView(Context);
            prepareView.SetClickStart();
            var titleView = new TitleView(Context) { Title = title };
            AddControlComponent(completeView, errorView, prepareView, titleView);
            if (isLive)
            {
                AddControlComponent(new LiveControlView(Context));
            }
            else
            {
                AddControlComponent(new VodControlView(Context));
            }

            AddControlComponent(new GestureView(Context));
            CanChangePosition = !isLive;
        }

        public void OnClick(View v)
        {
            int i = v.Id;
            if (i == Resource.Id.@lock)
            {
                ControlWrapper.ToggleLockState();
            }
        }

        protected override void OnLockStateChanged(bool isLocked)
        {
            if (isLocked)
            {
                mLockButton.Selected = true;
                Toast.MakeText(Context, Resource.String.dkplayer_locked, ToastLength.Short)?.Show();
            }
            else
            {
                mLockButton.Selected = false;
                Toast.MakeText(Context, Resource.String.dkplayer_unlocked, ToastLength.Short)?.Show();
            }
        }

        protected override void OnVisibilityChanged(bool isVisible, Animation anim)
        {
            if (ControlWrapper.FullScreen)
            {
                if (isVisible)
                {
                    if (mLockButton.Visibility == ViewStates.Gone)
                    {
                        mLockButton.Visibility = ViewStates.Visible;
                        if (anim != null)
                        {
                            mLockButton.StartAnimation(anim);
                        }
                    }
                }
                else
                {
                    mLockButton.Visibility = ViewStates.Gone;
                    if (anim != null)
                    {
                        mLockButton.StartAnimation(anim);
                    }
                }
            }
        }

        protected override void OnPlayerStateChanged(int playerState)
        {
            base.OnPlayerStateChanged(playerState);
            switch (playerState)
            {
                case VideoView.PLAYER_NORMAL:
                    LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent,
                        ViewGroup.LayoutParams.MatchParent);
                    mLockButton.Visibility = ViewStates.Gone;
                    break;
                case VideoView.PLAYER_FULL_SCREEN:
                    if (Showing)
                    {
                        mLockButton.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        mLockButton.Visibility = ViewStates.Gone;
                    }

                    break;
            }

            if (mActivity != null && HasCutout())
            {
                var orientation = mActivity.RequestedOrientation;
                var dp24 = PlayerUtils.Dp2Px(Context, 24);
                var cutoutHeight = CutoutHeight;
                if (orientation == ScreenOrientation.Portrait)
                {
                    var lblp = (LayoutParams)mLockButton.LayoutParameters;
                    lblp?.SetMargins(dp24, 0, dp24, 0);
                }
                else if (orientation == ScreenOrientation.Landscape)
                {
                    var layoutParams = (LayoutParams)mLockButton.LayoutParameters;
                    layoutParams?.SetMargins(dp24 + cutoutHeight, 0, dp24 + cutoutHeight, 0);
                }
                else if (orientation == ScreenOrientation.ReverseLandscape)
                {
                    var layoutParams = (LayoutParams)mLockButton.LayoutParameters;
                    layoutParams?.SetMargins(dp24, 0, dp24, 0);
                }
            }
        }

        protected override void OnPlayStateChanged(int playState)
        {
            base.OnPlayStateChanged(playState);
            switch (playState)
            {
                //调用release方法会回到此状态
                case VideoView.STATE_IDLE:
                    mLockButton.Selected = false;
                    mLoadingProgress.Visibility = ViewStates.Gone;
                    break;
                case VideoView.STATE_PLAYING:
                case VideoView.STATE_PAUSED:
                case VideoView.STATE_PREPARED:
                case VideoView.STATE_ERROR:
                case VideoView.STATE_BUFFERED:
                    mLoadingProgress.Visibility = ViewStates.Gone;
                    break;
                case VideoView.STATE_PREPARING:
                case VideoView.STATE_BUFFERING:
                    mLoadingProgress.Visibility = ViewStates.Visible;
                    break;
                case VideoView.STATE_PLAYBACK_COMPLETED:
                    mLoadingProgress.Visibility = ViewStates.Gone;
                    mLockButton.Visibility = ViewStates.Gone;
                    mLockButton.Selected = false;
                    break;
            }
        }

        public override bool OnBackPressed()
        {
            if (IsLocked)
            {
                Show();
                Toast.MakeText(Context, Resource.String.dkplayer_lock_tip, ToastLength.Short)?.Show();
                return true;
            }
            if (ControlWrapper.FullScreen)
            {
                return StopFullScreen();
            }

            return base.OnBackPressed();
        }
    }
}