// 
// DownloadStats.cs
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
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Cydin.Models
{
	public class DownloadStats
	{
		public List<StatSerie> Series = new List<StatSerie> ();
		StatSerie totalsSerie;
		
		public void AddValue (string serie, string label, int value, DateTime rangeStart, DateTime rangeEnd)
		{
			StatSerie s = Series.FirstOrDefault (t => t.Name == serie);
			if (s == null) {
				s = new StatSerie ();
				s.Name = serie;
				Series.Add (s);
			}
			s.Values.Add (new StatValue () { Label=label, Value=value, StartDate=rangeStart, EndDate=rangeEnd });
		}
		
		public void GenerateTotals ()
		{
			StatSerie total = new StatSerie () { Name = "Total" };
			Dictionary<string,StatValue> values = new Dictionary<string, StatValue> ();
			foreach (StatSerie s in Series) {
				foreach (StatValue v in s.Values) {
					StatValue cval;
					if (!values.TryGetValue (v.Label, out cval)) {
						cval = new StatValue () { Label = v.Label, Value = v.Value, StartDate = v.StartDate, EndDate = v.EndDate };
						values [v.Label] = cval;
						total.Values.Add (cval);
					} else
						cval.Value += v.Value;
				}
			}
			totalsSerie = total;
			Series.Add (total);
		}
		
		public void FillGaps (TimePeriod p, DateTime start, DateTime end)
		{
			foreach (var s in Series) {
				s.Sort ();
				s.FillGaps (p, start, end);
			}
		}
		
		public string ToJson ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("{\"series\":");
			sb.Append ('[');
			for (int n=0; n<Series.Count; n++) {
				if (n > 0)
					sb.Append (',');
				StatSerie s = Series [n];
				sb.Append ("{\"name\":\"").Append (s.Name).Append ("\",\"values\":");
				s.ToJson (sb);
				sb.Append ('}');
			}
			sb.Append (']');
			sb.Append (", \"totals\":[");
			bool fa = false;
			for (int n=0; n<Series.Count; n++) {
				StatSerie s = Series [n];
				if (s == totalsSerie)
					continue;
				if (fa)
					sb.Append (',');
				sb.Append ("[\"").Append (s.Name).Append ("\",").Append (s.GetTotal()).Append ("]");
				fa = true;
			}
			sb.Append ("]}");
			return sb.ToString ();
		}
		
		public string SeriesToJson ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ('[');
			for (int n=0; n<Series.Count; n++) {
				if (n > 0)
					sb.Append (',');
				StatSerie s = Series [n];
				sb.Append ('\'').Append (Series [n]).Append ('\'');
				sb.Append ('}');
			}
			sb.Append (']');
			return sb.ToString ();
		}
		
		public string ValuesToJson ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ('[');
			for (int n=0; n<Series.Count; n++) {
				if (n > 0)
					sb.Append (',');
				Series [n].ToJson (sb);
			}
			sb.Append (']');
			return sb.ToString ();
		}
		
		public static void ParseQuery (string period, string arg, out TimePeriod pd, out DateTime start, out DateTime end)
		{
			pd = TimePeriod.Auto;;
			
			if (period == "last") {
				int num = int.Parse (arg.Substring (0, arg.Length - 1));
				if (arg.EndsWith ("d")) {
					end = DateTime.Now;
					start = end.AddDays (-num);
				}
				else if (arg.EndsWith ("w")) {
					end = DateTime.Now;
					start = end.AddDays (-num*7);
				}
				else if (arg.EndsWith ("m")) {
					end = DateTime.Now;
					start = end.AddMonths (-num);
				}
				else if (arg.EndsWith ("y")) {
					end = DateTime.Now;
					start = end.AddYears (-num);
				}
				else
					throw new Exception ("Invalid period specified");
			}
			else if (period == "period") {
				string[] range = arg.Split ('.');
				start = DateTime.Parse (range[0]);
				end = DateTime.Parse (range[1]);
			}
			else if (period == "month") {
				DateTime d = DateTime.Parse (arg);
				start = new DateTime (d.Year, d.Month, 1);
				end = start.AddMonths (1);
			}
			else if (period == "year") {
				DateTime d = DateTime.Parse (arg);
				start = new DateTime (d.Year, 1, 1);
				end = start.AddYears (1);
			}
			else
				throw new Exception ("Invalid period specified");

			if (pd == TimePeriod.Auto) {
				int nd = (int)(end - start).TotalDays;
				if (nd <= 60)
					pd = TimePeriod.Day;
				else if (nd <= 30*6)
					pd = TimePeriod.Week;
				else
					pd = TimePeriod.Month;
			}
		}
	}
	
	public class StatSerie
	{
		public string Name;
		public List<StatValue> Values = new List<StatValue> ();
		
		public void Sort ()
		{
			Values.Sort (delegate (StatValue a, StatValue b) {
				return a.StartDate.CompareTo (b.StartDate);
			});
		}
		
		public int GetTotal ()
		{
			return Values.Sum (v => v.Value);
		}
		
		public void ToJson (StringBuilder sb)
		{
			sb.Append ('[');
			for (int n=0; n<Values.Count; n++) {
				if (n > 0)
					sb.Append (',');
				Values[n].ToJson (sb);
			}
			sb.Append (']');
		}
		
		public void FillGaps (TimePeriod p, DateTime start, DateTime end)
		{
			int pos = 0;
			foreach (DateTime date in GetPeriods (p, start, end)) {
				if (pos >= Values.Count || Values[pos].StartDate > date) {
					StatValue v = new StatValue () { Value = 0, StartDate = date, EndDate = GetNext (p, date) };
					Values.Insert (pos, v);
				}
				pos++;
			}
		}
		
		IEnumerable<DateTime> GetPeriods (TimePeriod p, DateTime start, DateTime end)
		{
			start = RoundDate (p, start);
			end = RoundDate (p, end);
			
			while (start < end) {
				yield return start;
				start = GetNext (p, start);
			}
		}
		
		DateTime GetNext (TimePeriod p, DateTime date)
		{
			switch (p) {
			case TimePeriod.Day: return date.AddDays (1);
			case TimePeriod.Week: return date.AddDays (7);
			case TimePeriod.Month: return date.AddMonths (1);
			case TimePeriod.Year: return date.AddYears (1);
			}
			throw new InvalidOperationException ();
		}
		
		DateTime RoundDate (TimePeriod period, DateTime t)
		{
			switch (period) {
			case TimePeriod.Day:
				return t.Date;
			case TimePeriod.Week:
				return t.Date.AddDays (-(int) t.DayOfWeek);
			case TimePeriod.Month:
				return new DateTime (t.Year, t.Month, 1);
			case TimePeriod.Year:
				return new DateTime (t.Year, 1, 1);
			}
			throw new InvalidOperationException ();
		}
	}
	
	public class StatValue
	{
		public string Label;
		public DateTime StartDate;
		public DateTime EndDate;
		public int Value;
		
		public void ToJson (StringBuilder sb)
		{
			DateTime t = (StartDate + TimeSpan.FromTicks ((EndDate - StartDate).Ticks/2)).Date;
			sb.Append ("[\"").Append (t.ToString ("MM-dd-yyyy")).Append ("\",").Append (Value);
			sb.Append ("]");
		}
	}
}

