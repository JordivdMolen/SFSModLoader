using System;
using NewBuildSystem;
using UnityEngine;

public class Touch : MonoBehaviour
{
	public void GetTouchInputs()
	{
		Double3 zero = Double3.zero;
		int num = 0;
		Ref.inputController.horizontalAxis = 0f;
		Ref.inputController.rcsInput = Vector2.zero;
		for (int i = 0; i < Input.touchCount; i++)
		{
			int fingerId = Input.GetTouch(i).fingerId;
			bool flag = Input.GetTouch(i).phase == TouchPhase.Ended || Input.GetTouch(i).phase == TouchPhase.Canceled;
			if (flag)
			{
				this.EndTouch(fingerId, i, Input.GetTouch(i).position);
			}
			TouchPhase phase = Input.GetTouch(i).phase;
			bool flag2 = false;
			if (flag2)
			{
				this.StartTouch(fingerId, i, Input.GetTouch(i).position);
			}
			this.StayTouch(fingerId, i, ref zero, ref num, ref Ref.inputController.horizontalAxis, ref Ref.inputController.rcsInput);
		}
		bool flag3 = this.GetTouchIdFromFingerId(this.zoomFinger1) != -1 && this.GetTouchIdFromFingerId(this.zoomFinger2) != -1;
		if (flag3)
		{
			float num2 = Vector2.Distance(this.touchesInfo[this.zoomFinger1].lastFingerPosPixels, this.touchesInfo[this.zoomFinger2].lastFingerPosPixels);
			float num3 = Vector2.Distance(Input.GetTouch(this.GetTouchIdFromFingerId(this.zoomFinger1)).position, Input.GetTouch(this.GetTouchIdFromFingerId(this.zoomFinger2)).position);
			float num4 = num2 / num3;
			Ref.inputController.ApplyZoom((num4 - 1f) * 1.5f + 1f);
		}
		bool flag4 = zero.x != 0.0 || zero.y != 0.0;
		if (flag4)
		{
			bool flag5 = num == 0;
			if (flag5)
			{
				num = 1;
			}
			Ref.inputController.ApplyDraging(zero / (double)num);
		}
		for (int j = 0; j < Input.touchCount; j++)
		{
			this.touchesInfo[Input.GetTouch(j).fingerId].lastFingerPosPixels = Input.GetTouch(j).position;
		}
	}

	private void StartTouch(int fingerId, int i, Vector2 posPixel)
	{
		this.touchesInfo[fingerId].touchDownTime = Time.time;
		this.touchesInfo[fingerId].lastFingerPosPixels = posPixel;
		GameObject gameObject = Ref.inputController.PointCastUI(posPixel, Ref.inputController.uIColliders);
		bool flag = gameObject != null;
		if (flag)
		{
			this.touchesInfo[fingerId].touchState = global::Touch.TouchState.OnUI;
			this.touchesInfo[fingerId].touchDownButton = gameObject.transform;
			bool flag2 = Ref.timeWarping && (gameObject.name == "Left Arrow Button" || gameObject.name == "Right Arrow Button");
			if (flag2)
			{
				Ref.controller.ShowMsg("Cannot turn while time warping");
			}
		}
		else
		{
			this.touchesInfo[fingerId].touchState = global::Touch.TouchState.OnEmpty;
			Vector2 posWorld = Camera.main.ScreenToWorldPoint((Vector3)posPixel + Vector3.forward * -Ref.cam.transform.position.z);
			Ref.inputController.StartTouchEmpty(posWorld, fingerId);
			int lastOnEmptyId = this.GetLastOnEmptyId(i);
			bool flag3 = lastOnEmptyId != -1;
			if (flag3)
			{
				this.zoomFinger1 = Input.GetTouch(i).fingerId;
				this.zoomFinger2 = lastOnEmptyId;
			}
		}
	}

	private int GetLastOnEmptyId(int i)
	{
		int result = -1;
		for (int j = 0; j < Input.touchCount; j++)
		{
			bool flag = j != i;
			if (flag)
			{
				bool flag2 = this.touchesInfo[Input.GetTouch(j).fingerId].touchState == global::Touch.TouchState.OnEmpty;
				if (flag2)
				{
					bool flag3 = Ref.currentScene != Ref.SceneType.Build || Build.HoldingPart.isNull(Build.main.holdingParts[Input.GetTouch(j).fingerId]);
					if (flag3)
					{
						result = Input.GetTouch(j).fingerId;
					}
				}
			}
		}
		return result;
	}

	private void EndTouch(int fingerId, int i, Vector2 posPixel)
	{
		bool flag = this.touchesInfo[fingerId].touchDownTime > Time.time - 0.2f;
		global::Touch.TouchState touchState = this.touchesInfo[fingerId].touchState;
		bool flag2 = touchState != global::Touch.TouchState.OnUI;
		if (flag2)
		{
			bool flag3 = touchState == global::Touch.TouchState.OnEmpty;
			if (flag3)
			{
				Ref.inputController.EndTouchEmpty(this.PixelPosToWorldPos(posPixel), fingerId, flag);
			}
		}
		else
		{
			bool flag4 = flag;
			if (flag4)
			{
				Ref.inputController.ClickUI(posPixel);
			}
		}
		bool flag5 = fingerId == this.zoomFinger1 || fingerId == this.zoomFinger2;
		if (flag5)
		{
			this.zoomFinger1 = -1;
			this.zoomFinger2 = -1;
		}
		this.touchesInfo[fingerId].touchState = global::Touch.TouchState.NotTouching;
	}

