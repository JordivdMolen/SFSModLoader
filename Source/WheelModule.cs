using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class WheelModule : Module
{
    public override List<object> SaveVariables
    {
        get
        {
            return new List<object>
            {
                this.on
            };
        }
    }

    public override void OnPartUsed()
    {
        for (int i = 0; i < this.resourceSources.Length; i++)
        {
            if (this.resourceSources[i].state != FlowModule.State.Flowing)
            {
                MsgController.ShowMsg((this.resourceSources[i].state != FlowModule.State.NoSource) ? ("Out of " + Resource.GetResourceName(this.resourceSources[i].resourceType).ToLower()) : ("No " + Resource.GetResourceName(this.resourceSources[i].resourceType).ToLower() + " source"));
                return;
            }
        }
        this.on.boolValue = !this.on.boolValue;
        MsgController.ShowMsg("Rover wheel " + ((!this.on.boolValue) ? "Off" : "On"));
    }

    public void OnSourceStateChange()
    {
        if (FlowModule.ConfirmSources(this.resourceSources, Ref.mainVessel == this.part.vessel))
        {
            return;
        }
        this.on.boolValue = false;
    }

    public Wheel wheel;

    [InlineProperty]
    public BoolValueHolder on;

    public FlowModule[] resourceSources;

    [InlineProperty]
    public FloatValueHolder throttle;
}
