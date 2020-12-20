using Android.Animation;
using Android.Content;
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
    /// 手势控制
    /// </summary>
    public  class GestureView : FrameLayout, IGestureComponent
    {
        public GestureView(Context context) : this(context, null)
        {
        }

        public GestureView(Context context, IAttributeSet attrs) : this(context, attrs, 0)
        {
        }

        public GestureView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Visibility = ViewStates.Gone;
            LayoutInflater.From(Context)?.Inflate(Resource.Layout.dkplayer_layout_gesture_control_view, this, true);
            mIcon = FindViewById<ImageView>(Resource.Id.iv_icon);
            mProgressPercent = FindViewById<ProgressBar>(Resource.Id.pro_percent);
            mTextPercent = FindViewById<TextView>(Resource.Id.tv_percent);
            mCenterContainer = FindViewById<LinearLayout>(Resource.Id.center_container);
        }

        private ControlWrapper mControlWrapper;

        private ImageView mIcon;
        private ProgressBar mProgressPercent;
        private TextView mTextPercent;

        private LinearLayout mCenterContainer;


        public void Attach(ControlWrapper controlWrapper)
        {
            mControlWrapper = controlWrapper;
        }

        public View View => this;

        public void OnVisibilityChanged(bool isVisible, Animation anim)
        {
        }

        public void OnPlayerStateChanged(int playerState)
        {
        }

        public void OnStartSlide()
        {
            mControlWrapper.Hide();
            mCenterContainer.Visibility = ViewStates.Visible;
            mCenterContainer.Alpha = 1f;
        }

        public void OnStopSlide()
        {
            mCenterContainer?.Animate()?.Alpha(0f)?.SetDuration(300)?
                .SetListener(new AnimatorListenerAdapterAnonymousInnerClass(this))?
                .Start();
        }

        private class AnimatorListenerAdapterAnonymousInnerClass : AnimatorListenerAdapter
        {
            private readonly GestureView _outerInstance;

            public AnimatorListenerAdapterAnonymousInnerClass(GestureView outerInstance)
            {
                _outerInstance = outerInstance;
            }

            public override void OnAnimationEnd(Animator animation)
            {
                base.OnAnimationEnd(animation);
                _outerInstance.mCenterContainer.Visibility = ViewStates.Gone;
            }
        }

        public void OnPositionChange(int slidePosition, int currentPosition, int duration)
        {
            mProgressPercent.Visibility = ViewStates.Gone;
            if (slidePosition > currentPosition)
            {
                mIcon.SetImageResource(Resource.Drawable.dkplayer_ic_action_fast_forward);
            }
            else
            {
                mIcon.SetImageResource(Resource.Drawable.dkplayer_ic_action_fast_rewind);
            }

            var x= PlayerUtils.StringForTime(slidePosition);
            var y = PlayerUtils.StringForTime(duration);
            mTextPercent.Text = $"{x}/{y}";
        }

        public void OnBrightnessChange(int percent)
        {
            mProgressPercent.Visibility = ViewStates.Visible;
            mIcon.SetImageResource(Resource.Drawable.dkplayer_ic_action_brightness);
            mTextPercent.Text = percent + "%";
            mProgressPercent.Progress = percent;
        }

        public void OnVolumeChange(int percent)
        {
            mProgressPercent.Visibility = ViewStates.Visible;
            if (percent <= 0)
            {
                mIcon.SetImageResource(Resource.Drawable.dkplayer_ic_action_volume_off);
            }
            else
            {
                mIcon.SetImageResource(Resource.Drawable.dkplayer_ic_action_volume_up);
            }

            mTextPercent.Text = percent + "%";
            mProgressPercent.Progress = percent;
        }

        public void OnPlayStateChanged(int playState)
        {
            if (playState == VideoView.STATE_IDLE || playState == VideoView.STATE_START_ABORT ||
                playState == VideoView.STATE_PREPARING || playState == VideoView.STATE_PREPARED ||
                playState == VideoView.STATE_ERROR || playState == VideoView.STATE_PLAYBACK_COMPLETED)
            {
                Visibility = ViewStates.Gone;
            }
            else
            {
                Visibility = ViewStates.Visible;
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