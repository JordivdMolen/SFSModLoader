using System;
using System.Collections.Generic;
using UnityEngine;

public class RcsModule : Module
{
	public override bool IsSorted()
	{
		return true;
	}

	public override List<string> DescriptionVariables
	{
		get
		{
			return (!this.showParametersDescription) ? new List<string>() : new List<string>
			{
				"Thrust: " + this.thrust.ToString() + "kN",
				"Isp: " + (int)(this.thrust / this.resourceConsuption / 9.8f) + "s"
			};
		}
	}

	public override void OnPartUsed()
	{
		this.part.vessel.partsManager.ToggleRCS(this.part.vessel, this.part.vessel == Ref.mainVessel);
	}

	public void UseRcs(Rigidbody2D rb2d, Vessel vessel, float horizontalAxis, float rcsInputDirection, bool directionInput, ref double fuelUsed)
	{
		this.effect.SetTargetTime(0f);
		Vector2 vector = this.effect.transform.TransformDirection(Vector3.left);
		Vector2 vector2 = base.transform.localPosition + this.part.centerOfMass * this.part.orientation;
		Vector2 posToCenterOfMass = rb2d.centerOfMass - vector2;
		if (this.TorqueThrust(vector, posToCenterOfMass, rb2d, vessel, horizontalAxis) || (directionInput && this.DirectionThrust(vector, rcsInputDirection)))
		{
			fuelUsed += (double)this.resourceConsuption * (double)Time.fixedDeltaTime;
			rb2d.AddForceAtPosition(vector * this.thrust, rb2d.GetRelativePoint(vector2));
			this.effect.SetTargetTime(0.9f);
			return;
		}
	}

	private bool TorqueThrust(Vector2 thrustNormal, Vector2 posToCenterOfMass, Rigidbody2D rb2d, Vessel vessel, float horizontalAxis)
	{
		if (Ref.inputController.horizontalAxis == 0f && Mathf.Abs(rb2d.angularVelocity) < 2f)
		{
			return false;
		}
		float current = Mathf.Atan2(thrustNormal.y, thrustNormal.x) * 57.29578f - rb2d.rotation;
		float num = Mathf.Atan2(posToCenterOfMass.y, posToCenterOfMass.x) * 57.29578f;
		return (horizontalAxis == 1f && Mathf.Abs(Mathf.DeltaAngle(current, num + 90f)) < this.torqueAngle) || (horizontalAxis == -1f && Mathf.Abs(Mathf.DeltaAngle(current, num - 90f)) < this.torqueAngle);
	}

	private bool DirectionThrust(Vector2 thrustNormal, float rcsInputDirection)
	{
		float current = Mathf.Atan2(thrustNormal.y, thrustNormal.x) * 57.29578f;
		return Mathf.Abs(Mathf.DeltaAngle(current, rcsInputDirection)) < this.directionAngle;
	}

	public MoveModule effect;

	public float torqueAngle;

	public float directionAngle;

	public float thrust;

	public float resourceConsuption;

	public bool showParametersDescription = true;
}
