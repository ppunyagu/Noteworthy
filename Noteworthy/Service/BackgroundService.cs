using System;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Media;


namespace Noteworthy
{

	[Service (Exported = false)]
	public class BackgroundService : Service
	{
		static readonly string TAG = "X:" + typeof(BackgroundService).Name;
		static readonly int TimerWait = 6000;
		Timer timer;
		DateTime startTime;
		bool isStarted = false;
		bool isRecording = false;
		string AudioRecordedPath;
		MediaRecorder mediaRecorder;

		public const string ActionAudioRecorded = "ACTION_AUDIO_RECORDED";
		public const string ExtraAudioRecordedAbsolutePath = "EXTRA_AUDIORECORDED_ABSOLUTE_PATH";

		public override void OnCreate()
		{
			mediaRecorder = new MediaRecorder();
		}

		public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
		{
			if (!isStarted)
			{
				startTime = DateTime.UtcNow;
				Log.Debug(TAG, $"Starting the service, at {startTime}.");
				timer = new Timer(HandleTimerCallback, startTime, 0, TimerWait);
				isStarted = true;
			}
			return StartCommandResult.NotSticky;
		}

		public override IBinder OnBind(Intent intent)
		{
			// This is a started service, not a bound service, so we just return null.
			return null;
		}


		public override void OnDestroy()
		{
			timer.Dispose();
			timer = null;
			isStarted = false;

			TimeSpan runtime = DateTime.UtcNow.Subtract(startTime);
			Log.Debug(TAG, $"Simple Service destroyed at {DateTime.UtcNow} after running for {runtime:c}.");
			base.OnDestroy();
		}

		async void HandleTimerCallback(object state)
		{
			try
			{
				using (var objGetWebPulseService = new GetWebPulseService())
				{
					bool isStressed = objGetWebPulseService.IsPulseStressed();
					Log.Debug(TAG, String.Format("isStressed: {0}, isRecording: {1}", isStressed, isRecording));
					if (isStressed)
					{
						if (!isRecording)
						{
							Log.Debug("HandleTimerCallback", "Pulse is stress and is not recording, so should begin recording");
							StartRecording();
							isRecording = true;
						}
						else {
							Log.Debug("HandleTimerCallback", "Pulse is stress but is already recording, so should continue recording");
						}
					}
					else {
						if (!isRecording)
						{
							Log.Debug("HandleTimerCallback", "Pulse is not stress and is not recording, so should continue idle");
						}
						else {
							Log.Debug("HandleTimerCallback", "Pulse is not stress and is recording, so should stop recording and broadcast event");
							StopRecording();
							isRecording = false;
							/*
							var url = await S3Utils.UploadS3Audios(AudioRecordedPath, "Audio");
							var AudioRecordedIntent = new Intent(url);
							{
								AudioRecordedIntent.PutExtra(ExtraAudioRecordedAbsolutePath, url);
							}
							*/

							var AudioRecordedIntent = new Intent(ActionAudioRecorded);
							{
								AudioRecordedIntent.PutExtra(ExtraAudioRecordedAbsolutePath, AudioRecordedPath);
							}
							SendBroadcast(AudioRecordedIntent);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler("BackgroundService", "HandleTimerCallback", ex);
			}
		}

		void StartRecording()
		{
			string path = "/sdcard/" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + "." + Utility.audio_file_format;
			AudioRecordedPath = path;
			Log.Debug(TAG, string.Format("Start recording audio file to path: {0}", path));
			mediaRecorder.SetAudioSource(AudioSource.Mic);
			mediaRecorder.SetOutputFormat(OutputFormat.Mpeg4);
			mediaRecorder.SetAudioEncoder(AudioEncoder.Aac);
			mediaRecorder.SetOutputFile(path);
			mediaRecorder.Prepare();
			mediaRecorder.Start();
		}

		void StopRecording()
		{
			Log.Debug(TAG, "Finish recording audio file to path");
			mediaRecorder.Stop();
			mediaRecorder.Reset();
		}
	}
}