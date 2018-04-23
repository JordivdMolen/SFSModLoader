using System;
using UnityEngine;

namespace NewBuildSystem
{
	public static class Utility
	{
		public static bool IsInsideRange(float value, float a, float b, bool canBeInsideEdge)
		{
			bool result;
			if (canBeInsideEdge)
			{
				result = (value >= Mathf.Min(a, b) && value <= Mathf.Max(a, b));
			}
			else
			{
				result = (value > Mathf.Min(a, b) && value < Mathf.Max(a, b));
			}
			return result;
		}

		public static bool IsInsideRange(float value, float a, float b, bool canBeInsideEdge, float edge)
		{
			bool result;
			if (canBeInsideEdge)
			{
				result = (value >= Mathf.Min(a, b) - edge && value <= Mathf.Max(a, b) + edge);
			}
			else
			{
				result = (value > Mathf.Min(a, b) - edge && value < Mathf.Max(a, b) + edge);
			}
			return result;
		}

		public static double Overlap(double a1, double a2, double b1, double b2)
		{
			return Math.Min(Math.Max(a1, a2), Math.Max(b1, b2)) - Math.Max(Math.Min(a1, a2), Math.Min(b1, b2));
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
				bool flag = descriptionRaw[i].ToString() == "/";
				if (flag)
				{
					num++;
				}
			}
			return num;
		}

		public static void DrawSquare(Vector2 posA, Vector2 posB, float time, Color color)
		{
			Debug.DrawLine(posA, new Vector3(posA.x, posB.y), color, time);
			Debug.DrawLine(posA, new Vector3(posB.x, posA.y), color, time);
			Debug.DrawLine(posB, new Vector3(posA.x, posB.y), color, time);
			Debug.DrawLine(posB, new Vector3(posB.x, posA.y), color, time);
			Debug.DrawLine(posB, posB, Color.red, time);
		}

		public static void DrawCircle(Vector2 position, float radius, int resolution, Color color)
		{
			for (float num = 0f; num < (float)resolution; num += 1f)
			{
				Vector2 v = position + new Vector2(Mathf.Cos(num / (float)resolution * 3.14159274f * 2f), Mathf.Sin(num / (float)resolution * 3.14159274f * 2f)) * radius;
				Vector2 v2 = position + new Vector2(Mathf.Cos((num + 1f) / (float)resolution * 3.14159274f * 2f), Mathf.Sin((num + 1f) / (float)resolution * 3.14159274f * 2f)) * radius;
				Debug.DrawLine(v, v2, color);
			}
		}

		public static Vector3 CameraRelativePosition(float fieldOfView, Vector3 posPixel)
		{
			Vector2 vector = new Vector2((posPixel.x - (float)(Screen.width / 2)) / (float)Screen.height, (posPixel.y - (float)(Screen.height / 2)) / (float)Screen.height);
			float f = fieldOfView * 0.0174532924f;
			float num = Mathf.Sin(f) / ((1f + Mathf.Cos(f)) * 0.5f);
			return new Vector3(vector.x * posPixel.z * num, vector.y * posPixel.z * num, posPixel.z);
		}

		public static Vector2 RotateZ(Vector2 a, float b)
		{
			float num = Mathf.Cos(b);
			float num2 = Mathf.Sin(b);
			return new Vector2(a.x * num - a.y * num2, a.x * num2 + a.y * num);
		}
	}
}
