using System;
using System.Collections;
using System.Collections.Generic;
using NewBuildSystem;
using UnityEngine;
using UnityEngine.UI;
using SFSML.HookSystem.ReWork.BaseHooks.UtilHooks;
using SFSML.HookSystem.ReWork;
using GooglePlay;

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
        if (this.instructionsPartsHolder.activeSelf || this.instructionSlideThrottleHolder.activeSelf || this.instructionToggleThrottleHolder.activeSelf)
        {
            return;
        }
        Saving.SaveSetting(Saving.SettingKey.seenGameInstructions, true);
        MyAchievements.main.UnlockAchievement("CgkIpOK5-NMPEAIQAQ");
    }

    private void LateUpdate()
    {
        if (Input.mouseScrollDelta.y != 0f)
        {
            this.ApplyZoom(1f - Input.mouseScrollDelta.y / 5f);
        }
        this.horizontalAxis = Input.GetAxisRaw("Horizontal");
        Vector2 posWorld = base.PixelPosToWorldPos(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            GameObject x = this.PointCastUI(Input.mousePosition, this.uIColliders);
            if (!(x != null))
            {
                this.StartTouchEmpty(posWorld, 0);
            }
            this.oldPos = Input.mousePosition;
        }
        if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) && !this.ClickUI(Input.mousePosition))
        {
            this.EndTouchEmpty(posWorld, 0, true);
        }
        if (Input.GetKey("q"))
        {
            this.ApplyZoom(1.02f);
        }
        if (Input.GetKey("e"))
        {
            this.ApplyZoom(0.98f);
        }
        if (Input.GetMouseButton(0))
        {
            this.TouchStayEmpty(posWorld, this.oldPos - Input.mousePosition, 0);
            if (!Input.GetMouseButtonDown(0))
            {
                this.ApplyDraging(Double3.ToDouble3(this.oldPos - Input.mousePosition));
            }
            this.oldPos = Input.mousePosition;
        }
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
                        if (!(name == "LOADING Menu"))
                        {
                            if (name == "Keyboard")
                            {
                                TouchKeyboard.main.OnKeyboardClick(clickPosPixel);
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
        foreach (InputController.UIColliders uicolliders in colliders)
        {
            GameObject gameObject = this.PointCastUI(clickPosPixel, uicolliders.colliders);
            if (gameObject != null)
            {
                return gameObject;
            }
        }
        return null;
    }

    public GameObject PointCastUI(Vector2 clickPosPixel, BoxCollider2D[] colliders)
    {
        float d = this.resolutionReference.position.x / 1000f;
        foreach (BoxCollider2D boxCollider2D in colliders)
        {
            if (boxCollider2D.gameObject.activeInHierarchy)
            {
                Vector2 a = (Vector2)boxCollider2D.GetComponent<RectTransform>().position + boxCollider2D.offset * d;
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
        List<RaycastHit2D> list = new List<RaycastHit2D>(Physics2D.RaycastAll(clickPos, Vector2.up, 0.01f, this.maskPartHibox));
        if (list.Count == 0)
        {
            return;
        }
        Transform transform = this.GetTopPart(list);
        while (transform.GetComponent<Part>() == null)
        {
            transform = transform.parent;
        }
        Part component = transform.GetComponent<Part>();
        if (component.vessel != Ref.mainVessel)
        {
            MsgController.ShowMsg("Cannot use a part on a rocket that you are not controlling");
            return;
        }
        if (!Ref.mainVessel.controlAuthority)
        {
            MsgController.ShowMsg("No control");
            return;
        }
        component.UsePart();
        if (Ref.inputController.instructionsPartsHolder.activeSelf && component.HasEngineModule())
        {
            Ref.inputController.instructionsPartsHolder.SetActive(false);
            Ref.inputController.CheckAllInstructions();
        }
    }

    private void PointCastOthers()
    {
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
            if (Ref.mapView)
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
                if (Ref.controller.viewPositons.sqrMagnitude > 2500f)
                {
                    Ref.controller.viewPositons /= Ref.controller.viewPositons.magnitude / 50f;
                }
            }
        }
    }

    public IEnumerator ClickGlow(GameObject glow)
    {
        if (glow.name == "Glow")
        {
            glow.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            glow.SetActive(false);
        }
        yield break;
    }

    public void PlayClickSound(float volume)
    {
        this.clickSound.Play();
        this.clickSound.volume = volume;
    }

    [Header("Input Controller")]
    [Space(6f)]
    public CanvasScaler[] canvasScalers;

    [Space]
    public LayerMask maskPartHibox;

    [Space]
    public LayerMask partsCollider;

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

    public MoveModule inputButtonsAnimation;

    [Space]
    public AudioSource clickSound;

    public AudioListener audioListener;

    private Vector3 oldPos;

    public Soundtrack soundtrackController;

    public Transform resolutionReference;

    [Serializable]
    public class UIColliders
    {
        public string grupName;

        public BoxCollider2D[] colliders;
    }
}
