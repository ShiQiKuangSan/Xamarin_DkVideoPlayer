using System;
using System.Linq;

using Android.Content;
using Android.Media;
using Android.Runtime;
using Android.Util;
using Android.Views;

using DkVideoPlayer.VideoPlayer.Player;
using DkVideoPlayer.VideoPlayer.Util;

namespace DkVideoPlayer.VideoPlayer.Controller
{
    /// <summary>
    /// 包含手势操作的VideoController
    /// Created by dueeeke on 2018/1/6.
    /// </summary>
    public abstract class GestureVideoController : BaseVideoController, GestureDetector.IOnGestureListener,
        GestureDetector.IOnDoubleTapListener, View.IOnTouchListener
    {
        private GestureDetector _mGestureDetector;
        private AudioManager _mAudioManager;
        private bool _mIsGestureEnabled = true;
        private int _mStreamVolume;
        private float _mBrightness;
        private int _mSeekPosition;
        private bool _mFirstTouch;
        private bool _mChangePosition;
        private bool _mChangeBrightness;
        private bool _mChangeVolume;

        private bool _mCanChangePosition = true;

        private bool _mEnableInNormal = true;

        private bool _mCanSlide;

        private int _mCurPlayState;


        public GestureVideoController(Context context) : base(context)
        {
        }

        public GestureVideoController(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public GestureVideoController(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs,
            defStyleAttr)
        {
        }

        protected override void InitView()
        {
            base.InitView();
            if (Context != null)
            {
                _mAudioManager = Context.GetSystemService(Context.AudioService).JavaCast<AudioManager>();
                _mGestureDetector = new GestureDetector(Context, this);
            }

            SetOnTouchListener(this);
        }

        /// <summary>
        /// 设置是否可以滑动调节进度，默认可以
        /// </summary>
        public virtual bool CanChangePosition
        {
            set => _mCanChangePosition = value;
        }

        /// <summary>
        /// 是否在竖屏模式下开始手势控制，默认关闭
        /// </summary>
        public virtual bool EnableInNormal
        {
            set => _mEnableInNormal = value;
        }

        /// <summary>
        /// 是否开启手势空控制，默认开启，关闭之后，双击播放暂停以及手势调节进度，音量，亮度功能将关闭
        /// </summary>
        public virtual bool GestureEnabled
        {
            set => _mIsGestureEnabled = value;
        }

        public override int PlayerState
        {
            set
            {
                base.PlayerState = value;
                if (value == VideoView.PLAYER_NORMAL)
                {
                    _mCanSlide = _mEnableInNormal;
                }
                else if (value == VideoView.PLAYER_FULL_SCREEN)
                {
                    _mCanSlide = true;
                }
            }
        }

        public override int PlayState
        {
            set
            {
                base.PlayState = value;
                _mCurPlayState = value;
            }
        }

        private bool InPlaybackState =>
            ControlWrapper != null && _mCurPlayState != VideoView.STATE_ERROR &&
            _mCurPlayState != VideoView.STATE_IDLE &&
            _mCurPlayState != VideoView.STATE_PREPARING &&
            _mCurPlayState != VideoView.STATE_PREPARED &&
            _mCurPlayState != VideoView.STATE_START_ABORT &&
            _mCurPlayState != VideoView.STATE_PLAYBACK_COMPLETED;

        public bool OnTouch(View v, MotionEvent @event)
        {
            return _mGestureDetector.OnTouchEvent(@event);
        }

        /// <summary>
        /// 手指按下的瞬间
        /// </summary>
        public bool OnDown(MotionEvent e)
        {
            if (!InPlaybackState || !_mIsGestureEnabled || PlayerUtils.IsEdge(Context, e)) //处于屏幕边沿 - 关闭了手势 - 不处于播放状态
            {
                return true;
            }

            _mStreamVolume = _mAudioManager.GetStreamVolume(Stream.Music);
            var activity = PlayerUtils.ScanForActivity(Context);
            if (activity == null)
            {
                _mBrightness = 0;
            }
            else
            {
                _mBrightness = activity.Window.Attributes.ScreenBrightness;
            }

            _mFirstTouch = true;
            _mChangePosition = false;
            _mChangeBrightness = false;
            _mChangeVolume = false;
            return true;
        }

        /// <summary>
        /// 单击
        /// </summary>
        public bool OnSingleTapConfirmed(MotionEvent e)
        {
            if (InPlaybackState)
            {
                ControlWrapper.ToggleShowState();
            }

            return true;
        }

        /// <summary>
        /// 双击
        /// </summary>
        public bool OnDoubleTap(MotionEvent e)
        {
            if (!Locked && InPlaybackState)
            {
                TogglePlay();
            }

            return true;
        }

        /// <summary>
        /// 在屏幕上滑动
        /// </summary>
        public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            if (!InPlaybackState || !_mIsGestureEnabled || !_mCanSlide || Locked || PlayerUtils.IsEdge(Context, e1)
            ) //处于屏幕边沿 - 锁住了屏幕 - 关闭了滑动手势 - 关闭了手势 - 不处于播放状态
            {
                return true;
            }

            float deltaX = e1.GetX() - e2.GetX();
            float deltaY = e1.GetY() - e2.GetY();
            if (_mFirstTouch)
            {
                _mChangePosition = Math.Abs(distanceX) >= Math.Abs(distanceY);
                if (!_mChangePosition)
                {
                    //半屏宽度
                    int halfScreen = PlayerUtils.GetScreenWidth(Context, true) / 2;
                    if (e2.GetX() > halfScreen)
                    {
                        _mChangeVolume = true;
                    }
                    else
                    {
                        _mChangeBrightness = true;
                    }
                }

                if (_mChangePosition)
                {
                    //根据用户设置是否可以滑动调节进度来决定最终是否可以滑动调节进度
                    _mChangePosition = _mCanChangePosition;
                }

                if (_mChangePosition || _mChangeBrightness || _mChangeVolume)
                {
                    foreach (var component in ControlComponents.Select(next => next.Key))
                    {
                        if (component is IGestureComponent gestureComponent)
                        {
                            gestureComponent.OnStartSlide();
                        }
                    }
                }

                _mFirstTouch = false;
            }

            if (_mChangePosition)
            {
                SlideToChangePosition(deltaX);
            }
            else if (_mChangeBrightness)
            {
                SlideToChangeBrightness(deltaY);
            }
            else if (_mChangeVolume)
            {
                SlideToChangeVolume(deltaY);
            }

            return true;
        }

        protected virtual void SlideToChangePosition(float deltaX)
        {
            deltaX = -deltaX;
            var width = MeasuredWidth;
            var duration = (int)ControlWrapper.Duration;
            var currentPosition = (int)ControlWrapper.CurrentPosition;
            var position = (int)(deltaX / width * 120000 + currentPosition);
            if (position > duration)
            {
                position = duration;
            }

            if (position < 0)
            {
                position = 0;
            }

            foreach (var component in ControlComponents.Select(next => next.Key))
            {
                if (component is IGestureComponent gestureComponent)
                {
                    gestureComponent.OnPositionChange(position, currentPosition, duration);
                }
            }

            _mSeekPosition = position;
        }

        protected virtual void SlideToChangeBrightness(float deltaY)
        {
            var activity = PlayerUtils.ScanForActivity(Context);
            if (activity == null)
            {
                return;
            }

            var window = activity.Window;
            var attributes = window.Attributes;
            var height = MeasuredHeight;
            if (_mBrightness == -1.0f)
            {
                _mBrightness = 0.5f;
            }

            var brightness = deltaY * 2 / height * 1.0f + _mBrightness;
            if (brightness < 0)
            {
                brightness = 0f;
            }

            if (brightness > 1.0f)
            {
                brightness = 1.0f;
            }

            var percent = (int)(brightness * 100);
            attributes.ScreenBrightness = brightness;
            window.Attributes = attributes;
            foreach (var component in ControlComponents.Select(next => next.Key))
            {
                if (component is IGestureComponent gestureComponent)
                {
                    gestureComponent.OnBrightnessChange(percent);
                }
            }
        }

        protected virtual void SlideToChangeVolume(float deltaY)
        {
            var streamMaxVolume = _mAudioManager.GetStreamMaxVolume(Stream.Music);
            var height = MeasuredHeight;
            var deltaV = deltaY * 2 / height * streamMaxVolume;
            var index = _mStreamVolume + deltaV;
            if (index > streamMaxVolume)
            {
                index = streamMaxVolume;
            }

            if (index < 0)
            {
                index = 0;
            }

            var percent = (int)(index / streamMaxVolume * 100);
            _mAudioManager.SetStreamVolume(Stream.Music, (int)index, 0);
            foreach (var component in ControlComponents.Select(next => next.Key))
            {
                if (component is IGestureComponent gestureComponent)
                {
                    gestureComponent.OnVolumeChange(percent);
                }
            }
        }

        public override bool OnTouchEvent(MotionEvent @event)
        {
            //滑动结束时事件处理
            if (_mGestureDetector.OnTouchEvent(@event))
                return base.OnTouchEvent(@event);

            var action = @event.Action;
            switch (action)
            {
                case MotionEventActions.Up:
                    {
                        StopSlide();

                        if (_mSeekPosition <= 0)
                            return base.OnTouchEvent(@event);

                        ControlWrapper.SeekTo(_mSeekPosition);
                        _mSeekPosition = 0;
                        break;
                    }
                case MotionEventActions.Cancel:
                    StopSlide();
                    _mSeekPosition = 0;
                    break;
            }

            return base.OnTouchEvent(@event);
        }

        private void StopSlide()
        {
            foreach (var component in ControlComponents.Select(next => next.Key))
            {
                if (component is IGestureComponent gestureComponent)
                {
                    gestureComponent.OnStopSlide();
                }
            }
        }

        public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            return false;
        }

        public void OnLongPress(MotionEvent e)
        {
        }

        public void OnShowPress(MotionEvent e)
        {
        }

        public bool OnDoubleTapEvent(MotionEvent e)
        {
            return false;
        }


        public bool OnSingleTapUp(MotionEvent e)
        {
            return false;
        }
    }
}