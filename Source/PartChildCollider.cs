using System;
using UnityEngine;

public class PartChildCollider : MonoBehaviour
{
	private void OnValidate()
	{
		Transform parent = base.transform.parent;
		if (parent != null)
		{
			while (parent.GetComponent<Part>() == null)
			{
				if (!(parent.parent != null))
				{
					MonoBehaviour.print(parent.name);
					break;
				}
				parent = parent.parent;
			}
			Part component = parent.GetComponent<Part>();
			this.part = ((!(component != null)) ? this.part : component);
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (Ref.unbreakableParts)
		{
			return;
		}
		if (!this.part.vessel.partsManager.partToPartDamage)
		{
			return;
		}
		if (collision.relativeVelocity.sqrMagnitude < this.breakVelocity * this.breakVelocity)
		{
			return;
		}
		this.part.DestroyPart(true, false);
	}

	public float breakVelocity = 5f;

	public Part part;
}
