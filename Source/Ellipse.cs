using System;
using UnityEngine;

public class Ellipse : MonoBehaviour
{
	private void Start()
	{
		Vector3[] array = this.CreateEllipse(this.x, this.y, this.resolution);
		this.lr.sortingOrder = 6;
		this.lr.sortingLayerName = "Map";
		this.lr.positionCount = this.resolution + 1;
		for (int i = 0; i <= this.resolution; i++)
		{
			this.lr.SetPosition(i, array[i]);
		}
	}

	private Vector3[] CreateEllipse(float x, float y, int resolution)
	{
		Vector3[] array = new Vector3[resolution + 1];
		float num = 6.28319f / (float)resolution;
		for (int i = 0; i <= resolution; i++)
		{
			float f = num * (float)i;
			array[i] = new Vector3(Mathf.Cos(f) * x, Mathf.Sin(f) * y, 0f);
		}
		return array;
	}

	public float x = 5f;

	public float y = 3f;

	public int resolution;

	public LineRenderer lr;
}
