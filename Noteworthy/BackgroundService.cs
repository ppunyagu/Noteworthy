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
		static readonly int TimerWait = 4000;
		Timer timer;
		DateTime startTime;
		bool isStarted = false;
		int TimeSpanCount = 0;
		MediaRecorder mediaRecorder;

		public override void OnCreate()
		{
			mediaRecorder = new MediaRecorder();
		}

		public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
		{
			Log.Debug(TAG, $"OnStartCommand called at {startTime}, flags={flags}, startid={startId}");
			if (isStarted)
			{
				TimeSpan runtime = DateTime.UtcNow.Subtract(startTime);
				Log.Debug(TAG, $"This service was already started, it's been running for {runtime:c}.");
			}
			else
			{
				startTime = DateTime.UtcNow;
				Log.Debug(TAG, $"Starting the service, at {startTime}.");
				StartRecording();
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

		void HandleTimerCallback(object state)
		{
			if (TimeSpanCount == 4)
			{
				//TimeSpanCount = -1;
				StopRecording();
			}
			TimeSpanCount++;
			Log.Debug(TAG, String.Format("TimeSpanCount: {0}", TimeSpanCount));
			TimeSpan runTime = DateTime.UtcNow.Subtract(startTime);
			Log.Debug(TAG, $"This service has been running for {runTime:c} (since ${state}).");
		}

		void StartRecording()
		{
			string path = "/sdcard/" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".3gpp";
			Log.Debug(TAG, string.Format("Start recording audio file to path: {0}", path));
			mediaRecorder.SetAudioSource(AudioSource.Mic);
			mediaRecorder.SetOutputFormat(OutputFormat.ThreeGpp);
			mediaRecorder.SetAudioEncoder(AudioEncoder.Default);
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