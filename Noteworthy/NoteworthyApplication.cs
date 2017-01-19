using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.App;
using System.Collections.Generic;
using TaskStackBuilderCompat = Android.Support.V4.App.TaskStackBuilder;
using Android.OS;

namespace Noteworthy
{
	[Application(Icon = "@mipmap/icon")]
	public class NoteworthyApplication
	{

		public static void StartBackgroundService()
		{
			StartBackgroundService(Application.Context);
		}

		public static void StartBackgroundService(Context context)
		{
			context.StartService(new Intent(context, typeof(BackgroundService)));
		}

		public static void StopBackgroundService()
		{
			StopBackgroundService(Application.Context);
		}

		public static void StopBackgroundService(Context context)
		{
			context.StopService(new Intent(context, typeof(BackgroundService)));
		}
	}
}