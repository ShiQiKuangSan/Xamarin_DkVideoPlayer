using System;
using System.Collections.Generic;
using System.IO;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;

using DkVideoPlayer.VideoPlayer.Controller;
using DkVideoPlayer.VideoPlayer.Render;
using DkVideoPlayer.VideoPlayer.Util;

using Uri = Android.Net.Uri;

namespace DkVideoPlayer.VideoPlayer.Player
{
    /// <summary>
    /// 播放器
    /// Created by dueeeke on 2017/4/7.
    /// </summary>
    public class VideoView : FrameLayout, IMediaPlayerControl, AbstractPlayer.IPlayerEventListener
    {
        protected AbstractPlayer MediaPlayer; //播放器

        private PlayerFactory _playerFactory; //工厂类，用于实例化播放核心

        protected BaseVideoController mVideoController; //控制器

        /// <summary>
        /// 真正承载播放器视图的容器
        /// </summary>
        protected FrameLayout PlayerContainer;

        protected IRenderView RenderView;
        protected RenderViewFactory mRenderViewFactory;

        public const int SCREEN_SCALE_DEFAULT = 0;
        public const int SCREEN_SCALE_16_9 = 1;
        public const int SCREEN_SCALE_4_3 = 2;
        public const int SCREEN_SCALE_MATCH_PARENT = 3;
        public const int SCREEN_SCALE_ORIGINAL = 4;
        public const int SCREEN_SCALE_CENTER_CROP = 5;
        protected internal int mCurrentScreenScaleType;

        protected internal int[] mVideoSize = new int[] { 0, 0 };

        protected internal bool mIsMute; //是否静音

        //--------- data sources ---------//
        protected internal string mUrl; //当前播放视频的地址
        protected internal IDictionary<string, string> mHeaders; //当前视频地址的请求头
        protected internal AssetFileDescriptor mAssetFileDescriptor; //assets文件

        protected internal long mCurrentPosition; //当前正在播放视频的位置

        //播放器的各种状态
        public const int STATE_ERROR = -1;
        public const int STATE_IDLE = 0;
        public const int STATE_PREPARING = 1;
        public const int STATE_PREPARED = 2;
        public const int STATE_PLAYING = 3;
        public const int STATE_PAUSED = 4;
        public const int STATE_PLAYBACK_COMPLETED = 5;
        public const int STATE_BUFFERING = 6;
        public const int STATE_BUFFERED = 7;
        public const int STATE_START_ABORT = 8; //开始播放中止
        protected internal int mCurrentPlayState = STATE_IDLE; //当前播放器的状态

        public const int PLAYER_NORMAL = 10; // 普通播放器
        public const int PLAYER_FULL_SCREEN = 11; // 全屏播放器
        public const int PLAYER_TINY_SCREEN = 12; // 小屏播放器
        protected int mCurrentPlayerState = PLAYER_NORMAL;

        protected bool mIsFullScreen; //是否处于全屏状态

        protected bool mIsTinyScreen; //是否处于小屏状态
        protected int[] mTinyScreenSize = new int[] { 0, 0 };

        /// <summary>
        /// 监听系统中音频焦点改变，见<seealso cref="#setEnableAudioFocus(boolean)"/>
        /// </summary>
        protected bool mEnableAudioFocus;

        protected AudioFocusHelper mAudioFocusHelper;

        /// <summary>
        /// OnStateChangeListener集合，保存了所有开发者设置的监听器
        /// </summary>
        protected IList<IOnStateChangeListener> mOnStateChangeListeners;

        /// <summary>
        /// 进度管理器，设置之后播放器会记录播放进度，以便下次播放恢复进度
        /// </summary>
        protected ProgressManager mProgressManager;

        /// <summary>
        /// 循环播放
        /// </summary>
        protected bool mIsLooping;

        /// <summary>
        /// <seealso cref="#mPlayerContainer"/>背景色，默认黑色
        /// </summary>
        private Color mPlayerBackgroundColor;

