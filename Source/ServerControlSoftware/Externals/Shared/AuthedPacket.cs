using System;
using ServerControlSoftware.Externals.Shared.Objects;
using ServerTransferProgram.ServerControlLiberary.DataControllers.PacketSystem;

namespace ServerControlSoftware.Externals.Shared
{
	public class AuthedPacket : DefaultPacket
	{
		public void Authenticate(AuthID creds)
		{
			this.Credentials = creds;
		}

		public bool Authed()
		{
			return this.Credentials != null;
		}

		public AuthID Credentials;
	}
}
