using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFSML.Exceptions
{
    class MyCoreException : Exception
    {
        public MyCaller caller;
        public readonly string file;
        public readonly string msg;
        public MyCoreException(string message, string myFile) : base("Whoops something went wrong!")
        {
            this.file = myFile;
            this.msg = message;
        }

        public class MyCaller
        {
            public readonly string function;
            public readonly string file;
            public MyCaller(string functionName, string fileName)
            {
                this.function = functionName;
                this.file = fileName;
            }
            public string construct()
            {
                return function + "()" + " [" + file + "]";
            }
        }
    }
}