        public VideoView(Context context) : this(context, null)
        {
        }

        public VideoView(Context context, IAttributeSet attrs) : this(context, attrs, 0)
        {
        }

        public VideoView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            //读取全局配置
            VideoViewConfig config = VideoViewManager.Config;
            mEnableAudioFocus = config.mEnableAudioFocus;
            mProgressManager = config.mProgressManager;
            _playerFactory = config.mPlayerFactory;
            mCurrentScreenScaleType = config.mScreenScaleType;
            mRenderViewFactory = config.mRenderViewFactory;

            //读取xml中的配置，并综合全局配置
            TypedArray a = context.ObtainStyledAttributes(attrs, Resource.Styleable.VideoView);
            mEnableAudioFocus = a.GetBoolean(Resource.Styleable.VideoView_enableAudioFocus, mEnableAudioFocus);
            mIsLooping = a.GetBoolean(Resource.Styleable.VideoView_looping, false);
            mCurrentScreenScaleType = a.GetInt(Resource.Styleable.VideoView_screenScaleType, mCurrentScreenScaleType);
            mPlayerBackgroundColor = a.GetColor(Resource.Styleable.VideoView_playerBackgroundColor, Color.Black);
            a.Recycle();

            // ReSharper disable once VirtualMemberCallInConstructor
            InitView();
        }

        /// <summary>
        /// 初始化播放器视图
        /// </summary>
        protected virtual void InitView()
        {
            PlayerContainer = new FrameLayout(Context);
            PlayerContainer.SetBackgroundColor(mPlayerBackgroundColor);
            var @params = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            this.AddView(PlayerContainer, @params);
        }

        /// <summary>
        /// 设置<seealso cref="#mPlayerContainer"/>的背景色
        /// </summary>
        public virtual Color PlayerBackgroundColor
        {
            set => PlayerContainer.SetBackgroundColor(value);
        }

        /// <summary>
        /// 开始播放，注意：调用此方法后必须调用<seealso cref="#release()"/>释放播放器，否则会导致内存泄漏
        /// </summary>
        public virtual void Start()
        {
            bool isStarted = false;
            if (InIdleState || InStartAbortState)
            {
                isStarted = StartPlay();
            }
            else if (InPlaybackState)
            {
                StartInPlaybackState();
                isStarted = true;
            }
            else
            {
                StartPrepare(true);
            }

            if (isStarted)
            {
                PlayerContainer.KeepScreenOn = true;
                mAudioFocusHelper?.RequestFocus();
            }
        }

