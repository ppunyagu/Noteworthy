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
using Android.Speech;
using Android.Runtime;

using Java.Util;


namespace Noteworthy
{

	[Service(Exported = false)]
	public class BackgroundServiceNativeApproach : Service, IRecognitionListener
	{
		static readonly string TAG = "X:" + typeof(BackgroundService).Name;
		static readonly int TimerWait = 6000;
		System.Threading.Timer timer;
		DateTime startTime;
		Stopwatch stopWatch;
		bool isStarted = false;
		bool isRecording = false;
		string AudioRecordedPath;
		MediaRecorder mediaRecorder;
		SpeechRecognizer Recognizer { get; set; }
		AudioManager am;
		Intent SpeechIntent { get; set; }
		Handler handler;

		string SpeechRecognitionResult;

		public BluetoothGatt mBluetoothGatt;

		public bool isConnected = false;

		public const string ActionAudioRecorded = "ACTION_AUDIO_RECORDED";
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

			Recognizer = SpeechRecognizer.CreateSpeechRecognizer(this);
			Recognizer.SetRecognitionListener(this);

			am = (AudioManager)GetSystemService(Context.AudioService);

			SpeechRecognitionResult = "";

			SpeechIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
			SpeechIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
			SpeechIntent.PutExtra(RecognizerIntent.ExtraCallingPackage, PackageName);
			SpeechIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 5000);
			SpeechIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 5000);
			SpeechIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 20000);

			//SpeechRecognitionStart();

			BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;

			LEScanCallBack _scanCallBack;

			if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
			{
				_scanCallBack = new LEScanCallBack();
				_scanCallBack.OnDeviceFound += (sender, device) =>
				{
					if (!isConnected)
					{
						isConnected = true;
					}
					adapter.BluetoothLeScanner.StopScan(_scanCallBack);
					BluetoothLEGattCallback mGattCallback = new BluetoothLEGattCallback();
					mGattCallback.OnDeviceReadyWrite += OnDeviceReadWriteFunc;
					mGattCallback.dataReceivedFromDevice += actionDataReceivedFromDevice;
					mBluetoothGatt = device.ConnectGatt(this, true, mGattCallback);
				};
			}
			else {
				throw new Exception("Needs to be greated than API level 21.");
			}
			adapter.BluetoothLeScanner.StartScan(_scanCallBack);
		}

		public void OnDeviceReadWriteFunc(BluetoothGatt gatt)
		{
			try
			{
				byte[] bufferWrite = ASCIIEncoding.Default.GetBytes("master");
				BluetoothGattService RxService = gatt.GetService(RX_SERVICE_UUID);
				BluetoothGattCharacteristic RxChar = RxService.GetCharacteristic(RX_CHAR_UUID);
				RxChar.SetValue(bufferWrite);
				if (gatt.WriteCharacteristic(RxChar))
				{
					Log.Debug("OnDeviceReadyWrite", "Write Successfull!");
				}
				else {
					Log.Debug("OnDeviceReadyWrite", "Write Unsuccessful... :(");
				}
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler("BluetoothLEActivity", "OnDeviceReadyWrite", ex);
			}
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
			Log.Debug("actionDataReceivedFromDevice", string.Format("valFromDevice {0}", valFromDevice));
			if (Convert.ToInt32(valFromDevice) == 1)
			{
				if (!isRecording)
				{
					Log.Debug("HandleTimerCallback", "Pulse is stress and is not recording, so should begin recording");
					stopWatch.Start();
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
					stopWatch.Stop();
					Log.Debug("HandleTimerCallback", string.Format("Recording was {0} seconds", (stopWatch.ElapsedMilliseconds / 1000)));
					StopRecording();
					isRecording = false;

					var AudioRecordedIntent = new Intent(ActionAudioRecorded);
					{
						AudioRecordedIntent.PutExtra(ExtraAudioRecordedAbsolutePath, AudioRecordedPath);
						AudioRecordedIntent.PutExtra(ExtraAudioRecordedDurations, (stopWatch.ElapsedMilliseconds / 1000).ToString());
					}
					stopWatch.Reset();
					SendBroadcast(AudioRecordedIntent);
				}
			}
		}

		void HandleTimerCallback(object state)
		{
			try
			{
				if (!isConnected)
				{
					Log.Debug("HandleTimerCallback", "Has not connected to wearable device");
				}
				else {
					OnDeviceReadWriteFunc(mBluetoothGatt);
				}
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler("BackgroundService", "HandleTimerCallback", ex);
			}
		}

		void StartRecording()
		{
			// Start Speech Recognition
			SpeechRecognitionStart();

			string path = "/sdcard/" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + "." + Utility.audio_file_format;
			AudioRecordedPath = path;
			Log.Debug(TAG, string.Format("Start recording audio file to path: {0}", path));
			mediaRecorder.SetAudioSource(AudioSource.Camcorder);
			mediaRecorder.SetOutputFormat(OutputFormat.Mpeg4);
			mediaRecorder.SetAudioEncoder(AudioEncoder.Aac);
			mediaRecorder.SetOutputFile(path);
			mediaRecorder.Prepare();
			mediaRecorder.Start();
		}

		void StopRecording()
		{
			// Stop Speech Recognition
			SpeechRecognitionStop();

			Log.Debug(TAG, "Finish recording audio file to path");
			mediaRecorder.Stop();
			mediaRecorder.Reset();
		}

		void SpeechRecognitionStart()
		{
			am.SetStreamMute(Stream.System, true);
			handler.Post(() =>
			{
				Log.Debug("SpeechRecognitionStart", "StartListening");
				Recognizer.StartListening(SpeechIntent);
			});
		}

		void SpeechRecognitionStop()
		{
			am.SetStreamMute(Stream.System, false);
			handler.Post(() =>
			{
				Recognizer.StopListening();
			});
		}

		public void OnResults(Bundle results)
		{
			var matches = results.GetStringArrayList(SpeechRecognizer.ResultsRecognition);
			if (matches != null && matches.Count > 0)
			{
				SpeechRecognitionResult = SpeechRecognitionResult + matches[0] + " ";
			}
			Log.Debug("OnResults", SpeechRecognitionResult);
			handler.Post(() =>
			{
				Recognizer.StartListening(SpeechIntent);
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
			if (error == SpeechRecognizerError.SpeechTimeout)
			{
				Log.Debug("isSpeechTimeout", "No conversation: Should stop service");
				Recognizer.StopListening();
			}
		}

		public void OnBufferReceived(byte[] buffer) { }

		public void OnEvent(int eventType, Bundle @params) { }

		public void OnPartialResults(Bundle partialResults) { }

		public void OnRmsChanged(float rmsdB) { }
	}
}