using System;

namespace JMtech.JDIS.Web.Request
{
	public class BaseRequest
	{
		public BaseRequest()
		{
			this.RequestIdentifier = BaseRequest.rnd.Next(0, 999999);
		}

		public void Authenticate(JDISClient sender)
		{
			this.AUTH = sender.authKey;
		}

		public int RequestIdentifier;

		public object AUTH;

		public string SEC;

		public static Random rnd = new Random();
	}
}
