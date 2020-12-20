using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Content.Res;
using Android.Graphics;
using Android.Net;
using Android.Telephony;
using Android.Util;
using Java.Text;
using Java.Util;

namespace DkVideoPlayer.VideoPlayer.Util
{
    public static class PlayerUtils
    {
        /// <summary>
        /// 获取状态栏高度
        /// </summary>
        public static double GetStatusBarHeight(Context context)
        {
            var statusBarHeight = 0;
            //获取status_bar_height资源的ID
            var resourceId = context.Resources.GetIdentifier("status_bar_height", "dimen", "android");
            if (resourceId > 0)
            {
                //根据资源ID获取响应的尺寸值
                statusBarHeight = context.Resources.GetDimensionPixelSize(resourceId);
            }
            return statusBarHeight;
        }

        /// <summary>
        /// 获取竖屏下状态栏高度
        /// </summary>
        public static double GetStatusBarHeightPortrait(Context context)
        {
            var statusBarHeight = 0;
            //获取status_bar_height_portrait资源的ID
            var resourceId = context.Resources.GetIdentifier("status_bar_height_portrait", "dimen", "android");
            if (resourceId > 0)
            {
                //根据资源ID获取响应的尺寸值
                statusBarHeight = context.Resources.GetDimensionPixelSize(resourceId);
            }
            return statusBarHeight;
        }

        /// <summary>
        /// 获取NavigationBar的高度
        /// </summary>
        public static int GetNavigationBarHeight(Context context)
        {
            if (!HasNavigationBar(context))
            {
                return 0;
            }
            var resources = context.Resources;
            var resourceId = resources.GetIdentifier("navigation_bar_height", "dimen", "android");
            //获取NavigationBar的高度
            return resources.GetDimensionPixelSize(resourceId);
        }