        /// <summary>
        /// 第一次播放 </summary>
        /// <returns> 是否成功开始播放 </returns>
        protected virtual bool StartPlay()
        {
            //如果要显示移动网络提示则不继续播放
            if (ShowNetWarning())
            {
                //中止播放
                PlayState = STATE_START_ABORT;
                return false;
            }

            try
            {
                //监听音频焦点改变
                if (mEnableAudioFocus)
                {
                    mAudioFocusHelper = new AudioFocusHelper(this);
                }

                //读取播放进度
                if (mProgressManager != null)
                {
                    mCurrentPosition = mProgressManager.GetSavedProgress(mUrl);
                }

                InitPlayer();
                AddDisplay();
                StartPrepare(false);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 是否显示移动网络提示，可在Controller中配置
        /// </summary>
        protected virtual bool ShowNetWarning()
        {
            //播放本地数据源时不检测网络
            if (LocalDataSource)
            {
                return false;
            }

            return mVideoController != null && mVideoController.ShowNetWarning();
        }

        /// <summary>
        /// 判断是否为本地数据源，包括 本地文件、Asset、raw
        /// </summary>
        protected virtual bool LocalDataSource
        {
            get
            {
                if (mAssetFileDescriptor != null)
                {
                    return true;
                }
                else if (!TextUtils.IsEmpty(mUrl))
                {
                    var uri = Uri.Parse(mUrl);
                    return ContentResolver.SchemeAndroidResource.Equals(uri.Scheme) ||
                           ContentResolver.SchemeFile.Equals(uri.Scheme) || "rawresource".Equals(uri.Scheme);
                }

                return false;
            }
        }

        /// <summary>
        /// 初始化播放器
        /// </summary>
        protected virtual void InitPlayer()
        {
            MediaPlayer = _playerFactory.CreatePlayer(Context);
            MediaPlayer.SetPlayerEventListener(this);
            SetInitOptions();
            MediaPlayer.InitPlayer();
            SetOptions();
        }

        /// <summary>
        /// 初始化之前的配置项
        /// </summary>
        protected virtual void SetInitOptions()
        {
        }

        /// <summary>
        /// 初始化之后的配置项
        /// </summary>
        protected virtual void SetOptions()
        {
            MediaPlayer.Looping = mIsLooping;
        }

        /// <summary>
        /// 初始化视频渲染View
        /// </summary>
        protected virtual void AddDisplay()
        {
            if (RenderView != null)
            {
                PlayerContainer.RemoveView(RenderView.View);
                RenderView.Release();
            }

            RenderView = mRenderViewFactory.CreateRenderView(Context);
            RenderView.AttachToPlayer(MediaPlayer);
            var @params = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent,
                GravityFlags.Center);
            PlayerContainer.AddView(RenderView.View, 0, @params);
        }

        /// <summary>
        /// 开始准备播放（直接播放）
        /// </summary>
        protected virtual void StartPrepare(bool reset)
        {
            if (reset)
            {
                MediaPlayer.Reset();
                //重新设置option，media player reset之后，option会失效
                SetOptions();
            }

            if (PrepareDataSource())
            {
                MediaPlayer.PrepareAsync();
                PlayState = STATE_PREPARING;
                PlayerState = FullScreen ? PLAYER_FULL_SCREEN : TinyScreen ? PLAYER_TINY_SCREEN : PLAYER_NORMAL;
            }
        }

        /// <summary>
        /// 设置播放数据 </summary>
        /// <returns> 播放数据是否设置成功 </returns>
        protected virtual bool PrepareDataSource()
        {
            if (mAssetFileDescriptor != null)
            {
                MediaPlayer.DataSource = mAssetFileDescriptor;
                return true;
            }
            else if (!TextUtils.IsEmpty(mUrl))
            {
                MediaPlayer.SetDataSource(mUrl, mHeaders);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 播放状态下开始播放
        /// </summary>
        protected virtual void StartInPlaybackState()
        {
            MediaPlayer.Start();
            PlayState = STATE_PLAYING;
        }

        /// <summary>
        /// 暂停播放
        /// </summary>
        public virtual void Pause()
        {
            if (InPlaybackState && MediaPlayer.Playing)
            {
                MediaPlayer.Pause();
                PlayState = STATE_PAUSED;
                if (mAudioFocusHelper != null)
                {
                    mAudioFocusHelper.AbandonFocus();
                }

                PlayerContainer.KeepScreenOn = false;
            }
        }

        /// <summary>
        /// 继续播放
        /// </summary>
        public virtual void Resume()
        {
            if (InPlaybackState && !MediaPlayer.Playing)
            {
                MediaPlayer.Start();
                PlayState = STATE_PLAYING;
                if (mAudioFocusHelper != null)
                {
                    mAudioFocusHelper.RequestFocus();
                }

                PlayerContainer.KeepScreenOn = true;
            }
        }

        /// <summary>
        /// 释放播放器
        /// </summary>
        public virtual void Release()
        {
            if (!InIdleState)
            {
                //释放播放器
                if (MediaPlayer != null)
                {
                    MediaPlayer.Release();
                    MediaPlayer = null;
                }

                //释放renderView
                if (RenderView != null)
                {
                    PlayerContainer.RemoveView(RenderView.View);
                    RenderView.Release();
                    RenderView = null;
                }

                //释放Assets资源
                if (mAssetFileDescriptor != null)
                {
                    try
                    {
                        mAssetFileDescriptor.Close();
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace);
                    }
                }

                //关闭AudioFocus监听
                if (mAudioFocusHelper != null)
                {
                    mAudioFocusHelper.AbandonFocus();
                    mAudioFocusHelper = null;
                }

                //关闭屏幕常亮
                PlayerContainer.KeepScreenOn = false;
                //保存播放进度
                SaveProgress();
                //重置播放进度
                mCurrentPosition = 0;
                //切换转态
                PlayState = STATE_IDLE;
            }
        }

        /// <summary>
        /// 保存播放进度
        /// </summary>
        protected virtual void SaveProgress()
        {
            if (mProgressManager != null && mCurrentPosition > 0)
            {
                L.D("saveProgress: " + mCurrentPosition);
                mProgressManager.SaveProgress(mUrl, mCurrentPosition);
            }
        }

        /// <summary>
        /// 是否处于播放状态
        /// </summary>
        protected virtual bool InPlaybackState =>
            MediaPlayer != null && mCurrentPlayState != STATE_ERROR &&
            mCurrentPlayState != STATE_IDLE && mCurrentPlayState != STATE_PREPARING &&
            mCurrentPlayState != STATE_START_ABORT && mCurrentPlayState != STATE_PLAYBACK_COMPLETED;

        /// <summary>
        /// 是否处于未播放状态
        /// </summary>
        protected virtual bool InIdleState => mCurrentPlayState == STATE_IDLE;

        /// <summary>
        /// 播放中止状态
        /// </summary>
        private bool InStartAbortState => mCurrentPlayState == STATE_START_ABORT;

        /// <summary>
        /// 重新播放
        /// </summary>
        /// <param name="resetPosition"> 是否从头开始播放 </param>
        public virtual void Replay(bool resetPosition)
        {
            if (resetPosition)
            {
                mCurrentPosition = 0;
            }

            AddDisplay();
            StartPrepare(true);
            PlayerContainer.KeepScreenOn = true;
        }

        /// <summary>
        /// 获取视频总时长
        /// </summary>
        public virtual long Duration
        {
            get
            {
                if (InPlaybackState)
                {
                    return MediaPlayer.Duration;
                }

                return 0;
            }
        }

        /// <summary>
        /// 获取当前播放的位置
        /// </summary>
        public virtual long CurrentPosition
        {
            get
            {
                if (InPlaybackState)
                {
                    mCurrentPosition = MediaPlayer.CurrentPosition;
                    return mCurrentPosition;
                }

                return 0;
            }
        }

        /// <summary>
        /// 调整播放进度
        /// </summary>
        public virtual void SeekTo(long pos)
        {
            if (InPlaybackState)
            {
                MediaPlayer.SeekTo(pos);
            }
        }

        /// <summary>
        /// 是否处于播放状态
        /// </summary>
        public virtual bool Playing => InPlaybackState && MediaPlayer.Playing;

        /// <summary>
        /// 获取当前缓冲百分比
        /// </summary>
        public virtual int BufferedPercentage => MediaPlayer?.BufferedPercentage ?? 0;

        /// <summary>
        /// 设置静音
        /// </summary>
        public virtual bool Mute
        {
            set
            {
                if (MediaPlayer != null)
                {
                    mIsMute = value;
                    var volume = value ? 0.0f : 1.0f;
                    MediaPlayer.SetVolume(volume, volume);
                }
            }
            get => mIsMute;
        }


        /// <summary>
        /// 视频播放出错回调
        /// </summary>
        public virtual void OnError()
        {
            PlayerContainer.KeepScreenOn = false;
            PlayState = STATE_ERROR;
        }

        /// <summary>
        /// 视频播放完成回调
        /// </summary>
        public virtual void OnCompletion()
        {
            PlayerContainer.KeepScreenOn = false;
            mCurrentPosition = 0;
            //播放完成，清除进度
            mProgressManager?.SaveProgress(mUrl, 0);

            PlayState = STATE_PLAYBACK_COMPLETED;
        }

        public virtual void OnInfo(int what, int extra)
        {
            switch (what)
            {
                case AbstractPlayer.MediaInfoBufferingStart:
                    PlayState = STATE_BUFFERING;
                    break;
                case AbstractPlayer.MediaInfoBufferingEnd:
                    PlayState = STATE_BUFFERED;
                    break;
                case AbstractPlayer.MediaInfoVideoRenderingStart: // 视频开始渲染
                    PlayState = STATE_PLAYING;
                    if (PlayerContainer.WindowVisibility != ViewStates.Visible)
                    {
                        Pause();
                    }

                    break;
                case AbstractPlayer.MediaInfoVideoRotationChanged:
                    if (RenderView != null)
                    {
                        RenderView.VideoRotation = extra;
                    }

                    break;
            }
        }

        /// <summary>
        /// 视频缓冲完毕，准备开始播放时回调
        /// </summary>
        public virtual void OnPrepared()
        {
            PlayState = STATE_PREPARED;
            if (mCurrentPosition > 0)
            {
                SeekTo(mCurrentPosition);
            }
        }

        /// <summary>
        /// 获取当前播放器的状态
        /// </summary>
        public virtual int CurrentPlayerState => mCurrentPlayerState;

        /// <summary>
        /// 获取当前的播放状态
        /// </summary>
        public virtual int CurrentPlayState => mCurrentPlayState;

        /// <summary>
        /// 获取缓冲速度
        /// </summary>
        public virtual long TcpSpeed => MediaPlayer?.TcpSpeed ?? 0;

        /// <summary>
        /// 设置播放速度
        /// </summary>
        public virtual float Speed
        {
            set
            {
                if (InPlaybackState)
                {
                    MediaPlayer.Speed = value;
                }
            }
            get
            {
                if (InPlaybackState)
                {
                    return MediaPlayer.Speed;
                }

                return 1f;
            }
        }


        /// <summary>
        /// 设置视频地址
        /// </summary>
        public virtual string Url
        {
            set => SetUrl(value, null);
        }

        /// <summary>
        /// 设置包含请求头信息的视频地址
        /// </summary>
        /// <param name="url">     视频地址 </param>
        /// <param name="headers"> 请求头 </param>
        public virtual void SetUrl(string url, IDictionary<string, string> headers)
        {
            mAssetFileDescriptor = null;
            mUrl = url;
            mHeaders = headers;
        }

        /// <summary>
        /// 用于播放assets里面的视频文件
        /// </summary>
        public virtual AssetFileDescriptor AssetFileDescriptor
        {
            set
            {
                mUrl = null;
                this.mAssetFileDescriptor = value;
            }
        }

        /// <summary>
        /// 一开始播放就seek到预先设置好的位置
        /// </summary>
        public virtual void SkipPositionWhenPlay(int position)
        {
            this.mCurrentPosition = position;
        }

        /// <summary>
        /// 设置音量 0.0f-1.0f 之间
        /// </summary>
        /// <param name="v1"> 左声道音量 </param>
        /// <param name="v2"> 右声道音量 </param>
        public virtual void SetVolume(float v1, float v2)
        {
            MediaPlayer?.SetVolume(v1, v2);
        }

        /// <summary>
        /// 设置进度管理器，用于保存播放进度
        /// </summary>
        public virtual ProgressManager ProgressManager
        {
            set => this.mProgressManager = value;
        }

        /// <summary>
        /// 循环播放， 默认不循环播放
        /// </summary>
        public virtual bool Looping
        {
            set
            {
                mIsLooping = value;
                if (MediaPlayer != null)
                {
                    MediaPlayer.Looping = value;
                }
            }
        }

        /// <summary>
        /// 是否开启AudioFocus监听， 默认开启，用于监听其它地方是否获取音频焦点，如果有其它地方获取了
        /// 音频焦点，此播放器将做出相应反应，具体实现见<seealso cref="AudioFocusHelper"/>
        /// </summary>
        public virtual bool EnableAudioFocus
        {
            set => mEnableAudioFocus = value;
        }

        /// <summary>
        /// 自定义播放核心，继承<seealso cref="PlayerFactory"/>实现自己的播放核心
        /// </summary>
        public virtual PlayerFactory PlayerFactory
        {
            set => _playerFactory = value ?? throw new System.ArgumentException("PlayerFactory can not be null!");
        }

        /// <summary>
        /// 自定义RenderView，继承<seealso cref="RenderViewFactory"/>实现自己的RenderView
        /// </summary>
        public virtual RenderViewFactory RenderViewFactory
        {
            set => mRenderViewFactory =
                value ?? throw new System.ArgumentException("RenderViewFactory can not be null!");
        }

        /// <summary>
        /// 进入全屏
        /// </summary>
        public virtual void StartFullScreen()
        {
            if (mIsFullScreen)
            {
                return;
            }

            var decorView = DecorView;
            if (decorView == null)
            {
                return;
            }

            mIsFullScreen = true;

            try
            {
                //隐藏NavigationBar和StatusBar
                HideSysBar(decorView);

                //从当前FrameLayout中移除播放器视图
                RemoveView(PlayerContainer);
                //将播放器视图添加到DecorView中即实现了全屏
                decorView.AddView(PlayerContainer);

                PlayerState = PLAYER_FULL_SCREEN;
            }
            catch (Exception)
            {
                Release();
            }
        }

        private void HideSysBar(ViewGroup decorView)
        {
            var uiOptions = (SystemUiFlags)decorView.SystemUiVisibility;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBean)
            {
                uiOptions |= SystemUiFlags.HideNavigation;
            }

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                uiOptions |= SystemUiFlags.ImmersiveSticky;
            }

            decorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;
            Activity.Window?.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
        }

