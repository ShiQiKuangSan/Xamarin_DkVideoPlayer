using DkVideoPlayer.VideoPlayer.Render;

namespace DkVideoPlayer.VideoPlayer.Player
{

 
	/// <summary>
	/// 播放器全局配置
	/// </summary>
	public class VideoViewConfig
	{

		public static Builder NewBuilder()
		{
			return new Builder();
		}

		public readonly bool mPlayOnMobileNetwork;

		public readonly bool mEnableOrientation;

		public readonly bool mEnableAudioFocus;

		public readonly bool mIsEnableLog;

		public readonly ProgressManager mProgressManager;

		public readonly PlayerFactory mPlayerFactory;

		public readonly int mScreenScaleType;

		public readonly RenderViewFactory mRenderViewFactory;

		public readonly bool mAdaptCutout;

		private VideoViewConfig(Builder builder)
		{
			mIsEnableLog = builder.IsEnableLog;
			mEnableOrientation = builder.EnableOrientation;
			mPlayOnMobileNetwork = builder.PlayOnMobileNetwork;
			mEnableAudioFocus = builder.EnableAudioFocus;
			mProgressManager = builder.ProgressManager;
			mScreenScaleType = builder.ScreenScaleType;
			if (builder.PlayerFactory == null)
			{
				//默认为AndroidMediaPlayer
				mPlayerFactory  = AndroidMediaPlayerFactory.Create() ;
			}
			else
			{
				mPlayerFactory = builder.PlayerFactory;
			}
			if (builder.RenderViewFactory == null)
			{
				//默认使用TextureView渲染视频
				mRenderViewFactory = TextureRenderViewFactory.Create();
			}
			else
			{
				mRenderViewFactory = builder.RenderViewFactory;
			}
			mAdaptCutout = builder.AdaptCutout;
		}


		public sealed class Builder
		{

			internal bool IsEnableLog;
			internal bool PlayOnMobileNetwork;
			internal bool EnableOrientation;
			internal bool EnableAudioFocus = true;
			internal ProgressManager ProgressManager;
			internal PlayerFactory  PlayerFactory;
			internal int ScreenScaleType;
			internal RenderViewFactory RenderViewFactory;
			internal bool AdaptCutout = true;

			/// <summary>
			/// 是否监听设备方向来切换全屏/半屏， 默认不开启
			/// </summary>
			public Builder SetEnableOrientation(bool enableOrientation)
			{
				EnableOrientation = enableOrientation;
				return this;
			}

			/// <summary>
			/// 在移动环境下调用start()后是否继续播放，默认不继续播放
			/// </summary>
			public Builder SetPlayOnMobileNetwork(bool playOnMobileNetwork)
			{
				PlayOnMobileNetwork = playOnMobileNetwork;
				return this;
			}

			/// <summary>
			/// 是否开启AudioFocus监听， 默认开启
			/// </summary>
			public Builder SetEnableAudioFocus(bool enableAudioFocus)
			{
				EnableAudioFocus = enableAudioFocus;
				return this;
			}

			/// <summary>
			/// 设置进度管理器，用于保存播放进度
			/// </summary>
			public Builder SetProgressManager(ProgressManager progressManager)
			{
				ProgressManager = progressManager;
				return this;
			}

			/// <summary>
			/// 是否打印日志
			/// </summary>
			public Builder SetLogEnabled(bool enableLog)
			{
				IsEnableLog = enableLog;
				return this;
			}

			/// <summary>
			/// 自定义播放核心
			/// </summary>
			public Builder SetPlayerFactory(PlayerFactory  playerFactory)
			{
				PlayerFactory = playerFactory;
				return this;
			}

			/// <summary>
			/// 设置视频比例
			/// </summary>
			public Builder SetScreenScaleType(int screenScaleType)
			{
				ScreenScaleType = screenScaleType;
				return this;
			}

			/// <summary>
			/// 自定义RenderView
			/// </summary>
			public Builder SetRenderViewFactory(RenderViewFactory renderViewFactory)
			{
				RenderViewFactory = renderViewFactory;
				return this;
			}

			/// <summary>
			/// 是否适配刘海屏，默认适配
			/// </summary>
			public Builder SetAdaptCutout(bool adaptCutout)
			{
				AdaptCutout = adaptCutout;
				return this;
			}

			public VideoViewConfig Build()
			{
				return new VideoViewConfig(this);
			}
		}
	}

}