using System;
using System.Collections;
using System.Collections.Generic;
using NewBuildSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
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
		bool flag = Ref.lastScene == Ref.SceneType.Build;
		if (flag)
		{
			bool flag2 = gameSave != null && gameSave.vessels.Count > 0;
			if (flag2)
			{
				gameSave.velocityOffset = Double3.zero;
				gameSave.selectedVesselId = -1;
				GameSaving.LoadForLaunch(gameSave, @double);
				MonoBehaviour.print("There is a Persistant-Save, and a Rocket To Create");
			}
			else
			{
				MonoBehaviour.print("There is No Persistant-Save, created rocket into new scene empty scene");
				Ref.planetManager.SwitchLocation(planetByName, @double, false, true, 0.0);
				Ref.map.following = new OrbitLines.Target(planetByName);
				Ref.map.UpdateMapPosition(new Double3(0.0, @double.y / 10000.0));
				Ref.map.UpdateMapZoom(@double.y / 10000.0 / 20.0);
			}
			this.CreatePartsFromBuild(@double);
			Ref.mainVessel.SetThrottle(new Vessel.Throttle(false, 0.65f));
			Ref.mapView = false;
			Ref.controller.SetCameraDistance(23f);
		}
		else
		{
			bool flag3 = gameSave != null;
			if (flag3)
			{
				GameSaving.LoadGame(gameSave);
				MonoBehaviour.print("No rocket to create, loaded persistant");
			}
		}
	}

	private void CreatePartsFromBuild(Double3 launchPadPosition)
	{
		Build.BuildSave buildSave = JsonUtility.FromJson<Build.BuildSave>(Ref.LoadJsonString(Saving.SaveKey.ToLaunch));
		PartGrid.PositionForLaunch(buildSave.parts, this.partDatabase, buildSave.rotation);
		List<Part> list = CreateRocket.CreateBuildParts((launchPadPosition - Ref.positionOffset).toVector3, buildSave, this.partDatabase);
		List<Part> list2 = new List<Part>();
		for (int i = 0; i < list.Count; i++)
		{
			bool flag = list[i].GetComponent<ControlModule>() != null;
			if (flag)
			{
				list2.Add(list[i]);
			}
		}
		list2.AddRange(list);
		List<Vessel> list3 = this.CreateVesselsFromParts(list2, Vector2.zero, 0f, new Vessel.Throttle(false, 0.65f), new List<string>());
		Ref.mainVessel = null;
		Ref.mainVessel = list3[0];
		Ref.map.SelectVessel(list3[0], false);
		for (int j = 0; j < list.Count; j++)
		{
			list[j].UpdateConnected();
		}
		Ref.map.UpdateVesselsMapIcons();
		this.UpdateVesselButtons();
	}

	public List<Vessel> CreateVesselsFromParts(List<Part> unatendedParts, Vector2 velocity, float angularVecloty, Vessel.Throttle throttle, List<string> inheritedArchivments)
	{
		List<Vessel> list = new List<Vessel>();
		for (int i = 0; i < unatendedParts.Count; i++)
		{
			bool flag = unatendedParts[i] != null && unatendedParts[i].vessel == null && unatendedParts[i].gameObject.activeSelf;
			if (flag)
			{
				Vessel item = Vessel.CreateVessel(unatendedParts[i], velocity, angularVecloty, throttle, inheritedArchivments, Ref.map.mapRefs[this.loadedPlanet].holder);
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
		bool flag = this.msgCoroutine != null;
		if (flag)
		{
			base.StopCoroutine(this.msgCoroutine);
		}
		this.msgCoroutine = this.MsgCoroutine(msgText, displayTime);
		base.StartCoroutine(this.msgCoroutine);
	}

	private IEnumerator MsgCoroutine(string msgText, float displayTime)
	{
		this.msgUI.text = msgText;
		this.msgUI.gameObject.SetActive(true);
		while (displayTime > 0f)
		{
			displayTime -= Time.deltaTime;
			this.msgUI.color = new Color(1f, 1f, 1f, displayTime);
			yield return new WaitForEndOfFrame();
		}
		this.msgUI.gameObject.SetActive(false);
		yield break;
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
		bool flag = Ref.timeWarping || Ref.mainVessel == null;
		if (!flag)
		{
			this.CheckPositionBounds();
			bool flag2 = Ref.velocityOffset.sqrMagnitude2d > 0.0;
			if (flag2)
			{
				Ref.planetManager.UpdatePositionOffset(Ref.positionOffset + Ref.velocityOffset * (double)Time.fixedDeltaTime);
			}
			this.enableVelocityOffset = this.GetEnableVelocityOffset(Ref.mainVessel);
			bool flag3 = this.enableVelocityOffset;
			if (flag3)
			{
				this.CheckVelocityBounds();
			}
			else
			{
				bool flag4 = Ref.velocityOffset.sqrMagnitude2d > 0.0;
				if (flag4)
				{
					this.OffsetSceneVelocity(-Ref.velocityOffset.toVector2);
				}
			}
		}
	}

	private void Update()
	{
		bool flag = this.regularUpdateTime < Time.time;
		if (flag)
		{
			this.RegularUpdate();
			this.regularUpdateTime = Time.time + 0.5f;
		}
		this.globalTime += (double)((float)this.warpSpeeds[this.timewarpPhase] * Time.deltaTime);
		bool flag2 = Ref.mainVessel != null && Ref.mainVessel.orbits.Count > 0;
		double num = (!flag2) ? double.PositiveInfinity : Ref.mainVessel.orbits[0].stopTimeWarpTime;
		bool flag3 = flag2;
		if (flag3)
		{
			Ref.planetManager.UpdatePositionOffset(Ref.mainVessel.GetGlobalPosition.roundTo1000);
		}
		bool flag4 = this.globalTime > num;
		if (flag4)
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
		bool flag2 = this.timewarpButtonColorMove.targetTime.floatValue == 1f != flag;
		if (flag2)
		{
			this.timewarpButtonColorMove.SetTargetTime((float)((!flag) ? 0 : 1));
			bool flag3 = flag && this.msgUI.color.a == 0f;
			if (flag3)
			{
				this.ShowMsg("Time warp enabled");
			}
		}
		bool flag4 = this.updatedPersistantTime < Time.time;
		if (flag4)
		{
			this.updatedPersistantTime = Time.time + 15f;
			GameSaving.GameSave.UpdatePersistantSave();
		}
	}

	private void LateUpdate()
	{
		bool flag = Ref.mainVessel != null;
		if (flag)
		{
			this.cameraPositionGame = ((!Ref.timeWarping) ? Ref.mainVessel.partsManager.rb2d.worldCenterOfMass : (Ref.mainVessel.GetGlobalPosition - Ref.positionOffset).toVector2);
		}
		bool flag2 = !Ref.mapView;
		if (flag2)
		{
			this.scaledSpaceThreshold = ((Ref.mainVesselHeight <= 500000.0) ? 100000f : 5000f);
			bool flag3 = this.cameraDistanceGame > this.scaledSpaceThreshold;
			Ref.cam.cullingMask = ((!flag3) ? this.gameView : this.scaledView);
			bool flag4 = !flag3;
			if (flag4)
			{
				Ref.cam.transform.position = new Vector3(this.cameraPositionGame.x, this.cameraPositionGame.y, -this.cameraDistanceGame) + (Vector3)(this.cameraOffset + this.viewPositons);
			}
			else
			{
				Ref.cam.transform.position = ((Ref.positionOffset + new Double3((double)this.cameraPositionGame.x, (double)this.cameraPositionGame.y, -(double)this.cameraDistanceGame)) / 10000.0).toVector3;
			}
			bool flag5 = flag3;
			if (flag5)
			{
				Ref.planetManager.PositionLowRezPlanets();
			}
			Ref.planetManager.scaledHolder.gameObject.SetActive(flag3);
			Ref.planetManager.chuncksHolder.gameObject.SetActive(!flag3);
		}
		this.cameraOffset = Vector2.zero;
		this.audioListener.transform.position = new Vector3(this.cameraPositionGame.x, this.cameraPositionGame.y, Ref.mapView ? (-Mathf.Max(this.cameraDistanceGame, 1000f)) : (-this.cameraDistanceGame));
		bool flag6 = Ref.mainVessel != null;
		if (flag6)
		{
			Ref.mainVesselHeight = Ref.mainVessel.GetGlobalPosition.magnitude2d - this.loadedPlanet.radius;
			Ref.mainVesselAngleToPlanet = (float)Math.Atan2(Ref.mainVessel.GetGlobalPosition.y, Ref.mainVessel.GetGlobalPosition.x) * 57.29578f;
			Ref.mainVesselTerrainHeight = Ref.mainVesselHeight - this.loadedPlanet.GetTerrainSampleAtAngle((double)Ref.mainVesselAngleToPlanet);
		}
		this.camTargetAngle = ((Ref.mainVesselHeight >= this.loadedPlanet.cameraSwitchHeightM) ? 0f : (Ref.mainVesselAngleToPlanet - 90f));
		Ref.cam.transform.eulerAngles = new Vector3(0f, 0f, Mathf.SmoothDampAngle(Ref.cam.transform.eulerAngles.z, this.camTargetAngle, ref this.camAngularVelocity, 0.5f));
		this.stars.transform.eulerAngles = new Vector3(0f, 0f, 100f);
		Ref.map.UpdateCameraRotation(Ref.cam.transform.localEulerAngles);
		this.UpdateUI((!(Ref.mainVessel != null)) ? 0.0 : Ref.mainVessel.GetGlobalVelocity.magnitude2d, (Ref.mainVesselTerrainHeight >= 2000.0) ? Ref.mainVesselHeight : Ref.mainVesselTerrainHeight, Ref.mainVesselTerrainHeight < 2000.0);
		float num = (!this.loadedPlanet.atmosphereData.hasAtmosphere) ? 0f : Mathf.Clamp01((float)(Ref.mainVesselHeight / this.loadedPlanet.atmosphereData.atmosphereHeightM));
		bool hasAtmosphere = this.loadedPlanet.atmosphereData.hasAtmosphere;
		if (hasAtmosphere)
		{
			float t = Mathf.Pow(num - 1f, 3f) + 1f;
			Ref.partShader.SetFloat("_Intensity", Mathf.Lerp(this.loadedPlanet.atmosphereData.shadowIntensity, 1.7f, t));
		}
		float num2 = (this.loadedPlanet.atmosphereData.atmosphereHeightKm != 30.0) ? 0f : (1f - num);
		this.flameByHeightMaterial.SetColor("_Color", new Color(1f, 1f, 1f, Mathf.Clamp01(num2 * 1.5f)));
		bool timeWarping = Ref.timeWarping;
		if (timeWarping)
		{
			this.warpedTimeCounterUI.text = Ref.GetTimeString(this.globalTime - this.startedTimewarpTime);
		}
		this.SomeStuff();
		Ref.cam.fieldOfView = 60f * ((Screen.width <= Screen.height) ? 1f : ((float)Screen.height / (float)Screen.width * 1.075f));
		this.SetVelocityDirectionMarker((!(Ref.mainVessel != null)) ? Vector2.zero : (Vector2)(Quaternion.Euler(0f, 0f, -Ref.cam.transform.rotation.eulerAngles.z) * Ref.mainVessel.GetGlobalVelocity.toVector3));
		bool flag7 = Ref.mainVessel != null && Ref.mainVessel.RCS;
		bool flag8 = Ref.inputController.rcsInputsHolder.gameObject.activeSelf != flag7;
		if (flag8)
		{
			Ref.inputController.rcsInputsHolder.gameObject.SetActive(flag7);
		}
	}

	private void SomeStuff()
	{
		bool keyDown = Input.GetKeyDown(".");
		if (keyDown)
		{
			this.AccelerateTime();
		}
		bool keyDown2 = Input.GetKeyDown(",");
		if (keyDown2)
		{
			this.DecelerateTime();
		}
		bool flag = Input.GetKeyDown("space") && !Ref.saving.savingMenuHolder.activeSelf;
		if (flag)
		{
			bool controlAuthority = Ref.mainVessel.controlAuthority;
			if (controlAuthority)
			{
				this.ToggleThrottle();
			}
			else
			{
				bool flag2 = Ref.controller.msgUI.color.a < 0.6f;
				if (flag2)
				{
					Ref.controller.ShowMsg("No control");
				}
			}
		}
		bool flag3 = Input.GetAxisRaw("Vertical") != 0f;
		if (flag3)
		{
			bool controlAuthority2 = Ref.mainVessel.controlAuthority;
			if (controlAuthority2)
			{
				Ref.mainVessel.SetThrottle(new Vessel.Throttle(Ref.mainVessel.throttle.throttleOn, Mathf.Clamp01(Ref.mainVessel.throttle.throttleRaw + Input.GetAxisRaw("Vertical") / 2f * Time.deltaTime)));
				bool activeSelf = Ref.inputController.instructionSlideThrottleHolder.activeSelf;
				if (activeSelf)
				{
					Ref.inputController.instructionSlideThrottleHolder.SetActive(false);
					Ref.inputController.CheckAllInstructions();
				}
			}
			else
			{
				bool flag4 = Ref.controller.msgUI.color.a < 0.6f;
				if (flag4)
				{
					Ref.controller.ShowMsg("No control");
				}
			}
		}
		bool flag5 = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		if (flag5)
		{
			bool controlAuthority3 = Ref.mainVessel.controlAuthority;
			if (controlAuthority3)
			{
				Ref.mainVessel.SetThrottle(new Vessel.Throttle(Ref.mainVessel.throttle.throttleOn, Mathf.Clamp01(Ref.mainVessel.throttle.throttleRaw + 0.5f * Time.deltaTime)));
			}
			else
			{
				bool flag6 = Ref.controller.msgUI.color.a < 0.6f;
				if (flag6)
				{
					Ref.controller.ShowMsg("No control");
				}
			}
		}
		bool flag7 = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
		if (flag7)
		{
			bool controlAuthority4 = Ref.mainVessel.controlAuthority;
			if (controlAuthority4)
			{
				Ref.mainVessel.SetThrottle(new Vessel.Throttle(Ref.mainVessel.throttle.throttleOn, Mathf.Clamp01(Ref.mainVessel.throttle.throttleRaw - 0.5f * Time.deltaTime)));
			}
			else
			{
				bool flag8 = Ref.controller.msgUI.color.a < 0.6f;
				if (flag8)
				{
					Ref.controller.ShowMsg("No control");
				}
			}
		}
		bool keyDown3 = Input.GetKeyDown("u");
		if (keyDown3)
		{
			this.topRightText.transform.parent.gameObject.SetActive(!Ref.inputController.switchToButton.transform.parent.parent.gameObject.activeSelf);
			this.bottomLeftHolder.gameObject.SetActive(!Ref.inputController.switchToButton.transform.parent.parent.gameObject.activeSelf);
			Ref.inputController.switchToButton.transform.parent.parent.gameObject.SetActive(!Ref.inputController.switchToButton.transform.parent.parent.gameObject.activeSelf);
		}
	}

	[Button("Take screenshot", 0)]
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
		bool flag = newVessel == null || newVessel == Ref.mainVessel;
		if (!flag)
		{
			Vessel mainVessel = Ref.mainVessel;
			Ref.mainVessel = newVessel;
			Ref.map.UpdateVesselsMapIcons();
			Double3 getGlobalPosition = newVessel.GetGlobalPosition;
			CelestialBodyData getVesselPlanet = newVessel.GetVesselPlanet;
			Ref.map.SelectVessel(Ref.selectedVessel, true);
			this.RepositionFuelIcons();
			this.UpdateVesselButtons();
			Ref.map.following = new OrbitLines.Target(newVessel.GetVesselPlanet);
			Ref.map.UpdateMapPosition(newVessel.GetGlobalPosition / 10000.0);
			Ref.map.UpdateMapZoom(Ref.map.mapPosition.z);
			newVessel.SetThrottle(newVessel.throttle);
			bool flag2 = newVessel.GetGlobalPosition.magnitude2d < newVessel.GetVesselPlanet.radius + newVessel.GetVesselPlanet.minTimewarpHeightKm * 1000.0;
			foreach (Vessel vessel in Ref.controller.vessels)
			{
				vessel.SetVesselState(Vessel.ToState.ToUnloaded);
			}
			Vessel.ToState toState = (Ref.timeWarping && !flag2) ? Vessel.ToState.ToTimewarping : Vessel.ToState.ToRealTime;
			Ref.planetManager.SwitchLocation(getVesselPlanet, getGlobalPosition, false, true, 0.0);
			bool flag3 = Ref.timeWarping && flag2;
			if (flag3)
			{
				this.timewarpPhase = 1;
				this.DecelerateTime();
				Ref.controller.ShowMsg("Cannot time warp below: " + newVessel.GetVesselPlanet.minTimewarpHeightKm.ToString() + "km");
			}
			else
			{
				bool flag4 = toState == Vessel.ToState.ToRealTime;
				if (flag4)
				{
					bool flag5 = this.GetEnableVelocityOffset(Ref.mainVessel);
					double x = (!flag5) ? 0.0 : (Math.Floor((Math.Abs(Ref.mainVessel.GetGlobalVelocity.x) + 25.0) / 50.0) * 50.0 * (double)Math.Sign(Ref.mainVessel.GetGlobalVelocity.x));
					double y = (!flag5) ? 0.0 : (Math.Floor((Math.Abs(Ref.mainVessel.GetGlobalVelocity.y) + 25.0) / 50.0) * 50.0 * (double)Math.Sign(Ref.mainVessel.GetGlobalVelocity.y));
					Ref.velocityOffset = new Double3(x, y);
				}
				Ref.mainVessel.SetVesselState(toState);
			}
			Ref.mainVessel.SetThrottle(new Vessel.Throttle(false, newVessel.throttle.throttleRaw));
			bool flag6 = mainVessel != null;
			if (flag6)
			{
				Ref.map.SelectVessel(mainVessel, false);
			}
			Ref.planetManager.totalDistanceMoved += 100000.0;
		}
	}

	public void StartRecovery()
	{
		Ref.inputController.leftArrow.parent.gameObject.SetActive(false);
		this.fuelIconsHolder.gameObject.SetActive(false);
		Vessel vessel = (!Ref.mapView) ? Ref.mainVessel : Ref.selectedVessel;
		Ref.inputController.recoverMenuHolder.transform.GetChild(8).GetChild(1).GetComponent<Text>().text = ((!(vessel == Ref.mainVessel)) ? "Complete Recovery" : "Complete Mission");
		List<string> list = (!(vessel != null)) ? new List<string>() : new List<string>(vessel.vesselAchievements);
		bool flag = list.Contains("Reached 10 km altitude.") && list.Count > 2;
		if (flag)
		{
			list.Remove("Reached 5 km altitude.");
		}
		bool flag2 = list.Contains("Reached 15 km altitude.") && list.Count > 2;
		if (flag2)
		{
			list.Remove("Reached 10 km altitude.");
		}
		bool flag3 = list.Contains("Passed the Karman Line, leaving the /atmosphere and reaching space.") && list.Count > 2;
		if (flag3)
		{
			list.Remove("Reached 15 km altitude.");
		}
		bool flag4 = list.Contains("Reached low " + Ref.controller.startAdress + " orbit.") && list.Count > 4;
		if (flag4)
		{
			list.Remove("Passed the Karman Line, leaving the /atmosphere and reaching space.");
		}
		Ref.inputController.recoverMenuHolder.SetActive(true);
		bool flag5 = Ref.inputController.canvasScalers[0].referenceResolution.x == 750f;
		Text component = Ref.inputController.recoverMenuHolder.transform.GetChild(4).GetComponent<Text>();
		Text component2 = Ref.inputController.recoverMenuHolder.transform.GetChild(6).GetComponent<Text>();
		component.transform.localPosition = new Vector3(0f, component.transform.localPosition.y);
		Ref.inputController.recoverMenuHolder.transform.GetChild(3).localPosition = new Vector3(0f, component.transform.localPosition.y + 10f);
		Ref.inputController.recoverMenuHolder.transform.GetChild(5).localPosition = new Vector3(5000f, component.transform.localPosition.y + 10f);
		Ref.inputController.recoverMenuHolder.transform.GetChild(3).localScale = new Vector3(1f, (!flag5) ? 0.51f : 1f);
		Ref.inputController.recoverMenuHolder.transform.GetChild(5).localScale = new Vector3(1f, (!flag5) ? 0.51f : 1f);
		component.text = string.Empty;
		component2.text = string.Empty;
		int num = 0;
		Text text = component;
		foreach (string text2 in list)
		{
			Text text3 = text;
			Text text4 = text3;
			text4.text += "- ";
			for (int i = 0; i < text2.Length; i++)
			{
				bool flag6 = text2[i].ToString() == "/";
				if (flag6)
				{
					Text text5 = text;
					Text text6 = text5;
					text6.text += "\r\n \r\n ";
					num += 2;
				}
				else
				{
					Text text7 = text;
					Text text8 = text7;
					text8.text += text2[i].ToString();
				}
			}
			Text text9 = text;
			Text text10 = text9;
			text10.text += "\r\n \r\n \r\n";
			num += 3;
			bool flag7 = num > ((!flag5) ? 24 : 51);
			if (flag7)
			{
				bool flag8 = text == component2 || flag5;
				if (flag8)
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
		bool flag2 = vessel == null;
		if (!flag2)
		{
			bool flag3 = !vessel.OnSurface;
			if (!flag3)
			{
				this.aaa(vessel);
				vessel.DestroyVessel();
				bool flag4 = flag;
				if (flag4)
				{
					this.ExitGameScene();
				}
				else
				{
					this.CancelRecovery();
				}
				this.UpdateVesselButtons();
			}
		}
	}

	public void AskEliminateDebris()
	{
		string warning = Utility.SplitLines("Are you sure you want to clear debris?//This will destroy ALL rockets without a capsule or a probe");
		Ref.warning.ShowWarning(warning, new Vector2(1300f, 335f), "Clear Debris", new Warning.EmptyDelegate(this.ConfirmEliminateDebris), "Cancel", null, 180);
	}

	private void ConfirmEliminateDebris()
	{
		for (int i = 0; i < this.vessels.Count; i++)
		{
			bool flag = !this.vessels[i].controlAuthority;
			if (flag)
			{
				this.vessels[i].DestroyVessel();
				i--;
			}
		}
	}

	private void aaa(Vessel vesselToRecover)
	{
		bool flag = vesselToRecover.vesselAchievements.Contains("Passed the Karman Line, leaving the /atmosphere and reaching space.");
		if (flag)
		{
		}
	}

	public void CancelRecovery()
	{
		Ref.inputController.recoverMenuHolder.SetActive(false);
		Ref.inputController.leftArrow.parent.gameObject.SetActive(true);
		this.fuelIconsHolder.gameObject.SetActive(true);
	}

	public void DestroySelectedVessel()
	{
		bool flag = Ref.selectedVessel == null || Ref.selectedVessel == Ref.mainVessel;
		if (!flag)
		{
			this.HideArrowButtons();
			string warning = Utility.SplitLines("Are you sure you want to destroy the selected rocket?");
			Ref.warning.ShowWarning(warning, new Vector2(1220f, 175f), "Destroy", new Warning.EmptyDelegate(this.ConfirmVesselDestroy), "Cancel", new Warning.EmptyDelegate(this.ShowArrowButtons), 160);
		}
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
		bool timeWarping = Ref.timeWarping;
		if (timeWarping)
		{
			Ref.controller.ShowMsg("Cannot toggle throttle while time warping");
		}
		else
		{
			bool flag = !Ref.mainVessel.controlAuthority;
			if (flag)
			{
				Ref.controller.ShowMsg("No control");
			}
			else
			{
				Ref.mainVessel.SetThrottle(new Vessel.Throttle(!Ref.mainVessel.throttle.throttleOn, Ref.mainVessel.throttle.throttleRaw));
				bool activeSelf = Ref.inputController.instructionToggleThrottleHolder.activeSelf;
				if (activeSelf)
				{
					Ref.inputController.instructionToggleThrottleHolder.SetActive(false);
					Ref.inputController.CheckAllInstructions();
				}
				bool flag2 = Ref.mainVessel.throttle.throttleRaw != 0f;
				if (flag2)
				{
					foreach (EngineModule engineModule in Ref.mainVessel.partsManager.engineModules)
					{
						bool boolValue = engineModule.engineOn.boolValue;
						if (boolValue)
						{
							return;
						}
					}
				}
				Ref.inputController.PlayClickSound(0.6f);
			}
		}
	}

	private void UpdateUI(double velocity, double height, bool terrainHeight)
	{
		float d = (Screen.height <= Screen.width) ? 1f : ((float)Screen.width / (float)Screen.height * 1.77f);
		this.topRightText.transform.parent.localScale = Vector3.one * d;
		this.bottomLeftHolder.localScale = Vector3.one * d;
		Vector3 vector = Utility.CameraRelativePosition(Ref.cam.fieldOfView, new Vector3(0f, 0f, 1f));
		this.bottomLeftHolder.localPosition = vector;
		this.topRightText.transform.parent.localPosition = new Vector3(-vector.x, -vector.y, vector.z);
		bool flag = velocity < 100.0;
		string text = ((int)velocity).ToString();
		bool flag2 = flag && ((int)(velocity * 10.0) % 10 != 0 || (int)velocity != 0);
		if (flag2)
		{
			text = text + "." + ((int)(velocity * 10.0) % 10).ToString();
		}
		this.topRightText.text = string.Concat(new string[]
		{
			(!terrainHeight) ? "Height: " : "Height (Terrain): ",
			this.RoundUI(Math.Max(0.0, height)),
			"\n Velocity: ",
			text,
			"m/s"
		});
	}

	private void SetVelocityDirectionMarker(Vector2 velocity)
	{
		bool flag = velocity.sqrMagnitude > 4f && !Ref.mapView;
		bool flag2 = this.directionArrow.gameObject.activeSelf != flag;
		if (flag2)
		{
			this.directionArrow.gameObject.SetActive(flag);
		}
		bool flag3 = !flag;
		if (!flag3)
		{
			this.directionArrow.eulerAngles = new Vector3(0f, 0f, (float)Math.Atan2((double)velocity.y, (double)velocity.x) * 57.29578f - 90f);
			float num = (Screen.height <= Screen.width) ? 1f : ((float)Screen.width / (float)Screen.height * 1.77f);
			Vector3 vector = Utility.CameraRelativePosition(Ref.cam.fieldOfView, new Vector3(0f, 0f, 1f));
			float num2 = -vector.x - 0.02f * num;
			float num3 = -vector.y - 0.1f * num;
			Vector3 vector2 = velocity;
			vector2 *= num2 / Mathf.Abs(vector2.x);
			bool flag4 = Mathf.Abs(vector2.y) > num3;
			if (flag4)
			{
				vector2 *= num3 / Mathf.Abs(vector2.y);
			}
			this.directionArrow.localPosition = new Vector3(vector2.x, vector2.y, 1f);
		}
	}

	public string RoundUI(double value)
	{
		string str = string.Empty;
		bool flag = value < 100.0;
		int num;
		if (flag)
		{
			str = "m";
			num = 1;
		}
		else
		{
			bool flag2 = value < 10000.0;
			if (flag2)
			{
				str = "m";
				num = 0;
			}
			else
			{
				bool flag3 = value < 100000000.0;
				if (flag3)
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
			}
		}
		int num2 = Ref.GetFigure(value);
		bool flag4 = num2 == 0;
		if (flag4)
		{
			num2 = 1;
		}
		bool flag5 = num != 0;
		if (flag5)
		{
			num2 += num + 1;
		}
		string text = value.ToString();
		bool flag6 = num2 > text.Length;
		if (flag6)
		{
			text += ".0";
		}
		return text.Substring(0, num2) + str;
	}

	private void CheckPositionBounds()
	{
		bool flag = Ref.mainVessel.partsManager.rb2d.position.y > 1000f;
		if (flag)
		{
			this.OffsetScenePosition(new Vector2(0f, 2000f));
		}
		bool flag2 = Ref.mainVessel.partsManager.rb2d.position.y < -1000f;
		if (flag2)
		{
			this.OffsetScenePosition(new Vector2(0f, -2000f));
		}
		bool flag3 = Ref.mainVessel.partsManager.rb2d.position.x > 1000f;
		if (flag3)
		{
			this.OffsetScenePosition(new Vector2(2000f, 0f));
		}
		bool flag4 = Ref.mainVessel.partsManager.rb2d.position.x < -1000f;
		if (flag4)
		{
			this.OffsetScenePosition(new Vector2(-2000f, 0f));
		}
	}

	private void CheckVelocityBounds()
	{
		Vector2 velocity = Ref.mainVessel.partsManager.rb2d.velocity;
		bool flag = Mathf.Abs(velocity.x) > 25f || Mathf.Abs(velocity.y) > 25f;
		if (flag)
		{
			float x = Mathf.Floor((Mathf.Abs(velocity.x) + 25f) / 50f) * 50f * Mathf.Sign(velocity.x);
			float y = Mathf.Floor((Mathf.Abs(velocity.y) + 25f) / 50f) * 50f * Mathf.Sign(velocity.y);
			this.OffsetSceneVelocity(new Vector2(x, y));
		}
	}

	public void OffsetScenePosition(Vector2 change)
	{
		Ref.planetManager.UpdatePositionOffset(Ref.positionOffset + new Double3((double)change.x, (double)change.y));
		foreach (Vessel vessel in this.vessels)
		{
			bool flag = vessel.state == Vessel.State.RealTime;
			if (flag)
			{
				vessel.partsManager.rb2d.position -= change;
				foreach (Part part in vessel.partsManager.parts)
				{
					for (int i = 0; i < part.modules.Length; i++)
					{
						bool flag2 = part.modules[i] is ParachuteModule;
						if (flag2)
						{
							(part.modules[i] as ParachuteModule).lastPos -= (Vector3)change;
						}
					}
				}
			}
		}
	}

	public void OffsetSceneVelocity(Vector2 change)
	{
		Ref.velocityOffset += new Double3((double)change.x, (double)change.y);
		foreach (Vessel vessel in this.vessels)
		{
			bool flag = vessel.state == Vessel.State.RealTime;
			if (flag)
			{
				vessel.partsManager.rb2d.velocity -= change;
			}
		}
	}

	public void DecelerateTime()
	{
		bool flag = this.timewarpPhase != 0;
		if (flag)
		{
			this.timewarpPhase--;
			this.ShowMsg(this.warpSpeeds[this.timewarpPhase].ToString() + "x Time acceleration");
			bool flag2 = this.timewarpPhase == 0 && Ref.timeWarping;
			if (flag2)
			{
				this.ExitTimeWarpMode();
			}
		}
		else
		{
			Ref.inputController.instructionsTimewarp.SetActive(false);
		}
	}

	public void AccelerateTime()
	{
		Ref.inputController.instructionsTimewarp.SetActive(false);
		bool flag = Ref.mainVesselHeight < this.loadedPlanet.minTimewarpHeightKm * 1000.0 && !Ref.timeWarping;
		if (flag)
		{
			bool flag2 = !Ref.mainVessel.OnSurface;
			if (flag2)
			{
				this.ShowMsg("Cannot time warp below " + this.loadedPlanet.minTimewarpHeightKm.ToString() + "km");
				return;
			}
			bool flag3 = Ref.mainVessel.GetGlobalVelocity.magnitude2d >= 0.05000000074505806;
			if (flag3)
			{
				this.ShowMsg("Cannot time warp while moving on the surface");
				return;
			}
		}
		bool flag4 = Ref.mainVessel.throttle.throttleOn && Ref.mainVessel.throttle.throttleRaw > 0f;
		if (flag4)
		{
			foreach (EngineModule engineModule in Ref.mainVessel.partsManager.engineModules)
			{
				bool boolValue = engineModule.engineOn.boolValue;
				if (boolValue)
				{
					this.ShowMsg("Cannot time warp while under acceleration");
					return;
				}
			}
		}
		bool flag5 = this.timewarpPhase == this.warpSpeeds.Length - 1;
		if (!flag5)
		{
			this.timewarpPhase++;
			this.ShowMsg(this.warpSpeeds[this.timewarpPhase].ToString() + "x Time acceleration");
			bool flag6 = !Ref.timeWarping;
			if (flag6)
			{
				this.EnterTimeWarpMode();
			}
		}
	}

	public void EnterTimeWarpMode()
	{
		Ref.timeWarping = true;
		this.startedTimewarpTime = this.globalTime;
		Ref.mainVessel.SetThrottle(new Vessel.Throttle(false, Ref.mainVessel.throttle.throttleRaw));
		this.warpedTimeCounterUI.text = "0s";
		foreach (Vessel vessel in Ref.controller.vessels)
		{
			bool flag = vessel.state == Vessel.State.RealTime;
			if (flag)
			{
				vessel.SetVesselState(Vessel.ToState.ToTimewarping);
			}
		}
		Ref.velocityOffset = Double3.zero;
		Ref.map.DrawOrbitLines();
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
		bool flag2 = Ref.mainVessel != null;
		if (flag2)
		{
			Ref.mainVessel.SetVesselState(Vessel.ToState.ToRealTime);
		}
		foreach (Vessel vessel in Ref.controller.vessels)
		{
			bool flag3 = vessel != Ref.mainVessel && (vessel.state == Vessel.State.OnRails || vessel.state == Vessel.State.Stationary);
			if (flag3)
			{
				vessel.SetVesselState(Vessel.ToState.ToRealTime);
			}
		}
	}

	private void UpdateTerrain()
	{
		Ref.planetManager.totalDistanceMoved += 100000.0;
	}

	public void RepositionFuelIcons()
	{
		for (int i = 0; i < this.vessels.Count; i++)
		{
			for (int j = 0; j < this.vessels[i].partsManager.resourceGrups.Count; j++)
			{
				this.vessels[i].partsManager.resourceGrups[j].fuelIcon = null;
			}
		}
		int k = (!(Ref.mainVessel == null)) ? Ref.mainVessel.partsManager.resourceGrups.Count : 0;
		while (k > this.fuelIcons.Count)
		{
			this.fuelIcons.Add(this.CreateNewFuelIcon());
		}
		for (int l = 0; l < this.fuelIcons.Count; l++)
		{
			bool flag = l < k;
			if (flag)
			{
				this.fuelIcons[l].gameObject.SetActive(true);
				this.fuelIcons[l].GetChild(0).localScale = new Vector3(Ref.mainVessel.partsManager.resourceGrups[l].resourcePercent, this.fuelIcons[l].GetChild(0).localScale.y, 1f);
				Ref.mainVessel.partsManager.resourceGrups[l].fuelIcon = this.fuelIcons[l];
			}
			else
			{
				this.fuelIcons[l].gameObject.SetActive(false);
			}
		}
	}

	private Transform CreateNewFuelIcon()
	{
		Transform transform = UnityEngine.Object.Instantiate<Transform>(this.fuelIconPrefab, this.fuelIconsHolder);
		transform.localPosition = new Vector3(0f, (float)this.fuelIcons.Count * 0.32f, 0f);
		transform.SetAsFirstSibling();
		return transform;
	}

	private bool GetEnableVelocityOffset(Vessel vessel)
	{
		double num = this.loadedPlanet.radius + Math.Max(this.loadedPlanet.terrainData.maxTerrainHeight + 5000.0, this.loadedPlanet.atmosphereData.atmosphereHeightM * 0.2);
		return vessel.GetGlobalPosition.sqrMagnitude2d > num * num;
	}

	public Controller()
	{
	}

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

	public Text throttlePercentUI;

	public Text throttleOnUI;

	public Text msgUI;

	public Text warpedTimeCounterUI;

	public TextMesh topRightText;

	[Space]
	public MoveModule timewarpButtonColorMove;

	public MoveModule throttleColorMove;

	[Space]
	public Transform bottomLeftHolder;

	[Space]
	public Transform fuelIconsHolder;

	public List<Transform> fuelIcons;

	[Header(" Prefabs Refs")]
	public Transform vesselPrefab;

	public bool enableVelocityOffset;

	[Header("Camera")]
	[BoxGroup]
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

	public Transform directionArrow;

	public CanvasScaler canvas;

	public Canvas canvass;

	private IEnumerator msgCoroutine;

	public PartDatabase partDatabase;

	public Material flameByHeightMaterial;

	public Vector2 viewPositons;
}
