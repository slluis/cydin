using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Common;
using MySql.Data.MySqlClient;
using System.Reflection;
using System.Text;

namespace Cydin.Models
{
	public static class DataUtil
	{
		static Dictionary<Type, DbMap> maps = new Dictionary<Type, DbMap> ();

		public static void ExecuteCommand (this DbConnection gdb, string sql, params object[] args)
		{
			ExecuteSelect (gdb, sql, args).Dispose ();
		}

		public static DbDataReader ExecuteSelect (this DbConnection gdb, string sql, params object[] args)
		{
			MySqlConnection db = (MySqlConnection)gdb;
			MySqlCommand cmd = db.CreateCommand ();
			GenerateSqlCommand (cmd, sql, args);
			return cmd.ExecuteReader ();
		}

		public static void InsertObject<T> (this DbConnection gdb, T obj)
		{
			MySqlConnection db = (MySqlConnection)gdb;
			using (MySqlCommand cmd = db.CreateCommand ()) {
				DbMap map = GetMap (obj.GetType ());
				StringBuilder sql = new StringBuilder ("INSERT INTO `");
				sql.Append (map.Table).Append ("` (");
				foreach (string f in map.Keys) {
					if (f != map.IdentityField)
						sql.Append (f).Append (',');
				}
				sql[sql.Length - 1] = ')';
				sql.Append (" VALUES (");

				foreach (var f in map) {
					if (f.Key == map.IdentityField)
						continue;
					string fp = "@_" + f.Key;
					sql.Append (fp).Append (',');
					cmd.Parameters.AddWithValue (fp, f.Value.GetValue (obj, null));
				}
				sql[sql.Length - 1] = ')';
				if (map.IdentityField != null)
					sql.Append ("; SELECT @@IDENTITY");

				cmd.CommandText = sql.ToString ();
				if (map.IdentityField == null)
					cmd.ExecuteNonQuery ();
				else {
					using (DbDataReader dr = cmd.ExecuteReader ()) {
						if (dr.Read ()) {
							PropertyInfo prop = map[map.IdentityField];
							object val = Convert.ChangeType (dr[0], prop.PropertyType);
							prop.SetValue (obj, val, null);
						}
						else
							throw new Exception ("Insertion failed");
					}
				}
			}
		}

		public static bool UpdateObject<T> (this DbConnection gdb, T obj)
		{
			MySqlConnection db = (MySqlConnection)gdb;
			using (MySqlCommand cmd = db.CreateCommand ()) {
				DbMap map = GetMap (obj.GetType ());
				StringBuilder sql = new StringBuilder ("UPDATE `");
				sql.Append (map.Table).Append ("` SET ");

				foreach (var f in map) {
					if (f.Key == map.IdentityField)
						continue;
					string fp = "@_" + f.Key;
					sql.Append (f.Key).Append ('=').Append (fp).Append (',');
					object val = f.Value.GetValue (obj, null);
/*					if (val is Enum)
						val = Convert.ChangeType (val, typeof(int));*/
					cmd.Parameters.AddWithValue (fp, val);
				}
				sql.Remove (sql.Length - 1, 1);

				sql.Append (" WHERE ");
				AppendObjectFilter (map, cmd, obj, sql);

				cmd.CommandText = sql.ToString ();
				return cmd.ExecuteNonQuery () > 0;
			}
		}

		public static void DeleteObject<T> (this DbConnection gdb, T obj)
		{
			MySqlConnection db = (MySqlConnection)gdb;
			using (MySqlCommand cmd = db.CreateCommand ()) {
				DbMap map = GetMap (obj.GetType ());
				StringBuilder sql = new StringBuilder ("DELETE FROM `");
				sql.Append (map.Table).Append ("` WHERE ");
				AppendObjectFilter (map, cmd, obj, sql);
				Console.WriteLine (sql);
				cmd.CommandText = sql.ToString ();
				cmd.ExecuteNonQuery ();
			}
		}

