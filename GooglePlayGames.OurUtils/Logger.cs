using System;
using UnityEngine;

namespace GooglePlayGames.OurUtils
{
	public class Logger
	{
		private static bool debugLogEnabled;

		private static bool warningLogEnabled = true;

		public static bool DebugLogEnabled
		{
			get
			{
				return Logger.debugLogEnabled;
			}
			set
			{
				Logger.debugLogEnabled = value;
			}
		}

		public static bool WarningLogEnabled
		{
			get
			{
				return Logger.warningLogEnabled;
			}
			set
			{
				Logger.warningLogEnabled = value;
			}
		}

		public static void d(string msg)
		{
			if (Logger.debugLogEnabled)
			{
				PlayGamesHelperObject.RunOnGameThread(delegate
				{
					Debug.Log(Logger.ToLogMessage(string.Empty, "DEBUG", msg));
				});
			}
		}

		public static void w(string msg)
		{
			if (Logger.warningLogEnabled)
			{
				PlayGamesHelperObject.RunOnGameThread(delegate
				{
					Debug.LogWarning(Logger.ToLogMessage("!!!", "WARNING", msg));
				});
			}
		}

		public static void e(string msg)
		{
			if (Logger.warningLogEnabled)
			{
				PlayGamesHelperObject.RunOnGameThread(delegate
				{
					Debug.LogWarning(Logger.ToLogMessage("***", "ERROR", msg));
				});
			}
		}

		public static string describe(byte[] b)
		{
			return (b != null) ? ("byte[" + b.Length + "]") : "(null)";
		}

		private static string ToLogMessage(string prefix, string logType, string msg)
		{
			return string.Format("{0} [Play Games Plugin DLL] {1} {2}: {3}", new object[]
			{
				prefix,
				DateTime.Now.ToString("MM/dd/yy H:mm:ss zzz"),
				logType,
				msg
			});
		}
	}
}
