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

namespace Noteworthy
{
	public static class Utility
	{
		public static int DBVersion = 1;

		static string db_file;

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
			
			Console.Error.WriteLine(string.Format("FileName : {0} and Method Name : {1} \n Error : {2}", fileName, methodName, ex));
		}
	}
}
