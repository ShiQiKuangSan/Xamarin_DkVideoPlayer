using System.Collections.Generic;
using Android.App; 
using DkVideoPlayer.VideoPlayer.Util;

namespace DkVideoPlayer.VideoPlayer.Player
{
 

	/// <summary>
	/// 视频播放器管理器，管理当前正在播放的VideoView，以及播放器配置
	/// 你也可以用来保存常驻内存的VideoView，但是要注意通过Application Context创建，
	/// 以免内存泄漏
	/// </summary>
	public class VideoViewManager
	{

		/// <summary>
		/// 保存VideoView的容器
		/// </summary>
		private Dictionary<string, VideoView> mVideoViews = new Dictionary<string, VideoView>();

		/// <summary>
		/// 是否在移动网络下直接播放视频
		/// </summary>
		private bool mPlayOnMobileNetwork;

		/// <summary>
		/// VideoViewManager实例
		/// </summary>
		private static VideoViewManager sInstance;

		/// <summary>
		/// VideoViewConfig实例
		/// </summary>
		private static VideoViewConfig sConfig;

		private VideoViewManager()
		{
			mPlayOnMobileNetwork = Config.mPlayOnMobileNetwork;
		}

		/// <summary>
		/// 设置VideoViewConfig
		/// </summary>
		public static VideoViewConfig Config
		{
			set
			{
				if (sConfig != null) 
					return;
				
				lock (typeof(VideoViewConfig))
				{
					if (sConfig == null)
					{
						sConfig = value ?? VideoViewConfig.NewBuilder().Build();
					}
				}
			}
			get
			{
				Config = null;
				return sConfig;
			}
		}


		/// <summary>
		/// 获取是否在移动网络下直接播放视频配置
		/// </summary>
		public virtual bool playOnMobileNetwork()
		{
			return mPlayOnMobileNetwork;
		}

		/// <summary>
		/// 设置是否在移动网络下直接播放视频
		/// </summary>
		public virtual bool PlayOnMobileNetwork
		{
			set => mPlayOnMobileNetwork = value;
		}

		public static VideoViewManager Instance()
		{
			if (sInstance != null) 
				return sInstance;
			
			lock (typeof(VideoViewManager))
			{
				if (sInstance == null)
				{
					sInstance = new VideoViewManager();
				}
			}
			return sInstance;
		}

		/// <summary>
		/// 添加VideoView </summary>
		/// <param name="videoView"></param>
		/// <param name="tag"> 相同tag的VideoView只会保存一个，如果tag相同则会release并移除前一个 </param>
		public virtual void Add(VideoView videoView, string tag)
		{
			if (!(videoView.Context is Application))
			{
				L.W("The Context of this VideoView is not an Application Context," + "you must remove it after release,or it will lead to memory leek.");
			}
			var old = Get(tag);
			if (old != null)
			{
				old.Release();
				Remove(tag);
			}
			mVideoViews.Add(tag, videoView);
		}

		public virtual VideoView Get(string tag)
		{
			return mVideoViews[tag];
		}

		public virtual void Remove(string tag)
		{
			mVideoViews.Remove(tag);
		}

		public virtual void RemoveAll()
		{
			mVideoViews.Clear();
		}

		/// <summary>
		/// 释放掉和tag关联的VideoView，并将其从VideoViewManager中移除
		/// </summary>
		public virtual void ReleaseByTag(string tag)
		{
			ReleaseByTag(tag, true);
		}

		public virtual void ReleaseByTag(string tag, bool isRemove)
		{
			var videoView = Get(tag);
			if (videoView != null)
			{
				videoView.Release();
				if (isRemove)
				{
					Remove(tag);
				}
			}
		}

    }

}