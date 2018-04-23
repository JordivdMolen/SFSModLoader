using System;
using UnityEngine;

public class PartChildCollider : MonoBehaviour
{
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
		bool flag = !this.part.vessel.partsManager.partToPartDamage;
		if (!flag)
		{
			bool flag2 = collision.relativeVelocity.sqrMagnitude < this.breakVelocity * this.breakVelocity;
			if (!flag2)
			{
				this.part.DestroyPart(true, false);
			}
		}
	}

	public PartChildCollider()
	{
	}

	public float breakVelocity = 5f;

	public Part part;
}
