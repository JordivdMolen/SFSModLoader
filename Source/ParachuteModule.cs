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
		bool flag = this.moveModule.targetTime.floatValue == 1f || this.moveModule.targetTime.floatValue == 2f;
		if (flag)
		{
			bool flag2 = this.moveModule.targetTime.floatValue == 1f && Ref.mainVesselTerrainHeight < 110.0;
			if (flag2)
			{
				this.DeployParachute(2f, false);
			}
			this.CalculateParachuteRotation();
			bool flag3 = base.GetComponentInParent<Rigidbody2D>().velocity.sqrMagnitude > 0.00250000018f;
			if (flag3)
			{
				this.lastMovingTime = Time.time;
			}
			else
			{
				bool flag4 = this.lastMovingTime + 0.5f < Time.time;
				if (flag4)
				{
					this.RelaseParachute();
				}
			}
		}
	}

	public void CalculateParachuteRotation()
	{
		Vector3 vector = this.lastPos - base.transform.position;
		float z = Mathf.Atan2(vector.y, vector.x) * 57.29578f - 90f + Mathf.Sin(Time.time) * 3f;
		this.parachute.eulerAngles = new Vector3(0f, 0f, z);
		bool flag = vector.sqrMagnitude > 100f;
		if (flag)
		{
			this.lastPos = base.transform.position + vector.normalized * 10f;
		}
	}

	public override void OnPartUsed()
	{
		bool timeWarping = Ref.timeWarping;
		if (timeWarping)
		{
			bool flag = this.moveModule.targetTime.floatValue == 0f;
			if (flag)
			{
				Ref.controller.ShowMsg("Cannot deploy parachute while time warping");
			}
		}
		else
		{
			Rigidbody2D componentInParent = base.GetComponentInParent<Rigidbody2D>();
			bool flag2 = this.moveModule.targetTime.floatValue == 0f;
			if (flag2)
			{
				bool flag3 = Ref.mainVesselHeight > Ref.controller.loadedPlanet.atmosphereData.atmosphereHeightM * 0.8;
				if (flag3)
				{
					Ref.controller.ShowMsg("Cannot deploy parachute in a vacuum");
				}
				else
				{
					bool flag4 = Ref.mainVesselTerrainHeight > this.minDeployHeight;
					if (flag4)
					{
						Ref.controller.ShowMsg("Cannot deploy parachute above " + this.minDeployHeight.ToString() + "m");
					}
					else
					{
						bool flag5 = componentInParent.velocity.magnitude > this.maxDeployVelocity;
						if (flag5)
						{
							Ref.controller.ShowMsg("Cannot deploy parachute while moving faster than " + this.maxDeployVelocity + "m/s");
						}
						else
						{
							bool flag6 = componentInParent.velocity.magnitude < 3f;
							if (flag6)
							{
								Ref.controller.ShowMsg("Cannot deploy parachute while moving slower than 3m/s");
							}
							else
							{
								Ref.controller.ShowMsg("Parachute half deployed");
								this.DeployParachute(1f, true);
							}
						}
					}
				}
			}
			else
			{
				bool flag7 = this.moveModule.targetTime.floatValue == 1f;
				if (flag7)
				{
					bool flag8 = Ref.mainVesselTerrainHeight > 500.0;
					if (flag8)
					{
						Ref.controller.ShowMsg("Cannot fully deploy parachute above 500m");
					}
					else
					{
						bool flag9 = componentInParent.velocity.magnitude > this.maxDeployVelocity / 5f;
						if (flag9)
						{
							Ref.controller.ShowMsg("Cannot fully deploy parachute while moving faster than " + this.maxDeployVelocity / 5f + "m/s");
						}
						else
						{
							Ref.controller.ShowMsg("Parachute fully deployed");
							this.DeployParachute(2f, false);
						}
					}
				}
				else
				{
					bool flag10 = this.moveModule.targetTime.floatValue == 2f;
					if (flag10)
					{
						bool flag11 = this.moveModule.time.floatValue < 2f;
						if (!flag11)
						{
							Ref.controller.ShowMsg("Parachute cut");
							this.RelaseParachute();
						}
					}
				}
			}
		}
	}

	public void DeployParachute(float targetTime, bool alignWithVelocity)
	{
		this.moveModule.SetTargetTime(targetTime);
		bool flag = !alignWithVelocity;
		if (!flag)
		{
			Rigidbody2D componentInParent = base.GetComponentInParent<Rigidbody2D>();
			this.lastPos = base.transform.position - (Vector3)componentInParent.velocity;
			this.CalculateParachuteRotation();
		}
	}

	public void RelaseParachute()
	{
		this.moveModule.SetTime(3f);
		this.moveModule.SetTargetTime(3f);
	}

	public ParachuteModule()
	{
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
