using GooglePlayGames.OurUtils;
using System;
using System.Linq;

namespace GooglePlayGames.BasicApi.Video
{
	public class VideoCapabilities
	{
		private bool mIsCameraSupported;

		private bool mIsMicSupported;

		private bool mIsWriteStorageSupported;

		private bool[] mCaptureModesSupported;

		private bool[] mQualityLevelsSupported;

		public bool IsCameraSupported
		{
			get
			{
				return this.mIsCameraSupported;
			}
		}

		public bool IsMicSupported
		{
			get
			{
				return this.mIsMicSupported;
			}
		}

		public bool IsWriteStorageSupported
		{
			get
			{
				return this.mIsWriteStorageSupported;
			}
		}

		internal VideoCapabilities(bool isCameraSupported, bool isMicSupported, bool isWriteStorageSupported, bool[] captureModesSupported, bool[] qualityLevelsSupported)
		{
			this.mIsCameraSupported = isCameraSupported;
			this.mIsMicSupported = isMicSupported;
			this.mIsWriteStorageSupported = isWriteStorageSupported;
			this.mCaptureModesSupported = captureModesSupported;
			this.mQualityLevelsSupported = qualityLevelsSupported;
		}

		public bool SupportsCaptureMode(VideoCaptureMode captureMode)
		{
			if (captureMode != VideoCaptureMode.Unknown)
			{
				return this.mCaptureModesSupported[(int)captureMode];
			}
			Logger.w("SupportsCaptureMode called with an unknown captureMode.");
			return false;
		}

		public bool SupportsQualityLevel(VideoQualityLevel qualityLevel)
		{
			if (qualityLevel != VideoQualityLevel.Unknown)
			{
				return this.mQualityLevelsSupported[(int)qualityLevel];
			}
			Logger.w("SupportsCaptureMode called with an unknown qualityLevel.");
			return false;
		}

		public override string ToString()
		{
			string arg_A9_0 = "[VideoCapabilities: mIsCameraSupported={0}, mIsMicSupported={1}, mIsWriteStorageSupported={2}, mCaptureModesSupported={3}, mQualityLevelsSupported={4}]";
			object[] expr_0B = new object[5];
			expr_0B[0] = this.mIsCameraSupported;
			expr_0B[1] = this.mIsMicSupported;
			expr_0B[2] = this.mIsWriteStorageSupported;
			expr_0B[3] = string.Join(",", (from p in this.mCaptureModesSupported
			select p.ToString()).ToArray<string>());
			expr_0B[4] = string.Join(",", (from p in this.mQualityLevelsSupported
			select p.ToString()).ToArray<string>());
			return string.Format(arg_A9_0, expr_0B);
		}
	}
}
