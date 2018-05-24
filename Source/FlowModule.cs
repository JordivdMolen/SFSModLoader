using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class FlowModule : Module
{
	public override List<string> DescriptionVariables
	{
		get
		{
			return (!this.showParametersDescription) ? new List<string>() : new List<string>
			{
				Resource.GetResourceName(this.resourceType) + ((this.flowType != FlowModule.FlowType.Negative) ? " generation: " : " consumption: ") + this.maxFlow.floatValue.ToString() + Resource.GetResourceUnit(this.resourceType)
			};
		}
	}

	public override bool IsSorted()
	{
		return true;
	}

	private void VoidStart()
	{
		this.UpdateState();
	}

	public void OnFlowChange(float empty)
	{
		this.GetSource();
		this.UpdateState();
	}

	public void FlowPerSecond(float empty)
	{
		this.UpdateState();
	}

	private void FixedUpdate()
	{
		if (this.flowType == FlowModule.FlowType.Negative)
		{
			if (Ref.infiniteFuel)
			{
				return;
			}
			this.FlowNegative();
		}
		else
		{
			this.FlowPositive();
		}
	}

	private void FlowNegative()
	{
		double amount = (double)this.flowPerSecond.floatValue * (double)Time.fixedDeltaTime / (double)this.withResource.Count;
		for (int i = 0; i < this.withResource.Count; i++)
		{
			this.withResource[i].TakeResource(amount);
			if (this.withResource[i].empty)
			{
				this.withResource.RemoveAt(i);
				i--;
				this.UpdateState();
			}
		}
	}

	private void FlowPositive()
	{
		double amount = (double)this.flowPerSecond.floatValue * (double)Time.fixedDeltaTime / (double)this.withSpace.Count;
		for (int i = 0; i < this.withSpace.Count; i++)
		{
			this.withSpace[i].AddResource(amount);
			if (this.withSpace[i].full)
			{
				this.withSpace.RemoveAt(i);
				i--;
				this.UpdateState();
			}
		}
	}

	public void UpdateState()
	{
		if (this.sources.Count == 0)
		{
			base.enabled = false;
			this.state = FlowModule.State.NoSource;
			this.onStateChange.Invoke();
			return;
		}
		if (this.flowType == FlowModule.FlowType.Negative)
		{
			if (this.withResource.Count == 0)
			{
				base.enabled = false;
				this.state = FlowModule.State.NoResource;
				this.onStateChange.Invoke();
				return;
			}
		}
		else if (this.withSpace.Count == 0)
		{
			base.enabled = false;
			this.state = FlowModule.State.NoSpace;
			this.onStateChange.Invoke();
			return;
		}
		base.enabled = (this.flowPerSecond.floatValue > 0f);
		this.state = FlowModule.State.Flowing;
		this.onStateChange.Invoke();
	}

	public void GetSource()
	{
		this.sources = new List<ResourceGroup>();
		this.withResource = new List<ResourceGroup>();
		this.withSpace = new List<ResourceGroup>();
		if (this.global || Resource.GrupsGlobally(this.resourceType))
		{
			for (int i = 0; i < this.part.vessel.partsManager.resourceGrups.Count; i++)
			{
				if (this.part.vessel.partsManager.resourceGrups[i].resourceType == this.resourceType)
				{
					this.AddSource(this.part.vessel.partsManager.resourceGrups[i]);
				}
			}
		}
		else
		{
			for (int j = 0; j < this.part.joints.Count; j++)
			{
				if (this.part.joints[j].resourceFlow)
				{
					ResourceModule resourceModule = ((!(this.part.joints[j].fromPart == this.part)) ? this.part.joints[j].fromPart : this.part.joints[j].toPart).GetResourceModule();
					if (!(resourceModule == null) && resourceModule.resourceType == this.resourceType)
					{
						this.AddSource(resourceModule.resourceGroup);
					}
				}
			}
		}
		this.UpdateState();
	}

	private void AddSource(ResourceGroup newSource)
	{
		this.sources.Add(newSource);
		if (newSource.resourcePercent > 0.0)
		{
			this.withResource.Add(newSource);
		}
		if (newSource.resourcePercent < 1.0)
		{
			this.withSpace.Add(newSource);
		}
		newSource.flowModules.Add(this);
	}

	public static bool ConfirmSources(FlowModule[] resourceSources, bool showMsg)
	{
		FlowModule.State state = FlowModule.State.Flowing;
		for (int i = 0; i < resourceSources.Length; i++)
		{
			if (resourceSources[i].state != FlowModule.State.Flowing)
			{
				if (resourceSources[i].state == FlowModule.State.NoSource)
				{
					if (showMsg)
					{
						MsgController.ShowMsg((resourceSources[i].state != FlowModule.State.NoSource) ? ("Out of " + Resource.GetResourceName(resourceSources[i].resourceType).ToLower()) : ("No " + Resource.GetResourceName(resourceSources[i].resourceType).ToLower() + " source"));
					}
					return false;
				}
				if (resourceSources[i].state == FlowModule.State.NoResource)
				{
					state = FlowModule.State.NoResource;
					if (showMsg)
					{
						MsgController.ShowMsg((resourceSources[i].state != FlowModule.State.NoSource) ? ("Out of " + Resource.GetResourceName(resourceSources[i].resourceType).ToLower()) : ("No " + Resource.GetResourceName(resourceSources[i].resourceType).ToLower() + " source"));
					}
				}
			}
		}
		return state == FlowModule.State.Flowing;
	}

	[BoxGroup("Data", true, false, 0)]
	[EnableIf("IsActive")]
	public Resource.Type resourceType;

	[BoxGroup("Data", true, false, 0)]
	public FlowModule.FlowType flowType;

	[BoxGroup("Data", true, false, 0)]
	[InlineProperty]
	public FloatValueHolder flowPerSecond;

	[BoxGroup("Data", true, false, 0)]
	[InlineProperty]
	[ShowIf("showParametersDescription", true)]
	public FloatValueHolder maxFlow;

	[BoxGroup("Data", true, false, 0)]
	[EnableIf("IsActive")]
	[HideIf("GlobalResource", true)]
	public bool global;

	[EnableIf("IsActive")]
	public FlowModule.State state;

	[FoldoutGroup("State", 0)]
	[Space]
	[EnableIf("IsActive")]
	public List<ResourceGroup> sources;

	[FoldoutGroup("State", 0)]
	[EnableIf("IsActive")]
	public List<ResourceGroup> withResource;

	[FoldoutGroup("State", 0)]
	[EnableIf("IsActive")]
	public List<ResourceGroup> withSpace;

	public UnityEvent onStateChange;

	public bool showParametersDescription = true;

	public enum FlowType
	{
		Negative,
		Positive
	}

	public enum State
	{
		NoSource,
		NoResource,
		NoSpace,
		Flowing,
		NoFlow
	}
}
