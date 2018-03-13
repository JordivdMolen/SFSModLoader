using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SFSML.HookSystem.ReWork.BaseHooks.UtilHooks
{
    public class MyKeyUpHook : MyHook
    {
        public KeyCode keyUp;
        public bool register;
        public MyKeyUpHook(KeyCode key)
        {
            this.keyUp = key;
        }
    }
}