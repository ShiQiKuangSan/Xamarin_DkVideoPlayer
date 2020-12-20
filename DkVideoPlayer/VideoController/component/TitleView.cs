using Android.App;
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
    /// 播放器顶部标题栏
    /// </summary>
    public class TitleView : FrameLayout, IControlComponent
    {
        private ControlWrapper mControlWrapper;

        private LinearLayout mTitleContainer;
        private TextView mTitle;
        private TextView mSysTime; //系统当前时间

        private BatteryReceiver mBatteryReceiver;
        private bool mIsRegister; //是否注册BatteryReceiver

        public TitleView(Context context) : this(context, null)
        {
        }

        public TitleView(Context context, IAttributeSet attrs) : this(context, attrs, 0)
        {
        }

        public TitleView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Visibility = ViewStates.Gone;
            LayoutInflater.From(Context)?.Inflate(Resource.Layout.dkplayer_layout_title_view, this, true);
            mTitleContainer = FindViewById<LinearLayout>(Resource.Id.title_container);
            mTitle = FindViewById<TextView>(Resource.Id.title);
            mSysTime = FindViewById<TextView>(Resource.Id.sys_time);
            //电量
            var batteryLevel = FindViewById<ImageView>(Resource.Id.iv_battery);
            mBatteryReceiver = new BatteryReceiver(batteryLevel);
        }

        public virtual string Title
        {
            set => mTitle.Text = value;
        }

        protected override void OnDetachedFromWindow()
        {
            base.OnDetachedFromWindow();
            if (mIsRegister)
            {
                Context?.UnregisterReceiver(mBatteryReceiver);
                mIsRegister = false;
            }
        }

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            if (!mIsRegister)
            {
                Context?.RegisterReceiver(mBatteryReceiver, new IntentFilter(Intent.ActionBatteryChanged));
                mIsRegister = true;
            }
        }

        public void Attach(ControlWrapper controlWrapper)
        {
            mControlWrapper = controlWrapper;
        }

        public View View => this;

        public void OnVisibilityChanged(bool isVisible, Animation anim)
        {
            //只在全屏时才有效
            if (!mControlWrapper.FullScreen)
            {
                return;
            }

            if (isVisible)
            {
                if (Visibility == ViewStates.Gone)
                {
                    mSysTime.Text = PlayerUtils.CurrentSystemTime;
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
            }
        }

        public void OnPlayerStateChanged(int playerState)
        {
            if (playerState == VideoView.PLAYER_FULL_SCREEN)
            {
                if (mControlWrapper.Showing && !mControlWrapper.Locked)
                {
                    Visibility = ViewStates.Visible;
                    mSysTime.Text = PlayerUtils.CurrentSystemTime;
                }

                mTitle.Selected = true;
            }
            else
            {
                Visibility = ViewStates.Gone;
                mTitle.Selected = false;
            }

            var activity = PlayerUtils.ScanForActivity(Context);
            if (activity != null && mControlWrapper.HasCutout())
            {
                var orientation = activity.RequestedOrientation;
                var cutoutHeight = mControlWrapper.CutoutHeight;
                if (orientation == ScreenOrientation.Portrait)
                {
                    mTitleContainer.SetPadding(0, 0, 0, 0);
                }
                else if (orientation == ScreenOrientation.Landscape)
                {
                    mTitleContainer.SetPadding(cutoutHeight, 0, 0, 0);
                }
                else if (orientation == ScreenOrientation.ReverseLandscape)
                {
                    mTitleContainer.SetPadding(0, 0, cutoutHeight, 0);
                }
            }
        }

        public void SetProgress(int duration, int position)
        {
        }

        public void OnLockStateChanged(bool isLocked)
        {
            if (isLocked)
            {
                Visibility = ViewStates.Gone;
            }
            else
            {
                Visibility = ViewStates.Visible;
                mSysTime.Text = PlayerUtils.CurrentSystemTime;
            }
        }

        private class BatteryReceiver : BroadcastReceiver
        {
            internal ImageView pow;

            public BatteryReceiver(ImageView pow)
            {
                this.pow = pow;
            }

            public override void OnReceive(Context context, Intent intent)
            {
                var extras = intent.Extras;
                if (extras == null)
                {
                    return;
                }

                var current = extras.GetInt("level"); // 获得当前电量
                var total = extras.GetInt("scale"); // 获得总电量
                var percent = current * 100 / total;
                pow.Drawable?.SetLevel(percent);
            }
        }
    }
}