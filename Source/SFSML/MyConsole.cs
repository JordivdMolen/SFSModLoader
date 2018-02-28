using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace SFSML
{
    public class MyConsole
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("Kernel32.dll")]
        private static extern bool AllocConsole();

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        private bool visible = false;

        private Action<string, LogType> logCustom;

        public MyConsole()
        {
            AllocConsole();
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
            this.visible = true;
        }
        public void hideConsole()
        {
            ShowWindow(GetConsoleWindow(), SW_HIDE);
            this.visible = false;
        }
        public void showConsole()
        {
            ShowWindow(GetConsoleWindow(), SW_SHOW);
            this.visible = true;
        }
        public void toggleConsole()
        {
            if (this.visible)
            {
                this.hideConsole();
            } else
            {
                this.showConsole();
            }
        }
        public void logError(Exception e)
        {
            StackTrace st = new StackTrace(e, true);
            StackFrame sf = st.GetFrame(0);
            int line = sf.GetFileLineNumber();
            string file = sf.GetFileName();
            this.tryLogCustom("##[ERROR]##","ErrorReporter",LogType.Error);
            this.tryLogCustom(e.Message+e.StackTrace, "ErrorReporter", LogType.Error);
            this.tryLogCustom(line + "@"+file, "ErrorReporter", LogType.Error);
            this.tryLogCustom("##[ERROR]##", "ErrorReporter", LogType.Error);
        }
        public void log(String msg, String tag)
        {
            Console.WriteLine("["+ tag + "]: " + msg);
        }
        public void log(String msg)
        {
            this.log(msg, "Unkwn");
        }

        public void tryLogCustom(String msg, String tag, LogType type)
        {
            if (this.logCustom == null)
            {
                this.log(msg, tag);
            } else
            {
                msg = "["+tag+"]: " + msg;
                this.logCustom(msg, type);
            }
        }
        public void tryLogCustom(String msg, LogType type)
        {
            this.tryLogCustom(msg, "Unkwn", type);
        }
        public void setLogger(Action<string,LogType> logfunc)
        {
            this.logCustom = logfunc;
        }

    }

    public enum LogType
    {
        Generic,
        Warning,
        Error
    }

}
