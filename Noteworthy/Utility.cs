using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Threading;
using Android.Preferences;
using Android.Util;
using Android.Content;
using Android.App;
using Android.Support.V7.Widget;

namespace Noteworthy
{
	public static class Utility
	{
		public static int DBVersion = 2;

		static string db_file;

		public static string server_heartRate { get; set; }

		public static string audio_file_format = "mp3";

		public static string Db_file
		{
			get
			{
				db_file = Prefs.GetString("db_file", string.Empty);
				return db_file;
			}
			set
			{
				db_file = value;
				Editor.PutString("db_file", db_file);
				Editor.Apply();
				Editor.Commit();
			}
		}

		static string dbPath;

		public static string DbPath
		{
			get
			{
				dbPath = Prefs.GetString("dbPath", string.Empty);
				return dbPath;
			}
			set
			{
				dbPath = value;
				Editor.PutString("dbPath", dbPath);
				Editor.Apply();
				Editor.Commit();
			}
		}

		public static void InitializeDatabase()
		{
			Db_file = string.Format("Noteworthy_{0}.sqlite", DBVersion);
			DbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), Db_file);
			bool exists = File.Exists(DbPath);
			Console.WriteLine("Created the dbFile: {0}", DbPath);
			if (!exists)
			{
				DataBase.Instance.CreateDB();
			}

			//S3Initialize();
		}
		/*
		public static async void S3Initialize()
		{
			bool bucketExists = await S3Utils.BucketExist();
			if (!bucketExists)
				await S3Utils.CreateBucket();
		}
		*/

		static ISharedPreferences prefs;
		public static ISharedPreferences Prefs
		{
			get
			{
				if (prefs == null)
				{
					prefs = PreferenceManager.GetDefaultSharedPreferences(Application.Context);
				}
				return prefs;
			}
		}

		static ISharedPreferencesEditor editor;
		public static ISharedPreferencesEditor Editor
		{
			get
			{
				if (editor == null)
				{
					editor = Prefs.Edit();
				}
				return editor;
			}
		}

		public static void ExceptionHandler(string fileName, string methodName, Exception ex)
		{
			Log.Error(string.Format("{0}-{1}", fileName, methodName), string.Format("Error: {0}", ex));
		}

		public static GridLayoutManager RecyclerViewUI(RecyclerView recyclerView, Activity activity)
		{
			try
			{
				GridLayoutManager gridmanager = new GridLayoutManager(activity, 2);
				recyclerView.AddItemDecoration(new GridSpacingItemDecoration());
				recyclerView.SetLayoutManager(gridmanager);
				return gridmanager;
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler(
					"Utility",
					"RecyclerViewUI",
					ex);
			}
			return new GridLayoutManager(activity, 2);
		}

		public static float ConvertDpToPixel(float dp, Context context)
		{
			float px = 0;
			try
			{
				DisplayMetrics metrics = context.Resources.DisplayMetrics;
				px = dp * ((float)metrics.DensityDpi / 160f);
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler("Utility", "ConvertDpToPixel", ex);
			}
			return px;
		}

		public static LinearLayoutManager RecyclerListViewUI(RecyclerView recyclerView, Activity activity)
		{
			LinearLayoutManager listmanager = null;
			try
			{
				listmanager = new LinearLayoutManager(activity);
				recyclerView.AddItemDecoration(new GridSpacingItemDecoration(1, 0, false, true));
				recyclerView.SetLayoutManager(listmanager);
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler(
					"Utility",
					"RecyclerListViewUI",
					ex);
			}
			return listmanager;
		}

		private static string[] EngMonth = new string[] {
			"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
		};

		public static string GetRelativeTime(DateTime dt) {
			var ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
			double delta = Math.Abs(ts.TotalSeconds);

			if (delta < 60)
			{
				return "just now";
			}
			if (delta < 120)
			{
				return "a min ago";
			}
			if (delta < 3600)
			{
				return ts.Minutes + " mins ago";
			}
			if (delta < 86400)
			{
				if (ts.Hours <= 1)
				{
					return "an hr ago";
				}
				else {
					return ts.Hours + " hrs ago";
				}
			}
			if (delta < 172800)
			{
				return "yesterday";
			}

			return EngMonth[dt.Month - 1] + " " + dt.ToString("dd");
		}
	}
}
