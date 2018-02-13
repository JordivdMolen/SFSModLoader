using NewBuildSystem;
using System;
using UnityEngine;

public class Touch : MonoBehaviour
{
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

	[Header("Touch"), Space(6f)]
	public global::Touch.FingerData[] touchesInfo = new global::Touch.FingerData[5];

	public int zoomFinger1 = -1;

	public int zoomFinger2 = -1;

	public void GetTouchInputs()
	{
		Double3 zero = Double3.zero;
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < Input.touchCount; i++)
		{
			int fingerId = Input.GetTouch(i).fingerId;
			if (Input.GetTouch(i).phase == TouchPhase.Ended || Input.GetTouch(i).phase == TouchPhase.Canceled)
			{
				this.EndTouch(fingerId, i, Input.GetTouch(i).position);
			}
			if (Input.GetTouch(i).phase == TouchPhase.Began)
			{
				this.StartTouch(fingerId, i, Input.GetTouch(i).position);
			}
			this.StayTouch(fingerId, i, ref zero, ref num, ref num2);
		}
		if (this.GetTouchIdFromFingerId(this.zoomFinger1) != -1 && this.GetTouchIdFromFingerId(this.zoomFinger2) != -1)
		{
			float num3 = Vector2.Distance(this.touchesInfo[this.zoomFinger1].lastFingerPosPixels, this.touchesInfo[this.zoomFinger2].lastFingerPosPixels);
			float num4 = Vector2.Distance(Input.GetTouch(this.GetTouchIdFromFingerId(this.zoomFinger1)).position, Input.GetTouch(this.GetTouchIdFromFingerId(this.zoomFinger2)).position);
			float zoomDelta = num3 / num4;
			Ref.inputController.ApplyZoom(zoomDelta);
		}
		if (Ref.mapView && (zero.x != 0.0 || zero.y != 0.0))
		{
			if (num == 0)
			{
				num = 1;
			}
			Ref.inputController.ApplyDraging(zero / (double)num);
		}
		for (int j = 0; j < Input.touchCount; j++)
		{
			this.touchesInfo[Input.GetTouch(j).fingerId].lastFingerPosPixels = Input.GetTouch(j).position;
		}
		Ref.inputController.horizontalAxis = (float)num2;
	}

	private void StartTouch(int fingerId, int i, Vector2 posPixel)
	{
		this.touchesInfo[fingerId].touchDownTime = Time.time;
		this.touchesInfo[fingerId].lastFingerPosPixels = posPixel;
		GameObject gameObject = Ref.inputController.PointCastUI(posPixel, Ref.inputController.uIColliders);
		if (gameObject != null)
		{
			this.touchesInfo[fingerId].touchState = global::Touch.TouchState.OnUI;
			this.touchesInfo[fingerId].touchDownButton = gameObject.transform;
			if (Ref.timeWarping && (gameObject.name == "Left Arrow Button" || gameObject.name == "Right Arrow Button"))
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
			if (lastOnEmptyId != -1)
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
			if (j != i)
			{
				if (this.touchesInfo[Input.GetTouch(j).fingerId].touchState == global::Touch.TouchState.OnEmpty)
				{
					if (Ref.currentScene != Ref.SceneType.Build || Build.HoldingPart.isNull(Build.main.holdingParts[Input.GetTouch(j).fingerId]))
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
		if (touchState != global::Touch.TouchState.OnUI)
		{
			if (touchState == global::Touch.TouchState.OnEmpty)
			{
				Ref.inputController.EndTouchEmpty(this.PixelPosToWorldPos(posPixel), fingerId, flag);
			}
		}
		else if (flag)
		{
			Ref.inputController.ClickUI(posPixel);
		}
		if (fingerId == this.zoomFinger1 || fingerId == this.zoomFinger2)
		{
			this.zoomFinger1 = -1;
			this.zoomFinger2 = -1;
		}
		this.touchesInfo[fingerId].touchState = global::Touch.TouchState.NotTouching;
	}

	private void StayTouch(int fingerId, int i, ref Double3 summedPositionDelta, ref int dragingFingerCount, ref int horizontalAxis)
	{
		global::Touch.TouchState touchState = this.touchesInfo[fingerId].touchState;
		if (touchState != global::Touch.TouchState.OnEmpty)
		{
			if (touchState == global::Touch.TouchState.OnUI)
			{
				if (this.touchesInfo[fingerId].touchDownButton == Ref.inputController.leftArrow)
				{
					horizontalAxis = -1;
				}
				if (this.touchesInfo[fingerId].touchDownButton == Ref.inputController.rightArrow)
				{
					horizontalAxis = 1;
				}
				if (Ref.controller != null && Ref.controller.throttlePercentUI != null && this.touchesInfo[fingerId].touchDownButton == Ref.controller.throttlePercentUI.transform.parent)
				{
					float num = this.touchesInfo[fingerId].lastFingerPosPixels.y - Input.GetTouch(i).position.y;
					if (num != 0f && Ref.mainVessel != null)
					{
						if (Ref.mainVessel.controlAuthority)
						{
							Ref.mainVessel.SetThrottle(new Vessel.Throttle(Ref.mainVessel.throttle.throttleOn, Mathf.Clamp01(Ref.mainVessel.throttle.throttleRaw - num / 318f)));
							if (Ref.inputController.instructionSlideThrottleHolder.activeSelf)
							{
								Ref.inputController.instructionSlideThrottleHolder.SetActive(false);
								Ref.inputController.CheckAllInstructions();
							}
						}
						else if (Ref.controller.msgUI.color.a < 0.6f)
						{
							Ref.controller.ShowMsg("No control");
						}
					}
				}
			}
		}
		else
		{
			if (Ref.currentScene == Ref.SceneType.Build)
			{
				Vector2 deltaPixel = this.touchesInfo[fingerId].lastFingerPosPixels - Input.GetTouch(i).position;
				Vector2 posWorld = Camera.main.ScreenToWorldPoint((Vector3)Input.GetTouch(i).position + Vector3.forward * -Ref.cam.transform.position.z);
				Ref.inputController.TouchStayEmpty(posWorld, deltaPixel, fingerId);
			}
			if (Ref.mapView)
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
			if (Input.GetTouch(i).fingerId == finger)
			{
				return i;
			}
		}
		return -1;
	}

	public Vector2 PixelPosToWorldPos(Vector2 posPixel)
	{
		if (Ref.mapView && Ref.currentScene == Ref.SceneType.Game)
		{
			Double3 a = Double3.ToDouble3((posPixel - new Vector2((float)(Screen.width / 2), (float)(Screen.height / 2))) / (float)Screen.height);
			float f = Ref.cam.fieldOfView * 0.0174532924f;
			float num = Mathf.Sin(f) / ((1f + Mathf.Cos(f)) * 0.5f);
			return Ref.cam.transform.position + (Vector3)(a * -Ref.map.mapPosition.z * (double)num).RotateZ((double)(Ref.cam.transform.eulerAngles.z * 0.0174532924f)).toVector2;
		}
		return Camera.main.ScreenToWorldPoint((Vector3)posPixel + Vector3.forward * -Ref.cam.transform.position.z);
	}
}
