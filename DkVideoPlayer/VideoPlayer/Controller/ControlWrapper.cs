using Android.App;
using Android.Content.PM;
using Android.Graphics;

namespace DkVideoPlayer.VideoPlayer.Controller
{
	/// <summary>
	/// 此类的目的是为了在ControlComponent中既能调用VideoView的api又能调用BaseVideoController的api，
	/// 并对部分api做了封装，方便使用
	/// </summary>
	public class ControlWrapper : IMediaPlayerControl, IVideoController
	{

		private readonly IMediaPlayerControl _playerControl;
		private readonly IVideoController _controller;

		public ControlWrapper(IMediaPlayerControl playerControl, IVideoController controller)
		{
			_playerControl = playerControl;
			_controller = controller;
		}

		public virtual void Start()
		{
			_playerControl.Start();
		}

		public virtual void Pause()
		{
			_playerControl.Pause();
		}

		public virtual long Duration => _playerControl.Duration;

        public virtual long CurrentPosition => _playerControl.CurrentPosition;

        public virtual void SeekTo(long pos)
		{
			_playerControl.SeekTo(pos);
		}

		public virtual bool Playing => _playerControl.Playing;

        public virtual int BufferedPercentage => _playerControl.BufferedPercentage;

        public virtual void StartFullScreen()
		{
			_playerControl.StartFullScreen();
		}

		public virtual void StopFullScreen()
		{
			_playerControl.StopFullScreen();
		}

		public virtual bool FullScreen => _playerControl.FullScreen;

        public virtual bool Mute
		{
			set => _playerControl.Mute = value;
            get => _playerControl.Mute;
        }


		public virtual int ScreenScaleType
		{
			set => _playerControl.ScreenScaleType = value;
        }

		public virtual float Speed
		{
			set => _playerControl.Speed = value;
            get => _playerControl.Speed;
        }


		public virtual void Replay(bool resetPosition)
		{
			_playerControl.Replay(resetPosition);
		}

		public virtual bool MirrorRotation
		{
			set => _playerControl.MirrorRotation = value;
        }

		public virtual Bitmap DoScreenShot()
		{
			return _playerControl.DoScreenShot();
		}

		public virtual int[] VideoSize => _playerControl.VideoSize;

        public virtual float Rotation
		{
			set => _playerControl.Rotation = value;
        }

		public virtual void StartTinyScreen()
		{
			_playerControl.StartTinyScreen();
		}

		public virtual void StopTinyScreen()
		{
			_playerControl.StopTinyScreen();
		}

		public virtual bool TinyScreen => _playerControl.TinyScreen;

        /// <summary>
		/// 播放和暂停
		/// </summary>
		public virtual void TogglePlay()
		{
			if (Playing)
			{
				Pause();
			}
			else
			{
				Start();
			}
		}

		/// <summary>
		/// 横竖屏切换，会旋转屏幕
		/// </summary>
		public virtual void ToggleFullScreen(Activity activity)
		{
			if (activity == null || activity.IsFinishing)
			{
				return;
			}
			if (FullScreen)
            {
                activity.RequestedOrientation = ScreenOrientation.Portrait;
				StopFullScreen();
			}
			else
            {
                activity.RequestedOrientation = ScreenOrientation.Landscape;
				StartFullScreen();
			}
		}

		/// <summary>
		/// 横竖屏切换，不会旋转屏幕
		/// </summary>
		public virtual void ToggleFullScreen()
		{
			if (FullScreen)
			{
				StopFullScreen();
			}
			else
			{
				StartFullScreen();
			}
		}

		/// <summary>
		/// 横竖屏切换，根据适配宽高决定是否旋转屏幕
		/// </summary>
		public virtual void ToggleFullScreenByVideoSize(Activity activity)
		{
			if (activity == null || activity.IsFinishing)
			{
				return;
			}
			var size = VideoSize;
			var width = size[0];
			var height = size[1];
			if (FullScreen)
			{
				StopFullScreen();
				if (width > height)
                {
                    activity.RequestedOrientation = ScreenOrientation.Portrait;
				}
			}
			else
			{
				StartFullScreen();
				if (width > height)
                {
                    activity.RequestedOrientation = ScreenOrientation.Landscape;
				}
			}
		}

		public virtual void StartFadeOut()
		{
			_controller.StartFadeOut();
		}

		public virtual void StopFadeOut()
		{
			_controller.StopFadeOut();
		}

		public virtual bool Showing => _controller.Showing;

        public virtual bool Locked
		{
			set => _controller.Locked = value;
            get => _controller.Locked;
        }


		public virtual void StartProgress()
		{
			_controller.StartProgress();
		}

		public virtual void StopProgress()
		{
			_controller.StopProgress();
		}

		public virtual void Hide()
		{
			_controller.Hide();
		}

		public virtual void Show()
		{
			_controller.Show();
		}

		public virtual bool HasCutout()
		{
			return _controller.HasCutout();
		}

		public virtual int CutoutHeight => _controller.CutoutHeight;

        /// <summary>
		/// 切换锁定状态
		/// </summary>
		public virtual void ToggleLockState()
		{
			Locked = !Locked;
		}


		/// <summary>
		/// 切换显示/隐藏状态
		/// </summary>
		public virtual void ToggleShowState()
		{
			if (Showing)
			{
				Hide();
			}
			else
			{
				Show();
			}
		}
	}

}