namespace DkVideoPlayer.VideoPlayer.Controller
{
	public interface IGestureComponent : IControlComponent
	{
		/// <summary>
		/// 开始滑动
		/// </summary>
		void OnStartSlide();

		/// <summary>
		/// 结束滑动
		/// </summary>
		void OnStopSlide();

		/// <summary>
		/// 滑动调整进度 </summary>
		/// <param name="slidePosition"> 滑动进度 </param>
		/// <param name="currentPosition"> 当前播放进度 </param>
		/// <param name="duration"> 视频总长度 </param>
		void OnPositionChange(int slidePosition, int currentPosition, int duration);

		/// <summary>
		/// 滑动调整亮度 </summary>
		/// <param name="percent"> 亮度百分比 </param>
		void OnBrightnessChange(int percent);

		/// <summary>
		/// 滑动调整音量 </summary>
		/// <param name="percent"> 音量百分比 </param>
		void OnVolumeChange(int percent);
	}

}