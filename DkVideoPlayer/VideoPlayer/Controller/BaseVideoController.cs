using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using DkVideoPlayer.VideoPlayer.Player;
using DkVideoPlayer.VideoPlayer.Util;
using Java.Lang;
using VideoView = DkVideoPlayer.VideoPlayer.Player.VideoView;

namespace DkVideoPlayer.VideoPlayer.Controller
{
    /// <summary>
    /// 控制器基类
    /// 此类集成各种事件的处理逻辑，包括
    /// 1.播放器状态改变: <seealso cref="#handlePlayerStateChanged(int)"/>
    /// 2.播放状态改变: <seealso cref="#handlePlayStateChanged(int)"/>
    /// 3.控制视图的显示和隐藏: <seealso cref="#handleVisibilityChanged(boolean, Animation)"/>
    /// 4.播放进度改变: <seealso cref="#handleSetProgress(int, int)"/>
    /// 5.锁定状态改变: <seealso cref="#handleLockStateChanged(boolean)"/>
    /// 6.设备方向监听: <seealso cref="#onOrientationChanged(int)"/>
    /// Created by dueeeke on 2017/4/12.
    /// </summary>
    public abstract class BaseVideoController : FrameLayout, IVideoController,
        OrientationHelper.IOnOrientationChangeListener
    {
        //播放器包装类，集合了MediaPlayerControl的api和IVideoController的api
        protected  ControlWrapper ControlWrapper;

        //JAVA TO C# CONVERTER CRACKED BY X-CRACKER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Nullable protected android.app.Activity mActivity;
        protected  Activity mActivity;

        //控制器是否处于显示状态
        protected  bool mShowing;

        //是否处于锁定状态
        protected  bool IsLocked;

        //播放视图隐藏超时
        protected  int DefaultTimeout = 4000;

        //是否开启根据屏幕方向进入/退出全屏
        private bool _mEnableOrientation;

        //屏幕方向监听辅助类
        protected  OrientationHelper OrientationHelper;

        //用户设置是否适配刘海屏
        private bool _mAdaptCutout;

        //是否有刘海
        private bool _mHasCutout;

        //刘海的高度
        private int _mCutoutHeight;

        //是否开始刷新进度
        private bool _mIsStartProgress;

        //保存了所有的控制组件
        protected readonly Dictionary<IControlComponent, bool> ControlComponents =
            new Dictionary<IControlComponent, bool>();

        private Animation _mShowAnim;
        private Animation _mHideAnim;
        protected  IRunnable mShowProgress;

        private Action _mShowProgressAction;


        public BaseVideoController(Context context) : this(context, null)
        {
        }

        public BaseVideoController(Context context, IAttributeSet attrs) : this(context, attrs, 0)
        {
        }

        public BaseVideoController(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs,
            defStyleAttr)
        {
            InitView();
        }

        protected virtual void InitView()
        {
            if (LayoutId != 0)
            {
                LayoutInflater.From(Context)?.Inflate(LayoutId, this, true);
            }

            OrientationHelper = new OrientationHelper(Context.ApplicationContext);
            _mEnableOrientation = VideoViewManager.Config.mEnableOrientation;
            _mAdaptCutout = VideoViewManager.Config.mAdaptCutout;

            _mShowAnim = new AlphaAnimation(0f, 1f) {Duration = 300};
            _mHideAnim = new AlphaAnimation(1f, 0f) {Duration = 300};

            mActivity = PlayerUtils.ScanForActivity(Context);
            _mShowProgressAction = new Action(() =>
            {
                int pos = SetProgress();
                if (ControlWrapper.Playing)
                {
                    PostDelayed(this._mShowProgressAction, (long) ((1000 - pos % 1000) / ControlWrapper.Speed));
                }
                else
                {
                    _mIsStartProgress = false;
                }
            });

            mShowProgress = new Runnable(_mShowProgressAction);
        }

        /// <summary>
        /// 设置控制器布局文件，子类必须实现
        /// </summary>
        protected abstract int LayoutId { get; }

        /// <summary>
        /// 重要：此方法用于将<seealso cref="VideoView"/> 和控制器绑定
        /// </summary>
        public virtual IMediaPlayerControl MediaPlayer
        {
            set
            {
                ControlWrapper = new ControlWrapper(value, this);
                //绑定ControlComponent和Controller
                foreach (var next in ControlComponents)
                {
                    var component = next.Key;
                    component.Attach(ControlWrapper);
                }

                //开始监听设备方向
                OrientationHelper.SetOnOrientationChangeListener(this);
            }
        }

        /// <summary>
        /// 添加控制组件，最后面添加的在最下面，合理组织添加顺序，可让ControlComponent位于不同的层级
        /// </summary>
        public virtual void AddControlComponent(params IControlComponent[] component)
        {
            foreach (var item in component)
            {
                AddControlComponent(item, false);
            }
        }

        /// <summary>
        /// 添加控制组件，最后面添加的在最下面，合理组织添加顺序，可让ControlComponent位于不同的层级
        /// </summary>
        /// <param name="component"></param>
        /// <param name="isPrivate"> 是否为独有的组件，如果是就不添加到控制器中 </param>
        public virtual void AddControlComponent(IControlComponent component, bool isPrivate)
        {
            ControlComponents.Add(component, isPrivate);
            if (ControlWrapper != null)
            {
                component.Attach(ControlWrapper);
            }

            var view = component.View;
            if (view != null && !isPrivate)
            {
                AddView(view, 0);
            }
        }

        /// <summary>
        /// 移除控制组件
        /// </summary>
        public virtual void RemoveControlComponent(IControlComponent component)
        {
            RemoveView(component.View);
            ControlComponents.Remove(component);
        }

        public virtual void RemoveAllControlComponent()
        {
            foreach (var next in ControlComponents)
            {
                RemoveView(next.Key.View);
            }

            ControlComponents.Clear();
        }

        public virtual void RemoveAllPrivateComponents()
        {
            var it = ControlComponents.GetEnumerator();
            while (it.MoveNext())
            {
                var next = it.Current;
                if (next.Value)
                {
                    it.Dispose();
                }
            }
        }

        /// <summary>
        /// <seealso cref="VideoView"/>调用此方法向控制器设置播放状态
        /// </summary>
        public virtual int PlayState
        {
            set => HandlePlayStateChanged(value);
        }

        /// <summary>
        /// <seealso cref="VideoView"/>调用此方法向控制器设置播放器状态
        /// </summary>
        public virtual int PlayerState
        {
            set => HandlePlayerStateChanged(value);
        }

        /// <summary>
        /// 设置播放视图自动隐藏超时
        /// </summary>
        public virtual int DismissTimeout
        {
            set
            {
                if (value > 0)
                {
                    DefaultTimeout = value;
                }
            }
        }

        /// <summary>
        /// 隐藏播放视图
        /// </summary>
        public virtual void Hide()
        {
            if (mShowing)
            {
                StopFadeOut();
                HandleVisibilityChanged(false, _mHideAnim);
                mShowing = false;
            }
        }

        /// <summary>
        /// 显示播放视图
        /// </summary>
        public virtual void Show()
        {
            if (!mShowing)
            {
                HandleVisibilityChanged(true, _mShowAnim);
                StartFadeOut();
                mShowing = true;
            }
        }

        public virtual bool Showing => mShowing;

        /// <summary>
        /// 开始计时
        /// </summary>
        public virtual void StartFadeOut()
        {
            //重新开始计时
            StopFadeOut();
            PostDelayed(Hide, DefaultTimeout);
        }

        /// <summary>
        /// 取消计时
        /// </summary>
        public virtual void StopFadeOut()
        {
            RemoveCallbacks(Hide);
        }


        public virtual bool Locked
        {
            set
            {
                IsLocked = value;
                HandleLockStateChanged(value);
            }
            get => IsLocked;
        }


        /// <summary>
        /// 开始刷新进度，注意：需在STATE_PLAYING时调用才会开始刷新进度
        /// </summary>
        public virtual void StartProgress()
        {
            if (_mIsStartProgress)
            {
                return;
            }

            Post(mShowProgress);
            _mIsStartProgress = true;
        }


        /// <summary>
        /// 停止刷新进度
        /// </summary>
        public virtual void StopProgress()
        {
            if (!_mIsStartProgress)
            {
                return;
            }

            RemoveCallbacks(mShowProgress);
            _mIsStartProgress = false;
        }


        private int SetProgress()
        {
            var position = (int) ControlWrapper.CurrentPosition;
            var duration = (int) ControlWrapper.Duration;
            HandleSetProgress(duration, position);
            return position;
        }

        /// <summary>
        /// 设置是否适配刘海屏
        /// </summary>
        public virtual bool AdaptCutout
        {
            set => _mAdaptCutout = value;
        }

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            CheckCutout();
        }

