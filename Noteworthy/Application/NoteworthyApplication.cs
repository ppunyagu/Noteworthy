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
		// Do not use 0 here.
		private static int NotificationIdDataIncoming = 1;

		private static int NotificationIdDataIncomingAutoIncremented
		{
			get
			{
				return NotificationIdDataIncoming++;
			}
		}

		public static void NotifyMemorized(string Path = "", bool notAttachedToLook = true)
		{

			CreateMemorizedNotification(Path, notAttachedToLook);
		}

		private static void CreateMemorizedNotification(string Path, bool notAttachedToLook)
		{
			try
			{

				var valuesForActivity = new Bundle();
				//valuesForActivity.PutString("ViewSimiliarProduct", Path);
				//valuesForActivity.PutBoolean("isScreenShot", true);
				var resultPendingIntent = MakePendingIntent(typeof(MainMemoryActivity), valuesForActivity);

				var appName = Application.Context.GetString(Resource.String.app_name);
				var text = "Your recent conversation was just memorized!";

				var notification =
					BuildNotification(
						largeIcon: null,
						title: appName,
						content: text,
						secondContent: Path,
						contentIntent: resultPendingIntent,
						ongoing: false,
						autoCancel: true,
						bigPictureStyle: true,
						maxPriority: notAttachedToLook,
						setDefaults: notAttachedToLook,
						bigPictureIcon: null);

				NotifyThroughNotificationManager(NotificationIdDataIncomingAutoIncremented, notification.Build());
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler("NoteworthyApplication", "CreateMemorizedNotification", ex);
			}
		}


		private static PendingIntent MakePendingIntent(Type activityType, Bundle bundle)
		{
			// Pass the data to the activity
			var resultIntent = new Intent(Application.Context, activityType);
			resultIntent.PutExtras(bundle);

			// Sets the Activity to start in a new, empty task
			resultIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);

			// Construct a back stack for cross-task navigation
			var stackBuilder = TaskStackBuilderCompat.Create(Application.Context);
			stackBuilder.AddParentStack(Java.Lang.Class.FromType(activityType));
			stackBuilder.AddNextIntent(resultIntent);

			// Create the PendingIntent with the back stack
			PendingIntent resultPendingIntent =
				stackBuilder.GetPendingIntent(0, (int)PendingIntentFlags.UpdateCurrent);

			return resultPendingIntent;
		}

		private static NotificationCompat.Builder BuildNotification(
			Bitmap largeIcon,
			string title,
			string content,
			string secondContent,
			PendingIntent contentIntent,
			bool ongoing,
			bool autoCancel,
			bool bigPictureStyle,
			bool maxPriority,
			bool setDefaults,
			Bitmap bigPictureIcon = null,
			bool showSecondContent = true,
			int iconResId = Resource.Drawable.icon2)
		{
			var builder = new NotificationCompat.Builder(Application.Context)
				.SetSmallIcon(iconResId);

			if (largeIcon != null)
				builder.SetLargeIcon(largeIcon);

			builder.SetContentTitle(title);

			if (!string.IsNullOrWhiteSpace(content))
				builder.SetContentText(content);

			if (showSecondContent && !string.IsNullOrWhiteSpace(secondContent))
				builder.SetSubText(secondContent);

			if (bigPictureStyle && bigPictureIcon != null)
			{
				var style = new NotificationCompat.BigPictureStyle()
					.BigPicture(bigPictureIcon)
					.SetBigContentTitle(title);

				if (!string.IsNullOrWhiteSpace(secondContent))
					style.SetSummaryText(secondContent);

				builder.SetStyle(style);
			}

			if (contentIntent != null)
			{
				builder.SetContentIntent(contentIntent);
			}

			builder
				.SetOngoing(contentIntent != null && ongoing)
				.SetAutoCancel(contentIntent == null || autoCancel)
				.SetVisibility(NotificationCompat.VisibilityPublic)
				.SetCategory(NotificationCompat.CategoryService);

			if (maxPriority)
				builder.SetPriority(NotificationCompat.PriorityHigh);

			if (setDefaults)
				builder.SetDefaults(
					NotificationCompat.DefaultLights |
					NotificationCompat.DefaultSound |
					NotificationCompat.DefaultVibrate);

			return builder;
		}

		private static void NotifyThroughNotificationManager(
			int notificationId,
			Notification notification)
		{
			var nm = (NotificationManager)Application.Context.GetSystemService(Context.NotificationService);
			nm.Notify(notificationId, notification);
		}


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

		public static void StartBackgroundServiceNative()
		{
			StartBackgroundServiceNative(Application.Context);
		}

		public static void StartBackgroundServiceNative(Context context)
		{
			context.StartService(new Intent(context, typeof(BackgroundServiceNativeApproach)));
		}

		public static void StopBackgroundServiceNative()
		{
			StopBackgroundServiceNative(Application.Context);
		}

		public static void StopBackgroundServiceNative(Context context)
		{
			context.StopService(new Intent(context, typeof(BackgroundServiceNativeApproach)));
		}
	}
}