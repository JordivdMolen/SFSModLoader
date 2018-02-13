using GooglePlay;
using NewBuildSystem;
using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class InputController : global::Touch
{
	[Serializable]
	public class UIColliders
	{
		public string grupName;

		public BoxCollider2D[] colliders;
	}

	[Header("Input Controller"), Space(6f)]
	public CanvasScaler canvasScaler;

	[Space]
	public LayerMask maskPartHibox;

	[Space]
	public InputController.UIColliders[] uIColliders;

	[Space, Space]
	public float horizontalAxis;

	public Transform leftArrow;

	public Transform rightArrow;

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

	[Space]
	public AudioSource clickSound;

	public AudioListener audioListener;

	private Vector3 oldPos;

	public Soundtrack soundtrackController;

	private void Start()
	{
		this.SetFps();
		this.SetEnableMusic();
		this.SetEnableSound();
		Screen.sleepTimeout = -1;
		Ref.SceneType currentScene = Ref.currentScene;
		if (currentScene != Ref.SceneType.MainMenu)
		{
			if (currentScene != Ref.SceneType.Build)
			{
				if (currentScene == Ref.SceneType.Game)
				{
					Screen.orientation = ((!Saving.LoadSetting(Saving.SettingKey.disableAutoRotate)) ? ScreenOrientation.AutoRotation : ScreenOrientation.Portrait);
					if (Ref.lastScene == Ref.SceneType.Build)
					{
						this.instructionToggleThrottleHolder.SetActive(true);
						if (!Saving.LoadSetting(Saving.SettingKey.seenGameInstructions))
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
		if (this.soundtrackController == null)
		{
			return;
		}
		this.soundtrackController.gameObject.SetActive(!Saving.LoadSetting(Saving.SettingKey.disableMusic));
		this.soundtrackController.CancelInvoke();
		this.soundtrackController.SelectRandomSoundtrack();
	}

	public void SetEnableSound()
	{
		AudioListener.volume = (float)((!Saving.LoadSetting(Saving.SettingKey.disableSound)) ? 1 : 0);
	}

	public void CheckAllInstructions()
	{
		if (this.instructionsPartsHolder.activeSelf || this.instructionSlideThrottleHolder.activeSelf || this.instructionToggleThrottleHolder.activeSelf)
		{
			return;
		}
		MonoBehaviour.print("Seen all instructions");
		Saving.SaveSetting(Saving.SettingKey.seenGameInstructions, true);
		MyAchievements.main.UnlockAchievement("CgkIpOK5-NMPEAIQAQ");
	}

	private void LateUpdate()
	{
		if (Input.mouseScrollDelta.y != 0f)
		{
			this.ApplyZoom(1f - Input.mouseScrollDelta.y / 10f);
		}
		this.horizontalAxis = Input.GetAxisRaw("Horizontal");
		Vector2 posWorld = base.PixelPosToWorldPos(Input.mousePosition);
		if (Input.GetMouseButtonDown(0))
		{
			this.StartTouchEmpty(posWorld, 0);
			this.oldPos = Input.mousePosition;
		}
		if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) && !this.ClickUI(Input.mousePosition))
		{
			this.EndTouchEmpty(posWorld, 0, true);
		}
		if (Input.GetKey("q"))
		{
			this.ApplyZoom(1.015f);
		}
		if (Input.GetMouseButton(0))
		{
			this.TouchStayEmpty(posWorld, this.oldPos - Input.mousePosition, 0);
			if (Ref.mapView && !Input.GetMouseButtonDown(0))
			{
				this.ApplyDraging(Double3.ToDouble3(this.oldPos - Input.mousePosition));
			}
			this.oldPos = Input.mousePosition;
		}
		float num = (float)((Screen.height <= Screen.width) ? 1334 : 750);
		if (this.canvasScaler.referenceResolution.x != num)
		{
			if (Ref.currentScene == Ref.SceneType.MainMenu)
			{
				return;
			}
			this.canvasScaler.referenceResolution = new Vector2(num, 1f);
			if (Ref.currentScene == Ref.SceneType.Game && this.recoverMenuHolder.activeSelf)
			{
				Ref.controller.StartRecovery();
			}
			if (Ref.saving != null && Ref.saving.loadingMenuHolder.activeSelf)
			{
				Ref.saving.TryStartLoadProcess();
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
		Ref.SceneType currentScene = Ref.currentScene;
		if (currentScene != Ref.SceneType.MainMenu)
		{
			if (currentScene != Ref.SceneType.Build)
			{
				if (currentScene != Ref.SceneType.Game)
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
		if (currentScene != Ref.SceneType.MainMenu)
		{
			if (currentScene != Ref.SceneType.Build)
			{
				if (currentScene != Ref.SceneType.Game)
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
		if (currentScene != Ref.SceneType.MainMenu)
		{
			if (currentScene != Ref.SceneType.Build)
			{
				if (currentScene == Ref.SceneType.Game)
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
		if (Build.main != null)
		{
			Build.main.OnClickUI(clickPosPixel);
		}
		GameObject gameObject = this.PointCastUI(clickPosPixel, this.uIColliders);
		if (gameObject == null)
		{
			return false;
		}
		base.StartCoroutine(this.ClickGlow(gameObject.transform.GetChild(0).gameObject));
		CustomEvent component = gameObject.GetComponent<CustomEvent>();
		if (component != null)
		{
			component.customEvent.Invoke();
			return true;
		}
		string name = gameObject.name;
		if (name != null)
		{
			if (!(name == "Recover Vessel Button"))
			{
				if (!(name == "Cancel Recovery Button"))
				{
					if (!(name == "Complete Mission Button"))
					{
						if (!(name == "LOADING Menu Holder"))
						{
							if (name == "Keyboard")
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
		return true;
	}

	private void ClickEmpty(Vector2 clickPosWorld)
	{
		Ref.SceneType currentScene = Ref.currentScene;
		if (currentScene != Ref.SceneType.MainMenu)
		{
			if (currentScene != Ref.SceneType.Build)
			{
				if (currentScene == Ref.SceneType.Game)
				{
					if (Ref.mapView)
					{
						Ref.map.OnClickEmpty(clickPosWorld);
					}
					else if (Ref.mainVessel != null)
					{
						this.PointCastParts(clickPosWorld);
					}
				}
			}
		}
	}

	public GameObject PointCastUI(Vector2 clickPosPixel, InputController.UIColliders[] colliders)
	{
		for (int i = 0; i < colliders.Length; i++)
		{
			InputController.UIColliders uIColliders = colliders[i];
			GameObject gameObject = this.PointCastUI(clickPosPixel, uIColliders.colliders);
			if (gameObject != null)
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
		for (int i = 0; i < colliders.Length; i++)
		{
			BoxCollider2D boxCollider2D = colliders[i];
			if (boxCollider2D.gameObject.activeInHierarchy)
			{
				Vector2 a = boxCollider2D.GetComponent<RectTransform>().position + (Vector3)boxCollider2D.offset * d;
				Vector2 vector = a - boxCollider2D.size * 0.5f * d;
				Vector2 vector2 = a + boxCollider2D.size * 0.5f * d;
				if (clickPosPixel.x >= vector.x && clickPosPixel.y >= vector.y && clickPosPixel.x <= vector2.x && clickPosPixel.y <= vector2.y)
				{
					return boxCollider2D.gameObject;
				}
			}
		}
		return null;
	}

	private void PointCastParts(Vector2 clickPos)
	{
		RaycastHit2D[] array = Physics2D.RaycastAll(clickPos, Vector2.up, 0.01f, this.maskPartHibox);
		if (array.Length > 0)
		{
			Transform transform = null;
			for (int i = 0; i < array.Length; i++)
			{
				transform = array[i].collider.transform.parent;
				if (array[i].collider.tag == "Priority")
				{
					break;
				}
			}
			while (transform.GetComponent<Part>() == null)
			{
				transform = transform.parent;
			}
			Part component = transform.GetComponent<Part>();
			if (component.vessel == Ref.mainVessel)
			{
				if (Ref.mainVessel.controlAuthority)
				{
					component.UsePart();
					if (component.HasEngineModule() && Ref.inputController.instructionsPartsHolder.activeSelf)
					{
						Ref.inputController.instructionsPartsHolder.SetActive(false);
						Ref.inputController.CheckAllInstructions();
					}
				}
				else
				{
					Ref.controller.ShowMsg("No control");
				}
			}
			else
			{
				Ref.controller.ShowMsg("Cannot use a part on a rocket that you are not controlling");
			}
		}
	}

	public void ToggleDropdownMenu()
	{
		Transform child = this.menuHolder.transform.parent.GetChild(this.menuHolder.transform.GetSiblingIndex() - 1);
		child.GetComponent<MoveModule>().SetTargetTime((child.GetComponent<MoveModule>().targetTime.floatValue != 0f) ? 0f : 1.2f);
	}

	public void CloseDropdownMenu()
	{
		Transform child = this.menuHolder.transform.parent.GetChild(this.menuHolder.transform.GetSiblingIndex() - 1);
		child.GetComponent<MoveModule>().SetTargetTime(0f);
	}

	public void ApplyZoom(float zoomDelta)
	{
		if (Ref.currentScene == Ref.SceneType.Game)
		{
			if (Ref.mapView)
			{
				Ref.map.UpdateMapZoom(-Ref.map.mapPosition.z * (double)zoomDelta);
			}
			else
			{
				Ref.controller.SetCameraDistance(Mathf.Clamp(Ref.controller.cameraDistanceGame * zoomDelta, 8f, 2.5E+10f));
				Ref.planetManager.UpdateAtmosphereFade();
			}
		}
		if (Ref.currentScene == Ref.SceneType.Build)
		{
			Build.main.MoveCamera(new Vector3(0f, 0f, (1f - zoomDelta) * 1.2f));
		}
	}

	public void ApplyDraging(Double3 posDeltaPixel)
	{
		if (Ref.currentScene == Ref.SceneType.Game)
		{
			double num = (double)Mathf.Cos(Ref.cam.transform.eulerAngles.z * 0.0174532924f);
			double num2 = (double)Mathf.Sin(Ref.cam.transform.eulerAngles.z * 0.0174532924f);
			float f = Ref.cam.fieldOfView * 0.0174532924f;
			float num3 = Mathf.Sin(f) / ((1f + Mathf.Cos(f)) * 0.5f);
			Ref.map.UpdateMapPosition(Ref.map.mapPosition + new Double3(posDeltaPixel.x * num - posDeltaPixel.y * num2, posDeltaPixel.x * num2 + posDeltaPixel.y * num) * -Ref.map.mapPosition.z / (double)Screen.height * (double)num3);
		}
	}

	[DebuggerHidden]
	public IEnumerator ClickGlow(GameObject glow)
	{
        if (glow.name == "Glow")
        {
            glow.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            glow.SetActive(false);
        }
    }
}
