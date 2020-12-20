using Android.Content;
using Android.Views;
using Java.Lang;

namespace DkVideoPlayer.VideoPlayer.Controller
{
    /// <summary>
    /// 设备方向监听
    /// </summary>
    public class OrientationHelper : OrientationEventListener
    {

        private long _lastTime;

        private IOnOrientationChangeListener _onOrientationChangeListener;

        public OrientationHelper(Context context) : base(context)
        {
        }

        public override void OnOrientationChanged(int orientation)
        {
            var currentTime = JavaSystem.CurrentTimeMillis();
            if (currentTime - _lastTime < 300)
            {
                return; //300毫秒检测一次
            }

            _onOrientationChangeListener?.OnOrientationChanged(orientation);
            _lastTime = currentTime;
        }


        public interface IOnOrientationChangeListener
        {
            void OnOrientationChanged(int orientation);
        }

        public virtual void SetOnOrientationChangeListener(IOnOrientationChangeListener onOrientationChangeListener)
        {
            this._onOrientationChangeListener = onOrientationChangeListener;
        }
    }

}