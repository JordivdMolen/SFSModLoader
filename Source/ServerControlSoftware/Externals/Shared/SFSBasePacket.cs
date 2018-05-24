using System;
using ServerTransferProgram.ServerControlLiberary.DataControllers.PacketSystem;

namespace ServerControlSoftware.Externals.Shared
{
	public class SFSBasePacket : DefaultPacket
	{
		public bool Authed;

		public int AuthID = -1;
	}
}
