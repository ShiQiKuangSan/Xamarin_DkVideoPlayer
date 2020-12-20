namespace DkVideoPlayer.VideoPlayer.Player
{
	/// <summary>
	/// 播放进度管理器，继承此接口实现自己的进度管理器。
	/// </summary>
	public abstract class ProgressManager
	{

		/// <summary>
		/// 此方法用于实现保存进度的逻辑 </summary>
		/// <param name="url"> 播放地址 </param>
		/// <param name="progress"> 播放进度 </param>
		public abstract void SaveProgress(string url, long progress);

		/// <summary>
		/// 此方法用于实现获取保存的进度的逻辑 </summary>
		/// <param name="url"> 播放地址 </param>
		/// <returns> 保存的播放进度 </returns>
		public abstract long GetSavedProgress(string url);

	}

}