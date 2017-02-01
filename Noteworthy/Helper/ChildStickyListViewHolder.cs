using XamDroid.ExpandableRecyclerView;
using Android.Widget;
using Android.Content;
using Android.Views;
using Android.Graphics;
using Android.App;
using Android.Util;
using Android.Media;

namespace Noteworthy
{
	public class ChildStickyListViewHolder : ChildViewHolder
	{

		Context _context;
		public ImageView imgPendingProfilePic;
		public TextView txtPendingUsername;
		public TextView txtPendingOrderStutas;
		public TextView txtPendigTime;
		public LinearLayout lnrOrderPending;
		public LinearLayout lnrRow;
		public Item _item;

		public ChildStickyListViewHolder(Activity context, MemoryAdapter adapter, View itemView) : base(itemView)
		{
			_context = context;

			txtPendingUsername = itemView.FindViewById<TextView>(Resource.Id.txtOrderPendingProfileName);
			txtPendingOrderStutas = itemView.FindViewById<TextView>(Resource.Id.txtOrderPendingstatus);
			txtPendigTime = itemView.FindViewById<TextView>(Resource.Id.txtOrderPendingtime);
			imgPendingProfilePic = itemView.FindViewById<ImageView>(Resource.Id.imgOrderPendingProfilePic);
			lnrOrderPending = itemView.FindViewById<LinearLayout>(Resource.Id.lnrOrderPending);
			lnrRow = itemView.FindViewById<LinearLayout>(Resource.Id.lnrRow);

			txtPendingUsername.SetTypeface(FontFactory.GetFontOpenSansBold(_context), TypefaceStyle.Normal);
			txtPendigTime.SetTypeface(FontFactory.GetFontOpenSansRegular(_context), TypefaceStyle.Normal);

			lnrRow.Click += (sender, e) =>
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
					}
					else {
						if (adapter.mp.IsPlaying)
						{
							adapter.mp.Stop();
							adapter.mp.Reset();
							adapter.mp = null;

							if (adapter.CurrentTrack != _item.memory.Audio_path)
							{
								Log.Debug("ChildStickyListViewHolder", string.Format("Playing audio from path: {0}", _item.memory.Audio_path));
								adapter.mp = new MediaPlayer();
								adapter.mp.SetDataSource(_item.memory.Audio_path);
								adapter.CurrentTrack = _item.memory.Audio_path;
								adapter.mp.Prepare();
								adapter.mp.Start();
							}
						}
						else {
							Log.Debug("ChildStickyListViewHolder", string.Format("Playing audio from path: {0}", _item.memory.Audio_path));
							adapter.mp = new MediaPlayer();
							adapter.mp.SetDataSource(_item.memory.Audio_path);
							adapter.CurrentTrack = _item.memory.Audio_path;
							adapter.mp.Prepare();
							adapter.mp.Start();
						}
					}
				}
			};
		}
	}
}

