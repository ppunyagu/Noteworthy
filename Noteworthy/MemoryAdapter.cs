using System;
using Android.Widget;
using System.Collections.Generic;
using Android.Views;
using Android.App;
using Android.Support.V7.Widget;
using Android.Content;

namespace Noteworthy
{
	public class MemoryAdapter : RecyclerView.Adapter
	{
		public List<Memory> _lstMemory;
		Activity _context;
		int viewTypeItem = 1;
		int viewTypeFooter = 2;
		private LayoutInflater _inflater;

		public MemoryAdapter(Activity context, List<Memory> lstMemory)
		{
			_lstMemory = lstMemory;
			_context = context;
			_inflater = context.LayoutInflater;
		}

		public void updateItems(List<Memory> lstMemory)
		{
			try
			{
				_lstMemory = lstMemory;
				NotifyDataSetChanged();
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler(Class.SimpleName + "Act: " + _context.Class.SimpleName, "updateItems", ex);
			}
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			try
			{

				if (holder is MemoryHolderItem)
				{
					var MemoryItem = _lstMemory[position];
					var viewHolder = holder as MemoryHolderItem;
					viewHolder.memImage.SetImageResource(Resource.Drawable.imgSad);
					viewHolder.memImage.Tag = position;
					viewHolder.rltMemoryImage.Tag = position;

					int paddingNormal = (int)Utility.ConvertDpToPixel(5, _context);
					if (position % 2 == 0)
					{
						viewHolder.rltMemoryImage.SetPadding(paddingNormal + (paddingNormal / 2), paddingNormal, 0, paddingNormal / 2);
					}
					else {
						viewHolder.rltMemoryImage.SetPadding(paddingNormal / 2, paddingNormal, paddingNormal + (paddingNormal / 2), paddingNormal / 2);
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler(Class.SimpleName + "Act: " + _context.Class.SimpleName, "OnBindViewHolder", ex);
			}
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			View view = null;
			RecyclerView.ViewHolder vh = null;
			try
			{
				view = _inflater.Inflate(Resource.Layout.RowMemoriesLayout, parent, false);
				vh = new MemoryHolderItem(_context, this, view);
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler(Class.SimpleName + "Act: " + _context.Class.SimpleName, "OnCreateViewHolder", ex);
			}
			return vh;
		}

		public override int ItemCount
		{
			get
			{
				if (_lstMemory != null)
				{
					return _lstMemory.Count;
				}
				else {
					return 0;
				}
			}
		}

		public override int GetItemViewType(int position)
		{
			return isfooter(position) ? viewTypeFooter : viewTypeItem;
		}

		public bool isfooter(int position)
		{
			return position == _lstMemory.Count;
		}
	}

	public class MemoryHolderItem : RecyclerView.ViewHolder
	{
		public ImageView memImage { get; set; }
		Activity _context;
		MemoryAdapter _adapter;
		public RelativeLayout rltMemoryImage;

		public MemoryHolderItem(Activity context, MemoryAdapter adapter, View view) : base(view)
		{
			try
			{
				_context = context;
				//_ProductItems = ProductItems;
				_adapter = adapter;
				memImage = view.FindViewById<ImageView>(Resource.Id.imgScreenShot);
				rltMemoryImage = view.FindViewById<RelativeLayout>(Resource.Id.rltScreenShotImage);
				memImage.Click += memImage_Click;
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler(Class.SimpleName + "Act: " + _context.Class.SimpleName, "SimiliarProductViewHolderItem", ex);
			}
		}

		void memImage_Click(object sender, EventArgs e)
		{
			try
			{

				ImageView img = (ImageView)sender;
				var SelctedProduct = _adapter._lstMemory[(int)img.Tag];
				/*
				Intent intent = new Intent(_context, typeof(ViewSimiliarProductActivity));
				intent.PutExtra("ViewSimiliarProduct", SelctedProduct.Screenshot_url);
				intent.PutExtra("isLoaded", "true");
				_context.StartActivityForResult(intent, 10);
				Helper.EnterAnim(_context);
				*/
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler(Class.SimpleName + "Act: " + _context.Class.SimpleName, "memImage_Click", ex);
			}
		}
	}
}

