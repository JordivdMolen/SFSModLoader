using System;
using System.Collections;
using System.Collections.Generic;
using NewBuildSystem;
using UnityEngine;
using UnityEngine.UI;
using SFSML.HookSystem.ReWork.BaseHooks.UtilHooks;
using SFSML.HookSystem.ReWork;

public class InputController : global::Touch
{
	private void Start()
	{
		this.SetFps();
		this.SetEnableMusic();
		this.SetEnableSound();
		Screen.sleepTimeout = -1;
		Ref.SceneType currentScene = Ref.currentScene;
		bool flag = currentScene > Ref.SceneType.MainMenu;
		if (flag)
		{
			bool flag2 = currentScene != Ref.SceneType.Build;
			if (flag2)
			{
				bool flag3 = currentScene == Ref.SceneType.Game;
				if (flag3)
				{
					Screen.orientation = ((!Saving.LoadSetting(Saving.SettingKey.disableAutoRotate)) ? ScreenOrientation.AutoRotation : ScreenOrientation.Portrait);
					bool flag4 = Ref.lastScene == Ref.SceneType.Build;
					if (flag4)
					{
						this.instructionToggleThrottleHolder.SetActive(true);
						bool flag5 = !Saving.LoadSetting(Saving.SettingKey.seenGameInstructions);
						if (flag5)
						{
							this.instructionsPartsHolder.SetActive(true);
							this.instructionSlideThrottleHolder.SetActive(true);
						}
					}
				}
			}
			else
			{
				Screen.orientation = ScreenOrientation.Portrait;
			}
		}
		else
		{
			Screen.orientation = ScreenOrientation.Portrait;
		}
	}

	public void SetFps()
	{
		Application.targetFrameRate = ((!Saving.LoadSetting(Saving.SettingKey.disable60fps)) ? 60 : 30);
	}

	public void SetEnableMusic()
	{
		bool flag = this.soundtrackController == null;
		if (!flag)
		{
			this.soundtrackController.gameObject.SetActive(!Saving.LoadSetting(Saving.SettingKey.disableMusic));
			this.soundtrackController.CancelInvoke();
			this.soundtrackController.SelectRandomSoundtrack();
		}
	}

	public void SetEnableSound()
	{
		AudioListener.volume = (float)((!Saving.LoadSetting(Saving.SettingKey.disableSound)) ? 1 : 0);
	}

	public void CheckAllInstructions()
	{
		bool flag = this.instructionsPartsHolder.activeSelf || this.instructionSlideThrottleHolder.activeSelf || this.instructionToggleThrottleHolder.activeSelf;
		if (!flag)
		{
			MonoBehaviour.print("Seen all instructions");
			Saving.SaveSetting(Saving.SettingKey.seenGameInstructions, true);
		}
	}

	private void LateUpdate()
	{
		bool flag = Input.mouseScrollDelta.y != 0f;
		if (flag)
		{
			this.ApplyZoom(1f - Input.mouseScrollDelta.y / 5f);
		}
		this.horizontalAxis = Input.GetAxisRaw("Horizontal");
		Vector2 posWorld = base.PixelPosToWorldPos(Input.mousePosition);
		bool mouseButtonDown = Input.GetMouseButtonDown(0);
		if (mouseButtonDown)
		{
			this.StartTouchEmpty(posWorld, 0);
			this.oldPos = Input.mousePosition;
		}
		bool flag2 = (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) && !this.ClickUI(Input.mousePosition);
		if (flag2)
		{
			this.EndTouchEmpty(posWorld, 0, true);
		}
		bool key = Input.GetKey("q");
		if (key)
		{
			this.ApplyZoom(1.02f);
		}
		bool key2 = Input.GetKey("e");
		if (key2)
		{
			this.ApplyZoom(0.98f);
		}
		bool mouseButton = Input.GetMouseButton(0);
		if (mouseButton)
		{
			this.TouchStayEmpty(posWorld, this.oldPos - Input.mousePosition, 0);
			bool flag3 = !Input.GetMouseButtonDown(0);
			if (flag3)
			{
				this.ApplyDraging(Double3.ToDouble3(this.oldPos - Input.mousePosition));
			}
			this.oldPos = Input.mousePosition;
		}
		float num = (float)((Screen.height <= Screen.width) ? 1334 : 750);
		bool flag4 = this.canvasScalers[0].referenceResolution.x != num;
		if (flag4)
		{
			bool flag5 = Ref.currentScene == Ref.SceneType.MainMenu;
			if (!flag5)
			{
				for (int i = 0; i < this.canvasScalers.Length; i++)
				{
					this.canvasScalers[i].referenceResolution = new Vector2(num, 1f);
				}
				bool flag6 = Ref.currentScene == Ref.SceneType.Game && this.recoverMenuHolder.activeSelf;
				if (flag6)
				{
					Ref.controller.StartRecovery();
				}
				bool flag7 = Ref.saving != null && Ref.saving.loadingMenuHolder.activeSelf;
				if (flag7)
				{
					Ref.saving.TryStartLoadProcess();
				}
			}
		}
	}

