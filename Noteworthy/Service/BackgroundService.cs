using System;
using System.Threading;
using System.Diagnostics;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Media;
using Android.Bluetooth;
using System.Collections.Generic;
using Android.Speech;
using Android.Runtime;
using Java.Util;


namespace Noteworthy
{

	[Service (Exported = false)]
	public class BackgroundService : Service, IRecognitionListener 
	{
		static readonly string TAG = "X:" + typeof(BackgroundService).Name;
		static readonly int TimerWait = 10000;
		System.Threading.Timer timer;
		DateTime startTime;
		Stopwatch stopWatch;
		Stopwatch stopWatchForStressTrigger;
		bool isRecording = false;
		bool isStarted = false;
		string AudioRecordedPath;
		MediaRecorder mediaRecorder;

		Queue<int> heartRates = new Queue<int>();

		public BluetoothGatt mBluetoothGatt;

		public bool isConnected = false;

		SpeechRecognizer Recognizer { get; set; }
		AudioManager am;
		Intent SpeechIntent { get; set; }
		Handler handler;
		public bool isTranslating = false;

		public const string ActionAudioRecorded = "ACTION_AUDIO_RECORDED";
		public const string ActionCheckNotifyUser = "ACTION_NOTIFY_USER";
		public const string ExtraAudioRecordedAbsolutePath = "EXTRA_AUDIORECORDED_ABSOLUTE_PATH";
		public const string ExtraAudioRecordedDurations = "EXTRA_AUDIORECORDED_DURATIONS";

		public UUID RX_SERVICE_UUID = UUID.FromString("6e400001-b5a3-f393-e0a9-e50e24dcca9e");
		public UUID RX_CHAR_UUID = UUID.FromString("6e400002-b5a3-f393-e0a9-e50e24dcca9e");
		public UUID TX_CHAR_UUID = UUID.FromString("6e400003-b5a3-f393-e0a9-e50e24dcca9e");
		public UUID CCCD = UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");

		public override void OnCreate()
		{

			handler = new Handler();

			mediaRecorder = new MediaRecorder();

			stopWatch = new Stopwatch();

			am = (AudioManager)GetSystemService(Context.AudioService);

			stopWatchForStressTrigger = new Stopwatch();

			BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;

			LEScanCallBack _scanCallBack;

			if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
			{
				_scanCallBack = new LEScanCallBack();
				_scanCallBack.OnDeviceFound += (sender, device) =>
				{
					adapter.BluetoothLeScanner.StopScan(_scanCallBack);
					BluetoothLEGattCallback mGattCallback = new BluetoothLEGattCallback();
					mGattCallback.dataReceivedFromDevice += actionDataReceivedFromDevice;
					mBluetoothGatt = device.ConnectGatt(this, true, mGattCallback);
				};
			}
			else {
				throw new Exception("Needs to be greated than API level 21.");
			}
			adapter.BluetoothLeScanner.StartScan(_scanCallBack);
		}

		public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
		{
			if (!isStarted)
			{
				startTime = DateTime.UtcNow;
				Log.Debug(TAG, $"Starting the service, at {startTime}.");
				timer = new System.Threading.Timer(HandleTimerCallback, startTime, 0, TimerWait);
				isStarted = true;
			}
			return StartCommandResult.NotSticky;
		}

		public override IBinder OnBind(Intent intent)
		{
			// This is a started service, not a bound service, so we just return null.
			return null;
		}

