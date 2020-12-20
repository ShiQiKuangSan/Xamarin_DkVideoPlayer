using Android.Content;

namespace DkVideoPlayer.VideoPlayer.Player
{
    /// <summary>
    /// 创建<seealso cref="AndroidMediaPlayer"/>的工厂类，不推荐，系统的MediaPlayer兼容性较差，建议使用IjkPlayer或者ExoPlayer
    /// </summary>
    public class AndroidMediaPlayerFactory : PlayerFactory 
    {
        public static AndroidMediaPlayerFactory Create()
        {
            return new AndroidMediaPlayerFactory();
        }

        public override AbstractPlayer CreatePlayer(Context context)
        {
            return new AndroidMediaPlayer(context);
        }
    }
}