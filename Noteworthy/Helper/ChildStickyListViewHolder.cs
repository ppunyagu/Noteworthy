﻿using XamDroid.ExpandableRecyclerView;
using Android.Widget;
using Android.Content;
using Android.Views;
using Android.Graphics;
using Android.App;
using Android.Util;
using Android.Media;
using System;

namespace Noteworthy
{
	public class ChildStickyListViewHolder : ChildViewHolder
	{

		Context _context;
		public Button btnPlayOrStop;
		public TextView txtPendingUsername;
		public TextView txtPendingOrderStutas;
		public TextView txtPendigTime;
		public LinearLayout lnrOrderPending;
		public LinearLayout lnrRow;
		public Item _item;

		public ChildStickyListViewHolder(Activity context, MemoryAdapter adapter, View itemView) : base(itemView)
		{
			try
			{
				_context = context;

				txtPendingUsername = itemView.FindViewById<TextView>(Resource.Id.txtOrderPendingProfileName);
				txtPendingOrderStutas = itemView.FindViewById<TextView>(Resource.Id.txtOrderPendingstatus);
				txtPendigTime = itemView.FindViewById<TextView>(Resource.Id.txtOrderPendingtime);
				btnPlayOrStop = itemView.FindViewById<Button>(Resource.Id.btnPlayOrStop);
				lnrOrderPending = itemView.FindViewById<LinearLayout>(Resource.Id.lnrOrderPending);
				lnrRow = itemView.FindViewById<LinearLayout>(Resource.Id.lnrRow);

				txtPendingUsername.SetTypeface(FontFactory.GetFontOpenSansBold(_context), TypefaceStyle.Normal);
				txtPendigTime.SetTypeface(FontFactory.GetFontOpenSansRegular(_context), TypefaceStyle.Normal);

				lnrRow.Click += (sender, e) =>
				{
					//AlertDialog.Builder alertDialog = new AlertDialog.Builder(_context, Resource.Style.Theme_AppCompat);
					AlertDialog.Builder alertDialog = new AlertDialog.Builder(_context, Resource.Style.CustomDialog);
					//alertDialog.SetInverseBackgroundForced(false);
					alertDialog.SetCancelable(false);

					string message = _item.memory.ConversationText;
					alertDialog.SetMessage(message);
					alertDialog.SetPositiveButton(
						"OK",
						delegate
						{
						}
					);
					/*
					alertDialog.SetNegativeButton(
						Resources.GetString(Resource.String.Cancel),
						delegate
						{
							Helper.logger.LogEvent("SnapScreenshot Decline ImageSearch");
							OnBackPressed();
						}
					);
					*/
					AlertDialog alert = alertDialog.Create();
					alert.RequestWindowFeature((int)WindowFeatures.NoTitle);
					alert.Show();
				};

				btnPlayOrStop.Click += (sender, e) =>
				{
					if (adapter is MemoryAdapter)
					{
						if (adapter.mp == null)
						{
							Log.Debug("ChildStickyListViewHolder", string.Format("Playing audio from path: {0}", _item.memory.Audio_path));
							adapter.mp = new MediaPlayer();
							adapter.mp.SetDataSource(_item.memory.Audio_path);
							adapter.CurrentTrack = _item.memory.Audio_path;
							adapter.mp.Prepare();
							adapter.mp.Start();
							btnPlayOrStop.SetBackgroundResource(Resource.Drawable.stop);
							adapter.currentPlayButton = btnPlayOrStop;
						}
						else {
							if (adapter.mp.IsPlaying)
							{
								adapter.mp.Stop();
								adapter.mp.Reset();
								adapter.mp = null;

								if (adapter.CurrentTrack != _item.memory.Audio_path)        // Stop current and play selected
								{
									Log.Debug("ChildStickyListViewHolder", string.Format("Playing audio from path: {0}", _item.memory.Audio_path));
									adapter.mp = new MediaPlayer();
									adapter.mp.SetDataSource(_item.memory.Audio_path);
									adapter.CurrentTrack = _item.memory.Audio_path;
									adapter.mp.Prepare();
									adapter.mp.Start();
									adapter.currentPlayButton.SetBackgroundResource(Resource.Drawable.play);
									btnPlayOrStop.SetBackgroundResource(Resource.Drawable.stop);
									adapter.currentPlayButton = btnPlayOrStop;
								}
								else {
									adapter.currentPlayButton.SetBackgroundResource(Resource.Drawable.play);
									btnPlayOrStop.SetBackgroundResource(Resource.Drawable.play);
								}
							}
							else {
								Log.Debug("ChildStickyListViewHolder", string.Format("Playing audio from path: {0}", _item.memory.Audio_path));
								adapter.mp = new MediaPlayer();
								adapter.mp.SetDataSource(_item.memory.Audio_path);
								adapter.CurrentTrack = _item.memory.Audio_path;
								adapter.mp.Prepare();
								adapter.mp.Start();
								adapter.currentPlayButton.SetBackgroundResource(Resource.Drawable.play);
								btnPlayOrStop.SetBackgroundResource(Resource.Drawable.stop);
								adapter.currentPlayButton = btnPlayOrStop;
							}
						}
					}
				};
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler("ChildStickyListViewHolder", "Class", ex);
			}
		}
	}
}