		void HandleTimerCallback(object state)
		{
			try
			{
				var ActionCheckNotifyUserIntent = new Intent(ActionCheckNotifyUser);
				SendBroadcast(ActionCheckNotifyUserIntent);
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler("BackgroundService", "HandleTimerCallback", ex);
			}
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

		void actionDataReceivedFromDevice(string valFromDevice)
		{
			try
			{
				Log.Debug("actionDataReceivedFromDevice", string.Format("valFromDevice {0}", valFromDevice));
				int latestHeartRate = Int32.Parse(valFromDevice);
				heartRates.Enqueue(latestHeartRate);
				if (heartRates.Count >= 50)
				{
					int sum = 0;
					double sumOfDerivation = 0;
					foreach (int heartRate in heartRates)
					{
						sum += heartRate;
						sumOfDerivation += (heartRate) * (heartRate);
					}
					double average = sum / heartRates.Count;
					Log.Debug("getStandardDeviation", string.Format("Average: {0}", average));
					double sumOfDerivationAverage = sumOfDerivation / (heartRates.Count - 1);
					double standardDeviation = Math.Sqrt(sumOfDerivationAverage - (average * average));
					Log.Debug("actionDataReceivedFromDevice", string.Format("StandardDeviation {0}", standardDeviation));

					// Logic for isStress conditional statement => Background Recording
					if (latestHeartRate > (average + standardDeviation) || latestHeartRate < (average - standardDeviation))
					{
						Log.Debug("actionDataReceivedFromDevice", "isStress should start recording!");
						if (!isRecording)
						{
							Log.Debug("actionDataReceivedFromDevice", "Pulse is stress and is not recording, so should begin testing if speaking");
							SpeechRecognitionStart();
							/*
							stopWatch.Start();
							stopWatchForStressTrigger.Start();
							StartRecording();
							isRecording = true;
							*/
						}
						else {
							Log.Debug("actionDataReceivedFromDevice", "Pulse is stress but is already recording, so should continue recording");
						}
					}
					else {
						if (!stopWatchForStressTrigger.IsRunning)
						{
							if (!isRecording)
							{
								Log.Debug("actionDataReceivedFromDevice", "Pulse is not stress and is not recording, so should continue idle");
							}
							else {
								Log.Debug("actionDataReceivedFromDevice", "Pulse is not stress and is recording, so should stop recording and broadcast event");
								stopWatch.Stop();
								Log.Debug("actionDataReceivedFromDevice", string.Format("Recording was {0} seconds", (stopWatch.ElapsedMilliseconds / 1000)));
								isRecording = false;
								StopRecording();

								var AudioRecordedIntent = new Intent(ActionAudioRecorded);
								{
									AudioRecordedIntent.PutExtra(ExtraAudioRecordedAbsolutePath, AudioRecordedPath);
									AudioRecordedIntent.PutExtra(ExtraAudioRecordedDurations, (stopWatch.ElapsedMilliseconds / 1000).ToString());
								}
								stopWatch.Reset();
								SendBroadcast(AudioRecordedIntent);
							}
						}
						else {
							Log.Debug("actionDataReceivedFromDevice", "Stopwatch Stress trigger is running despite not stress should continue recording!");
							Log.Debug("actionDataReceivedFromDevice", "isStress should start recording!");
							if (stopWatchForStressTrigger.ElapsedMilliseconds > 10000)
							{
								Log.Debug("actionDataReceivedFromDevice", "Stopwatch Stress trigger should stop");
								stopWatchForStressTrigger.Stop();
								stopWatchForStressTrigger.Reset();
							}
						}
					}

					heartRates.Dequeue();
				}
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler("BackgroundService", "actionDataReceivedFromDevice", ex);
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

		void SpeechRecognitionStart()
		{
			if (!isTranslating)
			{
				handler.Post(() =>
				{
					am.SetStreamMute(Stream.System, true);
					am.SetStreamMute(Stream.Alarm, true);
					am.SetStreamMute(Stream.Notification, true);
					am.SetStreamMute(Stream.Music, true);
					am.SetStreamMute(Stream.Ring, true);

					Recognizer = SpeechRecognizer.CreateSpeechRecognizer(this);
					Recognizer.SetRecognitionListener(this);

					SpeechIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
					SpeechIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
					SpeechIntent.PutExtra(RecognizerIntent.ExtraCallingPackage, PackageName);
					SpeechIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 5000);
					SpeechIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 5000);
					SpeechIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 20000);
					Log.Debug("SpeechRecognitionStart", "StartListening");
					Recognizer.StartListening(SpeechIntent);
				});
				isTranslating = true;
			}
		}

		void SpeechRecognitionContinue()
		{
			handler.Post(() =>
			{
				Log.Debug("SpeechRecognitionContinue", "ContinueListening");
				Recognizer.StartListening(SpeechIntent);
			});
		}

		void SpeechRecognitionStop()
		{
			if (isTranslating)
			{
				handler.Post(() =>
				{
					/* Unmute audio
					am.SetStreamMute(Stream.System, false);
					am.SetStreamMute(Stream.Alarm, false);
					am.SetStreamMute(Stream.Notification, false);
					am.SetStreamMute(Stream.Music, false);
					am.SetStreamMute(Stream.Ring, false);
					*/

					Recognizer.StopListening();
					Recognizer.Cancel();
					Recognizer.Destroy();
				});
				isTranslating = false;
			}
		}

		public void OnResults(Bundle results)
		{
			handler.Post(() =>
			{
				var matches = results.GetStringArrayList(SpeechRecognizer.ResultsRecognition);
				if (matches != null && matches.Count > 0)
				{
					Log.Debug("OnResults", matches[0]);
					SpeechRecognitionStop();
					Log.Debug("OnResults", "Pulse is stress, is not recording and is speaking! should start recording");
					stopWatch.Start();
					stopWatchForStressTrigger.Start();
					StartRecording();
					isRecording = true;
				}
				else {
					SpeechRecognitionStop();
				}
			});
		}

		public void OnReadyForSpeech(Bundle @params)
		{
			Log.Debug(TAG, "OnReadyForSpeech");
		}

		public void OnBeginningOfSpeech()
		{
			Log.Debug(TAG, "OnBeginningOfSpeech");
		}

		public void OnEndOfSpeech()
		{
			Log.Debug(TAG, "OnEndOfSpeech");
		}

		public void OnError([GeneratedEnum] SpeechRecognizerError error)
		{
			Log.Debug("SpeechReconizer: OnError", error.ToString());
			SpeechRecognitionStop();
		}

		public void OnBufferReceived(byte[] buffer) { }

		public void OnEvent(int eventType, Bundle @params) { }

		public void OnPartialResults(Bundle partialResults) { }

		public void OnRmsChanged(float rmsdB) { }
	}
}