        public override void OnWindowFocusChanged(bool hasWindowFocus)
        {
            base.OnWindowFocusChanged(hasWindowFocus);
            if (hasWindowFocus && mIsFullScreen)
            {
                //重新获得焦点时保持全屏状态
                HideSysBar(DecorView);
            }
        }

        /// <summary>
        /// 退出全屏
        /// </summary>
        public virtual void StopFullScreen()
        {
            if (!mIsFullScreen)
            {
                return;
            }

            var decorView = DecorView;
            if (decorView == null)
            {
                return;
            }

            try
            {
                mIsFullScreen = false;

                //显示NavigationBar和StatusBar
                ShowSysBar(decorView);

                //把播放器视图从DecorView中移除并添加到当前FrameLayout中即退出了全屏
                decorView.RemoveView(PlayerContainer);
                this.AddView(PlayerContainer);

                PlayerState = PLAYER_NORMAL;
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void ShowSysBar(ViewGroup decorView)
        {
            var uiOptions = (SystemUiFlags)decorView.SystemUiVisibility;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBean)
            {
                uiOptions &= ~SystemUiFlags.HideNavigation;
            }

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                uiOptions &= ~SystemUiFlags.ImmersiveSticky;
            }

            decorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;
            Activity.Window?.ClearFlags(WindowManagerFlags.Fullscreen);
        }

