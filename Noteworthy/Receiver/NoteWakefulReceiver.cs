
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;
using Android.Support.V4.Content;

namespace Noteworthy
{
	[BroadcastReceiver(Enabled = true)]
	public class NoteWakefulReceiver : WakefulBroadcastReceiver
	{
		public override void OnReceive(Context context, Intent intent)
		{
			Log.Debug("NoteWakefulReceiver", "The device is attempting to sleep! Wake it ta fuck up!");
			StartWakefulService(context, new Intent(context, typeof(BackgroundService)));
		}
	}
}
