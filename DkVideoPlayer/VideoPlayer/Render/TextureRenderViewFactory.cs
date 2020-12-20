using Android.Content;

namespace DkVideoPlayer.VideoPlayer.Render
{

	public class TextureRenderViewFactory : RenderViewFactory
	{

		public static TextureRenderViewFactory Create()
		{
			return new TextureRenderViewFactory();
		}

		public override IRenderView CreateRenderView(Context context)
		{
			return new TextureRenderView(context);
		}
	}

}