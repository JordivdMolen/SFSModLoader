using System;

namespace JMtech.JDIS.Web
{
	public class JDISResponse<T>
	{
		public JDISResponseType ResponseType()
		{
			return (JDISResponseType)this.sec;
		}

		public T getData()
		{
			return this.msg;
		}

		public bool isSuccess()
		{
			return this.type.Equals("success");
		}

		public int getServerExitCode()
		{
			return this.sec;
		}

		public int getRequestID()
		{
			return this.rID;
		}

		public string type;

		public int sec;

		public int rID;

		public T msg;

		[NonSerialized]
		public string errorMessage;
	}
}
