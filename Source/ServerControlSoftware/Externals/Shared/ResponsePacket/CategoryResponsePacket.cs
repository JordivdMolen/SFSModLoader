using System;
using System.Collections.Generic;
using ServerTransferProgram.ServerControlLiberary.DataControllers.PacketSystem;

namespace ServerControlSoftware.Externals.Shared.ResponsePacket
{
	public class CategoryResponsePacket : DefaultPacket
	{
		public List<string> onPage = new List<string>();
	}
}