        /// <summary>
        /// 获取DecorView
        /// </summary>
        protected virtual ViewGroup DecorView
        {
            get
            {
                var activity = Activity;

                return (ViewGroup)activity?.Window?.DecorView;
            }
        }

        /// <summary>
        /// 获取activity中的content view,其id为android.R.id.content
        /// </summary>
        protected virtual ViewGroup ContentView
        {
            get
            {
                var activity = Activity;

                return activity?.FindViewById<ViewGroup>(Android.Resource.Id.Content);
            }
        }

        /// <summary>
        /// 获取Activity，优先通过Controller去获取Activity
        /// </summary>
        protected virtual Activity Activity
        {
            get
            {
                Activity activity;
                if (mVideoController != null)
                {
                    activity = PlayerUtils.ScanForActivity(mVideoController.Context);
                    if (activity == null)
                    {
                        activity = PlayerUtils.ScanForActivity(Context);
                    }
                }
                else
                {
                    activity = PlayerUtils.ScanForActivity(Context);
                }

                return activity;
            }
        }

        /// <summary>
        /// 判断是否处于全屏状态
        /// </summary>
        public virtual bool FullScreen => mIsFullScreen;

        /// <summary>
        /// 开启小屏
        /// </summary>
        public virtual void StartTinyScreen()
        {
            if (mIsTinyScreen)
            {
                return;
            }

            var contentView = ContentView;
            if (contentView == null)
            {
                return;
            }

            this.RemoveView(PlayerContainer);
            var width = mTinyScreenSize[0];
            if (width <= 0)
            {
                width = PlayerUtils.GetScreenWidth(Context, false) / 2;
            }

            var height = mTinyScreenSize[1];
            if (height <= 0)
            {
                height = width * 9 / 16;
            }

            var @params = new LayoutParams(width, height)
            {
                Gravity = GravityFlags.Bottom | GravityFlags.End
            };
            contentView.AddView(PlayerContainer, @params);
            mIsTinyScreen = true;
            PlayerState = PLAYER_TINY_SCREEN;
        }

