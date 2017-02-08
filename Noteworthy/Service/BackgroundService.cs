﻿using System;
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

using Java.Util;


namespace Noteworthy
{

	[Service (Exported = false)]
	public class BackgroundService : Service
	{
		static readonly string TAG = "X:" + typeof(BackgroundService).Name;
		Stopwatch stopWatch;
		Stopwatch stopWatchForStressTrigger;
		bool isRecording = false;
		string AudioRecordedPath;
		MediaRecorder mediaRecorder;

		Queue<int> heartRates = new Queue<int>();

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
			mediaRecorder = new MediaRecorder();

			stopWatch = new Stopwatch();

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
			return StartCommandResult.NotSticky;
		}

		public override IBinder OnBind(Intent intent)
		{
			// This is a started service, not a bound service, so we just return null.
			return null;
		}


		public override void OnDestroy()
		{
			base.OnDestroy();
		}

		void actionDataReceivedFromDevice(string valFromDevice)
		{
			Log.Debug("actionDataReceivedFromDevice", string.Format("valFromDevice {0}", valFromDevice));
			int latestHeartRate = Int32.Parse(valFromDevice);
			heartRates.Enqueue(latestHeartRate);
			if (heartRates.Count == 50)
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
				if (latestHeartRate > (average + standardDeviation) || latestHeartRate < (average - standardDeviation))
				{
					Log.Debug("actionDataReceivedFromDevice", "isStress should start recording!");
					if (!isRecording)
					{
						Log.Debug("actionDataReceivedFromDevice", "Pulse is stress and is not recording, so should begin recording");
						stopWatch.Start();
						stopWatchForStressTrigger.Start();
						StartRecording();
						isRecording = true;
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
							stopWatch.Reset();
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