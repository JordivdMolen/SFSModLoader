using System;
using ServerControlFramework;
using ServerTransferProgram.LogicControllers;
using UnityEngine;

namespace JMtech.Main
{
	public class Checker : UpdateableObject
	{
		public Checker()
		{
			Debug.Log("Checker has started");
		}

		[DelayedUpdateTPS(15)]
		private void Update()
		{
			SyncContext.RunOnUI(delegate
			{
				if (!Application.isPlaying)
				{
					UpdateableObject.Running = false;
				}
			});
		}
	}
}
