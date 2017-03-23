using System;
using SQLite;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Noteworthy
{
	public class DataBase
	{
		public SQLiteConnection Conn;

		public static object SyncObject = new object();

		private static volatile DataBase _instance;

		private static Type[] DatabaseTypes = new Type[] {
			typeof(Memory),
			typeof(Sensitivity)
		};

		public DataBase()
		{
			try
			{
				Utility.Db_file = string.Format("Noteworthy_{0}.sqlite", Utility.DBVersion);
				Utility.DbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), Utility.Db_file);
				SQLite3.Config(SQLite3.ConfigOption.Serialized);
				Conn = new SQLiteConnection(Utility.DbPath);
			}
			catch (System.Exception ex)
			{
				Utility.ExceptionHandler("DataBase->Constructor", "DataBase()", ex);
			}

		}

		public void DataMigration(int OldDBVersion)
		{
			try
			{
				var DBPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), string.Format("Noteworthy_{0}.sqlite", OldDBVersion));
				SQLite3.Config(SQLite3.ConfigOption.Serialized);
				SQLiteConnection OldDBConn = new SQLiteConnection(DBPath);

				foreach (Type tableType in DatabaseTypes)
				{
					if (tableType.Name.Equals("Memory"))
					{
						var lst = SQLClient<Memory>.Instance.GetAll(OldDBConn);
						SQLClient<Memory>.Instance.InsertAll(lst);
					}
					else if (tableType.Name.Equals("Sensitivity")) {
						var lst = SQLClient<Sensitivity>.Instance.GetAll(OldDBConn);
						SQLClient<Sensitivity>.Instance.InsertAll(lst);
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler("DataBase->Constructor", "DataBase()", ex);
			}
		}

		public static DataBase Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (DataBase.SyncObject)
					{
						if (_instance == null)
						{
							_instance = new DataBase();
						}
					}
				}

				return _instance;
			}
		}

		public void RunInTransaction(Action action)
		{
			lock (DataBase.SyncObject)
			{
				this.Conn.RunInTransaction(action);
			}
		}

		public void DeleteDBData()
		{
			lock (DataBase.SyncObject)
			{
				foreach (var tableType in DatabaseTypes)
				{
					try
					{
						Conn.Execute("Delete from [" + tableType.Name + "]");
						Console.WriteLine("Delete table : {0}", tableType);
					}
					catch (Exception ex)
					{
						Utility.ExceptionHandler(
							"SQLClient",
							"DeleteDBData - " + tableType.Name,
							ex);
					}
				}
			}
		}

		public void CreateDB()
		{
			try
			{
				lock (DataBase.SyncObject)
				{
					foreach (var tableType in DatabaseTypes)
					{
						Conn.CreateTable(tableType);
						Console.WriteLine("Created table : {0}", tableType);
					}
				}

				if (Utility.DBVersion != 1)
				{
					for (int i = Utility.DBVersion - 1; i > 0; i--)
					{
						var DBPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), string.Format("Noteworthy_{0}.sqlite", i));
						if (File.Exists(DBPath))
						{
							DataMigration(i);
							File.Delete(DBPath);
							break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler("SQLClient", "CreateDB", ex);
			}
		}

		public void CloseConnection()
		{
			lock (DataBase.SyncObject)
			{
				this.Conn.Close();
				this.Conn.Dispose();
				this.Conn = null;
			}

			DataBase._instance = null;
		}
	}

	public class SQLClient<T> where T : new()
	{
		private static volatile SQLClient<T> _instance;

		public static SQLClient<T> Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (DataBase.SyncObject)
					{
						if (_instance == null)
						{
							_instance = new SQLClient<T>();
						}
					}
				}

				return _instance;
			}
		}

		public int Insert(T entity)
		{
			lock (DataBase.SyncObject)
			{

				return DataBase.Instance.Conn.Insert(entity);
			}
		}

		public int InsertOrReplace(T entity)
		{
			try
			{
				lock (DataBase.SyncObject)
				{
					return DataBase.Instance.Conn.InsertOrReplace(entity);
					/*
					if (typeof(T) == typeof(Memory))
					{
						return Insert(entity);
					}
					else {
						return DataBase.Instance.Conn.InsertOrReplace(entity);
					}
					*/
				}
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler("SQLClient->InsertOrReplace", "InsertOrReplace()", ex);
				return 0;
			}
		}

		public void InsertAll(List<T> entity)
		{
			lock (DataBase.SyncObject)
			{
				foreach (var item in entity)
				{
					DataBase.Instance.Conn.InsertOrReplace(item);
				}
			}
		}

		public void DeleteAll()
		{
			lock (DataBase.SyncObject)
			{
				DataBase.Instance.Conn.DeleteAll<T>();
			}
		}

		public void DeleteById(string Id)
		{
			lock (DataBase.SyncObject)
			{
				DataBase.Instance.Conn.Delete<T>(Id);
			}
		}


		public List<T> GetAll()
		{
			try
			{
				lock (DataBase.SyncObject)
				{
					var Datalist = DataBase.Instance.Conn.Table<T>();
					if (Datalist != null)
					{
						List<T> list = Datalist.ToList();
						return list;
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler("SQLClient", "GetAll", ex);
			}
			return new List<T>();
		}

		public List<T> GetAll(SQLiteConnection SQLiteConn)
		{
			try
			{
				lock (DataBase.SyncObject)
				{
					List<T> list = SQLiteConn.Table<T>().ToList();
					return list == null ? new List<T>() : list;
				}
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler("SQLClient", "GetAll", ex);
			}
			return new List<T>();
		}

		public T GetById(string Id)
		{
			try
			{
				lock (DataBase.SyncObject)
				{
					if (typeof(T) == typeof(Memory))
					{
						List<T> lst = DataBase.Instance.Conn.Query<T>(string.Format("Select * from Memory where RowId='{0}'", Id));
						if (lst != null || lst.Count > 0)
							return lst[0];
						else
							return new T();
					}
					else {
						T obj = DataBase.Instance.Conn.Get<T>(Id);
						return obj;
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler("SQLClient", "GetById", ex);
			}
			return default(T);
		}
	}
}