	private void StayTouch(int fingerId, int i, ref Double3 summedPositionDelta, ref int dragingFingerCount, ref float horizontalAxis, ref Vector2 rcsInput)
	{
		global::Touch.TouchState touchState = this.touchesInfo[fingerId].touchState;
		bool flag = touchState != global::Touch.TouchState.OnEmpty;
		if (flag)
		{
			bool flag2 = touchState == global::Touch.TouchState.OnUI;
			if (flag2)
			{
				bool flag3 = this.touchesInfo[fingerId].touchDownButton == Ref.inputController.leftArrow;
				if (flag3)
				{
					horizontalAxis = -1f;
				}
				else
				{
					bool flag4 = this.touchesInfo[fingerId].touchDownButton == Ref.inputController.rightArrow;
					if (flag4)
					{
						horizontalAxis = 1f;
					}
					else
					{
						bool flag5 = this.touchesInfo[fingerId].touchDownButton == Ref.inputController.up;
						if (flag5)
						{
							rcsInput.y = 1f;
						}
						else
						{
							bool flag6 = this.touchesInfo[fingerId].touchDownButton == Ref.inputController.down;
							if (flag6)
							{
								rcsInput.y = -1f;
							}
							else
							{
								bool flag7 = this.touchesInfo[fingerId].touchDownButton == Ref.inputController.right;
								if (flag7)
								{
									rcsInput.x = 1f;
								}
								else
								{
									bool flag8 = this.touchesInfo[fingerId].touchDownButton == Ref.inputController.left;
									if (flag8)
									{
										rcsInput.x = -1f;
									}
								}
							}
						}
					}
				}
				bool flag9 = Ref.controller != null && Ref.controller.throttlePercentUI != null && this.touchesInfo[fingerId].touchDownButton == Ref.controller.throttlePercentUI.transform.parent;
				if (flag9)
				{
					float num = this.touchesInfo[fingerId].lastFingerPosPixels.y - Input.GetTouch(i).position.y;
					bool flag10 = num != 0f && Ref.mainVessel != null;
					if (flag10)
					{
						bool controlAuthority = Ref.mainVessel.controlAuthority;
						if (controlAuthority)
						{
							Ref.mainVessel.SetThrottle(new Vessel.Throttle(Ref.mainVessel.throttle.throttleOn, Mathf.Clamp01(Ref.mainVessel.throttle.throttleRaw - num / 318f)));
							bool activeSelf = Ref.inputController.instructionSlideThrottleHolder.activeSelf;
							if (activeSelf)
							{
								Ref.inputController.instructionSlideThrottleHolder.SetActive(false);
								Ref.inputController.CheckAllInstructions();
							}
						}
						else
						{
							bool flag11 = Ref.controller.msgUI.color.a < 0.6f;
							if (flag11)
							{
								Ref.controller.ShowMsg("No control");
							}
						}
					}
				}
			}
		}
		else
		{
			bool flag12 = Ref.currentScene == Ref.SceneType.Build;
			if (flag12)
			{
				Vector2 deltaPixel = this.touchesInfo[fingerId].lastFingerPosPixels - Input.GetTouch(i).position;
				Vector2 posWorld = Camera.main.ScreenToWorldPoint((Vector3)Input.GetTouch(i).position + Vector3.forward * -Ref.cam.transform.position.z);
				Ref.inputController.TouchStayEmpty(posWorld, deltaPixel, fingerId);
			}
			bool flag13 = Ref.currentScene == Ref.SceneType.Game;
			if (flag13)
			{
				Vector2 v = this.touchesInfo[fingerId].lastFingerPosPixels - Input.GetTouch(i).position;
				summedPositionDelta += v;
				dragingFingerCount++;
			}
		}
	}

	private int GetTouchIdFromFingerId(int finger)
	{
		for (int i = 0; i < Input.touchCount; i++)
		{
			bool flag = Input.GetTouch(i).fingerId == finger;
			if (flag)
			{
				return i;
			}
		}
		return -1;
	}

	public Vector2 PixelPosToWorldPos(Vector2 posPixel)
	{
		bool flag = Ref.mapView && Ref.currentScene == Ref.SceneType.Game;
		Vector2 result;
		if (flag)
		{
			Double3 a = Double3.ToDouble3((posPixel - new Vector2((float)(Screen.width / 2), (float)(Screen.height / 2))) / (float)Screen.height);
			float f = Ref.cam.fieldOfView * 0.0174532924f;
			float num = Mathf.Sin(f) / ((1f + Mathf.Cos(f)) * 0.5f);
			result = Ref.cam.transform.position + (Vector3)(a * -Ref.map.mapPosition.z * (double)num).RotateZ((double)(Ref.cam.transform.eulerAngles.z * 0.0174532924f)).toVector2;
		}
		else
		{
			result = Camera.main.ScreenToWorldPoint((Vector3)posPixel + Vector3.forward * -Ref.cam.transform.position.z);
		}
		return result;
	}

	public Touch()
	{
	}

	[Header("Touch")]
	[Space(6f)]
	public global::Touch.FingerData[] touchesInfo = new global::Touch.FingerData[5];

	public int zoomFinger1 = -1;

	public int zoomFinger2 = -1;

	[Serializable]
	public struct FingerData
	{
		public global::Touch.TouchState touchState;

		public Transform touchDownButton;

		public float touchDownTime;

		public Vector2 lastFingerPosPixels;
	}

	public enum TouchState
	{
		NotTouching,
		OnEmpty,
		OnUI
	}
}
