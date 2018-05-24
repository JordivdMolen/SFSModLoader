using System;
using UnityEngine;

public class FlameRandomizer : MonoBehaviour
{
	private void Update()
	{
		if (!Ref.mapView)
		{
			base.transform.localScale = new Vector3(UnityEngine.Random.Range(this.min.x, this.max.x), UnityEngine.Random.Range(this.min.y, this.max.y));
		}
	}

	public Vector2 min;

	public Vector2 max;
}
