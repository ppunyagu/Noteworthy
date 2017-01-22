
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
			try
			{
				var stringUri = intent.GetStringExtra(BackgroundService.ExtraAudioRecordedAbsolutePath);
				// Update to AWSS3 for processing to audio
				//var url = await S3Utils.UploadS3Audios(stringUri, "Audio");
				Log.Debug("NoteReceiver", string.Format("Audio stored local at path: {0}", stringUri != null ? stringUri : "<null>"));
				Memory _mem = new Memory();
				_mem.Audio_path = stringUri;
				_mem.Time = DateTime.Now;
				SQLClient<Memory>.Instance.Insert(_mem);
				NoteworthyApplication.NotifyMemorized(stringUri);
				//Log.Debug("NoteReceiver", string.Format("Audio Uploaded to S3 file with url called: {0}", url != null ? url : "<null>"));
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler("NoteReceiver", "OnReceive", ex);
			}
		}
	}
}
