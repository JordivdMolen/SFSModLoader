using System;
using System.Collections.Generic;
using UnityEngine;

public class RcsModule : Module
{
	public override List<string> DescriptionVariables
	{
		get
		{
			List<string> result;
			if (this.showParametersDescription)
			{
				List<string> list = new List<string>();
				list.Add("Thrust: " + this.thrust.ToString() + "kN");
				result = list;
				list.Add("Isp: " + (int)(this.thrust / this.resourceConsuption / 9.8f) + "s");
			}
			else
			{
				result = new List<string>();
			}
			return result;
		}
	}

	public override void OnPartUsed()
	{
		this.part.vessel.partsManager.ToggleRCS(this.part.vessel, this.part.vessel == Ref.mainVessel);
	}

	public void UseRcs(Rigidbody2D rb2d, Vessel vessel, float horizontalAxis, float rcsInputDirection, bool directionInput, ref float fuelUsed)
	{
		this.effect.SetTargetTime(0f);
		Vector2 vector = this.effect.transform.TransformDirection(Vector3.left);
		Vector2 vector2 = base.transform.localPosition + this.part.centerOfMass * this.part.orientation;
		Vector2 posToCenterOfMass = rb2d.centerOfMass - vector2;
		bool flag = this.TorqueThrust(vector, posToCenterOfMass, rb2d, vessel, horizontalAxis) || (directionInput && this.DirectionThrust(vector, rcsInputDirection));
		if (flag)
		{
			fuelUsed += this.resourceConsuption * Time.fixedDeltaTime;
			rb2d.AddForceAtPosition(vector * this.thrust, rb2d.GetRelativePoint(vector2));
			this.effect.SetTargetTime(0.9f);
		}
	}

	private bool TorqueThrust(Vector2 thrustNormal, Vector2 posToCenterOfMass, Rigidbody2D rb2d, Vessel vessel, float horizontalAxis)
	{
		bool flag = Ref.inputController.horizontalAxis == 0f && Mathf.Abs(rb2d.angularVelocity) < 2f;
		bool result;
		if (flag)
		{
			result = false;
		}
		else
		{
			float current = Mathf.Atan2(thrustNormal.y, thrustNormal.x) * 57.29578f - rb2d.rotation;
			float num = Mathf.Atan2(posToCenterOfMass.y, posToCenterOfMass.x) * 57.29578f;
			result = ((horizontalAxis == 1f && Mathf.Abs(Mathf.DeltaAngle(current, num + 90f)) < this.torqueAngle) || (horizontalAxis == -1f && Mathf.Abs(Mathf.DeltaAngle(current, num - 90f)) < this.torqueAngle));
		}
		return result;
	}

	private bool DirectionThrust(Vector2 thrustNormal, float rcsInputDirection)
	{
		float current = Mathf.Atan2(thrustNormal.y, thrustNormal.x) * 57.29578f;
		return Mathf.Abs(Mathf.DeltaAngle(current, rcsInputDirection)) < this.directionAngle;
	}

	public RcsModule()
	{
	}

	public MoveModule effect;

	public float torqueAngle;

	public float directionAngle;

	public float thrust;

	public float resourceConsuption;

	public bool showParametersDescription = true;
}
