using System;

namespace ServerControlSoftware.Externals.Shared.RequestPackets
{
	public class CategoryRequestPacket : AuthedPacket
	{
		public int Page;

		public int PageLenght;

		public SharingCategory Category;
	}
}