		public static T SelectObjectById<T> (this DbConnection gdb, object id) where T : new ()
		{
			return SelectObjects<T> (gdb, ".", id).FirstOrDefault ();
		}

		public static T SelectObjectWhere<T> (this DbConnection gdb, string where, params object[] args) where T : new ()
		{
			return SelectObjects<T> (gdb, "*" + where, args).FirstOrDefault ();
		}

		public static IEnumerable<T> SelectObjectsWhere<T> (this DbConnection gdb, string where, params object[] args) where T : new ()
		{
			return SelectObjects<T> (gdb, "*" + where, args);
		}

		public static T SelectObject<T> (this DbConnection gdb, string sql, params object[] args) where T : new ()
		{
			return SelectObjects<T> (gdb, sql, args).FirstOrDefault ();
		}

		public static IEnumerable<T> SelectObjects<T> (this DbConnection gdb) where T : new ()
		{
			return SelectObjects<T> (gdb, "*");
		}

		public static IEnumerable<T> SelectObjects<T> (this DbConnection gdb, string sql, params object[] args) where T : new ()
		{
			List<T> res = new List<T> ();
			MySqlConnection db = (MySqlConnection)gdb;
			using (MySqlCommand cmd = db.CreateCommand ()) {
				DbMap map = GetMap (typeof (T));
				if (sql == "*") {
					// Select all
					sql = "SELECT * FROM `" + map.Table + "`";
				}
				else if (sql.StartsWith ("*")) {
					// Select with a where
					sql = "SELECT * FROM `" + map.Table + "` WHERE " + sql.Substring (1);
				}
				else if (sql == ".") {
					// Select by id
					if (map.KeyFields.Length != 1)
						throw new NotSupportedException ();
					sql = "SELECT * FROM `" + map.Table + "` WHERE " + map.KeyFields[0] + " = {0}";
				}
				GenerateSqlCommand (cmd, sql, args);

				using (DbDataReader dr = cmd.ExecuteReader ()) {
					while (dr.Read ())
						res.Add (ReadObject<T> (map, dr));
				}
			}
			return res;
		}
		
		public static T ReadSettings<T> (this DbConnection gdb) where T : new ()
		{
			T obj = new T ();
			DbMap map = GetMap (typeof(T));
			MySqlConnection db = (MySqlConnection)gdb;
			Dictionary<string, PropertyInfo> readProps = new Dictionary<string, PropertyInfo> (map);
			
			using (MySqlCommand cmd = db.CreateCommand ()) {
				cmd.CommandText = "SELECT * FROM `" + map.Table + "`";
				using (DbDataReader dr = cmd.ExecuteReader ()) {
					while (dr.Read ()) {
						string fname = (string) dr["Key"];
						string val = dr["Value"] as string;
						PropertyInfo prop;
						if (map.TryGetValue (fname, out prop)) {
							object cval;
							if (prop.PropertyType.IsEnum)
								cval = Enum.Parse (prop.PropertyType, val);
							else
								cval = Convert.ChangeType (val, prop.PropertyType);
							prop.SetValue (obj, cval, null);
							readProps.Remove (fname);
						}
					}
				}
			}
			foreach (PropertyInfo prop in readProps.Values) {
				DataMemberAttribute att = (DataMemberAttribute) Attribute.GetCustomAttribute (prop, typeof(DataMemberAttribute), true);
				if (att != null && att.DefaultValue != null)
					prop.SetValue (obj, att.DefaultValue, null);
			}
			return obj;
		}
		
