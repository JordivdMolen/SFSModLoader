using System;
using UnityEngine;

namespace JMtech.UIController
{
	public class ColorController : MonoBehaviour
	{
		[Range(0f, 255f)]
		public int R;

		[Range(0f, 255f)]
		public int G;

		[Range(0f, 255f)]
		public int B;

		[Range(0f, 255f)]
		public int A;
	}
}
