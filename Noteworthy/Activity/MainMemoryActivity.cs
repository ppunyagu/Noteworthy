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
using Android.Support.V4.Widget;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Graphics;
using Android.Content;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Noteworthy
{
	[Activity(Label = "Noteworthy", /* MainLauncher = true, */Icon = "@mipmap/icon", ScreenOrientation = ScreenOrientation.SensorPortrait)]
	public class MainMemoryActivity : BaseActivity
	{
		Toolbar toolBarHeader;
		RecyclerView recyclerListView;
		SwipeRefreshLayout refresher;
		LinearLayout lnrEmptyView;
		LinearLayout lnrLoad;
		List<Memory> _lstMemories;
		MemoryAdapter _memoryAdapter;
		LinearLayoutManager lnrmanager;
		int Header = 0, Child = 1;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			try
			{
				base.OnCreate(savedInstanceState);
				SetContentView(Resource.Layout.MemoryMainLayout);

				toolBarHeader = FindViewById<Toolbar>(Resource.Id.tool_bar_snapspinoff_header);
				SetupActionBar();

				recyclerListView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
				refresher = FindViewById<SwipeRefreshLayout>(Resource.Id.refresher);

				lnrEmptyView = FindViewById<LinearLayout>(Resource.Id.lnrEmptyView);
				lnrLoad = FindViewById<LinearLayout>(Resource.Id.lnrLoad);
				lnrmanager = Utility.RecyclerListViewUI(recyclerListView, this);

				lnrEmptyView = FindViewById<LinearLayout>(Resource.Id.lnrEmptyView);
				lnrEmptyView.Visibility = ViewStates.Gone;

				lnrLoad.Visibility = ViewStates.Visible;

				refresher.Enabled = false;

				bool isScreenShot = Intent.GetBooleanExtra("isScreenShot", false);
				string MemorySelected = Intent.GetStringExtra("MemorySelected");
				if (!string.IsNullOrEmpty(MemorySelected))
				{
					/* #todo Handle PN case
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
					// This is slow, should instead keep unqueried memory and only do those
					foreach (var mem in _lstMemories)
					{
						if (mem.ConversationText == "")
						{
							using (var objTranslationService = new TranslationService())
							{
								string translationText = objTranslationService.GetTextFromJobId(mem.JobId);
								if (translationText != "")
								{
									mem.ConversationText = translationText;
								}
								else {
									mem.ConversationText = "<Unrecognizable>";
								}
								SQLClient<Memory>.Instance.InsertOrReplace(mem);
							}
						}
					}
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
					List<Item> data = new List<Item>();
					var dicMemories = _lstMemories.GroupBy(x => x.Time.GetValueOrDefault().DayOfYear).ToDictionary(t => t.Key, t => t.ToList());
					for (int i = 0; i < dicMemories.Count; i++)
					{
						Item item = new Item
						{
							type = Header,
							text = dicMemories.ElementAt(i).Key.ToString(), //item.Key.ToString()
							invisibleChildren = new List<Item>()
						};
						data.Add(item);

						List<Memory> childLst = dicMemories.ElementAt(i).Value;
						if (childLst != null && childLst.Count > 0)
						{
							for (int j = 0; j < childLst.Count; j++)
							{
								Item objItem = new Item();
								objItem.type = Child;
								objItem.memory = childLst[j];
								data.Add(objItem);
							}
						}
					}
					_memoryAdapter = new MemoryAdapter(this, data);

					recyclerListView.SetAdapter(_memoryAdapter);
					recyclerListView.Visibility = ViewStates.Visible;
					lnrEmptyView.Visibility = ViewStates.Gone;
					lnrLoad.Visibility = ViewStates.Gone;
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

