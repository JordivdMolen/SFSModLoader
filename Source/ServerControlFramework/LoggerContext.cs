using System;
using UnityEngine;

namespace ServerControlFramework
{
	public class LoggerContext
	{
		public static LoggerContext getMainLogger()
		{
			if (LoggerContext.mainLogger == null)
			{
				throw new Exception("Logger has not been created!");
			}
			return LoggerContext.mainLogger;
		}

		public static void setMainLogger(LoggerContext logger)
		{
			LoggerContext.mainLogger = logger;
		}

		public virtual void Generic(object log, bool bold = false)
		{
			this.Log("[GENERIC]: " + log, Color.black, bold);
		}

		public virtual void Warning(object log, bool bold = false)
		{
			this.Log("[WARNING]: " + log, Color.yellow, bold);
		}

		public virtual void Error(object log)
		{
			this.Log("[ERROR]: " + log, Color.red, true);
		}

		public virtual void Log(object log, Color color, bool bold)
		{
		}

		private static LoggerContext mainLogger;
	}
}
