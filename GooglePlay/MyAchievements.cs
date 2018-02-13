using System;
using UnityEngine;

namespace GooglePlay
{
	public class MyAchievements : MonoBehaviour
	{
		public static MyAchievements main;

		private void Start()
		{
			MyAchievements.main = this;
			this.SignIn();
		}

		private void SignIn()
		{
			Social.localUser.Authenticate(delegate(bool success)
			{
			});
		}

		public void UnlockAchievement(string achievementsId)
		{
			Social.ReportProgress(achievementsId, 100.0, delegate(bool success)
			{
			});
		}

		public void ShowAchievementsUI()
		{
			Social.ShowAchievementsUI();
		}
	}
}
