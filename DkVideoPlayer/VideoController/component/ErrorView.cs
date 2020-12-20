using System;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using DkVideoPlayer.VideoPlayer.Controller;
using VideoView = DkVideoPlayer.VideoPlayer.Player.VideoView;

namespace DkVideoPlayer.VideoController.component
{
    /// <summary>
    /// 播放出错提示界面
    /// Created by dueeeke on 2017/4/13.
    /// </summary>
    public  class ErrorView : LinearLayout, IControlComponent
    {
        private float _downX;
        private float _downY;

        private ControlWrapper _controlWrapper;

        public ErrorView(Context context) : this(context, null)
        {
        }


        public ErrorView(Context context, IAttributeSet attrs) : this(context, attrs,0)
        {
        }

        public ErrorView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Visibility = ViewStates.Gone;
            LayoutInflater.From(Context)?.Inflate(Resource.Layout.dkplayer_layout_error_view, this, true);
            var btn = FindViewById(Resource.Id.status_btn);
            if (btn != null)
                btn.Click += (sender, args) =>
                {
                    Visibility = ViewStates.Gone;
                    _controlWrapper.Replay(false);
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
            if (playState == VideoView.STATE_ERROR)
            {
                BringToFront();
                Visibility = ViewStates.Visible;
            }
            else if (playState == VideoView.STATE_IDLE)
            {
                Visibility = ViewStates.Gone;
            }
        }

        public void OnPlayerStateChanged(int playerState)
        {
        }

        public void SetProgress(int duration, int position)
        {
        }

        public void OnLockStateChanged(bool isLock)
        {
        }

        public override bool DispatchTouchEvent(MotionEvent ev)
        {
            switch (ev.Action)
            {
                case MotionEventActions.Down:
                    _downX = ev.GetX();
                    _downY = ev.GetY();
                    Parent?.RequestDisallowInterceptTouchEvent(true);
                    break;
                case MotionEventActions.Move:
                    var absDeltaX = Math.Abs(ev.GetX() - _downX);
                    var absDeltaY = Math.Abs(ev.GetY() - _downY);
                    if (absDeltaX > ViewConfiguration.Get(Context)?.ScaledTouchSlop ||
                        absDeltaY > ViewConfiguration.Get(Context)?.ScaledTouchSlop)
                    {
                        Parent?.RequestDisallowInterceptTouchEvent(false);
                    }

                    break;
                case MotionEventActions.Up:
                    break;
            }

            return base.DispatchTouchEvent(ev);
        }
    }
}