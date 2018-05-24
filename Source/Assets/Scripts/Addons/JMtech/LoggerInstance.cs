using System;
using ServerControlFramework;
using UnityEngine;

namespace Assets.Scripts.Addons.JMtech
{
	public class LoggerInstance : LoggerContext
	{
		public override void Log(object log, Color color, bool bold)
		{
			string str = log.ToString();
			if (bold)
			{
				str = "<b>" + str + "</b>";
			}
			Debug.Log(string.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", new object[]
			{
				(byte)(color.r * 255f),
				(byte)(color.g * 255f),
				(byte)(color.b * 255f),
				log
			}));
		}
	}
}