	public void PlayClickSound(float volume)
	{
		this.clickSound.Play();
		this.clickSound.volume = volume;
	}

	public void StartTouchEmpty(Vector2 posWorld, int fingerId)
	{
        MyTouchStartHook myTouchStartHook = new MyTouchStartHook();
        myTouchStartHook = MyHookSystem.executeHook<MyTouchStartHook>(myTouchStartHook);
        if (myTouchStartHook.isCanceled())
        {
            return;
        }

        Ref.SceneType currentScene = Ref.currentScene;
		bool flag = currentScene > Ref.SceneType.MainMenu;
		if (flag)
		{
			bool flag2 = currentScene != Ref.SceneType.Build;
			if (flag2)
			{
				bool flag3 = currentScene != Ref.SceneType.Game;
				if (flag3)
				{
				}
			}
			else
			{
				Build.main.OnTouchStart(fingerId, posWorld);
			}
		}
	}

	public void TouchStayEmpty(Vector2 posWorld, Vector2 deltaPixel, int fingerId)
	{
		Ref.SceneType currentScene = Ref.currentScene;
		bool flag = currentScene > Ref.SceneType.MainMenu;
		if (flag)
		{
			bool flag2 = currentScene != Ref.SceneType.Build;
			if (flag2)
			{
				bool flag3 = currentScene != Ref.SceneType.Game;
				if (flag3)
				{
				}
			}
			else
			{
				Build.main.OnTouchStay(fingerId, posWorld, deltaPixel);
			}
		}
	}

	public void EndTouchEmpty(Vector2 posWorld, int fingerId, bool click)
	{
		Ref.SceneType currentScene = Ref.currentScene;
		bool flag = currentScene > Ref.SceneType.MainMenu;
		if (flag)
		{
			bool flag2 = currentScene != Ref.SceneType.Build;
			if (flag2)
			{
				bool flag3 = currentScene == Ref.SceneType.Game;
				if (flag3)
				{
					if (click)
					{
						this.ClickEmpty(posWorld);
					}
				}
			}
			else
			{
				Build.main.OnTouchEnd(fingerId, posWorld);
			}
		}
	}

	public bool ClickUI(Vector2 clickPosPixel)
	{
		bool flag = Build.main != null;
		if (flag)
		{
			Build.main.OnClickUI(clickPosPixel);
		}
		GameObject gameObject = this.PointCastUI(clickPosPixel, this.uIColliders);
		bool flag2 = gameObject == null;
		bool result;
		if (flag2)
		{
			result = false;
		}
		else
		{
			base.StartCoroutine(this.ClickGlow(gameObject.transform.GetChild(0).gameObject));
			CustomEvent component = gameObject.GetComponent<CustomEvent>();
			bool flag3 = component != null;
			if (flag3)
			{
				component.customEvent.Invoke();
				result = true;
			}
			else
			{
				string name = gameObject.name;
				bool flag4 = name != null;
				if (flag4)
				{
					bool flag5 = !(name == "Recover Vessel Button");
					if (flag5)
					{
						bool flag6 = !(name == "Cancel Recovery Button");
						if (flag6)
						{
							bool flag7 = !(name == "Complete Mission Button");
							if (flag7)
							{
								bool flag8 = !(name == "LOADING Menu Holder");
								if (flag8)
								{
									bool flag9 = name == "Keyboard";
									if (flag9)
									{
										Ref.saving.OnKeyboardClick(clickPosPixel);
									}
								}
								else
								{
									this.PlayClickSound(0.2f);
									Ref.saving.TrySelectLoad(clickPosPixel);
								}
							}
							else
							{
								this.PlayClickSound(0.6f);
								Ref.controller.Invoke("CompleteRecovery", (!Ref.mapView) ? 0.35f : 0.1f);
							}
						}
						else
						{
							this.PlayClickSound(0.6f);
							Ref.controller.Invoke("CancelRecovery", 0.1f);
						}
					}
					else
					{
						this.PlayClickSound(0.6f);
						Ref.controller.Invoke("StartRecovery", 0.1f);
					}
				}
				result = true;
			}
		}
		return result;
	}

