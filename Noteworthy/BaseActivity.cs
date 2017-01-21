using Android.App;
using Android.OS;
using Android.Views;
using Android.Content.PM;
using Android.Support.V7.App;

namespace Noteworthy
{
	[Activity(ScreenOrientation = ScreenOrientation.SensorPortrait)]
	public class BaseActivity : ActionBarActivity//
	{
		protected override void OnCreate(Bundle bundle)
		{
			try
			{
				base.OnCreate(bundle);
				StatusbarColor();
			}
			catch (System.Exception ex)
			{
				Utility.ExceptionHandler(Class.Name, "OnCreate", ex);
			}
		}

		protected override void OnResume()
		{
			try
			{
				base.OnResume();
			}
			catch (System.Exception ex)
			{
				Utility.ExceptionHandler(Class.Name, "OnResume", ex);
			}
		}
		void StatusbarColor()
		{
			try
			{
				if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
				{
					// clear FLAG_TRANSLUCENT_STATUS flag:
					Window.ClearFlags(WindowManagerFlags.TranslucentStatus);//(WindowManager.LayoutParams.FLAG_TRANSLUCENT_STATUS);
																			// add FLAG_DRAWS_SYSTEM_BAR_BACKGROUNDS flag to the window
					Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);//.FLAG_DRAWS_SYSTEM_BAR_BACKGROUNDS);
																				  // finally change the color
					Window.SetStatusBarColor(Resources.GetColor(Resource.Color.black)); //(activity.getResources().getColor(R.color.my_statusbar_color));
				}
			}
			catch (System.Exception ex)
			{
				Utility.ExceptionHandler(Class.Name, "StatusbarColor", ex);
			}
		}
	}
}