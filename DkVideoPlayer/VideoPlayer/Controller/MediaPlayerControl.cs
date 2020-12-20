using Android.Graphics;

namespace DkVideoPlayer.VideoPlayer.Controller
{
    public interface IMediaPlayerControl
    {
        void Start();

        void Pause();

        long Duration { get; }

        long CurrentPosition { get; }

        void SeekTo(long pos);

        bool Playing { get; }

        int BufferedPercentage { get; }

        void StartFullScreen();

        void StopFullScreen();

        bool FullScreen { get; }

        bool Mute { set; get; }


        int ScreenScaleType { set; }

        float Speed { set; get; }

        void Replay(bool resetPosition);

        bool MirrorRotation { set; }

        Bitmap DoScreenShot();

        int[] VideoSize { get; }

        float Rotation { set; }

        void StartTinyScreen();

        void StopTinyScreen();

        bool TinyScreen { get; }
    }
}