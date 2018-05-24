using System;
using System.Collections.Generic;
using JMtech.JDIS.Auth;
using JMtech.JDIS.Web;
using JMtech.JDIS.Web.Request;
using JMtech.JDIS.Web.Response;

namespace JMtech.JDIS
{
	public class JDISClient
	{
		public JDISClient()
		{
			JDISKey.getAuthDetails(this);
		}

		public bool Initialized()
		{
			return this.authKey != null;
		}

		public void Initialize()
		{
		}

		public void UploadRocket(string title, List<string> tags, string jsonData, Action<JDISResponse<object>> runAfterRequest)
		{
			JDISUploadRocket req = new JDISUploadRocket(title, tags, jsonData, Ref.hasPartsExpansion);
			this.WebService.Request<JDISUploadRocket, object>(req, runAfterRequest);
		}

		public void DownloadRocket(string id, Action<JDISResponse<JDISRocket>> runAfterRequest)
		{
			JDISRetreiveRocket req = new JDISRetreiveRocket(id);
			this.WebService.Request<JDISRetreiveRocket, JDISRocket>(req, runAfterRequest);
		}

		public void LoadPage(string pageCategory, int page, Action<JDISResponse<string[]>> runAfterRequest)
		{
			JDISRequestPage jdisrequestPage = new JDISRequestPage();
			jdisrequestPage.page = page;
			jdisrequestPage.category = pageCategory;
			this.WebService.Request<JDISRequestPage, string[]>(jdisrequestPage, runAfterRequest);
		}

		public void SelectiveRequest(string id, List<string> select, Action<JDISResponse<JDISSelective>> runAfterRequest)
		{
			JDISSelectiveRequest req = new JDISSelectiveRequest(id, select);
			this.WebService.Request<JDISSelectiveRequest, JDISSelective>(req, runAfterRequest);
		}

		public JDISKey authKey;

		public JDISWebService WebService = new JDISWebService();
	}
}
