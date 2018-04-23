using System;
using System.Collections.Generic;
using NewBuildSystem;
using UnityEngine;

public class DockingPortModule : Module
{
	private void OnEnable()
	{
		DockingPortModule.dockingPorts.Add(this);
		MonoBehaviour.print("enabled");
	}

	private void OnDisable()
	{
		DockingPortModule.dockingPorts.Remove(this);
		MonoBehaviour.print("disabled");
	}

	public override void OnPartLoaded()
	{
		for (int i = 0; i < this.part.joints.Count; i++)
		{
			Part part = (!(this.part.joints[i].fromPart == this.part)) ? this.part.joints[i].fromPart : this.part.joints[i].toPart;
			bool flag = part.HasDockingPortModule();
			if (flag)
			{
				base.enabled = false;
			}
		}
	}

	private void Update()
	{
		for (int i = 0; i < DockingPortModule.dockingPorts.Count; i++)
		{
			bool flag = this.part.vessel != DockingPortModule.dockingPorts[i].part.vessel && this != DockingPortModule.dockingPorts[i] && DockingPortModule.dockingPorts[i].part.vessel != Ref.mainVessel;
			if (flag)
			{
				bool flag2 = (base.transform.position - DockingPortModule.dockingPorts[i].transform.position).sqrMagnitude <= this.pullDistance * this.pullDistance;
				if (flag2)
				{
					this.part.vessel.partsManager.rb2d.AddForceAtPosition(-(base.transform.position - DockingPortModule.dockingPorts[i].transform.position).normalized * this.pullForce * Time.fixedDeltaTime, this.part.vessel.partsManager.rb2d.GetRelativePoint(this.part.transform.localPosition));
					bool flag3 = (base.transform.position - DockingPortModule.dockingPorts[i].transform.position).sqrMagnitude < this.dockDistance * this.dockDistance;
					if (flag3)
					{
						this.Dock(DockingPortModule.dockingPorts[i]);
					}
				}
			}
		}
	}

	private void Dock(DockingPortModule other)
	{
		new Part.Joint(Vector2.zero, this.part, other.part, 0, 0, false);
		Vector2 vector = Vector2.up * this.part.orientation;
		Vector2 vector2 = Vector2.up * other.part.orientation;
		Orientation orientation = new Orientation(1, 1, (int)(Mathf.Atan2(vector.y, vector.x) * 57.29578f - Mathf.Atan2(vector2.y, vector2.x) * 57.29578f) + 180);
		orientation.z = Mathf.RoundToInt((float)orientation.z / 90f) * 90;
		for (int i = 0; i < other.part.vessel.partsManager.parts.Count; i++)
		{
			other.part.vessel.partsManager.parts[i].orientation += orientation;
			for (int j = 0; j < other.part.vessel.partsManager.parts[i].joints.Count; j++)
			{
				bool flag = other.part.vessel.partsManager.parts[i].joints[j].fromPart == other.part.vessel.partsManager.parts[i];
				if (flag)
				{
					other.part.vessel.partsManager.parts[i].joints[j].anchor *= orientation;
				}
			}
		}
		this.part.vessel.MergeVessel(other.part.vessel);
		this.part.UpdateConnected();
		other.part.UpdateConnected();
		base.enabled = false;
		other.enabled = false;
	}

	public void EnableOther()
	{
		for (int i = 0; i < this.part.joints.Count; i++)
		{
			Part part = (!(this.part.joints[i].fromPart == this.part)) ? this.part.joints[i].fromPart : this.part.joints[i].toPart;
			bool flag = part.HasDockingPortModule();
			if (flag)
			{
				part.GetComponent<DockingPortModule>().enabled = true;
			}
		}
	}

	public void Enable()
	{
		base.enabled = true;
	}

	public DockingPortModule()
	{
	}

	static DockingPortModule()
	{
		// Note: this type is marked as 'beforefieldinit'.
	}

	public float pullDistance;

	public float pullForce;

	public float dockDistance;

	private static List<DockingPortModule> dockingPorts = new List<DockingPortModule>();
}
