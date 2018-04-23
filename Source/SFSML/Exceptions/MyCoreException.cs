using System;

namespace SFSML.Exceptions
{
	internal class MyCoreException : Exception
	{
		public MyCoreException(string message, string myFile) : base("Whoops something went wrong!")
		{
			this.file = myFile;
			this.msg = message;
		}

		public MyCoreException.MyCaller caller;

		public readonly string file;

		public readonly string msg;

		public class MyCaller
		{
			public MyCaller(string functionName, string fileName)
			{
				this.function = functionName;
				this.file = fileName;
			}

			public string construct()
			{
				return this.function + "() [" + this.file + "]";
			}

			public readonly string function;

			public readonly string file;
		}
	}
}
