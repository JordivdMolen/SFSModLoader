using System;
using UnityEngine;

namespace SFSML.HookSystem.ReWork.BaseHooks.UtilHooks
{
	public class MyKeyUpHook : MyHook
	{
		public MyKeyUpHook(KeyCode key)
		{
			this.keyUp = key;
		}

		public KeyCode keyUp;

		public bool register;
	}
}
