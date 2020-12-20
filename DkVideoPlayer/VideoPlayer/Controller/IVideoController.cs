namespace DkVideoPlayer.VideoPlayer.Controller
{
	public interface IVideoController
	{
		/// <summary>
		/// 开始控制视图自动隐藏倒计时
		/// </summary>
		void StartFadeOut();

		/// <summary>
		/// 取消控制视图自动隐藏倒计时
		/// </summary>
		void StopFadeOut();

		/// <summary>
		/// 控制视图是否处于显示状态
		/// </summary>
		bool Showing {get;}

		/// <summary>
		/// 设置锁定状态 </summary>
		/// <param name="locked"> 是否锁定 </param>
		bool Locked {set;get;}


		/// <summary>
		/// 开始刷新进度
		/// </summary>
		void StartProgress();

		/// <summary>
		/// 停止刷新进度
		/// </summary>
		void StopProgress();

		/// <summary>
		/// 显示控制视图
		/// </summary>
		void Hide();

		/// <summary>
		/// 隐藏控制视图
		/// </summary>
		void Show();

		/// <summary>
		/// 是否需要适配刘海
		/// </summary>
		bool HasCutout();

		/// <summary>
		/// 获取刘海的高度
		/// </summary>
		int CutoutHeight {get;}
	}

}