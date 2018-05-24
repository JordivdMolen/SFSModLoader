using System;
using NewBuildSystem;
using UnityEngine;

public class ParachuteModule : Module
{
	public void UpdateDeploymentPercent(float empty)
	{
		this.part.UpdateDragSurfaces(this.part.GetSurfacesCoveredAmount());
		this.part.activeDragSurfaces.Add(new Part.ActiveDragSurafce(new PartData.DragSurface(-1, true, float.NaN, this.drag * this.dragCurve.Evaluate(this.moveModule.time.floatValue)), this.part.orientation, 0f));
	}

	private void LateUpdate()
	{
		if (this.moveModule.targetTime.floatValue == 1f || this.moveModule.targetTime.floatValue == 2f)
		{
			if (this.moveModule.targetTime.floatValue == 1f && Ref.mainVesselTerrainHeight < 110.0)
			{
				this.DeployParachute(2f, false);
			}
			this.CalculateParachuteRotation();
			if (base.GetComponentInParent<Rigidbody2D>().velocity.sqrMagnitude > 0.00250000018f)
			{
				this.lastMovingTime = Time.time;
			}
			else if (this.lastMovingTime + 0.5f < Time.time)
			{
				this.RelaseParachute();
			}
		}
	}

	public void CalculateParachuteRotation()
	{
		Vector3 vector = this.lastPos - base.transform.position;
		float z = Mathf.Atan2(vector.y, vector.x) * 57.29578f - 90f + Mathf.Sin(Time.time) * 3f;
		this.parachute.eulerAngles = new Vector3(0f, 0f, z);
		if (vector.sqrMagnitude > 100f)
		{
			this.lastPos = base.transform.position + vector.normalized * 10f;
		}
	}

	public override void OnPartUsed()
	{
		if (Ref.timeWarping)
		{
			if (this.moveModule.targetTime.floatValue == 0f)
			{
				MsgController.ShowMsg("Cannot deploy parachute while time warping");
			}
		}
		else
		{
			Rigidbody2D componentInParent = base.GetComponentInParent<Rigidbody2D>();
			if (this.moveModule.targetTime.floatValue == 0f)
			{
				if (!Ref.controller.loadedPlanet.atmosphereData.hasAtmosphere || Ref.mainVesselHeight > Ref.controller.loadedPlanet.atmosphereData.atmosphereHeightM * 0.8)
				{
					MsgController.ShowMsg("Cannot deploy parachute in a vacuum");
				}
				else if (Ref.mainVesselTerrainHeight > this.minDeployHeight)
				{
					MsgController.ShowMsg("Cannot deploy parachute above " + this.minDeployHeight.ToString() + "m");
				}
				else if (componentInParent.velocity.magnitude > this.maxDeployVelocity)
				{
					MsgController.ShowMsg("Cannot deploy parachute while moving faster than " + this.maxDeployVelocity + "m/s");
				}
				else if (componentInParent.velocity.magnitude < 3f)
				{
					MsgController.ShowMsg("Cannot deploy parachute while moving slower than 3m/s");
				}
				else
				{
					MsgController.ShowMsg("Parachute half deployed");
					this.DeployParachute(1f, true);
				}
			}
			else if (this.moveModule.targetTime.floatValue == 1f)
			{
				if (Ref.mainVesselTerrainHeight > 500.0)
				{
					MsgController.ShowMsg("Cannot fully deploy parachute above 500m");
				}
				else if (componentInParent.velocity.magnitude > this.maxDeployVelocity / 5f)
				{
					MsgController.ShowMsg("Cannot fully deploy parachute while moving faster than " + this.maxDeployVelocity / 5f + "m/s");
				}
				else
				{
					MsgController.ShowMsg("Parachute fully deployed");
					this.DeployParachute(2f, false);
				}
			}
			else if (this.moveModule.targetTime.floatValue == 2f)
			{
				if (this.moveModule.time.floatValue < 2f)
				{
					return;
				}
				MsgController.ShowMsg("Parachute cut");
				this.RelaseParachute();
			}
		}
	}

	public void DeployParachute(float targetTime, bool alignWithVelocity)
	{
		this.moveModule.SetTargetTime(targetTime);
		if (!alignWithVelocity)
		{
			return;
		}
		Rigidbody2D componentInParent = base.GetComponentInParent<Rigidbody2D>();
		this.lastPos = base.transform.position - (Vector3)componentInParent.velocity;
		this.CalculateParachuteRotation();
	}

	public void RelaseParachute()
	{
		this.moveModule.SetTime(3f);
		this.moveModule.SetTargetTime(3f);
	}

	public float maxDeployVelocity;

	public double minDeployHeight;

	public float drag;

	public AnimationCurve dragCurve;

	public Vector3 lastPos;

	public float lastMovingTime;

	public MoveModule moveModule;

	public Transform parachute;

	public enum State
	{
		Packed,
		SemiDeployed,
		Deployed,
		Relased
	}
}