        /// <summary>
        /// 退出小屏
        /// </summary>
        public virtual void StopTinyScreen()
        {
            if (!mIsTinyScreen)
            {
                return;
            }

            var contentView = ContentView;
            if (contentView == null)
            {
                return;
            }

            contentView.RemoveView(PlayerContainer);
            var @params = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            AddView(PlayerContainer, @params);

            mIsTinyScreen = false;
            PlayerState = PLAYER_NORMAL;
        }

        public virtual bool TinyScreen => mIsTinyScreen;

        public virtual void OnVideoSizeChanged(int videoWidth, int videoHeight)
        {
            mVideoSize[0] = videoWidth;
            mVideoSize[1] = videoHeight;

            if (RenderView != null)
            {
                RenderView.ScaleType = mCurrentScreenScaleType;
                RenderView.SetVideoSize(videoWidth, videoHeight);
            }
        }

        /// <summary>
        /// 设置控制器，传null表示移除控制器
        /// </summary>
        public virtual BaseVideoController VideoController
        {
            set
            {
                PlayerContainer.RemoveView(mVideoController);
                mVideoController = value;
                if (value != null)
                {
                    value.MediaPlayer = this;
                    var @params = new LayoutParams(ViewGroup.LayoutParams.MatchParent,
                        ViewGroup.LayoutParams.MatchParent);
                    PlayerContainer.AddView(mVideoController, @params);
                }
            }
        }

