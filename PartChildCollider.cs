using System;
using UnityEngine;

public class PartChildCollider : MonoBehaviour
{
	public Part part;

	private void OnValidate()
	{
		Transform parent = base.transform.parent;
		while (parent.GetComponent<Part>() == null)
		{
			parent = parent.parent;
		}
		this.part = parent.GetComponent<Part>();
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		this.part.Collision(collision);
	}
}
