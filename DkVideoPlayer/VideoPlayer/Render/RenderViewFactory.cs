using Android.Content;

namespace DkVideoPlayer.VideoPlayer.Render
{

	/// <summary>
	/// 此接口用于扩展自己的渲染View。使用方法如下：
	/// 1.继承IRenderView实现自己的渲染View。
	/// 2.重写createRenderView返回步骤1的渲染View。
	/// 可参考<seealso cref="TextureRenderView"/>和<seealso cref="TextureRenderViewFactory"/>的实现。
	/// </summary>
	public abstract class RenderViewFactory
	{
		public abstract IRenderView CreateRenderView(Context context);
	}
}