using Android.Graphics;
using Android.Views;
using DkVideoPlayer.VideoPlayer.Player;

namespace DkVideoPlayer.VideoPlayer.Render
{
    public interface IRenderView
    {

        /// <summary>
        /// 关联AbstractPlayer
        /// </summary>
        void AttachToPlayer(AbstractPlayer player);

        /// <summary>
        /// 设置视频宽高 </summary>
        /// <param name="videoWidth"> 宽 </param>
        /// <param name="videoHeight"> 高 </param>
        void SetVideoSize(int videoWidth, int videoHeight);

        /// <summary>
        /// 设置视频旋转角度 </summary>
        /// <param name="degree"> 角度值 </param>
        int VideoRotation { set; }

        /// <summary>
        /// 设置screen scale type </summary>
        /// <param name="scaleType"> 类型 </param>
        int ScaleType { set; }

        /// <summary>
        /// 获取真实的RenderView
        /// </summary>
        View View { get; }

        /// <summary>
        /// 截图
        /// </summary>
        Bitmap DoScreenShot();

        /// <summary>
        /// 释放资源
        /// </summary>
        void Release();

    }
}