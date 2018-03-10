using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SFSML.HookSystem.ReWork.BaseHooks.UtilHooks
{
    public class MyKeyDownHook : MyHook
    {
        public KeyCode keyDown;
        public bool register;
        public MyKeyDownHook(KeyCode key)
        {
            this.keyDown = key;
        }
    }
}