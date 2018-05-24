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
            if (part.HasDockingPortModule())
            {
                base.enabled = false;
            }
        }
    }

    private void Update()
    {
        for (int i = 0; i < DockingPortModule.dockingPorts.Count; i++)
        {
            if (this.part.vessel != DockingPortModule.dockingPorts[i].part.vessel && this != DockingPortModule.dockingPorts[i] && DockingPortModule.dockingPorts[i].part.vessel != Ref.mainVessel)
            {
                if ((base.transform.position - DockingPortModule.dockingPorts[i].transform.position).sqrMagnitude <= this.pullDistance * this.pullDistance)
                {
                    this.part.vessel.partsManager.rb2d.AddForceAtPosition(-(base.transform.position - DockingPortModule.dockingPorts[i].transform.position).normalized * this.pullForce * Time.fixedDeltaTime, this.part.vessel.partsManager.rb2d.GetRelativePoint(this.part.transform.localPosition));
                    if ((base.transform.position - DockingPortModule.dockingPorts[i].transform.position).sqrMagnitude < this.dockDistance * this.dockDistance)
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
                if (other.part.vessel.partsManager.parts[i].joints[j].fromPart == other.part.vessel.partsManager.parts[i])
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
            if (part.HasDockingPortModule())
            {
                part.GetComponent<DockingPortModule>().enabled = true;
            }
        }
    }

    public void Enable()
    {
        base.enabled = true;
    }

    public float pullDistance;

    public float pullForce;

    public float dockDistance;

    private static List<DockingPortModule> dockingPorts = new List<DockingPortModule>();
}
