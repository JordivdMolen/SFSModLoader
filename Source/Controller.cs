using NewBuildSystem;
using SFSML;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
	[Header(" Controller")]
	public string startAdress;

	public CelestialBodyData loadedPlanet;

	public List<Vessel> vessels;

	public float camTargetAngle;

	public float camAngularVelocity;

	public float createHeight;

	[Header(" Time")]
	public double globalTime;

	public int timewarpPhase;

	public int[] warpSpeeds;

	public double startedTimewarpTime;

	[Header(" Rotation Controll")]
	public float xAxis;

	[Header(" UI Refs")]
	public Image throttleBar;

	public Transform fuelIconPrefab;

	public Text heightUI;

	public Text velocityUI;

	public Text periapsisUI;

	public Text apoapsisUI;

	public Text throttlePercentUI;

	public Text throttleOnUI;

	public Text msgUI;

	public Text warpedTimeCounterUI;

	[Space]
	public MoveModule timewarpButtonColorMove;

	public MoveModule throttleColorMove;

	[Space]
	public List<EngineModule.FuelIcon> fuelIcons;

	[Header(" Prefabs Refs")]
	public Transform vesselPrefab;

	public bool enableVelocityOffset;

	[BoxGroup, Header("Camera")]
	public float cameraDistanceGame;

	[BoxGroup]
	public LayerMask gameView;

	[BoxGroup]
	public LayerMask scaledView;

	[BoxGroup]
	public LayerMask mapView;

	[BoxGroup]
	public float scaledSpaceThreshold;

	public Transform stars;

	public Transform audioListener;

	public Transform explosionParticle;

	public float updatedPersistantTime;

	public float regularUpdateTime;

	public Vector2 cameraPositionGame;

	public Vector2 cameraOffset;

	public RectTransform directionArrow;

	public CanvasScaler canvas;

	public Canvas canvass;

	private IEnumerator msgCoroutine;

	public bool showOrbitParameters;

	public PartDatabase partDatabase;

	public void SetCameraDistance(float newDistance)
	{
		this.cameraDistanceGame = newDistance;
	}

	private void Start()
	{
		this.IniciateGameScene();
	}

	private void IniciateGameScene()
	{
		this.updatedPersistantTime = Time.time + 10f;
		CelestialBodyData planetByName = Ref.GetPlanetByName(this.startAdress);
		Double3 @double = new Double3(-450.0, planetByName.radius + 30.0);
		Ref.map.InitializeMap();
		GameSaving.GameSave gameSave = GameSaving.GameSave.LoadPersistant();
		if (Ref.lastScene == Ref.SceneType.Build)
		{
			if (gameSave != null && gameSave.vessels.Count > 0)
			{
				gameSave.velocityOffset = Double3.zero;
				gameSave.selectedVesselId = -1;
				GameSaving.LoadForLaunch(gameSave, @double);
				MonoBehaviour.print("There is a Persistant-Save, and a Rocket To Create");
			}
			else
			{
				MonoBehaviour.print("There is No Persistant-Save, created rocket into new scene empty scene");
				Ref.planetManager.SwitchLocation(planetByName, @double, false, true);
				Ref.map.following = planetByName;
				Ref.map.UpdateMapPosition(new Double3(0.0, @double.y / 10000.0));
				Ref.map.UpdateMapZoom(@double.y / 10000.0 / 20.0);
			}
			this.CreatePartsFromBuild(@double);
			Ref.mainVessel.SetThrottle(new Vessel.Throttle(false, 0.65f));
			Ref.mapView = false;
			Ref.controller.SetCameraDistance(23f);
		}
		else if (gameSave != null)
		{
			GameSaving.LoadGame(gameSave);
			MonoBehaviour.print("No rocket to create, loaded persistant");
		}
	}

	private void CreatePartsFromBuild(Double3 launchPadPosition)
	{
		Build.BuildSave buildSave = JsonUtility.FromJson<Build.BuildSave>(Ref.LoadJsonString(Saving.SaveKey.ToLaunch));
		PartGrid.PositionForLaunch(buildSave.parts, this.partDatabase);
		List<Part> list = CreateRocket.CreateBuildParts((launchPadPosition - Ref.positionOffset).toVector3, buildSave, this.partDatabase);
		List<Part> list2 = new List<Part>();
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].GetComponent<ControlModule>() != null)
			{
				list2.Add(list[i]);
			}
		}
		list2.AddRange(list);
		List<Vessel> list3 = this.CreateVesselsFromParts(list2, Vector2.zero, 0f, new Vessel.Throttle(false, 0.65f), new List<string>());
		Ref.mainVessel = null;
		foreach (Vessel current in list3)
		{
			if (current.controlAuthority)
			{
				Ref.mainVessel = current;
				Ref.map.SelectVessel(current);
				break;
			}
		}
		if (Ref.mainVessel == null)
		{
			Ref.mainVessel = list3[0];
			Ref.map.SelectVessel(list3[0]);
		}
		Ref.map.UpdateVesselsMapIcons();
		this.UpdateVesselButtons();
	}

	public List<Vessel> CreateVesselsFromParts(List<Part> unatendedParts, Vector2 velocity, float angularVecloty, Vessel.Throttle throttle, List<string> inheritedArchivments)
	{
		List<Vessel> list = new List<Vessel>();
		for (int i = 0; i < unatendedParts.Count; i++)
		{
			if (unatendedParts[i] != null && unatendedParts[i].vessel == null && unatendedParts[i].gameObject.activeSelf)
			{
				Vessel item = Vessel.CreateVessel(unatendedParts[i], velocity, angularVecloty, throttle, inheritedArchivments, this.loadedPlanet.mapRefs.holder);
				list.Add(item);
			}
		}
		return list;
	}

	public void ShowMsg(string msgText)
	{
		this.ShowMsg(msgText, 3.5f);
	}

	public void ShowMsg(string msgText, float displayTime)
	{
		if (this.msgCoroutine != null)
		{
			base.StopCoroutine(this.msgCoroutine);
		}
		this.msgCoroutine = this.MsgCoroutine(msgText, displayTime);
		base.StartCoroutine(this.msgCoroutine);
	}

	[DebuggerHidden]
	private IEnumerator MsgCoroutine(string msgText, float displayTime)
	{
        msgUI.text = msgText;
        msgUI.gameObject.SetActive(true);

        while (displayTime > 0)
        {
            displayTime -= Time.deltaTime;
            msgUI.color = new Color(1, 1, 1, displayTime);

            yield return new WaitForEndOfFrame();
        }

        msgUI.gameObject.SetActive(false);
    }

	public void Relaunch()
	{
		Ref.LoadScene(Ref.SceneType.Build);
		Ref.loadLaunchedRocket = true;
		GameSaving.GameSave.UpdatePersistantSave();
	}

	public void BuildNew()
	{
		Ref.LoadScene(Ref.SceneType.Build);
		GameSaving.GameSave.UpdatePersistantSave();
	}

	public void AskOpenTutorial()
	{
		this.OpenTutorial();
	}

	public void OpenTutorial()
	{
		Ref.openTutorial = true;
		Ref.LoadScene(Ref.SceneType.MainMenu);
	}

	public void ExitGameScene()
	{
		Ref.LoadScene(Ref.SceneType.MainMenu);
		GameSaving.GameSave.UpdatePersistantSave();
	}

	private void FixedUpdate()
	{
		if (Ref.timeWarping || Ref.mainVessel == null)
		{
			return;
		}
		this.CheckPositionBounds();
		if (Ref.velocityOffset.sqrMagnitude2d > 0.0)
		{
			Ref.planetManager.UpdatePositionOffset(Ref.positionOffset + Ref.velocityOffset * (double)Time.fixedDeltaTime);
		}
		this.enableVelocityOffset = this.GetEnableVelocityOffset(Ref.mainVessel);
		if (this.enableVelocityOffset)
		{
			this.CheckVelocityBounds();
		}
		else if (Ref.velocityOffset.sqrMagnitude2d > 0.0)
		{
			this.OffsetSceneVelocity(-Ref.velocityOffset.toVector2);
		}
	}

	private void Update()
	{
		if (this.regularUpdateTime < Time.time)
		{
			this.RegularUpdate();
			this.regularUpdateTime = Time.time + 0.5f;
		}
		this.globalTime += (double)((float)this.warpSpeeds[this.timewarpPhase] * Time.deltaTime);
		bool flag = Ref.mainVessel != null && Ref.mainVessel.orbits.Count > 0;
		double num = (!flag) ? double.PositiveInfinity : Ref.mainVessel.orbits[0].stopTimeWarpTime;
		if (flag)
		{
			Ref.planetManager.UpdatePositionOffset(Ref.mainVessel.GetGlobalPosition.roundTo1000);
		}
		if (this.globalTime > num)
		{
			this.globalTime = num;
			Ref.planetManager.UpdatePositionOffset(Ref.mainVessel.GetGlobalPosition.roundTo1000);
			this.timewarpPhase = 1;
			this.DecelerateTime();
			Ref.controller.ShowMsg("Cannot time warp below: " + this.loadedPlanet.minTimewarpHeightKm.ToString() + "km");
		}
      
	}

	private void RegularUpdate()
	{
		this.UpdateVesselButtons();
		bool flag = Ref.timeWarping || Ref.mainVesselHeight > this.loadedPlanet.minTimewarpHeightKm * 1000.0 || (Ref.mainVessel != null && Ref.mainVessel.GetGlobalVelocity.magnitude2d < 0.10000000149011612 && Ref.mainVessel.OnSurface);
		if (this.timewarpButtonColorMove.targetTime.floatValue == 1f != flag)
		{
			this.timewarpButtonColorMove.SetTargetTime((float)((!flag) ? 0 : 1));
			if (flag && this.msgUI.color.a == 0f)
			{
				this.ShowMsg("Time warp enabled");
			}
		}
		if (this.updatedPersistantTime < Time.time)
		{
			this.updatedPersistantTime = Time.time + 15f;
			GameSaving.GameSave.UpdatePersistantSave();
		}
	}

	private void LateUpdate()
	{
		double apo = 0.0;
		double peri = 0.0;
		if (Ref.mainVessel != null)
		{
			this.cameraPositionGame = ((!Ref.timeWarping) ? Ref.mainVessel.partsManager.rb2d.worldCenterOfMass : (Ref.mainVessel.GetGlobalPosition - Ref.positionOffset).toVector2);
		}
		if (!Ref.mapView)
		{
			this.scaledSpaceThreshold = ((Ref.mainVesselHeight <= 500000.0) ? 100000f : 5000f);
			bool flag = this.cameraDistanceGame > this.scaledSpaceThreshold;
			Ref.cam.cullingMask = ((!flag) ? this.gameView : this.scaledView);
			if (!flag)
			{
				Ref.cam.transform.position = new Vector3(this.cameraPositionGame.x, this.cameraPositionGame.y, -this.cameraDistanceGame) + (Vector3)this.cameraOffset;
			}
			else
			{
				Ref.cam.transform.position = ((Ref.positionOffset + new Double3((double)this.cameraPositionGame.x, (double)this.cameraPositionGame.y, (double)(-(double)this.cameraDistanceGame))) / 10000.0).toVector3;
			}
			if (flag)
			{
				Ref.planetManager.PositionLowRezPlanets();
			}
			Ref.planetManager.scaledHolder.gameObject.SetActive(flag);
			Ref.planetManager.chuncksHolder.gameObject.SetActive(!flag);
		}
		this.cameraOffset = Vector2.zero;
		this.audioListener.transform.position = new Vector3(this.cameraPositionGame.x, this.cameraPositionGame.y, Ref.mapView ? (-Mathf.Max(this.cameraDistanceGame, 1000f)) : (-this.cameraDistanceGame));
		if (Ref.mainVessel != null)
		{
			Ref.mainVesselHeight = Ref.mainVessel.GetGlobalPosition.magnitude2d - this.loadedPlanet.radius;
			Ref.mainVesselAngleToPlanet = (float)Math.Atan2(Ref.mainVessel.GetGlobalPosition.y, Ref.mainVessel.GetGlobalPosition.x) * 57.29578f;
			Ref.mainVesselTerrainHeight = Ref.mainVesselHeight - this.loadedPlanet.GetTerrainSampleAtAngle((double)Ref.mainVesselAngleToPlanet);
		}
		this.camTargetAngle = ((Ref.mainVesselHeight >= this.loadedPlanet.cameraSwitchHeightM) ? 0f : (Ref.mainVesselAngleToPlanet - 90f));
		Ref.cam.transform.eulerAngles = new Vector3(0f, 0f, Mathf.SmoothDampAngle(Ref.cam.transform.eulerAngles.z, this.camTargetAngle, ref this.camAngularVelocity, 0.5f));
		this.stars.transform.eulerAngles = new Vector3(0f, 0f, 100f);
		Ref.map.UpdateCameraRotation(Ref.cam.transform.localEulerAngles);
		this.UpdateUI((!(Ref.mainVessel != null)) ? 0.0 : Ref.mainVessel.GetGlobalVelocity.magnitude2d, (Ref.mainVesselTerrainHeight >= 2000.0) ? Ref.mainVesselHeight : Ref.mainVesselTerrainHeight, apo, peri, Ref.mainVesselTerrainHeight < 2000.0);
		if (this.loadedPlanet.atmosphereData.hasAtmosphere)
		{
			float num = Mathf.Clamp01((float)(Ref.mainVesselHeight / this.loadedPlanet.atmosphereData.atmosphereHeightM));
			num = Mathf.Pow(num - 1f, 3f) + 1f;
			Ref.partShader.SetFloat("_Intensity", Mathf.Lerp(this.loadedPlanet.atmosphereData.shadowIntensity, 1.7f, num));
		}
		if (Ref.timeWarping)
		{
			this.warpedTimeCounterUI.text = Ref.GetTimeString(this.globalTime - this.startedTimewarpTime);
		}
		this.SomeStuff();
		Ref.cam.fieldOfView = 60f * ((Screen.width <= Screen.height) ? 1f : ((float)Screen.height / (float)Screen.width * 1.075f));
		this.SetVelocityDirectionMarker((!(Ref.mainVessel != null)) ? Vector2.zero : (Vector2)(Quaternion.Euler(0f, 0f, -Ref.cam.transform.rotation.eulerAngles.z) * Ref.mainVessel.GetGlobalVelocity.toVector3));
	}

	private void SomeStuff()
	{
		if (Input.GetKeyDown("."))
		{
			this.AccelerateTime();
		}
		if (Input.GetKeyDown(","))
		{
			this.DecelerateTime();
		}
		if (Input.GetKeyDown("space") && !Ref.saving.savingMenuHolder.activeSelf)
		{
			if (Ref.mainVessel.controlAuthority)
			{
				this.ToggleThrottle();
			}
			else if (Ref.controller.msgUI.color.a < 0.6f)
			{
				Ref.controller.ShowMsg("No control");
			}
		}
        
		if (Input.GetAxisRaw("Vertical") != 0f)
		{
			if (Ref.mainVessel.controlAuthority)
			{
				Ref.mainVessel.SetThrottle(new Vessel.Throttle(Ref.mainVessel.throttle.throttleOn, Mathf.Clamp01(Ref.mainVessel.throttle.throttleRaw + Input.GetAxisRaw("Vertical") / 2f * Time.deltaTime)));
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
		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		{
			if (Ref.mainVessel.controlAuthority)
			{
				Ref.mainVessel.SetThrottle(new Vessel.Throttle(Ref.mainVessel.throttle.throttleOn, Mathf.Clamp01(Ref.mainVessel.throttle.throttleRaw + 0.5f * Time.deltaTime)));
			}
			else if (Ref.controller.msgUI.color.a < 0.6f)
			{
				Ref.controller.ShowMsg("No control");
			}
		}
		if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
		{
			if (Ref.mainVessel.controlAuthority)
			{
				Ref.mainVessel.SetThrottle(new Vessel.Throttle(Ref.mainVessel.throttle.throttleOn, Mathf.Clamp01(Ref.mainVessel.throttle.throttleRaw - 0.5f * Time.deltaTime)));
			}
			else if (Ref.controller.msgUI.color.a < 0.6f)
			{
				Ref.controller.ShowMsg("No control");
			}
		}
		if (Input.GetKeyDown("i"))
		{
			Ref.inputController.switchToButton.transform.parent.parent.gameObject.SetActive(!Ref.inputController.switchToButton.transform.parent.parent.gameObject.activeSelf);
		}
	}

	[Button("Take screenshot", ButtonSizes.Small)]
	private void A()
	{
		ScreenCapture.CaptureScreenshot("Assets/Screenshots/Screenshot" + UnityEngine.Random.Range(0, 100000).ToString() + ".png");
	}

	public void UpdateVesselButtons()
	{
		Vessel vessel = (!Ref.mapView) ? Ref.mainVessel : Ref.selectedVessel;
		bool flag = Ref.mapView && Ref.selectedVessel != Ref.mainVessel && Ref.selectedVessel != null;
		bool flag2 = vessel != null && vessel.OnSurface && vessel.GetVesselPlanet.bodyName == Ref.controller.startAdress;
		bool flag3 = Ref.mapView && !flag2 && Ref.selectedVessel != Ref.mainVessel && Ref.selectedVessel != null;
		float num = -90f;
		Ref.inputController.switchToButton.transform.localPosition = new Vector2(0f, (!flag) ? 100f : num);
		num += (float)((!flag) ? 0 : -54);
		Ref.inputController.recoverButton.transform.localPosition = new Vector2(0f, (!flag2) ? 100f : num);
		num += (float)((!flag2) ? 0 : -54);
		Ref.inputController.destroyButton.transform.localPosition = new Vector2(0f, (!flag3) ? 100f : num);
		num += (float)((!flag3) ? 0 : -54);
		this.warpedTimeCounterUI.transform.localPosition = new Vector2(4.5f, num + 2f);
	}

	public void SwitchVessel()
	{
		this.SwitchVessel(Ref.selectedVessel);
	}

	public void SwitchVessel(Vessel newVessel)
	{
		if (newVessel == null || newVessel == Ref.mainVessel)
		{
			return;
		}
		Ref.mainVessel = newVessel;
		Ref.map.UpdateVesselsMapIcons();
		Double3 getGlobalPosition = newVessel.GetGlobalPosition;
		CelestialBodyData getVesselPlanet = newVessel.GetVesselPlanet;
		Ref.map.SelectVessel(Ref.selectedVessel);
		this.RepositionFuelIcons();
		this.UpdateVesselButtons();
		Ref.map.following = newVessel.GetVesselPlanet;
		Ref.map.UpdateMapPosition(newVessel.GetGlobalPosition / 10000.0);
		Ref.map.UpdateMapZoom(Ref.map.mapPosition.z);
		newVessel.SetThrottle(newVessel.throttle);
		bool flag = newVessel.GetGlobalPosition.magnitude2d < newVessel.GetVesselPlanet.radius + newVessel.GetVesselPlanet.minTimewarpHeightKm * 1000.0;
		Vessel.ToState toState = (Ref.timeWarping && !flag) ? Vessel.ToState.ToTimewarping : Vessel.ToState.ToRealTime;
		Ref.planetManager.SwitchLocation(getVesselPlanet, getGlobalPosition, false, true);
		bool flag2 = toState == Vessel.ToState.ToRealTime && this.GetEnableVelocityOffset(newVessel);
		double x = (!flag2) ? 0.0 : (Math.Floor((Math.Abs(Ref.mainVessel.GetGlobalVelocity.x) + 25.0) / 50.0) * 50.0 * (double)Math.Sign(Ref.mainVessel.GetGlobalVelocity.x));
		double y = (!flag2) ? 0.0 : (Math.Floor((Math.Abs(Ref.mainVessel.GetGlobalVelocity.y) + 25.0) / 50.0) * 50.0 * (double)Math.Sign(Ref.mainVessel.GetGlobalVelocity.y));
		Ref.velocityOffset = new Double3(x, y);
		if (Ref.timeWarping && flag)
		{
			this.timewarpPhase = 1;
			this.DecelerateTime();
			Ref.controller.ShowMsg("Cannot time warp below: " + newVessel.GetVesselPlanet.minTimewarpHeightKm.ToString() + "km");
		}
		Ref.mainVessel.SetThrottle(new Vessel.Throttle(false, newVessel.throttle.throttleRaw));
		newVessel.SetVesselState(toState);
		Ref.planetManager.totalDistanceMoved += 100000.0;
	}

	public void StartRecovery()
	{
		Ref.inputController.leftArrow.parent.gameObject.SetActive(false);
		foreach (EngineModule.FuelIcon current in Ref.controller.fuelIcons)
		{
			if (current.icon != null)
			{
				current.icon.gameObject.SetActive(false);
			}
		}
		Vessel vessel = (!Ref.mapView) ? Ref.mainVessel : Ref.selectedVessel;
		Ref.inputController.recoverMenuHolder.transform.GetChild(8).GetChild(1).GetComponent<Text>().text = ((!(vessel == Ref.mainVessel)) ? "Complete Recovery" : "Complete Mission");
		List<string> list = (!(vessel != null)) ? new List<string>() : new List<string>(vessel.vesselAchievements);
		if (list.Contains("Reached 10 km altitude.") && list.Count > 2)
		{
			list.Remove("Reached 5 km altitude.");
		}
		if (list.Contains("Reached 15 km altitude.") && list.Count > 2)
		{
			list.Remove("Reached 10 km altitude.");
		}
		if (list.Contains("Passed the Karman Line, leaving the /atmosphere and reaching space.") && list.Count > 2)
		{
			list.Remove("Reached 15 km altitude.");
		}
		if (list.Contains("Reached low " + Ref.controller.startAdress + " orbit.") && list.Count > 4)
		{
			list.Remove("Passed the Karman Line, leaving the /atmosphere and reaching space.");
		}
		Ref.inputController.recoverMenuHolder.SetActive(true);
		bool flag = Ref.inputController.canvasScaler.referenceResolution.x == 750f;
		Text component = Ref.inputController.recoverMenuHolder.transform.GetChild(4).GetComponent<Text>();
		Text component2 = Ref.inputController.recoverMenuHolder.transform.GetChild(6).GetComponent<Text>();
		component.transform.localPosition = new Vector3(0f, component.transform.localPosition.y);
		Ref.inputController.recoverMenuHolder.transform.GetChild(3).localPosition = new Vector3(0f, component.transform.localPosition.y + 10f);
		Ref.inputController.recoverMenuHolder.transform.GetChild(5).localPosition = new Vector3(5000f, component.transform.localPosition.y + 10f);
		Ref.inputController.recoverMenuHolder.transform.GetChild(3).localScale = new Vector3(1f, (!flag) ? 0.51f : 1f);
		Ref.inputController.recoverMenuHolder.transform.GetChild(5).localScale = new Vector3(1f, (!flag) ? 0.51f : 1f);
		component.text = string.Empty;
		component2.text = string.Empty;
		int num = 0;
		Text text = component;
		foreach (string current2 in list)
		{
			Text expr_36B = text;
			expr_36B.text += "- ";
			for (int i = 0; i < current2.Length; i++)
			{
				if (current2[i].ToString() == "/")
				{
					Text expr_3B1 = text;
					expr_3B1.text += "\r\n \r\n ";
					num += 2;
				}
				else
				{
					Text expr_3D3 = text;
					expr_3D3.text += current2[i].ToString();
				}
			}
			Text expr_411 = text;
			expr_411.text += "\r\n \r\n \r\n";
			num += 3;
			if (num > ((!flag) ? 24 : 51))
			{
				if (text == component2 || flag)
				{
					break;
				}
				text = component2;
				num = 0;
				component.transform.localPosition = new Vector3(-300f, component.transform.localPosition.y);
				component2.transform.localPosition = new Vector3(300f, component.transform.localPosition.y);
				Ref.inputController.recoverMenuHolder.transform.GetChild(3).localPosition = new Vector3(-300f, component.transform.localPosition.y + 10f);
				Ref.inputController.recoverMenuHolder.transform.GetChild(5).localPosition = new Vector3(300f, component.transform.localPosition.y + 10f);
			}
		}
	}

	public void CompleteRecovery()
	{
		Vessel vessel = (!Ref.mapView) ? Ref.mainVessel : Ref.selectedVessel;
		bool flag = vessel == Ref.mainVessel;
		if (vessel == null)
		{
			return;
		}
		if (!vessel.OnSurface)
		{
			return;
		}
		this.aaa(vessel);
		vessel.DestroyVessel();
		if (flag)
		{
			this.ExitGameScene();
		}
		else
		{
			this.CancelRecovery();
		}
		this.UpdateVesselButtons();
	}

	private void aaa(Vessel vesselToRecover)
	{

	}

	public void CancelRecovery()
	{
		Ref.inputController.recoverMenuHolder.SetActive(false);
		Ref.inputController.leftArrow.parent.gameObject.SetActive(true);
		foreach (EngineModule.FuelIcon current in Ref.controller.fuelIcons)
		{
			if (current.icon != null)
			{
				current.icon.gameObject.SetActive(true);
			}
		}
	}

	public void DestroySelectedVessel()
	{
		if (Ref.selectedVessel == null || Ref.selectedVessel == Ref.mainVessel)
		{
			return;
		}
		this.HideArrowButtons();
		string warning = Utility.SplitLines("Are you sure you want to destroy the selected rocket?");
		Ref.warning.ShowWarning(warning, new Vector2(1220f, 175f), "Destroy", new Warning.EmptyDelegate(this.ConfirmVesselDestroy), "Cancel", new Warning.EmptyDelegate(this.ShowArrowButtons), 160);
	}

	public void ConfirmVesselDestroy()
	{
		this.ShowArrowButtons();
		Ref.selectedVessel.DestroyVessel();
		this.UpdateVesselButtons();
	}

	public void ShowArrowButtons()
	{
		Ref.inputController.leftArrow.parent.gameObject.SetActive(true);
	}

	public void HideArrowButtons()
	{
		Ref.inputController.leftArrow.parent.gameObject.SetActive(false);
	}

	public void ToggleThrottle()
	{
		if (Ref.timeWarping)
		{
			Ref.controller.ShowMsg("Cannot toggle throttle while time warping");
			return;
		}
		if (!Ref.mainVessel.controlAuthority)
		{
			Ref.controller.ShowMsg("No control");
			return;
		}
		Ref.mainVessel.SetThrottle(new Vessel.Throttle(!Ref.mainVessel.throttle.throttleOn, Ref.mainVessel.throttle.throttleRaw));
		if (Ref.inputController.instructionToggleThrottleHolder.activeSelf)
		{
			Ref.inputController.instructionToggleThrottleHolder.SetActive(false);
			Ref.inputController.CheckAllInstructions();
		}
		if (Ref.mainVessel.throttle.throttleRaw != 0f)
		{
			foreach (EngineModule current in Ref.mainVessel.partsManager.engineModules)
			{
				if (current.engineOn.boolValue)
				{
					return;
				}
			}
		}
		Ref.inputController.PlayClickSound(0.6f);
	}

	private void UpdateUI(double velocity, double height, double apo, double peri, bool terrainHeight)
	{
		bool flag = velocity < 1000.0;
		string text = ((int)velocity).ToString();
		if (flag && ((int)(velocity * 10.0) % 10 != 0 || (int)velocity != 0))
		{
			text = text + "." + ((int)(velocity * 10.0) % 10).ToString();
		}
		this.velocityUI.text = "Velocity: " + text + "m/s";
		if (height < 0.0)
		{
			height = 0.0;
		}
		this.heightUI.text = ((!terrainHeight) ? "Height: " : "Height (Terrain): ") + this.RoundUI(Math.Max(0.0, height));
		if (!this.showOrbitParameters)
		{
			return;
		}
		this.apoapsisUI.text = "Apoapsis: " + this.RoundUI(apo);
		this.periapsisUI.text = "Periapsis: " + this.RoundUI(peri);
	}

	private void SetVelocityDirectionMarker(Vector2 velocity)
	{
		bool flag = velocity.sqrMagnitude > 4f && !Ref.mapView;
		this.directionArrow.gameObject.SetActive(flag);
		if (!flag)
		{
			return;
		}
		this.directionArrow.eulerAngles = new Vector3(0f, 0f, (float)Math.Atan2((double)velocity.y, (double)velocity.x) * 57.29578f - 90f);
		float num = this.canvas.referenceResolution.x / 2f - 24f;
		float num2 = this.velocityUI.rectTransform.localPosition.y - 56f;
		Vector3 vector = velocity;
		vector *= num / Mathf.Abs(vector.x);
		if (Mathf.Abs(vector.y) > num2)
		{
			vector *= num2 / Mathf.Abs(vector.y);
		}
		this.directionArrow.localPosition = vector;
	}

	public string RoundUI(double value)
	{
		string str = string.Empty;
		int num;
		if (value < 100.0)
		{
			str = "m";
			num = 1;
		}
		else if (value < 10000.0)
		{
			str = "m";
			num = 0;
		}
		else if (value < 100000000.0)
		{
			num = 1;
			value /= 1000.0;
			str = "km";
		}
		else
		{
			num = 2;
			value /= 1000000.0;
			str = "Mm";
		}
		int num2 = Ref.GetFigure(value);
		if (num2 == 0)
		{
			num2 = 1;
		}
		if (num != 0)
		{
			num2 += num + 1;
		}
		string text = value.ToString();
		if (num2 > text.Length)
		{
			text += ".0";
		}
		return text.Substring(0, num2) + str;
	}

	private void CheckPositionBounds()
	{
		if (Ref.mainVessel.partsManager.rb2d.position.y > 1000f)
		{
			this.OffsetScenePosition(new Vector2(0f, 2000f));
		}
		if (Ref.mainVessel.partsManager.rb2d.position.y < -1000f)
		{
			this.OffsetScenePosition(new Vector2(0f, -2000f));
		}
		if (Ref.mainVessel.partsManager.rb2d.position.x > 1000f)
		{
			this.OffsetScenePosition(new Vector2(2000f, 0f));
		}
		if (Ref.mainVessel.partsManager.rb2d.position.x < -1000f)
		{
			this.OffsetScenePosition(new Vector2(-2000f, 0f));
		}
	}

	private void CheckVelocityBounds()
	{
		Vector2 velocity = Ref.mainVessel.partsManager.rb2d.velocity;
		if (Mathf.Abs(velocity.x) > 25f || Mathf.Abs(velocity.y) > 25f)
		{
			float x = Mathf.Floor((Mathf.Abs(velocity.x) + 25f) / 50f) * 50f * Mathf.Sign(velocity.x);
			float y = Mathf.Floor((Mathf.Abs(velocity.y) + 25f) / 50f) * 50f * Mathf.Sign(velocity.y);
			this.OffsetSceneVelocity(new Vector2(x, y));
		}
	}

	public void OffsetScenePosition(Vector2 change)
	{
		Ref.planetManager.UpdatePositionOffset(Ref.positionOffset + new Double3((double)change.x, (double)change.y));
		foreach (Vessel current in this.vessels)
		{
			if (current.state == Vessel.State.RealTime)
			{
				current.partsManager.rb2d.position -= change;
				foreach (Part current2 in current.partsManager.parts)
				{
					for (int i = 0; i < current2.modules.Length; i++)
					{
						if (current2.modules[i] is ParachuteModule)
						{
							(current2.modules[i] as ParachuteModule).lastPos -= (Vector3)change;
						}
					}
				}
			}
		}
	}

	public void OffsetSceneVelocity(Vector2 change)
	{
		Ref.velocityOffset += new Double3((double)change.x, (double)change.y);
		foreach (Vessel current in this.vessels)
		{
			if (current.state == Vessel.State.RealTime)
			{
				current.partsManager.rb2d.velocity -= change;
			}
		}
	}

	public void DecelerateTime()
	{
		if (this.timewarpPhase != 0)
		{
			this.timewarpPhase--;
			this.ShowMsg(this.warpSpeeds[this.timewarpPhase].ToString() + "x Time acceleration");
			if (this.timewarpPhase == 0 && Ref.timeWarping)
			{
				this.ExitTimeWarpMode();
			}
			return;
		}
		Ref.inputController.instructionsTimewarp.SetActive(false);
	}

	public void AccelerateTime()
	{
		Ref.inputController.instructionsTimewarp.SetActive(false);
		if (Ref.mainVesselHeight < this.loadedPlanet.minTimewarpHeightKm * 1000.0 && !Ref.timeWarping)
		{
			if (!Ref.mainVessel.OnSurface)
			{
				this.ShowMsg("Cannot time warp below " + this.loadedPlanet.minTimewarpHeightKm.ToString() + "km");
				return;
			}
			if (Ref.mainVessel.GetGlobalVelocity.magnitude2d >= 0.05000000074505806)
			{
				this.ShowMsg("Cannot time warp while moving on the surface");
				return;
			}
		}
		if (Ref.mainVessel.throttle.throttleOn && Ref.mainVessel.throttle.throttleRaw > 0f)
		{
			foreach (EngineModule current in Ref.mainVessel.partsManager.engineModules)
			{
				if (current.engineOn.boolValue)
				{
					this.ShowMsg("Cannot time warp while under acceleration");
					return;
				}
			}
		}
		if (this.timewarpPhase == this.warpSpeeds.Length - 1)
		{
			return;
		}
		this.timewarpPhase++;
		this.ShowMsg(this.warpSpeeds[this.timewarpPhase].ToString() + "x Time acceleration");
		if (!Ref.timeWarping)
		{
			this.EnterTimeWarpMode();
		}
	}

	public void EnterTimeWarpMode()
	{
		Ref.timeWarping = true;
		this.startedTimewarpTime = this.globalTime;
		Ref.mainVessel.SetThrottle(new Vessel.Throttle(false, Ref.mainVessel.throttle.throttleRaw));
		this.warpedTimeCounterUI.text = "0s";
		foreach (Vessel current in Ref.controller.vessels)
		{
			if (current.state == Vessel.State.RealTime)
			{
				current.SetVesselState(Vessel.ToState.ToTimewarping);
			}
		}
		Ref.velocityOffset = Double3.zero;
		if (Ref.selectedVessel != null)
		{
			Ref.map.UpdateVesselOrbitLines(Ref.selectedVessel.orbits, true);
		}
	}

	public void ExitTimeWarpMode()
	{
		this.UpdateTerrain();
		Ref.timeWarping = false;
		this.warpedTimeCounterUI.text = string.Empty;
		Ref.planetManager.UpdatePositionOffset(Ref.mainVessel.GetGlobalPosition.roundTo1000);
		bool flag = this.GetEnableVelocityOffset(Ref.mainVessel);
		double x = (!flag) ? 0.0 : (Math.Floor((Math.Abs(Ref.mainVessel.GetGlobalVelocity.x) + 25.0) / 50.0) * 50.0 * (double)Math.Sign(Ref.mainVessel.GetGlobalVelocity.x));
		double y = (!flag) ? 0.0 : (Math.Floor((Math.Abs(Ref.mainVessel.GetGlobalVelocity.y) + 25.0) / 50.0) * 50.0 * (double)Math.Sign(Ref.mainVessel.GetGlobalVelocity.y));
		Ref.velocityOffset = new Double3(x, y);
		if (Ref.mainVessel != null)
		{
			Ref.mainVessel.SetVesselState(Vessel.ToState.ToRealTime);
		}
		foreach (Vessel current in Ref.controller.vessels)
		{
			if (current != Ref.mainVessel && (current.state == Vessel.State.OnRails || current.state == Vessel.State.Stationary))
			{
				current.SetVesselState(Vessel.ToState.ToRealTime);
			}
		}
	}

	private void UpdateTerrain()
	{
		Ref.planetManager.totalDistanceMoved += 100000.0;
	}

	public void RepositionFuelIcons()
	{
		for (int i = 0; i < this.fuelIcons.Count; i++)
		{
			if (this.fuelIcons[i].icon != null)
			{
				UnityEngine.Object.Destroy(this.fuelIcons[i].icon.gameObject);
			}
		}
		this.fuelIcons.Clear();
		if (Ref.mainVessel != null)
		{
			foreach (EngineModule current in Ref.mainVessel.partsManager.engineModules)
			{
				if (current.displayFuel.boolValue)
				{
					current.CheckResourceSource(false);
					Transform transform = UnityEngine.Object.Instantiate<Transform>(Ref.controller.fuelIconPrefab, Ref.controller.velocityUI.transform.parent);
					transform.SetAsFirstSibling();
					current.fuelIcon = new EngineModule.FuelIcon(transform.GetComponent<Image>(), transform.GetChild(1).GetComponent<Image>())
					{
						bar = 
						{
							fillAmount = current.resourceSource.resourcePercent
						}
					};
					this.fuelIcons.Add(current.fuelIcon);
				}
			}
		}
		for (int j = 0; j < this.fuelIcons.Count; j++)
		{
			this.fuelIcons[j].icon.rectTransform.position = new Vector3(25f, (float)(50 + j * 60), 0f);
		}
	}

	private bool GetEnableVelocityOffset(Vessel vessel)
	{
		double num = this.loadedPlanet.radius + Math.Max(this.loadedPlanet.terrainData.maxTerrainHeight + 5000.0, this.loadedPlanet.atmosphereData.atmosphereHeightM * 0.2);
		return vessel.GetGlobalPosition.sqrMagnitude2d > num * num;
	}
}
