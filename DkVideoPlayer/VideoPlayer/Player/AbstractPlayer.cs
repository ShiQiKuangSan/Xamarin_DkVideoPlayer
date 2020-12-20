using System.Collections.Generic;

using Android.Content.Res; 
using Android.Views;

namespace DkVideoPlayer.VideoPlayer.Player
{
    /// <summary>
    /// 抽象的播放器，继承此接口扩展自己的播放器
    /// Created by dueeeke on 2017/12/21.
    /// </summary>
    public abstract class AbstractPlayer
    {

        /// <summary>
        /// 开始渲染视频画面
        /// </summary>
        public const int MediaInfoVideoRenderingStart = 3;

        /// <summary>
        /// 缓冲开始
        /// </summary>
        public const int MediaInfoBufferingStart = 701;

        /// <summary>
        /// 缓冲结束
        /// </summary>
        public const int MediaInfoBufferingEnd = 702;

        /// <summary>
        /// 视频旋转信息
        /// </summary>
        public const int MediaInfoVideoRotationChanged = 10001;

        /// <summary>
        /// 播放器事件回调
        /// </summary>
        protected internal IPlayerEventListener PlayerEventListener;

        /// <summary>
        /// 初始化播放器实例
        /// </summary>
        public abstract void InitPlayer();

        /// <summary>
        /// 设置播放地址
        /// </summary>
        /// <param name="path">    播放地址 </param>
        /// <param name="headers"> 播放地址请求头 </param>
        public abstract void SetDataSource(string path, IDictionary<string, string> headers);

        /// <summary>
        /// 用于播放raw和asset里面的视频文件
        /// </summary>
        public abstract AssetFileDescriptor DataSource { set; }

        /// <summary>
        /// 播放
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// 暂停
        /// </summary>
        public abstract void Pause();

        /// <summary>
        /// 停止
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// 准备开始播放（异步）
        /// </summary>
        public abstract void PrepareAsync();

        /// <summary>
        /// 重置播放器
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// 是否正在播放
        /// </summary>
        public abstract bool Playing { get; }

        /// <summary>
        /// 调整进度
        /// </summary>
        public abstract void SeekTo(long time);

        /// <summary>
        /// 释放播放器
        /// </summary>
        public abstract void Release();

        /// <summary>
        /// 获取当前播放的位置
        /// </summary>
        public abstract long CurrentPosition { get; }

        /// <summary>
        /// 获取视频总时长
        /// </summary>
        public abstract long Duration { get; }

        /// <summary>
        /// 获取缓冲百分比
        /// </summary>
        public abstract int BufferedPercentage { get; }

        /// <summary>
        /// 设置渲染视频的View,主要用于TextureView
        /// </summary>
        public abstract Surface Surface { set; }

        /// <summary>
        /// 设置渲染视频的View,主要用于SurfaceView
        /// </summary>
        public abstract ISurfaceHolder Display { set; }

        /// <summary>
        /// 设置音量
        /// </summary>
        public abstract void SetVolume(float v1, float v2);

        /// <summary>
        /// 设置是否循环播放
        /// </summary>
        public abstract bool Looping { set; }

        /// <summary>
        /// 设置其他播放配置
        /// </summary>
        public abstract void SetOptions();

        /// <summary>
        /// 设置播放速度
        /// </summary>
        public abstract float Speed { set; get; }


        /// <summary>
        /// 获取当前缓冲的网速
        /// </summary>
        public abstract long TcpSpeed { get; }

        /// <summary>
        /// 绑定VideoView
        /// </summary>
        public virtual void SetPlayerEventListener(IPlayerEventListener playerEventListener)
        {
            this.PlayerEventListener = playerEventListener;
        }

        public interface IPlayerEventListener
        {

            void OnError();

            void OnCompletion();

            void OnInfo(int what, int extra);

            void OnPrepared();

            void OnVideoSizeChanged(int width, int height);
        }

    }

}