using System;
using System.IO;
using JMtech.JDIS.Web;
using JMtech.JDIS.Web.Request;
using JMtech.JDIS.Web.Response;
using Newtonsoft.Json;
using UnityEngine;

namespace JMtech.JDIS.Auth
{
	public class JDISKey
	{
		public static void getAuthDetails(JDISClient requester)
		{
			if (File.Exists(Path.Combine(Application.dataPath, "auth.token")))
			{
				string value = File.ReadAllText(Path.Combine(Application.dataPath, "auth.token"));
				JDISKey authKey = JsonConvert.DeserializeObject<JDISKey>(value);
				requester.authKey = authKey;
				requester.Initialize();
			}
			else
			{
				requester.WebService.Request<JDISAuthRequest, JDISAuthResponse>(new JDISAuthRequest(), delegate(JDISResponse<JDISAuthResponse> res)
				{
					requester.authKey = res.getData().key();
					requester.authKey.storeKey();
					requester.Initialize();
				});
			}
		}

		public static JDISKey fromAuthResponse(JDISAuthResponse r)
		{
			return new JDISKey
			{
				ID = r.ID,
				KEY1 = r.KEY1,
				KEY2 = r.KEY2
			};
		}

		public void storeKey()
		{
			string contents = JsonConvert.SerializeObject(this);
			File.WriteAllText(Path.Combine(Application.dataPath, "auth.token"), contents);
			Debug.Log("Key stored to: " + Path.Combine(Application.dataPath, "auth.token"));
		}

		public string ID;

		public string KEY1;

		public string KEY2;
	}
}
