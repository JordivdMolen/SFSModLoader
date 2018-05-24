using System;
using ServerTransferProgram.ServerControlLiberary.DataControllers.PacketSystem;

namespace ServerControlSoftware.Externals.Shared.Packets
{
	public class ServerErrorPacket : DefaultPacket
	{
		public ServerErrorPacket(string task, string error)
		{
			base.SetError(error);
			this.Task = task;
		}

		public string Task = string.Empty;
	}
}
