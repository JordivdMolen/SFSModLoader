using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SeparatorModule : Module
{
	public override List<object> SaveVariables
	{
		get
		{
			return new List<object>
			{
				this.separated
			};
		}
	}

	public override List<string> DescriptionVariables
	{
		get
		{
			return (!this.showParametersDescription) ? new List<string>() : new List<string>
			{
				"Separation Force: " + this.separationForce.magnitude.ToString() + "kN"
			};
		}
	}

	public override void OnPartUsed()
	{
		if (this.separated.boolValue)
		{
			return;
		}
		if (Ref.timeWarping)
		{
			MsgController.ShowMsg("Cannot use part while time warping");
			return;
		}
		this.separated.boolValue = true;
		this.onSeparate.Invoke();
	}

	public void SetSeparated(bool value)
	{
		this.separated.boolValue = value;
	}

	public void SeparateSurface(int surfaceId)
	{
		List<Part> list = new List<Part>();
		List<float> list2 = new List<float>();
		float num = 0f;
		for (int i = 0; i < this.part.joints.Count; i++)
		{
			int num2 = (!(this.part.joints[i].fromPart == this.part)) ? this.part.joints[i].toSurfaceIndex : this.part.joints[i].fromSurfaceIndex;
			if (num2 == surfaceId)
			{
				list.Add((!(this.part.joints[i].fromPart == this.part)) ? this.part.joints[i].fromPart : this.part.joints[i].toPart);
				list2.Add((!(this.part.joints[i].fromPart == this.part)) ? this.part.joints[i].coveredAmount : this.part.joints[i].coveredAmount);
				num += list2[list2.Count - 1];
				Part.DestroyJoint(this.part.joints[i], true);
				i--;
			}
		}
		Vector2 relativePoint = this.part.vessel.partsManager.rb2d.GetRelativePoint(base.transform.localPosition + this.part.centerOfMass * this.part.orientation);
		Vector2 a = Quaternion.Euler(0f, 0f, base.transform.rotation.eulerAngles.z) * new Vector2(this.separationForce.x * (float)this.part.orientation.x, this.separationForce.y * (float)this.part.orientation.y);
		for (int j = 0; j < list.Count; j++)
		{
			if (list[j].vessel.partsManager.rb2d != null && !float.IsNaN(list2[j] / num))
			{
				list[j].vessel.partsManager.rb2d.AddForceAtPosition(a * (list2[j] / num), relativePoint);
			}
		}
		if (list.Count > 0 && this.part.vessel.partsManager.rb2d != null)
		{
			this.part.vessel.partsManager.rb2d.AddForceAtPosition(-a, relativePoint);
		}
	}

	public void ApplyForce()
	{
		Vector2 relativePoint = this.part.vessel.partsManager.rb2d.GetRelativePoint(base.transform.localPosition + this.part.centerOfMass * this.part.orientation);
		Vector2 force = Quaternion.Euler(0f, 0f, base.transform.rotation.eulerAngles.z) * new Vector2(this.separationForce.x * (float)this.part.orientation.x, this.separationForce.y * (float)this.part.orientation.y);
		this.part.vessel.partsManager.rb2d.AddForceAtPosition(force, relativePoint);
	}

	public void SideSeparator()
	{
		Part part = CreateRocket.CreatePart(this.leftHalfPrefab, this.part.orientation, this.part.transform.parent, this.part.transform.localPosition);
		Part part2 = CreateRocket.CreatePart(this.rightHalfPrefab, this.part.orientation, this.part.transform.parent, this.part.transform.localPosition);
		SeparatorModule.TransferJoints(this.part, 0, part, 0);
		SeparatorModule.TransferJoints(this.part, 1, part2, 0);
		this.part.vessel.partsManager.parts.Add(part);
		this.part.vessel.partsManager.parts.Add(part2);
		this.part.vessel.partsManager.UpdatePartsGruping(true, this.part.vessel);
		part.UsePart();
		part2.UsePart();
		this.part.DestroyPart(false, true);
	}

	public static void TransferJoints(Part part, int transferSurfaceId, Part transferToPart, int toSurfaceId)
	{
		for (int i = 0; i < part.joints.Count; i++)
		{
			Part.Joint joint = part.joints[i];
			bool flag = joint.fromPart == part;
			if (((!flag) ? joint.toSurfaceIndex : joint.fromSurfaceIndex) == transferSurfaceId)
			{
				if (flag)
				{
					joint.fromPart = transferToPart;
					joint.fromSurfaceIndex = toSurfaceId;
				}
				else
				{
					joint.toPart = transferToPart;
					joint.toSurfaceIndex = toSurfaceId;
				}
				transferToPart.joints.Add(joint);
				part.joints.RemoveAt(i);
				i--;
			}
		}
	}

	public BoolValueHolder separated;

	public Vector2 separationForce;

	public Transform leftHalfPrefab;

	public Transform rightHalfPrefab;

	public bool showParametersDescription = true;

	public UnityEvent onSeparate;
}
