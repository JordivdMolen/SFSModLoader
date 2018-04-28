using System;

namespace SFSML.Translation.Languages
{
	public class ModLoaderLang
	{
		public ModLoaderLang()
		{
		}

		public string Version = "1.3.1";

		public string LoaderTag = "Hydria [1.3.1]";

		public string LoaderVersion = "1.3.1";

		public string EntryMessage = "Loading Hydria Modding Environment Version: %LoaderVersion%";

		public string LoadingMod = "Loading {0}, please wait.";

		public string ModEntry = "\n#-{0} [{1}]#-\n{2}";

		public string ModNoAssetFolder = "{0} doesn't have the Asset Folder.\nSkipping asset load procedure.";

		public string AchievementsDisabled = "Whoops, achievements are disabled in SFSML";

		public string ModloaderInitated = "%LoaderTag% got initiated trough: {0}.";
	}
}
