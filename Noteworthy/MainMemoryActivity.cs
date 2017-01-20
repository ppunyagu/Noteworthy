using Android.App;
using Android.Widget;
using Android.OS;
using Android.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using Android.Views;
using Android.Content.PM;
using Android.Support.V7.Widget;
using Android.Support.V7.App;
using Android.Support.V4.Widget;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Graphics;
using Android.Content;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Noteworthy
{
	[Activity(Label = "Noteworthy", MainLauncher = true, Icon = "@mipmap/icon", ScreenOrientation = ScreenOrientation.SensorPortrait)]
	public class MainMemoryActivity : ActionBarActivity
	{
		Toolbar toolBarHeader;
		RecyclerView recyclerListView;
		SwipeRefreshLayout refresher;
		LinearLayout lnrEmptyView;
		LinearLayout lnrLoad;
		List<Memory> _lstMemories;
		MemoryAdapter _memoryAdapter;
		GridLayoutManager gridmanager;
		protected override void OnCreate(Bundle savedInstanceState)
		{
			try
			{
				base.OnCreate(savedInstanceState);
				SetContentView(Resource.Layout.MemoryMainLayout);

				NoteworthyApplication.StartBackgroundService();

				//DataBase Initalize
				Utility.InitializeDatabase();

				toolBarHeader = FindViewById<Toolbar>(Resource.Id.tool_bar_snapspinoff_header);
				SetupActionBar();

				recyclerListView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
				refresher = FindViewById<SwipeRefreshLayout>(Resource.Id.refresher);

				lnrEmptyView = FindViewById<LinearLayout>(Resource.Id.lnrEmptyView);
				lnrLoad = FindViewById<LinearLayout>(Resource.Id.lnrLoad);
				recyclerListView.HasFixedSize = true;
				gridmanager = Utility.RecyclerViewUI(recyclerListView, this);

				lnrEmptyView = FindViewById<LinearLayout>(Resource.Id.lnrEmptyView);
				lnrEmptyView.Visibility = ViewStates.Gone;

				lnrLoad.Visibility = ViewStates.Visible;

				refresher.Enabled = false;

				bool isScreenShot = Intent.GetBooleanExtra("isScreenShot", false);
				string MemorySelected = Intent.GetStringExtra("MemorySelected");
				if (!string.IsNullOrEmpty(MemorySelected))
				{
					/*
					Intent intent = new Intent(this, typeof(ProductDetailActivity));
					intent.PutExtra("SelctedProduct", productJson);
					intent.SetFlags(ActivityFlags.NewTask);
					StartActivity(intent);
					*/
				}
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
				_lstMemories = SQLClient<Memory>.Instance.GetAll().ToList();
				if (_lstMemories.Count > 0)
				{
					_lstMemories.Reverse();
					SetMemoryAdapter();
				}
				else {
					lnrEmptyView.Visibility = ViewStates.Visible;
				}
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler(Class.SimpleName, "OnResume", ex);
			}
		}

		void SetMemoryAdapter()
		{
			try
			{
				if (_lstMemories != null && _lstMemories.Count > 0)
				{
					if (_memoryAdapter == null)
					{
						_memoryAdapter = new MemoryAdapter(this, _lstMemories);
						gridmanager.SetSpanSizeLookup(new SpanSizeLookup(_memoryAdapter, gridmanager));
						recyclerListView.SetAdapter(_memoryAdapter);
					}
					else {
						_memoryAdapter.updateItems(_lstMemories);
					}
					recyclerListView.Visibility = ViewStates.Visible;
					lnrEmptyView.Visibility = ViewStates.Gone;
					lnrLoad.Visibility = ViewStates.Gone;
				}
				else {
					recyclerListView.Visibility = ViewStates.Gone;
					lnrEmptyView.Visibility = ViewStates.Visible;
				}
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler(Class.Name, "SetMemoryAdapter", ex);
			}
		}

		void SetupActionBar()
		{
			try
			{
				SetSupportActionBar(toolBarHeader);
				SupportActionBar.SetDisplayShowTitleEnabled(false);
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler(Class.SimpleName, "SetupActionBar", ex);
			}
		}

		protected override void OnPause()
		{
			try
			{
				base.OnPause();
				//lnrEmptyView.Click -= LnrEmptyView_Click;
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler(Class.SimpleName, "OnPause", ex);
			}
		}

		void LnrEmptyView_Click(object sender, EventArgs e)
		{
			try
			{
				lnrEmptyView.Visibility = ViewStates.Gone;
				lnrLoad.Visibility = ViewStates.Visible;
				_lstMemories = SQLClient<Memory>.Instance.GetAll().ToList();
				if (_lstMemories.Count > 0)
				{
					_lstMemories.Reverse();
					SetMemoryAdapter();
				}
				else {
					lnrEmptyView.Visibility = ViewStates.Visible;
				}
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler(Class.SimpleName, "LnrEmptyView_Click", ex);
			}
		}
	}
}

