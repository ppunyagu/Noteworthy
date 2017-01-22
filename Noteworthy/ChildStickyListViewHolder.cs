using XamDroid.ExpandableRecyclerView;
using Android.Widget;
using Android.Content;
using Android.Views;
using Android.Graphics;
using Android.App;

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
		public ChildStickyListViewHolder(Activity context, dynamic adapter, View itemView) : base(itemView)
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
				//LinearLayout lnrvw = (LinearLayout)sender;
				//var SelectedOrderTag = (OrderTag)lnrvw.Tag;
				if (adapter is MemoryAdapter)
				{
					/* #todo Implement inner memory activity
					var SelectedOrder = _item.Order; //SelectedOrderTag.objOrder;
					Intent intent = new Intent(_context, typeof(OrderActivity));
					intent.PutExtra("OrderStatus", SelectedOrder.OrderStatus.ToString());
					intent.PutExtra("OrderID", SelectedOrder.ID);
					context.StartActivityForResult(intent, 11);
					Helper.EnterAnim(context);
					*/
				}
			};
		}
	}
}