	private void ClickEmpty(Vector2 clickPosWorld)
	{
		Ref.SceneType currentScene = Ref.currentScene;
		bool flag = currentScene > Ref.SceneType.MainMenu;
		if (flag)
		{
			bool flag2 = currentScene != Ref.SceneType.Build;
			if (flag2)
			{
				bool flag3 = currentScene == Ref.SceneType.Game;
				if (flag3)
				{
					bool mapView = Ref.mapView;
					if (mapView)
					{
						Ref.map.OnClickEmpty(clickPosWorld);
					}
					else
					{
						bool flag4 = Ref.mainVessel != null;
						if (flag4)
						{
							this.PointCastParts(clickPosWorld);
						}
					}
				}
			}
		}
	}

	public GameObject PointCastUI(Vector2 clickPosPixel, InputController.UIColliders[] colliders)
	{
		foreach (InputController.UIColliders uicolliders in colliders)
		{
			GameObject gameObject = this.PointCastUI(clickPosPixel, uicolliders.colliders);
			bool flag = gameObject != null;
			if (flag)
			{
				return gameObject;
			}
		}
		return null;
	}

	public GameObject PointCastUI(Vector2 clickPosPixel, BoxCollider2D[] colliders)
	{
		float num = (float)((Screen.height <= Screen.width) ? 1334 : 750);
		float d = (float)Screen.width / num;
		foreach (BoxCollider2D boxCollider2D in colliders)
		{
			bool activeInHierarchy = boxCollider2D.gameObject.activeInHierarchy;
			if (activeInHierarchy)
			{
				Vector2 a = (Vector2)boxCollider2D.GetComponent<RectTransform>().position + boxCollider2D.offset * d;
				Vector2 vector = a - boxCollider2D.size * 0.5f * d;
				Vector2 vector2 = a + boxCollider2D.size * 0.5f * d;
				bool flag = clickPosPixel.x >= vector.x && clickPosPixel.y >= vector.y && clickPosPixel.x <= vector2.x && clickPosPixel.y <= vector2.y;
				if (flag)
				{
					return boxCollider2D.gameObject;
				}
			}
		}
		return null;
	}

	private void PointCastParts(Vector2 clickPos)
	{
		List<RaycastHit2D> list = new List<RaycastHit2D>(Physics2D.RaycastAll(clickPos, Vector2.up, 0.01f, this.maskPartHibox));
		bool flag = list.Count == 0;
		if (!flag)
		{
			Transform transform = this.GetTopPart(list);
			while (transform.GetComponent<Part>() == null)
			{
				transform = transform.parent;
			}
			Part component = transform.GetComponent<Part>();
			bool flag2 = component.vessel != Ref.mainVessel;
			if (flag2)
			{
				Ref.controller.ShowMsg("Cannot use a part on a rocket that you are not controlling");
			}
			else
			{
				bool flag3 = !Ref.mainVessel.controlAuthority;
				if (flag3)
				{
					Ref.controller.ShowMsg("No control");
				}
				else
				{
					component.UsePart();
					bool flag4 = Ref.inputController.instructionsPartsHolder.activeSelf && component.HasEngineModule();
					if (flag4)
					{
						Ref.inputController.instructionsPartsHolder.SetActive(false);
						Ref.inputController.CheckAllInstructions();
					}
				}
			}
		}
	}

	private Transform GetTopPart(List<RaycastHit2D> hitParts)
	{
		Transform transform = hitParts[0].collider.transform;
		for (int i = 0; i < hitParts.Count; i++)
		{
			bool flag = hitParts[i].collider.transform.localPosition.z > transform.transform.localPosition.z;
			if (flag)
			{
				transform = hitParts[i].collider.transform;
			}
		}
		return transform;
	}

	public void ToggleDropdownMenu()
	{
		Transform child = this.menuHolder.transform.parent.GetChild(this.menuHolder.transform.GetSiblingIndex() - 1);
		child.GetComponent<MoveModule>().SetTargetTime((child.GetComponent<MoveModule>().targetTime.floatValue != 0f) ? 0f : 1.4f);
	}

