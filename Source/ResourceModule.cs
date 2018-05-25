using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class ResourceModule : Module
{
    public override bool IsSorted()
    {
        return true;
    }

    public override List<object> SaveVariables
    {
        get
        {
            return new List<object>
            {
                this.resourceAmount
            };
        }
    }

    public override List<string> DescriptionVariables
    {
        get
        {
            return (!this.showParametersDescription) ? new List<string>() : new List<string>
            {
                string.Concat(new object[]
                {
                    Resource.GetResourceName(this.resourceType),
                    ": ",
                    this.resourceSpace,
                    Resource.GetResourceUnit(this.resourceType)
                })
            };
        }
    }

    public override void OnPartUsed()
    {
        if (this.part.vessel != Ref.mainVessel)
        {
            return;
        }
        if (this.resourceType != Resource.Type.Fuel)
        {
            return;
        }
        bool flag = Ref.controller.fromTank != null;
        bool flag2 = Ref.controller.toTank != null;
        if ((flag && Ref.controller.fromTank.resourceGroup == this.resourceGroup) || (flag2 && Ref.controller.toTank.resourceGroup == this.resourceGroup) || (flag && flag2))
        {
            this.CancelFuelTranfer();
            return;
        }
        if (!flag)
        {
            if (this.resourceGroup.empty)
            {
                MsgController.ShowMsg("Out of fuel");
                return;
            }
            Ref.controller.fromTank = this;
            Ref.controller.toTank = null;
            MsgController.ShowMsg("Selected fuel tank for fuel transfer");
            return;
        }
        else
        {
            if (flag2)
            {
                return;
            }
            if (this.resourceGroup.full)
            {
                MsgController.ShowMsg("Cannot transfer fuel into a full tank");
                return;
            }
            Ref.controller.toTank = this;
            return;
        }
    }

    private void CancelFuelTranfer()
    {
        Ref.controller.fromTank = null;
        Ref.controller.toTank = null;
        MsgController.ShowMsg("Transfer canceled");
    }

    [BoxGroup]
    public Resource.Type resourceType;

    [BoxGroup]
    [InlineProperty]
    public FloatValueHolder resourceAmount;

    [BoxGroup]
    public float resourceSpace;

    [BoxGroup("B", false, false, 0)]
    public ResourceGroup resourceGroup;

    public bool showParametersDescription = true;
}