        /// <summary>
        /// 检查是否需要适配刘海
        /// </summary>
        private void CheckCutout()
        {
            if (!_mAdaptCutout) return;
            if (mActivity != null && _mHasCutout == true)
            {
                _mHasCutout = CutoutUtil.AllowDisplayToCutout(mActivity);
                if (_mHasCutout)
                {
                    //竖屏下的状态栏高度可认为是刘海的高度
                    _mCutoutHeight = (int) PlayerUtils.GetStatusBarHeightPortrait(mActivity);
                }
            }

            L.D("hasCutout: " + _mHasCutout + " cutout height: " + _mCutoutHeight);
        }

        /// <summary>
        /// 是否有刘海屏
        /// </summary>
        public virtual bool HasCutout()
        {
            return _mHasCutout;
        }

        /// <summary>
        /// 刘海的高度
        /// </summary>
        public virtual int CutoutHeight => _mCutoutHeight;

        /// <summary>
        /// 显示移动网络播放提示
        /// </summary>
        /// <returns> 返回显示移动网络播放提示的条件，false:不显示, true显示
        /// 此处默认根据手机网络类型来决定是否显示，开发者可以重写相关逻辑 </returns>
        public virtual bool ShowNetWarning()
        {
            return PlayerUtils.GetNetworkType(Context) == PlayerUtils.NetworkMobile &&
                   !VideoViewManager.Instance().playOnMobileNetwork();
        }

