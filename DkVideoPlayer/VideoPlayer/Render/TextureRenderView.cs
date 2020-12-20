using Android.Content;
using Android.Graphics;
using Android.Views;

using DkVideoPlayer.VideoPlayer.Player;

namespace DkVideoPlayer.VideoPlayer.Render
{
    public sealed class TextureRenderView : TextureView, IRenderView, TextureView.ISurfaceTextureListener
    {
        private void InitializeInstanceFields()
        {
            mMeasureHelper = new MeasureHelper();
            SurfaceTextureListener = this;
        }

        private MeasureHelper mMeasureHelper;
        private SurfaceTexture mSurfaceTexture;

        private AbstractPlayer mMediaPlayer;
        private Surface mSurface;

        public TextureRenderView(Context context) : base(context)
        {
            InitializeInstanceFields();
        }

        public void AttachToPlayer(AbstractPlayer player)
        {
            this.mMediaPlayer = player;
        }

        public void SetVideoSize(int videoWidth, int videoHeight)
        {
            if (videoWidth > 0 && videoHeight > 0)
            {
                mMeasureHelper.SetVideoSize(videoWidth, videoHeight);
                RequestLayout();
            }
        }

        public int VideoRotation
        {
            set
            {
                mMeasureHelper.VideoRotation = value;
                Rotation = value;
            }
        }

        public int ScaleType
        {
            set
            {
                mMeasureHelper.ScreenScale = value;
                RequestLayout();
            }
        }

        public View View => this;

        public Bitmap DoScreenShot()
        {
            return Bitmap;
        }

        public void Release()
        {
            mSurface?.Release();
            mSurfaceTexture?.Release();
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            int[] measuredSize = mMeasureHelper.DoMeasure(widthMeasureSpec, heightMeasureSpec);
            SetMeasuredDimension(measuredSize[0], measuredSize[1]);
        }

        public void OnSurfaceTextureAvailable(SurfaceTexture surfaceTexture, int width, int height)
        {
            if (mSurfaceTexture != null)
            {
                SurfaceTexture = mSurfaceTexture;
            }
            else
            {
                mSurfaceTexture = surfaceTexture;
                mSurface = new Surface(surfaceTexture);
                if (mMediaPlayer != null)
                {
                    mMediaPlayer.Surface = mSurface;
                }
            }
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
        {
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            return false;
        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {
        }
    }
}