using GooglePlayGames.OurUtils;
using System;

namespace GooglePlayGames.BasicApi.Nearby
{
	public struct NearbyConnectionConfiguration
	{
		public const int MaxUnreliableMessagePayloadLength = 1168;

		public const int MaxReliableMessagePayloadLength = 4096;

		private readonly Action<InitializationStatus> mInitializationCallback;

		private readonly long mLocalClientId;

		public long LocalClientId
		{
			get
			{
				return this.mLocalClientId;
			}
		}

		public Action<InitializationStatus> InitializationCallback
		{
			get
			{
				return this.mInitializationCallback;
			}
		}

		public NearbyConnectionConfiguration(Action<InitializationStatus> callback, long localClientId)
		{
			this.mInitializationCallback = Misc.CheckNotNull<Action<InitializationStatus>>(callback);
			this.mLocalClientId = localClientId;
		}
	}
}