        /// <summary>
        /// 播放和暂停
        /// </summary>
        protected virtual void TogglePlay()
        {
            ControlWrapper.TogglePlay();
        }

        /// <summary>
        /// 横竖屏切换
        /// </summary>
        protected internal virtual void ToggleFullScreen()
        {
            ControlWrapper.ToggleFullScreen(mActivity);
        }

        /// <summary>
        /// 子类中请使用此方法来进入全屏
        /// </summary>
        /// <returns> 是否成功进入全屏 </returns>
        protected internal virtual bool StartFullScreen()
        {
            if (mActivity == null || mActivity.IsFinishing)
            {
                return false;
            }

            mActivity.RequestedOrientation = ScreenOrientation.Landscape;
            ControlWrapper.StartFullScreen();
            return true;
        }

        /// <summary>
        /// 子类中请使用此方法来退出全屏
        /// </summary>
        /// <returns> 是否成功退出全屏 </returns>
        protected internal virtual bool StopFullScreen()
        {
            if (mActivity == null || mActivity.IsFinishing)
            {
                return false;
            }

            mActivity.RequestedOrientation = ScreenOrientation.Portrait;
            ControlWrapper.StopFullScreen();
            return true;
        }

        /// <summary>
        /// 改变返回键逻辑，用于activity
        /// </summary>
        /// <returns></returns>
        public virtual bool OnBackPressed()
        {
            return false;
        }

