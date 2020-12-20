using System;
using System.IO;
using Android.Content;
using Android.Content.Res;
using TV.Danmaku.Ijk.Media.Player.Misc;

namespace DkVideoPlayer.Ijk
{
    public class RawDataSourceProvider : Java.Lang.Object, IMediaDataSource
    {
        private AssetFileDescriptor _descriptor;

        private byte[] _mediaBytes;

        public RawDataSourceProvider(AssetFileDescriptor descriptor)
        {
            _descriptor = descriptor;
        }

        public  int ReadAt(long position, byte[] buffer, int offset, int size)
        {
            if (position + 1 >= _mediaBytes.Length)
            {
                return -1;
            }

            int length;
            if (position + size < _mediaBytes.Length)
            {
                length = size;
            }
            else
            {
                length = (int) (_mediaBytes.Length - position);
                if (length > buffer.Length)
                {
                    length = buffer.Length;
                }

                length--;
            }

            Array.Copy(_mediaBytes, (int) position, buffer, offset, length);

            return length;
        }


        public  long Size
        {
            get
            {
                var length = _descriptor.Length;
                if (_mediaBytes == null)
                {
                    var inputStream = _descriptor.CreateInputStream();
                    _mediaBytes = ReadBytes(inputStream);
                }


                return length;
            }
        }


        public  void Close()
        {
            _descriptor?.Close();
            _descriptor = null;
            _mediaBytes = null;
        }

        private static byte[] ReadBytes(Stream inputStream)
        {
            var byteBuffer = new MemoryStream();

            var bufferSize = 1024;
            var buffer = new byte[bufferSize];

            int len;
            while ((len = inputStream.Read(buffer, 0, buffer.Length)) != -1)
            {
                byteBuffer.Write(buffer, 0, len);
            }

            return byteBuffer.ToArray();
        }

        public static RawDataSourceProvider Create(Context context, Android.Net.Uri uri)
        {
            try
            {
                var fileDescriptor = context.ContentResolver?.OpenAssetFileDescriptor(uri, "r");
                return new RawDataSourceProvider(fileDescriptor);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }

            return null;
        }
    }
}