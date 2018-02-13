using System;
using UnityEngine;

namespace NewBuildSystem
{
	public static class Utility
	{
		public static bool IsInsideRange(float value, float a, float b, bool canBeInsideEdge)
		{
			if (canBeInsideEdge)
			{
				return value >= Mathf.Min(a, b) && value <= Mathf.Max(a, b);
			}
			return value > Mathf.Min(a, b) && value < Mathf.Max(a, b);
		}

		public static bool LineOverlaps(float a1, float a2, float b1, float b2)
		{
			return Mathf.Max(a1, a2) - 0.01f > Mathf.Min(b1, b2) && Mathf.Max(b1, b2) - 0.01f > Mathf.Min(a1, a2);
		}

		public static float Overlap(float a1, float a2, float b1, float b2)
		{
			return Utility.RoundToHalf(Mathf.Min(Mathf.Max(a1, a2), Mathf.Max(b1, b2)) - Mathf.Max(Mathf.Min(a1, a2), Mathf.Min(b1, b2)));
		}

		public static bool IsInside(float a1, float a2, float b1, float b2)
		{
			return Utility.IsInsideRange(a1, b1, b2, true) && Utility.IsInsideRange(a2, b1, b2, true);
		}

		public static Vector2 RoundToHalf(Vector2 a)
		{
			return new Vector2(Mathf.Round(a.x * 2f) / 2f, Mathf.Round(a.y * 2f) / 2f);
		}

		public static float RoundToHalf(float a)
		{
			return Mathf.Round(a * 2f) / 2f;
		}

		public static void DrawCross(Vector2 position, float size)
		{
			Debug.DrawRay(position + Vector2.down * size / 2f, Vector2.up * size, Color.red);
			Debug.DrawRay(position + Vector2.left * size / 2f, Vector2.right * size, Color.red);
		}

		public static Transform GetByPath(Transform root, int[] path)
		{
			for (int i = 0; i < path.Length; i++)
			{
				root = root.GetChild(path[i]);
			}
			return root;
		}

		public static Vector2 ToDepth(Vector3 worldPos, float targetDepth)
		{
			Vector2 vector = Camera.main.WorldToScreenPoint(worldPos);
			return Camera.main.ScreenToWorldPoint(new Vector3(vector.x, vector.y, targetDepth));
		}

		public static float GetDeltaV(float isp, float fullMass, float dryMass)
		{
			return isp * 9.8f * Mathf.Log(fullMass / dryMass);
		}

		public static string SplitLines(string textToSplit)
		{
			string text = string.Empty;
			for (int i = 0; i < textToSplit.Length; i++)
			{
				text += ((!(textToSplit[i].ToString() == "/")) ? ((!(textToSplit[i].ToString() == "&")) ? textToSplit[i].ToString() : "/") : "\r\n");
			}
			return text;
		}

		public static int GetLinesCount(string descriptionRaw)
		{
			int num = 0;
			for (int i = 0; i < descriptionRaw.Length; i++)
			{
				if (descriptionRaw[i].ToString() == "/")
				{
					num++;
				}
			}
			return num;
		}
	}
}