        /// <summary>
        /// 设置视频比例
        /// </summary>
        public virtual int ScreenScaleType
        {
            set
            {
                mCurrentScreenScaleType = value;
                if (RenderView != null)
                {
                    RenderView.ScaleType = value;
                }
            }
        }

        /// <summary>
        /// 设置镜像旋转，暂不支持SurfaceView
        /// </summary>
        public virtual bool MirrorRotation
        {
            set
            {
                if (RenderView != null)
                {
                    RenderView.View.ScaleX = value ? -1 : 1;
                }
            }
        }

        /// <summary>
        /// 截图，暂不支持SurfaceView
        /// </summary>
        public virtual Bitmap DoScreenShot()
        {
            return RenderView?.DoScreenShot();
        }

        /// <summary>
        /// 获取视频宽高,其中width: mVideoSize[0], height: mVideoSize[1]
        /// </summary>
        public virtual int[] VideoSize => mVideoSize;

        /// <summary>
        /// 旋转视频画面
        /// </summary>
        /// <param name="rotation"> 角度 </param>
        public override float Rotation
        {
            set
            {
                if (RenderView != null)
                {
                    RenderView.VideoRotation = (int)value;
                }
            }
        }

        /// <summary>
        /// 设置小屏的宽高
        /// </summary>
        /// <param name="tinyScreenSize"> 其中tinyScreenSize[0]是宽，tinyScreenSize[1]是高 </param>
        public virtual int[] TinyScreenSize
        {
            set => this.mTinyScreenSize = value;
        }

