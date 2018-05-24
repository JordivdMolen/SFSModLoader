using System;
using ServerControlSoftware.Externals.Shared.Objects;

namespace ServerControlSoftware.Externals.Shared.RequestPackets
{
	public class UploadRocketRequest : AuthedPacket
	{
		public CLRocket toUpload;

		public bool Premium = Ref.hasPartsExpansion;

		private string FukUrself_Dont_LOOK_in_My_Code = "FUCK OFF PLEASE";
	}
}
