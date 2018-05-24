using System;
using ServerTransferProgram.ServerControlLiberary.DataControllers.PacketSystem;

namespace ServerControlFramework.ServerControlLiberary.DataControllers.PacketSystem.Packets
{
	public class MessagePacket : DefaultPacket
	{
		public MessagePacket(string content)
		{
			this.inside = content;
		}

		public string inside;

		public string orgin;
	}
}