	public void CloseDropdownMenu()
	{
		Transform child = this.menuHolder.transform.parent.GetChild(this.menuHolder.transform.GetSiblingIndex() - 1);
		child.GetComponent<MoveModule>().SetTargetTime(0f);
	}

	public void ApplyZoom(float zoomDelta)
	{
		bool flag = Ref.currentScene == Ref.SceneType.Game;
		if (flag)
		{
			bool mapView = Ref.mapView;
			if (mapView)
			{
				Ref.map.UpdateMapZoom(-Ref.map.mapPosition.z * (double)zoomDelta);
			}
			else
			{
				Ref.controller.SetCameraDistance(Mathf.Clamp(Ref.controller.cameraDistanceGame * zoomDelta, 8f, 2.5E+10f));
				Ref.planetManager.UpdateAtmosphereFade();
			}
		}
		bool flag2 = Ref.currentScene == Ref.SceneType.Build;
		if (flag2)
		{
			Build.main.MoveCamera(new Vector3(0f, 0f, (1f - zoomDelta) * 1.2f));
		}
	}

	public void ApplyDraging(Double3 posDeltaPixel)
	{
		bool flag = Ref.currentScene == Ref.SceneType.Game;
		if (flag)
		{
			bool mapView = Ref.mapView;
			if (mapView)
			{
				double num = (double)Mathf.Cos(Ref.cam.transform.eulerAngles.z * 0.0174532924f);
				double num2 = (double)Mathf.Sin(Ref.cam.transform.eulerAngles.z * 0.0174532924f);
				float f = Ref.cam.fieldOfView * 0.0174532924f;
				float num3 = Mathf.Sin(f) / ((1f + Mathf.Cos(f)) * 0.5f);
				Ref.map.UpdateMapPosition(Ref.map.mapPosition + new Double3(posDeltaPixel.x * num - posDeltaPixel.y * num2, posDeltaPixel.x * num2 + posDeltaPixel.y * num) * -Ref.map.mapPosition.z / (double)Screen.height * (double)num3);
			}
			else
			{
				float num4 = Mathf.Cos(Ref.cam.transform.eulerAngles.z * 0.0174532924f);
				float num5 = Mathf.Sin(Ref.cam.transform.eulerAngles.z * 0.0174532924f);
				float f2 = Ref.cam.fieldOfView * 0.0174532924f;
				float d = Mathf.Sin(f2) / ((1f + Mathf.Cos(f2)) * 0.5f);
				Ref.controller.viewPositons += new Vector2((float)posDeltaPixel.x * num4 - (float)posDeltaPixel.y * num5, (float)posDeltaPixel.x * num5 + (float)posDeltaPixel.y * num4) * Ref.controller.cameraDistanceGame / (float)Screen.height * d;
				bool flag2 = Ref.controller.viewPositons.sqrMagnitude > 2500f;
				if (flag2)
				{
					Ref.controller.viewPositons /= Ref.controller.viewPositons.magnitude / 50f;
				}
			}
		}
	}

	public IEnumerator ClickGlow(GameObject glow)
	{
		bool flag = glow.name == "Glow";
		if (flag)
		{
			glow.SetActive(true);
			yield return new WaitForSeconds(0.2f);
			glow.SetActive(false);
		}
		yield break;
	}

	public InputController()
	{
	}

	[Header("Input Controller")]
	[Space(6f)]
	public CanvasScaler[] canvasScalers;

	[Space]
	public LayerMask maskPartHibox;

	[Space]
	public InputController.UIColliders[] uIColliders;

	[Space]
	[Space]
	public float horizontalAxis;

	public Vector2 rcsInput;

	public Transform leftArrow;

	public Transform rightArrow;

	public Transform up;

	public Transform down;

	public Transform left;

	public Transform right;

	public GameObject switchToButton;

	public GameObject destroyButton;

	public GameObject recoverButton;

	public GameObject recoverMenuHolder;

	public GameObject instructionsPartsHolder;

	public GameObject instructionToggleThrottleHolder;

	public GameObject instructionSlideThrottleHolder;

	public GameObject instructionsMap;

	public GameObject instructionsTimewarp;

	public GameObject menuHolder;

	public GameObject rcsInputsHolder;

	[Space]
	public AudioSource clickSound;

	public AudioListener audioListener;

	private Vector3 oldPos;

	public Soundtrack soundtrackController;

	[Serializable]
	public class UIColliders
	{
		public UIColliders()
		{
		}

		public string grupName;

		public BoxCollider2D[] colliders;
	}
}
