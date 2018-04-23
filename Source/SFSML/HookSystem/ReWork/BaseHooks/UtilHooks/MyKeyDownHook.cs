using System;
using UnityEngine;

namespace SFSML.HookSystem.ReWork.BaseHooks.UtilHooks
{
	public class MyKeyDownHook : MyHook
	{
		public MyKeyDownHook(KeyCode key)
		{
			this.keyDown = key;
		}

		public KeyCode keyDown;

		public bool register;
	}
}
