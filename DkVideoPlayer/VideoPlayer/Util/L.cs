

using Android.Util;
using DkVideoPlayer.VideoPlayer.Player;

namespace DkVideoPlayer.VideoPlayer.Util
{
    public static class L
    {
        private const string TAG = "DKPlayer";

        private static bool isDebug = VideoViewManager.Config.mIsEnableLog;


        public static void D(string msg)
        {
            if (isDebug)
            {
                Log.Debug(TAG, msg);
            }
        }

        public static void E(string msg)
        {
            if (isDebug)
            {
                Log.Error(TAG, msg);
            }
        }

        public static void I(string msg)
        {
            if (isDebug)
            {
                Log.Info(TAG, msg);
            }
        }

        public static void W(string msg)
        {
            if (isDebug)
            {
                Log.Warn(TAG, msg);
            }
        }

        public static bool Debug
        {
            set => L.isDebug = value;
        }
    }
}