using Android.Content;

namespace DkVideoPlayer.VideoPlayer.Player
{

    /// <summary>
    /// 此接口使用方法：
    /// 1.继承<seealso cref="TAbstractPlayer"/>扩展自己的播放器。
    /// 2.继承此接口并实现<seealso cref="#createPlayer(Context)"/>，返回步骤1中的播放器。
    /// 可参照<seealso cref="AndroidMediaPlayer"/>和<seealso cref="AndroidMediaPlayerFactory"/>的实现。
    /// </summary>
    public abstract class PlayerFactory
    {
        public abstract AbstractPlayer CreatePlayer(Context context);
    }
}