        public override void OnWindowFocusChanged(bool hasWindowFocus)
        {
            base.OnWindowFocusChanged(hasWindowFocus);
            if (!ControlWrapper.Playing || !_mEnableOrientation && !ControlWrapper.FullScreen) 
                return;
            
            if (hasWindowFocus)
            {
                PostDelayed(() => { OrientationHelper.Enable(); }, 800);
            }
            else
            {
                OrientationHelper.Disable();
            }
        }

        /// <summary>
        /// 是否自动旋转， 默认不自动旋转
        /// </summary>
        public virtual bool EnableOrientation
        {
            set => _mEnableOrientation = value;
        }

        private int _mOrientation = 0;

        public virtual void OnOrientationChanged(int orientation)
        {
            if (mActivity == null || mActivity.IsFinishing)
            {
                return;
            }

            //记录用户手机上一次放置的位置
            var lastOrientation = _mOrientation;

            if (orientation == OrientationEventListener.OrientationUnknown)
            {
                //手机平放时，检测不到有效的角度
                //重置为原始位置 -1
                _mOrientation = -1;
                return;
            }

            if (orientation > 350 || orientation < 10)
            {
                var o = mActivity.RequestedOrientation;
                //手动切换横竖屏
                if (o == ScreenOrientation.Landscape && lastOrientation == 0)
                {
                    return;
                }

                if (_mOrientation == 0)
                {
                    return;
                }

                //0度，用户竖直拿着手机
                _mOrientation = 0;
                OnOrientationPortrait(mActivity);
            }
            else if (orientation > 80 && orientation < 100)
            {
                var o = mActivity.RequestedOrientation;
                //手动切换横竖屏
                if (o == ScreenOrientation.Portrait && lastOrientation == 90)
                {
                    return;
                }

                if (_mOrientation == 90)
                {
                    return;
                }

                //90度，用户右侧横屏拿着手机
                _mOrientation = 90;
                OnOrientationReverseLandscape(mActivity);
            }
            else if (orientation > 260 && orientation < 280)
            {
                var o = mActivity.RequestedOrientation;
                //手动切换横竖屏
                if (o == ScreenOrientation.Portrait && lastOrientation == 270)
                {
                    return;
                }

                if (_mOrientation == 270)
                {
                    return;
                }

                //270度，用户左侧横屏拿着手机
                _mOrientation = 270;
                OnOrientationLandscape(mActivity);
            }
        }

        /// <summary>
        /// 竖屏
        /// </summary>
        protected virtual void OnOrientationPortrait(Activity activity)
        {
            //屏幕锁定的情况
            if (IsLocked)
            {
                return;
            }

            //没有开启设备方向监听的情况
            if (!_mEnableOrientation)
            {
                return;
            }

            activity.RequestedOrientation = ScreenOrientation.Portrait;
            ControlWrapper.StopFullScreen();
        }

        /// <summary>
        /// 横屏
        /// </summary>
        protected virtual void OnOrientationLandscape(Activity activity)
        {
            activity.RequestedOrientation = ScreenOrientation.Landscape;
            if (ControlWrapper.FullScreen)
            {
                HandlePlayerStateChanged(VideoView.PLAYER_FULL_SCREEN);
            }
            else
            {
                ControlWrapper.StartFullScreen();
            }
        }

        /// <summary>
        /// 反向横屏
        /// </summary>
        protected virtual void OnOrientationReverseLandscape(Activity activity)
        {
            activity.RequestedOrientation = ScreenOrientation.Landscape;
            if (ControlWrapper.FullScreen)
            {
                HandlePlayerStateChanged(VideoView.PLAYER_FULL_SCREEN);
            }
            else
            {
                ControlWrapper.StartFullScreen();
            }
        }

