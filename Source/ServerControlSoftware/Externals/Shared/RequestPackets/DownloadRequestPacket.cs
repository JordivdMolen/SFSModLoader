using System;

namespace ServerControlSoftware.Externals.Shared.RequestPackets
{
	public class DownloadRequestPacket : AuthedPacket
	{
		public string targetRocket;
	}
}
