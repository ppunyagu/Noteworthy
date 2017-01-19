
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Util;
using Android.Widget;

namespace Noteworthy
{
	[BroadcastReceiver]
	[IntentFilter(new[] { BackgroundService.ActionAudioRecorded })]
	public class NoteReceiver : BroadcastReceiver
	{
		public override void OnReceive(Context context, Intent intent)
		{
			var stringUri = intent.GetStringExtra(BackgroundService.ExtraAudioRecordedAbsolutePath);
			Log.Debug("NoteReceiver", string.Format("Audio Recorded path: {0}", stringUri != null ? stringUri : "<null>"));
		}
	}
}
