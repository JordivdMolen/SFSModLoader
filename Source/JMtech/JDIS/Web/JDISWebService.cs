using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using JMtech.JDIS.Web.Request;
using JMtech.Main;
using Newtonsoft.Json;
using UnityEngine;

namespace JMtech.JDIS.Web
{
	public class JDISWebService
	{
		public JDISWebService()
		{
			this.wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
			ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(this.MyRemoteCertificateValidationCallback);
		}

		public bool MyRemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			bool result = true;
			if (sslPolicyErrors != SslPolicyErrors.None)
			{
				for (int i = 0; i < chain.ChainStatus.Length; i++)
				{
					if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
					{
						chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
						chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
						chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
						chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
						if (!chain.Build((X509Certificate2)certificate))
						{
							result = false;
							break;
						}
					}
				}
			}
			return result;
		}

		public void Request<T, T1>(T req, Action<JDISResponse<T1>> onComplete)
		{
			JDISRequest<T> jdisrequest = new JDISRequest<T>();
			if (GameTracker.Tracker.AuthClient != null && GameTracker.Tracker.AuthClient.Initialized())
			{
				jdisrequest.Authenticate(GameTracker.Tracker.AuthClient);
			}
			JDISRequestExtension jdisrequestExtension = (JDISRequestExtension)((object)req);
			jdisrequest.DATA = (T)((object)jdisrequestExtension.Initiate(jdisrequest));
			string text = JsonConvert.SerializeObject(jdisrequest);
			NameValueCollection nameValueCollection = new NameValueCollection();
			nameValueCollection["JDIS"] = text;
			UnityEngine.Debug.Log("REQUESTING: " + text);
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			byte[] bytes = this.wc.UploadValues("https://jmnet.one/SFSAPI/core.php?JDIS", nameValueCollection);
			string @string = Encoding.Default.GetString(bytes);
			stopwatch.Stop();
			UnityEngine.Debug.Log("JDIS Response: " + @string);
			UnityEngine.Debug.Log("Server response time: " + stopwatch.ElapsedMilliseconds);
			bool flag = false;
			JDISResponse<T1> obj = null;
			try
			{
				obj = JsonConvert.DeserializeObject<JDISResponse<T1>>(@string);
				flag = true;
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.Log("mainJson Parse Error: " + ex.Message);
			}
			if (flag)
			{
				onComplete(obj);
				return;
			}
			JDISResponse<string> jdisresponse = JsonConvert.DeserializeObject<JDISResponse<string>>(@string);
			onComplete(new JDISResponse<T1>
			{
				type = jdisresponse.type,
				sec = jdisresponse.sec,
				errorMessage = jdisresponse.msg,
				rID = jdisresponse.rID
			});
		}

		private WebClient wc = new WebClient();

		private Dictionary<int, UploadStringCompletedEventHandler> requests = new Dictionary<int, UploadStringCompletedEventHandler>();
	}
}
