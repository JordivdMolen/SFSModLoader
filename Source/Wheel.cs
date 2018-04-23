using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class Wheel : MonoBehaviour
{
	private void OnCollisionStay2D(Collision2D collision)
	{
		float d = this.angularVelocity * 0.0174532924f * this.wheelSize;
		Vector2 a = Quaternion.Euler(0f, 0f, 270f) * collision.contacts[0].normal;
		Vector2 a2 = collision.contacts[0].relativeVelocity - a * d;
		float magnitude = a2.magnitude;
		float num = 1f;
		num = num * 0.1f * (float)Ref.controller.loadedPlanet.surfaceGravity;
		float num2 = this.traction / this.wheelModule.part.vessel.partsManager.rb2d.mass * Time.fixedDeltaTime * 10f;
		bool flag = num2 > 1f;
		if (flag)
		{
			num /= num2;
		}
		this.wheelModule.part.vessel.partsManager.rb2d.AddForceAtPosition(a2 * this.traction * num, base.transform.position);
		bool flag2 = collision.rigidbody != null;
		if (flag2)
		{
			collision.rigidbody.AddForceAtPosition(-a2 * this.traction * num, collision.contacts[0].point);
		}
		float num3 = magnitude * this.traction * num;
		Vector2 vector = collision.contacts[0].relativeVelocity - a * ((this.angularVelocity + num3) * 0.0174532924f * this.wheelSize);
		Vector2 vector2 = collision.contacts[0].relativeVelocity - a * ((this.angularVelocity - num3) * 0.0174532924f * this.wheelSize);
		float sqrMagnitude = vector.sqrMagnitude;
		float sqrMagnitude2 = vector2.sqrMagnitude;
		bool flag3 = sqrMagnitude > sqrMagnitude2;
		if (flag3)
		{
			this.angularVelocity -= magnitude * this.traction * num;
		}
		else
		{
			this.angularVelocity += magnitude * this.traction * num;
		}
	}

	private void Update()
	{
		float num = (!(this.wheelModule.part.vessel == Ref.mainVessel) || !this.wheelModule.on.boolValue) ? 0f : (-Ref.inputController.horizontalAxis);
		bool flag = num == 0f;
		if (flag)
		{
			num = Mathf.Clamp(-this.angularVelocity / Time.deltaTime / this.power, (!this.wheelModule.on.boolValue) ? -0.05f : -0.25f, (!this.wheelModule.on.boolValue) ? 0.05f : 0.25f);
		}
		bool flag2 = num != 0f;
		if (flag2)
		{
			this.angularVelocity = Mathf.Clamp(this.angularVelocity + num * this.power * Time.deltaTime, -this.maxAngularVelocity, this.maxAngularVelocity);
		}
		bool flag3 = this.angularVelocity != 0f;
		if (flag3)
		{
			base.transform.eulerAngles = new Vector3(0f, 0f, base.transform.eulerAngles.z + this.angularVelocity * Time.deltaTime);
		}
		bool flag4 = float.IsNaN(this.angularVelocity);
		if (flag4)
		{
			this.angularVelocity = 0f;
		}
	}

	public Wheel()
	{
	}

	public float power;

	public float traction;

	public float maxAngularVelocity;

	public float wheelSize;

	[BoxGroup]
	public float angularVelocity;

	[BoxGroup]
	public WheelModule wheelModule;
}
