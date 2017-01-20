using Android.App;
using Android.Widget;
using Android.OS;
using Android.Media;
using System;

namespace Noteworthy
{
	[Activity(Label = "Noteworthy", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : Activity
	{
		MediaRecorder _recorder;
		MediaPlayer _player;
		string path = "/sdcard/test.3gpp";
		bool IsRecording;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			try
			{
				base.OnCreate(savedInstanceState);
				SetContentView(Resource.Layout.Main);

				NoteworthyApplication.StartBackgroundService();

				//DataBase Initalize
				Utility.InitializeDatabase();

				// Get our button from the layout resource,
				// and attach an event to it
				Button button = FindViewById<Button>(Resource.Id.myButton);
				button.Text = "Record";
				IsRecording = false;

				button.Click += delegate
				{
					if (!IsRecording)
					{
						button.Text = "Record";
						IsRecording = true;
						_recorder.SetAudioSource(AudioSource.Mic);
						_recorder.SetOutputFormat(OutputFormat.ThreeGpp);
						_recorder.SetAudioEncoder(AudioEncoder.Default);
						_recorder.SetOutputFile(path);
						_recorder.Prepare();
						_recorder.Start();
					}
					else {
						button.Text = "Stop Recording";
						IsRecording = false;
						_recorder.Stop();
						_recorder.Reset();
						/*
						_player.SetDataSource(path);
						_player.Prepare();
						_player.Start();
						*/
					}
				};


			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler(Class.SimpleName, "OnCreate", ex);
			}
		}

		protected override void OnResume()
		{
			try
			{
				base.OnResume();

				_recorder = new MediaRecorder();
				_player = new MediaPlayer();
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler(Class.SimpleName, "OnResume", ex);
			}
		}

		protected override void OnPause()
		{
			try
			{
				base.OnPause();

				_player.Release();
				_recorder.Release();
				_player.Dispose();
				_recorder.Dispose();
				_player = null;
				_recorder = null;
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler(Class.SimpleName, "OnPause", ex);
			}
		}
	}
}

