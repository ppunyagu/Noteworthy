﻿using System;
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
	[IntentFilter(new[] { BackgroundService.ActionCheckNotifyUser })]
	public class CheckNotifyActionReceiver : BroadcastReceiver
	{
		public override void OnReceive(Context context, Intent intent)
		{
			try
			{
				List<Memory> _lstMemories = SQLClient<Memory>.Instance.GetAll().ToList();
				if (_lstMemories.Count > 0)
				{
					// This is slow, should instead keep unqueried memory and only do those
					Log.Debug("CheckNotifyActionReceiver", "Checking for new conversation completion");
					foreach (var mem in _lstMemories)
					{
						if (mem.ConversationText == "<notloaded>")
						{
							using (var objTranslationService = new TranslationService())
							{
								string translationText = objTranslationService.GetTextFromJobId(mem.JobId);
								if (translationText != "")
								{
									mem.ConversationText = translationText;
									NoteworthyApplication.NotifyMemorized(mem.StressStarterSentence);
									Log.Debug("CheckNotifyActionReceiver", "Notifying Memory stored and transcribed!");
									SQLClient<Memory>.Instance.InsertOrReplace(mem);
								}
								/*
								else {
									mem.ConversationText = "<transcribing>";
								}
								SQLClient<Memory>.Instance.InsertOrReplace(mem);
								*/
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler("CheckNotifyActionReceiver", "OnReceive", ex);
			}
		}
	}
}
