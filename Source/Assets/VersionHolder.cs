using System;
using UnityEngine;

namespace Assets
{
	public static class VersionHolder
	{
		public static int CompareVersion(this string subject, string toCompareTo)
		{
			string[] array = subject.Split(new char[]
			{
				'.'
			});
			string[] array2 = toCompareTo.Split(new char[]
			{
				'.'
			});
			int num = 0;
			while (num < array2.Length && num < array.Length)
			{
				int num2 = int.Parse(array[num]);
				int num3 = int.Parse(array2[num]);
				if (num2 > num3)
				{
					Debug.Log(string.Concat(new object[]
					{
						num2,
						">",
						num3,
						" on ",
						subject,
						" with ",
						toCompareTo
					}));
					return 1;
				}
				if (num2 < num3)
				{
					Debug.Log(string.Concat(new object[]
					{
						num2,
						"<",
						num3,
						" on ",
						subject,
						" with ",
						toCompareTo
					}));
					return -1;
				}
				num++;
			}
			return 0;
		}

		public static string SharingVersion = "0.5";

		public static string ProtocolVersion = "1.0";

		public static string ClientVersion = "1.35";
	}
}
