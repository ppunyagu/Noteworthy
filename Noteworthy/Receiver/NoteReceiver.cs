
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
				Log.Debug("NoteReceiver", string.Format("Audio stored local at path: {0}", stringUri != null ? stringUri : "<null>"));

				// Update to AWSS3 for processing to audio
				//var url = await S3Utils.UploadS3Audios(stringUri, "Audio");

				using (var objTranslationService = new TranslationService())
				{
					int jobId = objTranslationService.ConvertAudioToText(stringUri);
					if (jobId != 0)
					{
						Memory _mem = new Memory();
						_mem.Audio_path = stringUri;
						_mem.Duration = Convert.ToInt32(intent.GetStringExtra(BackgroundService.ExtraAudioRecordedDurations));
						_mem.JobId = jobId;
						_mem.ConversationText = "";
						_mem.Time = DateTime.Now;
						SQLClient<Memory>.Instance.Insert(_mem);
						NoteworthyApplication.NotifyMemorized(stringUri);
					}
					else {
						Log.Debug("NoteReceiver", "TranslationService failed");
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler("NoteReceiver", "OnReceive", ex);
			}
		}
	}
}
