using Android.Content;
using DkVideoPlayer.VideoPlayer.Player;

namespace DkVideoPlayer.Ijk
{
    public class IjkPlayerFactory : PlayerFactory
    {
        public static IjkPlayerFactory Create()
        {
            return new IjkPlayerFactory();
        }

        public override AbstractPlayer CreatePlayer(Context context)
        {
            return new IjkPlayer(context);
        }
    }
}