        /// <summary>
        /// 向Controller设置播放状态，用于控制Controller的ui展示
        /// </summary>
        protected virtual int PlayState
        {
            set
            {
                mCurrentPlayState = value;
                if (mVideoController != null)
                {
                    mVideoController.PlayState = value;
                }

                if (mOnStateChangeListeners != null)
                {
                    foreach (var l in PlayerUtils.GetSnapshot(mOnStateChangeListeners))
                    {
                        l?.OnPlayStateChanged(value);
                    }
                }
            }
        }

        /// <summary>
        /// 向Controller设置播放器状态，包含全屏状态和非全屏状态
        /// </summary>
        protected virtual int PlayerState
        {
            set
            {
                mCurrentPlayerState = value;
                if (mVideoController != null)
                {
                    mVideoController.PlayerState = value;
                }

                if (mOnStateChangeListeners != null)
                {
                    foreach (var l in PlayerUtils.GetSnapshot(mOnStateChangeListeners))
                    {
                        l?.OnPlayerStateChanged(value);
                    }
                }
            }
        }

        /// <summary>
        /// 播放状态改变监听器
        /// </summary>
        public interface IOnStateChangeListener
        {
            void OnPlayerStateChanged(int playerState);
            void OnPlayStateChanged(int playState);
        }

        /// <summary>
        /// OnStateChangeListener的空实现。用的时候只需要重写需要的方法
        /// </summary>
        public class SimpleOnStateChangeListener : IOnStateChangeListener
        {
            public virtual void OnPlayerStateChanged(int playerState)
            {
            }

            public virtual void OnPlayStateChanged(int playState)
            {
            }
        }

        /// <summary>
        /// 添加一个播放状态监听器，播放状态发生变化时将会调用。
        /// </summary>
        public virtual void AddOnStateChangeListener(IOnStateChangeListener listener)
        {
            if (mOnStateChangeListeners == null)
            {
                mOnStateChangeListeners = new List<IOnStateChangeListener>();
            }

            mOnStateChangeListeners.Add(listener);
        }

        /// <summary>
        /// 移除某个播放状态监听
        /// </summary>
        public virtual void RemoveOnStateChangeListener(IOnStateChangeListener listener)
        {
            mOnStateChangeListeners?.Remove(listener);
        }

        /// <summary>
        /// 设置一个播放状态监听器，播放状态发生变化时将会调用，
        /// 如果你想同时设置多个监听器，推荐 <seealso cref="#addOnStateChangeListener(OnStateChangeListener)"/>。
        /// </summary>
        public virtual void SetOnStateChangeListener(IOnStateChangeListener listener)
        {
            if (mOnStateChangeListeners == null)
            {
                mOnStateChangeListeners = new List<IOnStateChangeListener>();
            }
            else
            {
                mOnStateChangeListeners.Clear();
            }

            mOnStateChangeListeners.Add(listener);
        }

        /// <summary>
        /// 移除所有播放状态监听
        /// </summary>
        public virtual void ClearOnStateChangeListeners()
        {
            mOnStateChangeListeners?.Clear();
        }

        /// <summary>
        /// 改变返回键逻辑，用于activity
        /// </summary>
        /// <returns></returns>
        public bool OnBackPressed()
        {
            return mVideoController != null && mVideoController.OnBackPressed();
        }

        protected override IParcelable OnSaveInstanceState()
        {
            L.D("onSaveInstanceState: " + mCurrentPosition);
            //activity切到后台后可能被系统回收，故在此处进行进度保存
            SaveProgress();
            return base.OnSaveInstanceState();
        }
    }
}