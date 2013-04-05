// 
// StatsModel.cs
//  
// Author:
//       Lluis Sanchez Gual <lluis@novell.com>
// 
// Copyright (c) 2010 Novell, Inc (http://www.novell.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Cydin.Models
{
	public class StatsModel
	{
		UserModel userModel;
		MySqlConnection db;
		
		internal StatsModel (UserModel um, MySqlConnection db)
		{
			userModel = um;
			this.db = db;
		}
		
		public void IncRepoDownloadCount (string platform, string version)
		{
			try {
				RepositoryDownload rp = null;
				do {
					rp = db.SelectObjectWhere<RepositoryDownload> ("Platform={0} AND TargetAppVersion={1} AND Date={2} AND ApplicationId={3}", platform, version, DateTime.Now.Date, userModel.CurrentApplication.Id);
					if (rp == null) {
						rp = new RepositoryDownload ();
						rp.Platform = platform;
						rp.TargetAppVersion = version;
						rp.Date = DateTime.Now.Date;
						rp.ApplicationId = userModel.CurrentApplication.Id;
						rp.Downloads = 1;
						db.InsertObject (rp);
						return;
					}
					rp.Downloads++;
				} while (!db.UpdateObject (rp));
			} catch (Exception ex) {
				Console.WriteLine (ex);
			}
		}
		
		public void IncDownloadCount (string file)
		{
			try {
				ReleasePackage rp = null;
				do {
					rp = db.SelectObjectWhere<ReleasePackage> ("FileId={0} AND Date={1}", file, DateTime.Now.Date);
					if (rp == null) {
						rp = db.SelectObject<ReleasePackage> ("SELECT * FROM ReleasePackage WHERE FileId={0} ORDER BY Date DESC", file);
						if (rp != null) {
							rp.Downloads = 1;
							rp.Date = DateTime.Now;
							db.InsertObject (rp);
						}
						return;
					}
					rp.Downloads++;
				} while (!db.UpdateObject (rp));
			} catch (Exception ex) {
				Console.WriteLine (ex);
			}
		}
		
		public string GetDownloadSummary (Release rel)
		{
			int total = 0;
			List<string> platforms = new List<string> ();
			List<int> counts = new List<int> ();
			
			foreach (string plat in rel.PlatformsList) {
				using (DbDataReader r = db.ExecuteSelect ("SELECT SUM(Downloads) Total FROM ReleasePackage WHERE ReleaseId={0} AND Platform={1}", rel.Id, plat)) {
					if (r.Read ()) {
						int ptotal = r.IsDBNull (0) ? 0 : r.GetInt32 (0);
						if (ptotal > 0) {
							platforms.Add (plat);
							counts.Add (ptotal);
							total += ptotal;
						}
					}
				}
			}
			StringBuilder sb = new StringBuilder ();
			sb.Append (total);
			if (counts.Count > 0) {
				sb.Append (" (");
				for (int n=0; n<counts.Count; n++) {
					if (n > 0)
						sb.Append (", ");
					sb.Append (counts [n]).Append (" ").Append (platforms[n]);
				}
				sb.Append (")");
			}
			return sb.ToString ();
		}

		public string GetDownloadSummary (Project p)
		{
			int total = 0;
			Dictionary<string,int> platforms = new Dictionary<string, int> ();
			
			foreach (Release rel in userModel.GetProjectReleases (p.Id)) {
				foreach (string plat in rel.PlatformsList) {
					using (DbDataReader r = db.ExecuteSelect ("SELECT SUM(Downloads) Total FROM ReleasePackage WHERE ReleaseId={0} AND Platform={1}", rel.Id, plat)) {
						if (r.Read ()) {
							int ptotal = r.IsDBNull (0) ? 0 : r.GetInt32 (0);
							if (ptotal > 0) {
								int c = 0;
								platforms.TryGetValue (plat, out c);
								c += ptotal;
								platforms [plat] = c;
								total += ptotal;
							}
						}
					}
				}
			}
			StringBuilder sb = new StringBuilder ();
			sb.Append (total);
			if (platforms.Count > 0) {
				sb.Append (" (");
				List<KeyValuePair<string,int>> list = platforms.ToList ();
				list.Sort (delegate (KeyValuePair<string,int> a, KeyValuePair<string,int> b) {
					return a.Key.CompareTo (b.Key);
				});
				for (int n=0; n<list.Count; n++) {
					if (n > 0)
						sb.Append (", ");
					sb.Append (list [n].Value).Append (" ").Append (list[n].Key);
				}
				sb.Append (")");
			}
			return sb.ToString ();
		}
		
		public List<DownloadInfo> GetTopDownloads (DateTime startDate, DateTime endDate)
		{
			string sql = "SELECT sum(Downloads), RP.Platform, RE.* " +
			 	"FROM ReleasePackage RP, `Release` RE, Project P " +
			 	"WHERE RP.Date >= {0} AND RP.Date < {1} AND RP.ReleaseId = RE.Id AND RE.ProjectId = P.Id AND P.ApplicationId = {2} " +
			 	"GROUP BY RP.Platform, RP.ReleaseId " +
			 	"ORDER BY sum(Downloads) DESC";
			
			List<DownloadInfo> stats = new List<DownloadInfo> ();
			using (DbDataReader r = db.ExecuteSelect (sql, startDate, endDate, userModel.CurrentApplication.Id)) {
				while (r.Read ()) {
					DownloadInfo di = new DownloadInfo ();
					di.Count = r.IsDBNull (0) ? 0 : r.GetInt32 (0);
					di.Platform = r.GetString (1);
					di.Release = db.ReadObject<Release> (r);
					stats.Add (di);
				}
			}
			return stats;
		}
		
		public DownloadStats GetTotalRepoDownloadStats (TimePeriod period, DateTime startDate, DateTime endDate)
		{
			string filter = "Date >= {0} AND Date < {1} ";
			string sql = null;
			switch (period) {
			case TimePeriod.Day:
				sql = "SELECT sum(Downloads), Platform, Date, Date FROM RepositoryDownload R where " + filter + " GROUP BY Platform, Date";
				break;
			case TimePeriod.Week:
				sql = "SELECT sum(Downloads), Platform, MIN(Date), DATE_FORMAT(R.Date,'%u/%Y') FROM RepositoryDownload R where " + filter + " GROUP BY Platform, DATE_FORMAT(R.Date,'%u/%Y')";
				break;
			case TimePeriod.Month:
				sql = "SELECT sum(Downloads), Platform, MIN(Date), DATE_FORMAT(R.Date,'%m/%Y') FROM RepositoryDownload R where " + filter + " GROUP BY Platform, DATE_FORMAT(R.Date,'%m/%Y')";
				break;
			case TimePeriod.Year:
				sql = "SELECT sum(Downloads), Platform, MIN(Date), YEAR(Date) FROM RepositoryDownload R where " + filter + " GROUP BY Platform, YEAR(Date)";
				break;
			case TimePeriod.All:
				sql = "SELECT sum(Downloads), Platform, MIN(Date), 'Total' FROM RepositoryDownload R where " + filter + " GROUP BY Platform";
				break;
			}
			
			DownloadStats stats = new DownloadStats ();
			using (DbDataReader r = db.ExecuteSelect (sql, startDate, endDate)) {
				while (r.Read ()) {
					int count = r.IsDBNull (0) ? 0 : r.GetInt32 (0);
					string plat = r.GetString (1);
					DateTime date = r.GetDateTime (2);
					string label = r[3].ToString ();
					DateTime start, end;
					GetPeriod (period, date, out start, out end);
					stats.AddValue (plat, label, count, start, end);
				}
			}
			stats.GenerateTotals ();
			stats.FillGaps (period, startDate, endDate);
			return stats;
		}

		public string GetRepoDownloadStatsCSV ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("Release,Platform,Date,Downloads\n");
			foreach (var rd in db.SelectObjectsWhere<RepositoryDownload> ("ApplicationId={0}", userModel.CurrentApplication.Id)) {
				sb.Append (rd.TargetAppVersion).Append (',');
				sb.Append (rd.Platform).Append (',');
				sb.Append (rd.Date).Append (',');
				sb.Append (rd.Downloads).AppendLine ();
			}
			return sb.ToString ();
		}

		public string GetDownloadStatsCSV ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("Release,Platform,Date,Addin,Version,DevStatus,Project,Downloads\n");
			
			string sql = "SELECT R.TargetAppVersion, R.Platform, R.Date, E.AddinId, E.Version, E.DevStatus, P.Name, R.Downloads FROM ReleasePackage R, `Release` E, Project P where P.ApplicationId={0} AND R.ReleaseId = E.Id AND E.ProjectId = P.Id";
			using (DbDataReader r = db.ExecuteSelect (sql, userModel.CurrentApplication.Id)) {
				while (r.Read ()) {
					sb.Append (r.GetString (0)).Append (',');
					sb.Append (r.GetString (1)).Append (',');
					sb.Append (r.GetDateTime (2).ToString ("u")).Append (',');
					sb.Append (r.GetString (3)).Append (',');
					sb.Append (r.GetString (4)).Append (',');
					sb.Append ((DevStatus)r.GetInt32 (5)).Append (','); // DevStatus
					sb.Append (r.GetString (6)).Append (',');
					sb.Append (r.GetInt32 (7)).AppendLine ();
				}
			}
			return sb.ToString ();
		}

		public DownloadStats GetTotalDownloadStats (TimePeriod period, DateTime startDate, DateTime endDate)
		{
			return GetDownloadStats (period, startDate, endDate, "", "1=1", null);
		}

		public DownloadStats GetProjectDownloadStats (int projectId, TimePeriod period, DateTime startDate, DateTime endDate)
		{
			return GetDownloadStats (period, startDate, endDate, ", Release E", "R.ReleaseId = E.Id AND E.ProjectId={0}", projectId);
		}

		public DownloadStats GetReleaseDownloadStats (int releaseId, TimePeriod period, DateTime startDate, DateTime endDate)
		{
			return GetDownloadStats (period, startDate, endDate, "", "R.ReleaseId={0}", releaseId);
		}
		
		DownloadStats GetDownloadStats (TimePeriod period, DateTime startDate, DateTime endDate, string from, string filter, object arg)
		{
			string sql = null;
			filter += " AND R.Date >= {1} AND R.Date < {2} ";
			switch (period) {
			case TimePeriod.Day:
				sql = "SELECT sum(Downloads), Platform, Date, Date FROM ReleasePackage R " + from + " where " + filter + " GROUP BY Platform, Date";
				break;
			case TimePeriod.Week:
				sql = "SELECT sum(Downloads), Platform, MIN(Date), DATE_FORMAT(R.Date,'%u/%Y') FROM ReleasePackage R " + from + " where " + filter + " GROUP BY Platform, DATE_FORMAT(R.Date,'%u/%Y')";
				break;
			case TimePeriod.Month:
				sql = "SELECT sum(Downloads), Platform, MIN(Date), DATE_FORMAT(R.Date,'%m/%Y') FROM ReleasePackage R " + from + " where " + filter + " GROUP BY Platform, DATE_FORMAT(R.Date,'%m/%Y')";
				break;
			case TimePeriod.Year:
				sql = "SELECT sum(Downloads), Platform, MIN(Date), YEAR(Date) FROM ReleasePackage R " + from + " where " + filter + " GROUP BY Platform, YEAR(Date)";
				break;
			case TimePeriod.All:
				sql = "SELECT sum(Downloads), Platform, MIN(Date), 'Total' FROM ReleasePackage R " + from + " where " + filter + " GROUP BY Platform";
				break;
			}
			
			DownloadStats stats = new DownloadStats ();
			using (DbDataReader r = db.ExecuteSelect (sql, arg, startDate, endDate)) {
				while (r.Read ()) {
					int count = r.IsDBNull (0) ? 0 : r.GetInt32 (0);
					string plat = r.GetString (1);
					DateTime date = r.GetDateTime (2);
					string label = r[3].ToString ();
					DateTime start, end;
					GetPeriod (period, date, out start, out end);
					stats.AddValue (plat, label, count, start, end);
				}
			}
			stats.GenerateTotals ();
			stats.FillGaps (period, startDate, endDate);
			return stats;
		}
		
		void GetPeriod (TimePeriod period, DateTime t, out DateTime start, out DateTime end)
		{
			start = end = t;
			switch (period) {
			case TimePeriod.Day:
				start = t.Date;
				end = start.AddDays (1);
				break;
			case TimePeriod.Week:
				int dw = (int) t.DayOfWeek;
				start = t.Date.AddDays (-dw);
				end = start.AddDays (7);
				break;
			case TimePeriod.Month:
				start = new DateTime (t.Year, t.Month, 1);
				end = start.AddMonths (1);
				break;
			case TimePeriod.Year:
				start = new DateTime (t.Year, 1, 1);
				end = start.AddYears (1);
				break;
			}
		}
	}
	
	public class DownloadInfo
	{
		public int Count;
		public string Platform;
		public Release Release;
	}
}

