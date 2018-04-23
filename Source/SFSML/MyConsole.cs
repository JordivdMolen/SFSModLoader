using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace SFSML
{
	public class MyConsole
	{
		[DllImport("kernel32.dll")]
		private static extern IntPtr GetConsoleWindow();

		[DllImport("user32.dll")]
		private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("Kernel32.dll")]
		private static extern bool AllocConsole();

		public MyConsole()
		{
			MyConsole.AllocConsole();
			Console.SetOut(new StreamWriter(Console.OpenStandardOutput())
			{
				AutoFlush = true
			});
			this.visible = true;
		}

		public void hideConsole()
		{
			MyConsole.ShowWindow(MyConsole.GetConsoleWindow(), 0);
			this.visible = false;
		}

		public void showConsole()
		{
			MyConsole.ShowWindow(MyConsole.GetConsoleWindow(), 5);
			this.visible = true;
		}

		public void toggleConsole()
		{
			bool flag = this.visible;
			if (flag)
			{
				this.hideConsole();
			}
			else
			{
				this.showConsole();
			}
		}

		public void logError(Exception e)
		{
			StackTrace stackTrace = new StackTrace(e, true);
			StackFrame frame = stackTrace.GetFrame(0);
			int fileLineNumber = frame.GetFileLineNumber();
			string fileName = frame.GetFileName();
			this.tryLogCustom("##[ERROR]##", "ErrorReporter", LogType.Error);
			this.tryLogCustom(e.Message + e.StackTrace, "ErrorReporter", LogType.Error);
			this.tryLogCustom(fileLineNumber + "@" + fileName, "ErrorReporter", LogType.Error);
			this.tryLogCustom("##[ERROR]##", "ErrorReporter", LogType.Error);
		}

		public void log(string msg, string tag)
		{
			Console.WriteLine("[" + tag + "]: " + msg);
		}

		public void log(string msg)
		{
			this.log(msg, "Unkwn");
		}

		public void tryLogCustom(string msg, string tag, LogType type)
		{
			bool flag = this.logCustom == null;
			if (flag)
			{
				this.log(msg, tag);
			}
			else
			{
				msg = "[" + tag + "]: " + msg;
				try
				{
					this.logCustom(msg, type);
				}
				catch (Exception e)
				{
					this.logError(e);
				}
			}
		}

		public void tryLogCustom(string msg, LogType type)
		{
			this.tryLogCustom(msg, "Unkwn", type);
		}

		public void setLogger(Action<string, LogType> logfunc)
		{
			this.logCustom = logfunc;
		}

		private const int SW_HIDE = 0;

		private const int SW_SHOW = 5;

		private bool visible = false;

		private Action<string, LogType> logCustom;
	}
}
