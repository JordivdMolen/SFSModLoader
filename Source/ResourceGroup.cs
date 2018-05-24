using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class ResourceGroup : MonoBehaviour
{
	public void Initialize(List<ResourceModule> resourceModulesAll, int currentIndex, Resource.Type newResourceType)
	{
		this.resourceType = newResourceType;
		this.resourceModules = new List<ResourceModule>();
		this.flowModules = new List<FlowModule>();
		if (Resource.GrupsGlobally(this.resourceType))
		{
			for (int i = currentIndex; i < resourceModulesAll.Count; i++)
			{
				if (resourceModulesAll[i].resourceType == this.resourceType)
				{
					this.AddResourceModule(resourceModulesAll[i]);
				}
			}
		}
		else
		{
			this.GetConnectedResourceModules(resourceModulesAll[currentIndex]);
		}
		this.SetTanks();
		this.empty = (this.resourcePercent == 0.0);
		this.full = (this.resourcePercent >= 1.0);
	}

	private void AddResourceModule(ResourceModule newModule)
	{
		this.resourceModules.Add(newModule);
		this.resourceSpace += (double)newModule.resourceSpace;
		this.resourceAmount += (double)newModule.resourceAmount.floatValue;
		newModule.resourceGroup = this;
	}

	private void GetConnectedResourceModules(ResourceModule start)
	{
		this.AddResourceModule(start);
		Ref.connectionCheckId++;
		start.part.connectionCheckId = Ref.connectionCheckId;
		List<Part> list = new List<Part>(1)
		{
			start.part
		};
		while (list.Count > 0)
		{
			for (int i = 0; i < list[0].joints.Count; i++)
			{
				if (list[0].joints[i].resourceFlow)
				{
					Part part = (!(list[0].joints[i].fromPart == list[0])) ? list[0].joints[i].fromPart : list[0].joints[i].toPart;
					if (part.connectionCheckId != Ref.connectionCheckId)
					{
						part.connectionCheckId = Ref.connectionCheckId;
						ResourceModule resourceModule = part.GetResourceModule();
						if (!(resourceModule == null) && start.resourceType == resourceModule.resourceType)
						{
							list.Add(part);
							this.AddResourceModule(resourceModule);
						}
					}
				}
			}
			list.RemoveAt(0);
		}
	}

	public void TakeResource(double amount)
	{
		if (this.resourceAmount > amount)
		{
			this.resourceAmount -= amount;
		}
		else
		{
			this.resourceAmount = 0.0;
			this.empty = true;
		}
		this.SetTanks();
		if (this.full)
		{
			this.full = false;
			for (int i = 0; i < this.flowModules.Count; i++)
			{
				if (this.flowModules[i].state == FlowModule.State.NoSpace && this.flowModules[i].flowType == FlowModule.FlowType.Positive)
				{
					this.flowModules[i].withSpace.Add(this);
					this.flowModules[i].UpdateState();
				}
			}
		}
	}

	public void AddResource(double amount)
	{
		if (this.resourceSpace - this.resourceAmount > amount)
		{
			this.resourceAmount += amount;
		}
		else
		{
			this.resourceAmount = this.resourceSpace;
			this.full = true;
		}
		this.SetTanks();
		if (this.empty)
		{
			this.empty = false;
			for (int i = 0; i < this.flowModules.Count; i++)
			{
				if (this.flowModules[i].state == FlowModule.State.NoResource && this.flowModules[i].flowType == FlowModule.FlowType.Negative)
				{
					this.flowModules[i].withResource.Add(this);
					this.flowModules[i].UpdateState();
				}
			}
		}
	}

	private void SetTanks()
	{
		this.resourcePercent = this.resourceAmount / this.resourceSpace;
		for (int i = 0; i < this.resourceModules.Count; i++)
		{
			this.resourceModules[i].resourceAmount.floatValue = this.resourceModules[i].resourceSpace * (float)this.resourcePercent;
		}
		if (this.fuelIcon != null)
		{
			this.fuelIcon.GetChild(0).localScale = new Vector3((float)this.resourcePercent, this.fuelIcon.GetChild(0).localScale.y, 1f);
		}
	}

	public void DestroyGroup()
	{
		UnityEngine.Object.Destroy(this);
		for (int i = 0; i < this.resourceModules.Count; i++)
		{
			this.resourceModules[i].resourceGroup = null;
		}
	}

	[BoxGroup]
	public Resource.Type resourceType;

	[BoxGroup]
	public double resourceAmount;

	[BoxGroup]
	public double resourceSpace;

	[BoxGroup]
	public double resourcePercent;

	[BoxGroup]
	public bool empty;

	[BoxGroup]
	public bool full;

	[BoxGroup]
	public Transform fuelIcon;

	[BoxGroup]
	public TextMesh counter;

	public List<ResourceModule> resourceModules;

	public List<FlowModule> flowModules;
}
