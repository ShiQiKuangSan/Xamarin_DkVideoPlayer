using Android.Views;
using Android.Views.Animations;

namespace DkVideoPlayer.VideoPlayer.Controller
{
	public interface IControlComponent
	{
		void Attach(ControlWrapper controlWrapper);

		View View {get;}

		void OnVisibilityChanged(bool isVisible, Animation anim);

		void OnPlayStateChanged(int playState);

		void OnPlayerStateChanged(int playerState);

		void SetProgress(int duration, int position);

		void OnLockStateChanged(bool isLocked);
	}

}