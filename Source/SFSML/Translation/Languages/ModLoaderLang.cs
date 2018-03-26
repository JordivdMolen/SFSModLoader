using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFSML.Translation.Languages
{
    public class ModLoaderLang
    {
        public String Version =                 "1.0.3.B1";
        public String LoaderTag =               "Hydria [1.0.3.R1]";
        public String LoaderVersion =           "1.0.3.R1";
        public String EntryMessage =            "Loading Hydria Modding Environment Version: {0}";
        public String LoadingMod =              "Loading {0}, please wait.";
        public String ModEntry =                "\n#-{0} [{1}]#-\n{2}";
        public String ModNoAssetFolder =        "{0} doesn't have the Asset Folder.\nSkipping asset load procedure.";
        public String AchievementsDisabled =    "Whoops, achievements are disabled in SFSML";
        public String ModloaderInitated =       "%LoaderTag% got initiated trough: {0}.";
    }
}