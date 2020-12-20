using Android.App;
using Android.Content;
using Android.OS;		 
using Android.Views;	  
using Java.Lang;			
using Exception = System.Exception;

namespace DkVideoPlayer.VideoPlayer.Util
{
    public static class CutoutUtil
    {
		/// <summary>
		/// 是否为允许全屏界面显示内容到刘海区域的刘海屏机型（与AndroidManifest中配置对应）
		/// </summary>
		public static bool AllowDisplayToCutout(Activity activity)
		{
			if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
			{
				// 9.0系统全屏界面默认会保留黑边，不允许显示内容到刘海区域
				var window = activity.Window;
                if (window != null)
                {
                    var windowInsets = window.DecorView.RootWindowInsets;
                    var displayCutout = windowInsets?.DisplayCutout;
                    if (displayCutout == null)
                    {
                        return false;
                    }
                    var boundingRects = displayCutout.BoundingRects;
                    return boundingRects.Count > 0;
                }
            }
			else
			{
				return HasCutoutHuawei(activity) || HasCutoutOppo(activity) || HasCutoutVivo(activity) || HasCutoutXiaomi(activity);
			}

            return false;
        }

		/// <summary>
		/// 是否是华为刘海屏机型
		/// </summary>
		private static bool HasCutoutHuawei(Activity activity)
		{
			if (Build.Manufacturer != null && !Build.Manufacturer.Equals("HUAWEI"))
			{
				return false;
			}
			try
			{
				var cl = activity.ClassLoader;
                var hwNotchSizeUtil = cl?.LoadClass("com.huawei.android.util.HwNotchSizeUtil");
                if (hwNotchSizeUtil != null)
                {
                    var get = hwNotchSizeUtil.GetMethod("hasNotchInScreen");
                    return (bool)get.Invoke(hwNotchSizeUtil);
                }

                return false;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// 是否是oppo刘海屏机型
		/// </summary>
		private static bool HasCutoutOppo(Activity activity)
		{
			if (Build.Manufacturer != null && !Build.Manufacturer.Equals("oppo"))
			{
				return false;
			}
			return activity.PackageManager != null && activity.PackageManager.HasSystemFeature("com.oppo.feature.screen.heteromorphism");
		}

		/// <summary>
		/// 是否是vivo刘海屏机型
		/// </summary>
		private static bool HasCutoutVivo(Activity activity)
		{
			if (Build.Manufacturer != null && !Build.Manufacturer.Equals("vivo"))
			{
				return false;
			}
			try
			{
				var cl = activity.ClassLoader;
				var ftFeatureUtil = cl?.LoadClass("android.util.FtFeature");
				if (ftFeatureUtil != null)
				{
					var get = ftFeatureUtil.GetMethod("isFeatureSupport", Class.FromType(typeof(int)));
					return (bool)get.Invoke(ftFeatureUtil, 0x00000020);
				}
				return false;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// 是否是小米刘海屏机型
		/// </summary>
		private static bool HasCutoutXiaomi(Activity activity)
		{
			if (Build.Manufacturer != null && !Build.Manufacturer.Equals("xiaomi"))
			{
				return false;
			}
			try
			{
				var cl = activity.ClassLoader;
				var SystemProperties = cl.LoadClass("android.os.SystemProperties");
				var paramTypes = new Class[2];
				paramTypes[0] = Class.FromType(typeof(string));
				paramTypes[1] = Class.FromType(typeof(int));
				var getInt = SystemProperties.GetMethod("getInt", paramTypes);
				//参数
				var @params = new Java.Lang.Object[2];
				@params[0] = "ro.miui.notch";
				@params[1] = 0;
				var hasCutout = (int)getInt.Invoke(SystemProperties, @params);
				return hasCutout == 1;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// 适配刘海屏，针对Android P以上系统
		/// </summary>
		public static void AdaptCutoutAboveAndroidP(Context context, bool isAdapt)
		{
			var activity = PlayerUtils.ScanForActivity(context);
			if (activity == null)
			{
				return;
			}

			if (Build.VERSION.SdkInt < BuildVersionCodes.P)
				return;
			
			var lp = activity.Window?.Attributes;
			if (isAdapt)
			{
				if (lp != null) lp.LayoutInDisplayCutoutMode = LayoutInDisplayCutoutMode.ShortEdges;
			}
			else
			{
				if (lp != null) lp.LayoutInDisplayCutoutMode = LayoutInDisplayCutoutMode.Default;
			}

			if (activity.Window != null) activity.Window.Attributes = lp;
		}
	}
}