        //------------------------ start handle event change ------------------------//

        private void HandleVisibilityChanged(bool isVisible, Animation anim)
        {
            if (!IsLocked)
            {
                //没锁住时才向ControlComponent下发此事件
                foreach (var component in ControlComponents.Select(next => next.Key))
                {
                    component.OnVisibilityChanged(isVisible, anim);
                }
            }

            OnVisibilityChanged(isVisible, anim);
        }

        /// <summary>
        /// 子类重写此方法监听控制的显示和隐藏
        /// </summary>
        /// <param name="isVisible"> 是否可见 </param>
        /// <param name="anim">      显示/隐藏动画 </param>
        protected virtual void OnVisibilityChanged(bool isVisible, Animation anim)
        {
        }

        private void HandlePlayStateChanged(int playState)
        {
            foreach (var component in ControlComponents.Select(next => next.Key))
            {
                component.OnPlayStateChanged(playState);
            }

            OnPlayStateChanged(playState);
        }

        /// <summary>
        /// 子类重写此方法并在其中更新控制器在不同播放状态下的ui
        /// </summary>
        protected virtual void OnPlayStateChanged(int playState)
        {
            switch (playState)
            {
                case VideoView.STATE_IDLE:
                    OrientationHelper.Disable();
                    _mOrientation = 0;
                    IsLocked = false;
                    mShowing = false;
                    RemoveAllPrivateComponents();
                    break;
                case VideoView.STATE_PLAYBACK_COMPLETED:
                    IsLocked = false;
                    mShowing = false;
                    break;
                case VideoView.STATE_ERROR:
                    mShowing = false;
                    break;
            }
        }

        private void HandlePlayerStateChanged(int playerState)
        {
            foreach (var component in ControlComponents.Select(next => next.Key))
            {
                component.OnPlayerStateChanged(playerState);
            }

            OnPlayerStateChanged(playerState);
        }

        /// <summary>
        /// 子类重写此方法并在其中更新控制器在不同播放器状态下的ui
        /// </summary>
        protected virtual void OnPlayerStateChanged(int playerState)
        {
            switch (playerState)
            {
                case VideoView.PLAYER_NORMAL:
                    if (_mEnableOrientation)
                    {
                        OrientationHelper.Enable();
                    }
                    else
                    {
                        OrientationHelper.Disable();
                    }

                    if (HasCutout())
                    {
                        CutoutUtil.AdaptCutoutAboveAndroidP(Context, false);
                    }

                    break;
                case VideoView.PLAYER_FULL_SCREEN:
                    //在全屏时强制监听设备方向
                    OrientationHelper.Enable();
                    if (HasCutout())
                    {
                        CutoutUtil.AdaptCutoutAboveAndroidP(Context, true);
                    }

                    break;
                case VideoView.PLAYER_TINY_SCREEN:
                    OrientationHelper.Disable();
                    break;
            }
        }

        private void HandleSetProgress(int duration, int position)
        {
            foreach (var component in ControlComponents.Select(next => next.Key))
            {
                component.SetProgress(duration, position);
            }

            SetProgress(duration, position);
        }

        /// <summary>
        /// 刷新进度回调，子类可在此方法监听进度刷新，然后更新ui
        /// </summary>
        /// <param name="duration"> 视频总时长 </param>
        /// <param name="position"> 视频当前时长 </param>
        protected virtual void SetProgress(int duration, int position)
        {
        }

        private void HandleLockStateChanged(bool isLocked)
        {
            foreach (var component in ControlComponents.Select(next => next.Key))
            {
                component.OnLockStateChanged(isLocked);
            }

            OnLockStateChanged(isLocked);
        }

        /// <summary>
        /// 子类可重写此方法监听锁定状态发生改变，然后更新ui
        /// </summary>
        protected virtual void OnLockStateChanged(bool isLocked)
        {
        }

        //------------------------ end handle event change ------------------------//
    }
}