//Please note that the application must have RECORD_AUDIO permission to use this class. 

using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Widget;
using Android.OS;
using Android.Speech;
using Android.Util;

namespace Noteworthy
{
	[Activity(Label = "Noteworthy", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : Activity, IRecognitionListener
	{
		public const string Tag = "VoiceRec";

		SpeechRecognizer Recognizer { get; set; }
		Intent SpeechIntent { get; set; }
		TextView Label { get; set; }

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			SetContentView(Resource.Layout.MainRecording);

			// Set this on if want Audio Background: 
			Utility.server_heartRate = "http://157.252.187.36:5000";
			NoteworthyApplication.StartBackgroundService();

			//DataBase Initalize
			Utility.InitializeDatabase();

			Recognizer = SpeechRecognizer.CreateSpeechRecognizer(this);
			Recognizer.SetRecognitionListener(this);

			SpeechIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
			SpeechIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
			SpeechIntent.PutExtra(RecognizerIntent.ExtraCallingPackage, PackageName);
			SpeechIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 5000);
			SpeechIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 5000);
			SpeechIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 20000);

			// Set Up
			/*
			SpeechIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 1500);
			SpeechIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 1500);
			SpeechIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 15000);
			SpeechIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);
			SpeechIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
			*/

			var button = FindViewById<Button>(Resource.Id.btnRecord);
			var buttonStop = FindViewById<Button>(Resource.Id.btnStopRecord);
			button.Click += ButtonStartRecording;
			buttonStop.Click += ButtonStopRecording;

			Label = FindViewById<TextView>(Resource.Id.textYourText);


			Intent intent = new Intent(this, typeof(MainMemoryActivity));
			intent.SetFlags(ActivityFlags.NewTask);
			StartActivity(intent);

		}

		private void ButtonStartRecording(object sender, EventArgs e)
		{
			Recognizer.StartListening(SpeechIntent);
		}

		private void ButtonStopRecording(object sender, EventArgs e)
		{
			Recognizer.StopListening();
		}

		public void OnResults(Bundle results)
		{
			var matches = results.GetStringArrayList(SpeechRecognizer.ResultsRecognition);
			if (matches != null && matches.Count > 0)
			{
				Label.Text = matches[0];
			}
		}

		public void OnReadyForSpeech(Bundle @params)
		{
			Log.Debug(Tag, "OnReadyForSpeech");
		}

		public void OnBeginningOfSpeech()
		{
			Log.Debug(Tag, "OnBeginningOfSpeech");
		}

		public void OnEndOfSpeech()
		{
			Log.Debug(Tag, "OnEndOfSpeech");
		}

		public void OnError([GeneratedEnum] SpeechRecognizerError error)
		{
			Log.Debug("OnError", error.ToString());
		}

		public void OnBufferReceived(byte[] buffer) { }

		public void OnEvent(int eventType, Bundle @params) { }

		public void OnPartialResults(Bundle partialResults) { }

		public void OnRmsChanged(float rmsdB) { }
	}
}