        /// <summary>
        /// 是否存在NavigationBar
        /// </summary>
        public static bool HasNavigationBar(Context context)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr1)
            {
                var display = GetWindowManager(context).DefaultDisplay;
                var size = new Point();
                var realSize = new Point();
                display?.GetSize(size);
                display?.GetRealSize(realSize);
                return realSize.X != size.X || realSize.Y != size.Y;
            }
            else
            {
                var menu = ViewConfiguration.Get(context)?.HasPermanentMenuKey ?? false;
                var back = KeyCharacterMap.DeviceHasKey(Keycode.Back);
                return !(menu || back);
            }
        }

        /// <summary>
        /// 获取屏幕宽度
        /// </summary>
        public static int GetScreenWidth(Context context, bool isIncludeNav)
        {
            if (isIncludeNav)
            {
                return context.Resources.DisplayMetrics.WidthPixels + GetNavigationBarHeight(context);
            }
            else
            {
                return context.Resources.DisplayMetrics.WidthPixels;
            }
        }

        /// <summary>
        /// 获取屏幕高度
        /// </summary>
        public static int GetScreenHeight(Context context, bool isIncludeNav)
        {
            if (isIncludeNav)
            {
                return context.Resources.DisplayMetrics.HeightPixels + GetNavigationBarHeight(context);
            }
            else
            {
                return context.Resources.DisplayMetrics.HeightPixels;
            }
        }

        /// <summary>
        /// 获取Activity
        /// </summary>
        public static Activity ScanForActivity(Context context)
        {
            if (context == null)
            {
                return null;
            }
            if (context is Activity activity)
            {
                return activity;
            }
            else if (context is ContextWrapper wrapper)
            {
                return ScanForActivity(wrapper.BaseContext);
            }
            return null;
        }

        /// <summary>
        /// dp转为px
        /// </summary>
        public static int Dp2Px(Context context, float dpValue)
        {
            return (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, dpValue, context.Resources.DisplayMetrics);
        }

        /// <summary>
        /// sp转为px
        /// </summary>
        public static int Sp2Px(Context context, float dpValue)
        {
            return (int)TypedValue.ApplyDimension(ComplexUnitType.Sp, dpValue, context.Resources.DisplayMetrics);
        }

        /// <summary>
        /// 如果WindowManager还未创建，则创建一个新的WindowManager返回。否则返回当前已创建的WindowManager。
        /// </summary>
        public static IWindowManager GetWindowManager(Context context)
        {
            return context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
        }

        /// <summary>
        /// 边缘检测
        /// </summary>
        public static bool IsEdge(Context context, MotionEvent e)
        {
            var edgeSize = Dp2Px(context, 40);
            return e.RawX < edgeSize || e.RawX > GetScreenWidth(context, true) - edgeSize || e.RawY < edgeSize || e.RawY > GetScreenHeight(context, true) - edgeSize;
        }

        public const int NoNetwork = 0;
        public const int NetworkClosed = 1;
        public const int NetworkEthernet = 2;
        public const int NetworkWifi = 3;
        public const int NetworkMobile = 4;
        public const int NetworkUnknown = -1;

        /// <summary>
        /// 判断当前网络类型
        /// </summary>
        public static int GetNetworkType(Context context)
        {
            //改为context.getApplicationContext()，防止在Android 6.0上发生内存泄漏
            var connectMgr = context.ApplicationContext.GetSystemService(Context.ConnectivityService).JavaCast<ConnectivityManager>();

            var networkInfo = connectMgr?.ActiveNetworkInfo;
            if (networkInfo == null)
            {
                // 没有任何网络
                return NoNetwork;
            }
            if (!networkInfo.IsConnected)
            {
                // 网络断开或关闭
                return NetworkClosed;
            }
            if (networkInfo.Type == ConnectivityType.Ethernet)
            {
                // 以太网网络
                return NetworkEthernet;
            }
            else if (networkInfo.Type == ConnectivityType.Wifi)
            {
                // wifi网络，当激活时，默认情况下，所有的数据流量将使用此连接
                return NetworkWifi;
            }
            else if (networkInfo.Type == ConnectivityType.Mobile)
            {
                // 移动数据连接,不能与连接共存,如果wifi打开，则自动关闭
                var state = (NetworkType)networkInfo.Subtype;

                switch (state)
                {
                    // 2G
                    case NetworkType.Gprs:
                    case NetworkType.Edge:
                    case NetworkType.Cdma:
                    case NetworkType.OneXrtt:
                    case NetworkType.Iden:
                    // 3G
                    case NetworkType.Umts:
                    case NetworkType.Evdo0:
                    case NetworkType.EvdoA:
                    case NetworkType.Hsdpa:
                    case NetworkType.Hsupa:
                    case NetworkType.Hspa:
                    case NetworkType.EvdoB:
                    case NetworkType.Ehrpd:
                    case NetworkType.Hspap:
                    // 4G
                    case NetworkType.Lte:
                    // 5G
                    case NetworkType.Nr:
                        return NetworkMobile;
                }
            }
            // 未知网络
            return NetworkUnknown;
        }


        /// <summary>
        /// 获取当前系统时间
        /// </summary>
        public static string CurrentSystemTime
        {
            get
            {
                var simpleDateFormat = new SimpleDateFormat("HH:mm", Locale.Default);
                var date = new Date();
                return simpleDateFormat.Format(date);
            }
        }

        /// <summary>
        /// 格式化时间
        /// </summary>
        public static string StringForTime(int timeMs)
        {
            var totalSeconds = timeMs / 1000;

            var seconds = totalSeconds % 60;
            var minutes = (totalSeconds / 60) % 60;
            var hours = totalSeconds / 3600;

            if (hours > 0)
            {
                return Java.Lang.String.Format(Locale.Default, "%d:%02d:%02d", hours, minutes, seconds);
            }
            else
            {
                return Java.Lang.String.Format(Locale.Default, "%02d:%02d", minutes, seconds);
            }
        }

        /// <summary>
        /// 获取集合的快照
        /// </summary>
        public static IList<T> GetSnapshot<T>(ICollection<T> other)
        {
            var result = new List<T>(other.Count);
            result.AddRange(other.Where(item => item != null));
            return result;
        }
    }
}