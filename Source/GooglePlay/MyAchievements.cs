using System;
using UnityEngine;

namespace GooglePlay
{
	public class MyAchievements : MonoBehaviour
	{
		private void Start()
		{
			MyAchievements.main = this;
		}

		private void SignIn()
		{
		}

		public void UnlockAchievement(string achievementsId)
		{
		}

		public void ShowAchievementsUI()
		{
		}

		public static MyAchievements main;
	}
}