		public static void WriteSettings<T> (this DbConnection gdb, T obj) where T : new ()
		{
			DbMap map = GetMap (typeof(T));
			MySqlConnection db = (MySqlConnection)gdb;
			foreach (KeyValuePair<string, PropertyInfo> prop in map) {
				object val = prop.Value.GetValue (obj, null);
				if (val == null) {
					DataMemberAttribute att = (DataMemberAttribute) Attribute.GetCustomAttribute (prop.Value, typeof(DataMemberAttribute), true);
					if (att != null)
						val = att.DefaultValue;
				}
				
				if (val != null)
					val = val.ToString ();
				
				using (MySqlCommand cmd = db.CreateCommand ()) {
					GenerateSqlCommand (cmd, "UPDATE `" + map.Table + "` SET `Value`={0} WHERE `Key`={1}", val, prop.Key);
					int count = cmd.ExecuteNonQuery ();
					if (count > 0)
						continue;
				}
				using (MySqlCommand cmd = db.CreateCommand ()) {
					// New property. It has to be inserted
					GenerateSqlCommand (cmd, "INSERT INTO `" + map.Table + "` (`Key`,`Value`) VALUES ({0},{1})", prop.Key, val);
					cmd.ExecuteNonQuery ();
				}
			}
		}

		static T ReadObject<T> (DbMap map, DbDataReader r) where T : new ()
		{
			T obj = new T ();
			foreach (KeyValuePair<string, PropertyInfo> prop in map) {
				object val = r[prop.Key];
				if (val is DBNull)
					prop.Value.SetValue (obj, null, null);
				else
					prop.Value.SetValue (obj, val, null);
			}
			return obj;
		}

		static void AppendObjectFilter (DbMap map, MySqlCommand cmd, object obj, StringBuilder sql)
		{
			int n=0;
			foreach (var prop in map) {
				if (map.KeyFields.Length != 0 && !map.KeyFields.Contains (prop.Key))
					continue;
				if (n > 0)
					sql.Append (" AND ");
				sql.Append (prop.Key).Append ("=@__v" + n);
				cmd.Parameters.AddWithValue ("@__v" + n, prop.Value.GetValue (obj, null));
				n++;
			}
		}

		static void GenerateSqlCommand (MySqlCommand cmd, string sql, params object[] args)
		{
			object[] argNames = new object[args.Length];
			for (int n = 0; n < args.Length; n++) {
				string pn = "@_arg" + n;
				argNames[n] = pn;
				cmd.Parameters.AddWithValue (pn, args[n]);
			}
			cmd.CommandText = string.Format (sql, argNames);
		}

		static DbMap GetMap (Type t)
		{
			DbMap map;
			lock (maps) {

				if (maps.TryGetValue (t, out map))
					return map;
				
				List<string> keys = new List<string> ();
				map = new DbMap ();
				foreach (PropertyInfo prop in t.GetProperties (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
					DataMemberAttribute dm = (DataMemberAttribute)Attribute.GetCustomAttribute (prop, typeof (DataMemberAttribute));
					if (dm != null) {
						string fn;
						if (!string.IsNullOrEmpty (dm.Field))
							fn = dm.Field;
						else
							fn = prop.Name;
						map[fn] = prop;
						if (dm.Identity)
							map.IdentityField = fn;
						if (dm.Key || dm.Identity)
							keys.Add (fn);
					}
				}

				DataTypeAttribute dt = (DataTypeAttribute)Attribute.GetCustomAttribute (t, typeof (DataTypeAttribute));
				if (dt != null && !string.IsNullOrEmpty (dt.Table))
					map.Table = dt.Table;
				else
					map.Table = t.Name;
				map.KeyFields = keys.ToArray ();
				maps[t] = map;
				return map;
			}
		}

		class DbMap: Dictionary<string, PropertyInfo>
		{
			public string Table;
			public string IdentityField;
			public string[] KeyFields;
		}
	}

	public class DataMemberAttribute: Attribute
	{
		public DataMemberAttribute ()
		{
		}

		public DataMemberAttribute (string field)
		{
			Field = field;
		}

		public string Field { get; set; }
		public bool Identity { get; set; }
		public bool Key { get; set; }
		public object DefaultValue { get; set; }
	}

	public class DataTypeAttribute: Attribute
	{
		public DataTypeAttribute ()
		{
		}

		public DataTypeAttribute (string table)
		{
			Table = table;
		}

		public string Table { get; set; }
	}
}
