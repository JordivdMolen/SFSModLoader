using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SFSML.HookSystem.ReWork.BaseHooks.SaveHooks
{
    public class MySaveLoadedHook : MyHook
    {
        public MySaveLoadedHook(GameSaving.GameSave save)
        {
            this.save = save;
        }

        public GameSaving.GameSave save;
    }
}
