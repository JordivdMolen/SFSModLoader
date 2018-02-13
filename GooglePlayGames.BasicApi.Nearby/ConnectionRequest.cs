using GooglePlayGames.OurUtils;
using System;

namespace GooglePlayGames.BasicApi.Nearby
{
	public struct ConnectionRequest
	{
		private readonly EndpointDetails mRemoteEndpoint;

		private readonly byte[] mPayload;

		public EndpointDetails RemoteEndpoint
		{
			get
			{
				return this.mRemoteEndpoint;
			}
		}

		public byte[] Payload
		{
			get
			{
				return this.mPayload;
			}
		}

		public ConnectionRequest(string remoteEndpointId, string remoteEndpointName, string serviceId, byte[] payload)
		{
			Logger.d("Constructing ConnectionRequest");
			this.mRemoteEndpoint = new EndpointDetails(remoteEndpointId, remoteEndpointName, serviceId);
			this.mPayload = Misc.CheckNotNull<byte[]>(payload);
		}
	}
}
