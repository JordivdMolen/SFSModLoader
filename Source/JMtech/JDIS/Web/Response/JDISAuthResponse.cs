using System;
using JMtech.JDIS.Auth;

namespace JMtech.JDIS.Web.Response
{
	public class JDISAuthResponse
	{
		public JDISKey key()
		{
			return JDISKey.fromAuthResponse(this);
		}

		public string ID;

		public string KEY1;

		public string KEY2;
	}
}
