using Android.Views;
using DkVideoPlayer.VideoPlayer.Player;

namespace DkVideoPlayer.VideoPlayer.Render
{

	public sealed class MeasureHelper
	{

		private int _mVideoWidth;

		private int _mVideoHeight;

		private int _mCurrentScreenScale;

		private int _mVideoRotationDegree;

		public int VideoRotation
		{
			set => _mVideoRotationDegree = value;
		}

		public void SetVideoSize(int width, int height)
		{
			_mVideoWidth = width;
			_mVideoHeight = height;
		}

		public int ScreenScale
		{
			set => _mCurrentScreenScale = value;
		}

		/// <summary>
		/// 注意：VideoView的宽高一定要定死，否者以下算法不成立
		/// </summary>
		public int[] DoMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			if (_mVideoRotationDegree == 90 || _mVideoRotationDegree == 270)
			{ // 软解码时处理旋转信息，交换宽高
				widthMeasureSpec = widthMeasureSpec + heightMeasureSpec;
				heightMeasureSpec = widthMeasureSpec - heightMeasureSpec;
				widthMeasureSpec = widthMeasureSpec - heightMeasureSpec;
			}

			var width = View.MeasureSpec.GetSize(widthMeasureSpec);
			var height = View.MeasureSpec.GetSize(heightMeasureSpec);

			if (_mVideoHeight == 0 || _mVideoWidth == 0)
			{
				return new int[]{width, height};
			}

			//如果设置了比例
			switch (_mCurrentScreenScale)
			{
				case VideoView.SCREEN_SCALE_DEFAULT:
				default:
					if (_mVideoWidth * height < width * _mVideoHeight)
					{
						width = height * _mVideoWidth / _mVideoHeight;
					}
					else if (_mVideoWidth * height > width * _mVideoHeight)
					{
						height = width * _mVideoHeight / _mVideoWidth;
					}
					break;
				case VideoView.SCREEN_SCALE_ORIGINAL:
					width = _mVideoWidth;
					height = _mVideoHeight;
					break;
				case VideoView.SCREEN_SCALE_16_9:
					if (height > width / 16 * 9)
					{
						height = width / 16 * 9;
					}
					else
					{
						width = height / 9 * 16;
					}
					break;
				case VideoView.SCREEN_SCALE_4_3:
					if (height > width / 4 * 3)
					{
						height = width / 4 * 3;
					}
					else
					{
						width = height / 3 * 4;
					}
					break;
				case VideoView.SCREEN_SCALE_MATCH_PARENT:
					width = widthMeasureSpec;
					height = heightMeasureSpec;
					break;
				case VideoView.SCREEN_SCALE_CENTER_CROP:
					if (_mVideoWidth * height > width * _mVideoHeight)
					{
						width = height * _mVideoWidth / _mVideoHeight;
					}
					else
					{
						height = width * _mVideoHeight / _mVideoWidth;
					}
					break;
			}
			return new int[]{width, height};
		}
	}

}