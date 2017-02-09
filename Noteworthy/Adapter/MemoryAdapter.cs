using System;
using Android.Support.V7.Widget;
using System.Collections.Generic;
using Android.Views;
using Android.Widget;
using Android.App;
using Android.Media;

namespace Noteworthy
{
	public class MemoryAdapter : RecyclerView.Adapter
	{
		//1 for child and 0 for header , viewtype
		public List<Item> data;
		Activity _context;
		public MediaPlayer mp;
		public Button currentPlayButton;
		public string CurrentTrack;

		public MemoryAdapter(Activity context, List<Item> data)
		{
			this.data = data;
			_context = context;
		}

		#region implemented abstract members of Adapter

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			try
			{
				Item item = data[position];
				var res = Application.Context.Resources;

				switch (item.type)
				{
					case 0:
						MemoryDateViewHolder itemController = (MemoryDateViewHolder)holder;
						itemController.refferalItem = item;
						//itemController.header_title.Text = (item.text);
						if (Convert.ToInt32(item.text) == DateTime.Now.DayOfYear)
						{
							itemController.header_title.Text = "Today";
						}
						else if (Convert.ToInt32(item.text) == (DateTime.Now.DayOfYear - 1))
						{
							itemController.header_title.Text = "Yesterday";
						}
						else {
							DateTime theDate = new DateTime(DateTime.Now.Year, 1, 1).AddDays(Convert.ToInt32(item.text) - 1);
							itemController.header_title.Text = theDate.ToString("dd-M");;
						}
						itemController.header_title.SetTypeface(Android.Graphics.Typeface.DefaultBold, Android.Graphics.TypefaceStyle.Bold);
						itemController.header_title.SetTextColor(res.GetColor(Resource.Color.black));
						itemController.header_title.Clickable = false;
						break;
					case 1:
						ChildStickyListViewHolder objChildHolder = (ChildStickyListViewHolder)holder;
						objChildHolder._item = item;
						var childOrderItem = (data[position]).memory;
						//string orderStatus = childOrderItem.Audio_path.ToString();
						string[] convo = childOrderItem.ConversationText.Split(Environment.NewLine.ToCharArray());
						string convoText;
						if (convo.Length >= 2)
						{
							convoText = convo[1];
						}
						else {
							convoText = "<unrecognizable>";
						}
						objChildHolder.txtPendingUsername.Text = convoText;
						objChildHolder.txtPendigTime.Text = childOrderItem.Time.GetValueOrDefault().ToString("hh:mm tt");
						//objChildHolder.txtPendingOrderStutas.Text = "<Speech to Text will go here>";
						objChildHolder.txtPendingOrderStutas.Text = string.Format("Seconds: {0}", childOrderItem.Duration.ToString());
						//objChildHolder.txtPendingUsername.Text = "<What's this?>";
						/* #todo Circle image with Text for seconds
						if (childOrderItem.user != null && !string.IsNullOrEmpty(childOrderItem.user.resolved.ProfilePhoto))
							Helper.SetRemoteImage(objChildHolder.imgPendingProfilePic, childOrderItem.user.resolved.ProfilePhoto, Resource.Color.reviewBackground);
						*/
						/*
						if (childOrderItem.shop.resolved != null && !string.IsNullOrEmpty(childOrderItem.shop.resolved.UserName))
							objChildHolder.txtPendingUsername.Text = childOrderItem.user.resolved.UserName.ToLower();
						*/
						break;
					default:
						break;
				}
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler(Class.SimpleName, "OnBindViewHolder", ex);
			}
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			try
			{
				View view = null;
				switch (viewType)
				{
					case 0:
						view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.RowSimpleTextLayout, parent, false);
						MemoryDateViewHolder header = new MemoryDateViewHolder(view, this);
						return header;
					case 1:
						view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.RowMemoriesLayout, parent, false);
						ChildStickyListViewHolder childHolder = new ChildStickyListViewHolder(_context, this, view);
						return childHolder;
				}
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler(Class.SimpleName, "OnCreateViewHolder", ex);
			}
			return null;
		}

		public override int ItemCount
		{
			get
			{
				return data.Count;
			}
		}

		public override int GetItemViewType(int position)
		{
			return data[position].type;
		}

		#endregion

		public class MemoryDateViewHolder : RecyclerView.ViewHolder
		{
			public TextView header_title { get; set; }
			public RelativeLayout rltExpaned { get; set; }
			public LinearLayout youp { get; set; }
			public Item refferalItem;

			public MemoryDateViewHolder(View itemView, MemoryAdapter adapter) : base(itemView)
			{
				header_title = (TextView)itemView.FindViewById(Resource.Id.txtStickyHeader);
				rltExpaned = (RelativeLayout)itemView.FindViewById(Resource.Id.rltExpaned);
				youp = (LinearLayout)itemView.FindViewById(Resource.Id.youp);
				rltExpaned.Click += (sender, e) =>
				{
					try
					{
						return;
						/* #todo Segment by date
						if (refferalItem.text == OrderStatus.WaitingForPaymentApproval.ToString())
						{
							return;
						}
						var data = adapter.data;
						int pos = data.IndexOf(refferalItem);
						if (refferalItem.invisibleChildren == null)
						{
							refferalItem.invisibleChildren = new List<Item>();
							int count = 0;
							while (data.Count > pos + 1 && data[pos + 1].type == 1)
							{
								var invisibleItem = new Item();
								invisibleItem = data[pos + 1];
								data.RemoveAt(pos + 1);
								refferalItem.invisibleChildren.Add(invisibleItem);
								count++;
							}
							adapter.NotifyItemRangeRemoved(pos + 1, count);
						}
						else {
							int index = pos + 1;
							foreach (var i in refferalItem.invisibleChildren)
							{
								data.Insert(index, i);
								index++;
							}
							adapter.NotifyItemRangeInserted(pos + 1, index - pos - 1);
							refferalItem.invisibleChildren = null;

						}
						*/
					}
					catch (Exception ex)
					{
						Utility.ExceptionHandler(Class.SimpleName, "rltExpaned _Click", ex);
					}
				};
			}
		}
	}

	public class Item
	{
		public int type;
		public String text;
		public List<Item> invisibleChildren;
		public Memory memory;
	}
}

