using System;
using System.Collections.Generic;
using NewBuildSystem;
using UnityEngine;
using SFSML;
using SFSML.HookSystem.ReWork;
using SFSML.HookSystem.ReWork.BaseHooks.SaveHooks;

public class GameSaving : MonoBehaviour
{
    public static GameSaving.GameSave GetGameSaveData(string saveName)
    {
        return new GameSaving.GameSave(saveName, GameSaving.GetVesselListIndex(Ref.mainVessel), GameSaving.GetVesselListIndex(Ref.selectedVessel), GameSaving.GetVesselSaveData(Ref.controller.vessels), Ref.positionOffset, Ref.velocityOffset, Ref.controller.globalTime, Ref.controller.timewarpPhase, Ref.controller.startedTimewarpTime, Ref.controller.cameraDistanceGame, Ref.mapView, Ref.map.mapPosition, Ref.map.following.GetFollowingBody().bodyName);
    }

    private static List<GameSaving.VesselSave> GetVesselSaveData(List<Vessel> vesselsToSave)
    {
        List<GameSaving.VesselSave> list = new List<GameSaving.VesselSave>();
        for (int i = 0; i < vesselsToSave.Count; i++)
        {
            list.Add(new GameSaving.VesselSave(vesselsToSave[i].state, vesselsToSave[i].GetVesselPlanet.bodyName, vesselsToSave[i].GetGlobalPosition, vesselsToSave[i].GetGlobalVelocity, vesselsToSave[i].partsManager.rb2d.rotation, vesselsToSave[i].partsManager.rb2d.angularVelocity, vesselsToSave[i].throttle, GameSaving.GetPartsSaveData(vesselsToSave[i].partsManager.parts), GameSaving.GetPartsJointsSave(vesselsToSave[i].partsManager.parts), vesselsToSave[i].vesselAchievements, vesselsToSave[i].RCS));
        }
        return list;
    }

    private static Part.Save[] GetPartsSaveData(List<Part> partsToSave)
    {
        int count = partsToSave.Count;
        Part.Save[] array = new Part.Save[count];
        for (int i = 0; i < count; i++)
        {
            array[i] = new Part.Save(partsToSave[i], partsToSave[i].orientation, partsToSave);
        }
        return array;
    }

    private static Part.Joint.Save[] GetPartsJointsSave(List<Part> parts)
    {
        List<Part.Joint.Save> list = new List<Part.Joint.Save>();
        foreach (Part part in parts)
        {
            foreach (Part.Joint joint in part.joints)
            {
                if (joint.fromPart == part)
                {
                    list.Add(new Part.Joint.Save(Part.GetPartListIndex(joint.fromPart, parts), Part.GetPartListIndex(joint.toPart, parts), joint.anchor, joint.fromSurfaceIndex, joint.toSurfaceIndex, joint.resourceFlow));
                }
            }
        }
        return list.ToArray();
    }

    public static void LoadGame(GameSaving.GameSave loadedData)
	{
		GameSaving.ClearScene();
        MySaveLoadedHook mySaveLoadedHook = new MySaveLoadedHook(loadedData);
        mySaveLoadedHook = MyHookSystem.executeHook<MySaveLoadedHook>(mySaveLoadedHook);
        if (mySaveLoadedHook.isCanceled())
        {
            return;
        }

        Ref.planetManager.SwitchLocation(Ref.GetPlanetByName(loadedData.vessels[loadedData.mainVesselId].adress), loadedData.vessels[loadedData.mainVesselId].globalPosition, false, true, 0.0);
        Ref.velocityOffset = loadedData.velocityOffset;
        Ref.controller.globalTime = loadedData.globalTime;
        Ref.controller.timewarpPhase = loadedData.timeWarpPhase;
        Ref.controller.startedTimewarpTime = loadedData.startedTimewapTime;
        Ref.timeWarping = (Ref.controller.timewarpPhase != 0);
        Ref.controller.SetCameraDistance(loadedData.camDistance);
        foreach (GameSaving.VesselSave vesselToLoad in loadedData.vessels)
        {
            GameSaving.LoadVessel(vesselToLoad);
        }
        Ref.planetManager.FullyLoadTerrain(Ref.GetPlanetByName(loadedData.vessels[loadedData.mainVesselId].adress));
        Ref.mainVessel = Ref.controller.vessels[loadedData.mainVesselId];
        if (loadedData.selectedVesselId != -1)
        {
            Ref.map.SelectVessel(Ref.controller.vessels[loadedData.selectedVesselId], false);
        }
        Ref.map.UpdateVesselsMapIcons();
        Ref.map.following = new OrbitLines.Target(Ref.GetPlanetByName(loadedData.mapFollowingAdress));
        Ref.map.UpdateMapPosition(loadedData.mapPosition);
        Ref.map.UpdateMapZoom(-loadedData.mapPosition.z);
        Ref.map.ToggleMap();
        if (Ref.mapView != loadedData.mapView)
        {
            Ref.map.ToggleMap();
        }
        Ref.mainVessel.SetThrottle(Ref.mainVessel.throttle);
        Ref.map.DrawOrbitLines();
        Ref.controller.RepositionFuelIcons();
        Ref.controller.warpedTimeCounterUI.text = string.Empty;
        Ref.planetManager.UpdateAtmosphereFade();
        Ref.mainVesselHeight = loadedData.vessels[loadedData.mainVesselId].globalPosition.magnitude2d - Ref.GetPlanetByName(loadedData.vessels[loadedData.mainVesselId].adress).radius;
        Ref.mainVesselAngleToPlanet = (float)Math.Atan2(loadedData.vessels[loadedData.mainVesselId].globalPosition.y, loadedData.vessels[loadedData.mainVesselId].globalPosition.x) * 57.29578f;
        if (Ref.mainVesselHeight < Ref.GetPlanetByName(loadedData.vessels[loadedData.mainVesselId].adress).cameraSwitchHeightM)
        {
            Ref.controller.camTargetAngle = Ref.mainVesselAngleToPlanet - 90f;
        }
        else
        {
            Ref.controller.camTargetAngle = 0f;
        }
        Ref.cam.transform.eulerAngles = new Vector3(0f, 0f, Ref.controller.camTargetAngle);
        Ref.controller.camAngularVelocity = 0f;
    }

    public static void LoadForLaunch(GameSaving.GameSave loadedData, Double3 launchPadPosition)
    {
        for (int i = 0; i < loadedData.vessels.Count; i++)
        {
            if (loadedData.vessels[i].adress == Ref.controller.startAdress && Math.Abs(loadedData.vessels[i].globalPosition.x - launchPadPosition.x) < 10.0 && Math.Abs(loadedData.vessels[i].globalPosition.y - launchPadPosition.y) < 40.0)
            {
                loadedData.vessels.RemoveAt(i);
                i--;
            }
            else
            {
                Vessel.State state = loadedData.vessels[i].state;
                if (state != Vessel.State.RealTime)
                {
                    if (state != Vessel.State.OnRails)
                    {
                        if (state == Vessel.State.Stationary)
                        {
                            loadedData.vessels[i].state = Vessel.State.StationaryUnloaded;
                        }
                    }
                    else
                    {
                        loadedData.vessels[i].state = Vessel.State.OnRailsUnloaded;
                    }
                }
                else if ((loadedData.vessels[i].globalPosition - launchPadPosition).magnitude2d > 1000.0 || loadedData.vessels[i].adress != Ref.controller.startAdress)
                {
                    loadedData.vessels[i].state = ((Math.Abs(loadedData.vessels[i].globalVelocity.x) <= 1.0 && Math.Abs(loadedData.vessels[i].globalVelocity.y) <= 1.0) ? Vessel.State.StationaryUnloaded : Vessel.State.OnRailsUnloaded);
                }
            }
        }
        Ref.planetManager.SwitchLocation(Ref.GetPlanetByName(Ref.controller.startAdress), launchPadPosition, false, true, 0.0);
        Ref.planetManager.UpdatePositionOffset(new Double3(0.0, 315000.0));
        Ref.velocityOffset = Double3.zero;
        Ref.controller.globalTime = loadedData.globalTime;
        Ref.controller.timewarpPhase = 0;
        Ref.timeWarping = false;
        foreach (GameSaving.VesselSave vesselToLoad in loadedData.vessels)
        {
            GameSaving.LoadVessel(vesselToLoad);
        }
        Ref.map.following = new OrbitLines.Target(Ref.GetPlanetByName(Ref.controller.startAdress));
        Ref.map.UpdateMapPosition(new Double3(0.0, launchPadPosition.y / 10000.0));
        Ref.map.UpdateMapZoom(launchPadPosition.y / 10000.0 / 20.0);
        Ref.planetManager.UpdateAtmosphereFade();
        Ref.controller.warpedTimeCounterUI.text = string.Empty;
    }

    private static Vessel LoadVessel(GameSaving.VesselSave vesselToLoad)
    {
        List<Part> list = new List<Part>(GameSaving.LoadParts(vesselToLoad));
        Part[] loadedParts = list.ToArray();
        GameSaving.LoadPartsData(vesselToLoad, loadedParts);
        list.Remove(null);
        Orientation.ApplyOrientation(list[0].transform, list[0].orientation);
        Vessel vessel = Vessel.CreateVessel(list[0].GetComponent<Part>(), Vector2.zero, vesselToLoad.angularVelocity, vesselToLoad.throttle, vesselToLoad.vesselArchivments, Ref.map.mapRefs[Ref.GetPlanetByName(vesselToLoad.adress)].holder);
        for (int i = 0; i < vessel.partsManager.parts.Count; i++)
        {
            if (vessel.partsManager.parts[i] != null)
            {
                vessel.partsManager.parts[i].UpdateConnected();
            }
        }
        vessel.partsManager.UpdateCenterOfMass();
        vessel.partsManager.ReCenter();
        vessel.transform.position = (vesselToLoad.globalPosition - Ref.positionOffset).toVector2;
        vessel.transform.eulerAngles = new Vector3(0f, 0f, vesselToLoad.rotation);
        if (vesselToLoad.state == Vessel.State.RealTime)
        {
            vessel.partsManager.rb2d.velocity = (vesselToLoad.globalVelocity - Ref.velocityOffset).toVector2;
        }
        else if (vesselToLoad.state == Vessel.State.OnRails || vesselToLoad.state == Vessel.State.OnRailsUnloaded)
        {
            vessel.orbits = Orbit.CalculateOrbits(vesselToLoad.globalPosition, vesselToLoad.globalVelocity, Ref.GetPlanetByName(vesselToLoad.adress));
            vessel.partsManager.rb2d.bodyType = RigidbodyType2D.Static;
            vessel.state = Vessel.State.OnRails;
            if (vessel.state == Vessel.State.OnRailsUnloaded)
            {
                vessel.SetVesselState(Vessel.ToState.ToUnloaded);
            }
            if (double.IsNaN(vessel.orbits[0].meanMotion))
            {
                MonoBehaviour.print("Cannot orbit NaN, went stationary instead");
                vessel.orbits.Clear();
                vessel.stationaryData.posToPlane = vesselToLoad.globalPosition;
                vessel.stationaryData.planet = Ref.GetPlanetByName(vesselToLoad.adress);
                vessel.state = Vessel.State.Stationary;
                vessel.SetVesselState(Vessel.ToState.ToUnloaded);
                vessel.mapIcon.localPosition = (vessel.stationaryData.posToPlane / 10000.0).toVector3;
            }
        }
        else
        {
            vessel.stationaryData.posToPlane = vesselToLoad.globalPosition;
            vessel.stationaryData.planet = Ref.GetPlanetByName(vesselToLoad.adress);
            vessel.state = Vessel.State.Stationary;
            if (vesselToLoad.state == Vessel.State.StationaryUnloaded)
            {
                vessel.SetVesselState(Vessel.ToState.ToUnloaded);
            }
            vessel.mapIcon.localPosition = (vessel.stationaryData.posToPlane / 10000.0).toVector3;
        }
        vessel.mapIcon.rotation = vessel.partsManager.parts[0].transform.rotation;
        vessel.RCS = vesselToLoad.RCS;
        return vessel;
    }

    private static Part[] LoadParts(GameSaving.VesselSave vesselToLoad)
	{
		int num = vesselToLoad.parts.Length;
		Part[] array = new Part[num];
		for (int i = 0; i < num; i++)
		{
			PartData partByName = Ref.controller.partDatabase.GetPartByName(vesselToLoad.parts[i].partName);
			bool flag = !(partByName == null);
			if (flag)
			{
				array[i] = Instantiate(partByName.prefab, Vector3.zero, Quaternion.identity).GetComponent<Part>();
				array[i].orientation = vesselToLoad.parts[i].orientation;

                array[i].partData.tags = new Dictionary<string, object>();
                try
                {
                    array[i].partData.GUID = new Guid(vesselToLoad.parts[i].GUID);
                } catch
                {
                    array[i].partData.GUID = Guid.NewGuid();
                }
                try
                {
                    if (vesselToLoad.parts[i].tagsString != null)
                    {
                        foreach (string text in vesselToLoad.parts[i].tagsString.Split('|'))
                        {
                            if (!(text == ""))
                            {
                                Type type = Type.GetType(text.Split('#')[0]);
                                string key = text.Split('#')[1];

                                object obj = JsonUtility.FromJson(text.Split('#')[2], type);

                                array[i].partData.tags.Add(key, obj);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    ModLoader.mainConsole.logError(e);
                }
            }
		}
		for (int j = 0; j < vesselToLoad.joints.Length; j++)
		{
			Part.Joint.Save save = vesselToLoad.joints[j];
			bool flag2 = save.fromPartId != -1 && save.toPartId != -1;
			if (flag2)
			{
				bool flag3 = !(array[save.fromPartId] == null) && !(array[save.toPartId] == null);
				if (flag3)
				{
					new Part.Joint(save.anchor, array[save.fromPartId], array[save.toPartId], save.fromSurfaceIndex, save.toSurfaceIndex, save.fuelFlow);
				}
			}
		}
		return array;
	}

	private static void LoadPartsData(GameSaving.VesselSave vesselToLoad, Part[] loadedParts)
	{
		for (int i = 0; i < vesselToLoad.parts.Length; i++)
		{
			Part part = loadedParts[i];
			bool flag = !(part == null);
			if (flag)
			{
				Part.Save save = vesselToLoad.parts[i];
				Module[] components = part.GetComponents<Module>();
				Array.Reverse(components);
				Array.Reverse(save.moduleSaves);
				for (int j = 0; j < Mathf.Min(part.modules.Length, save.moduleSaves.Length); j++)
				{
					components[j].Load(save.moduleSaves[j]);
				}
			}
		}
		for (int k = 0; k < vesselToLoad.parts.Length; k++)
		{
			bool flag2 = !(loadedParts[k] == null);
			if (flag2)
			{
				Module[] components2 = loadedParts[k].GetComponents<Module>();
				for (int l = 0; l < Mathf.Min(loadedParts[k].modules.Length, vesselToLoad.parts[k].moduleSaves.Length); l++)
				{
					components2[l].OnPartLoaded();
				}
			}
		}
	}

	private static void ClearScene()
	{
		while (Ref.controller.vessels.Count > 0)
		{
			Ref.controller.vessels[0].DestroyVessel();
		}
		Ref.controller.vessels.Clear();
	}

	private static int GetVesselListIndex(Vessel vessel)
	{
		for (int i = 0; i < Ref.controller.vessels.Count; i++)
		{
			bool flag = Ref.controller.vessels[i] == vessel;
			if (flag)
			{
				return i;
			}
		}
		return -1;
	}

	public GameSaving()
	{
	}

	[Serializable]
	public class Quicksaves
	{
		public Quicksaves()
		{
			this.quicksaves = new List<GameSaving.GameSave>();
		}

		public static void AddQuicksave(GameSaving.GameSave newQuicksave)
		{
            MySaveSavedHook mySaveSavedHook = new MySaveSavedHook(newQuicksave);
            mySaveSavedHook = MyHookSystem.executeHook<MySaveSavedHook>(mySaveSavedHook);
            if (mySaveSavedHook.isCanceled())
            {
                return;
            }

            GameSaving.Quicksaves quicksaves = GameSaving.Quicksaves.LoadQuicksaves();
			quicksaves.quicksaves.Add(newQuicksave);
			GameSaving.Quicksaves.SaveQuicksaves(quicksaves);
		}

		public static void RemoveQuicksaveAt(int index)
		{
			GameSaving.Quicksaves quicksaves = GameSaving.Quicksaves.LoadQuicksaves();
			bool flag = index > -1 && index < quicksaves.QuicksavesCount;
			if (flag)
			{
				quicksaves.quicksaves.RemoveAt(index);
				GameSaving.Quicksaves.SaveQuicksaves(quicksaves);
			}
		}

		public int QuicksavesCount
		{
			get
			{
				return this.quicksaves.Count;
			}
		}

		private static void SaveQuicksaves(GameSaving.Quicksaves newQuicksaves)
		{
			Ref.SaveJsonString(JsonUtility.ToJson(newQuicksaves), Saving.SaveKey.GameQuicksaves);
		}

		public static GameSaving.Quicksaves LoadQuicksaves()
		{
			string text = Ref.LoadJsonString(Saving.SaveKey.GameQuicksaves);
			return (!(text != string.Empty)) ? new GameSaving.Quicksaves() : JsonUtility.FromJson<GameSaving.Quicksaves>(text);
		}

		public List<GameSaving.GameSave> quicksaves;
	}

	[Serializable]
	public class GameSave
	{
		public GameSave(string saveName, int mainVesselId, int selectedVesselId, List<GameSaving.VesselSave> vessels, Double3 positionOffset, Double3 velocityOffset, double globalTime, int timeWarpPhase, double startedTimewapTime, float camDistance, bool mapView, Double3 mapPosition, string mapFollowingAdress)
		{
			this.saveName = saveName;
			this.globalTime = globalTime;
			this.timeWarpPhase = timeWarpPhase;
			this.startedTimewapTime = startedTimewapTime;
			this.camDistance = camDistance;
			this.mapView = mapView;
			this.mapPosition = mapPosition;
			this.mapFollowingAdress = mapFollowingAdress;
			this.mainVesselId = mainVesselId;
			this.selectedVesselId = selectedVesselId;
			this.vessels = vessels;
			this.positionOffset = positionOffset;
			this.velocityOffset = velocityOffset;
		}

		public static GameSaving.GameSave LoadPersistant()
		{
			string text = Ref.LoadJsonString(Saving.SaveKey.PersistantGameSave);
			return (!(text != string.Empty)) ? null : JsonUtility.FromJson<GameSaving.GameSave>(text);
		}

		public static void DeletePersistant()
		{
			PlayerPrefs.DeleteAll();
		}

		public static void UpdatePersistantSave()
		{
			GameSaving.GameSave gameSaveData = GameSaving.GetGameSaveData("Persistant Game Save");
			Ref.SaveJsonString(JsonUtility.ToJson(gameSaveData), Saving.SaveKey.PersistantGameSave);
		}

		public string saveName;

		public double globalTime;

		public int timeWarpPhase;

		public double startedTimewapTime;

		public float camDistance;

		public bool mapView;

		public Double3 mapPosition;

		public string mapFollowingAdress;

		public int mainVesselId;

		public int selectedVesselId;

		public List<GameSaving.VesselSave> vessels;

		public Double3 positionOffset;

		public Double3 velocityOffset;
	}

	[Serializable]
	public class VesselSave
	{
		public VesselSave(Vessel.State state, string adress, Double3 globalPosition, Double3 globalVelocity, float rotation, float angularVelocity, Vessel.Throttle throttle, Part.Save[] parts, Part.Joint.Save[] joints, List<string> vesselArchivments, bool RCS)
		{
			this.globalPosition = globalPosition;
			this.globalVelocity = globalVelocity;
			this.rotation = rotation;
			this.angularVelocity = angularVelocity;
			this.adress = adress;
			this.state = state;
			this.throttle = throttle;
			this.parts = parts;
			this.joints = joints;
			this.vesselArchivments = vesselArchivments;
			this.RCS = RCS;
		}

		public string adress;

		public Double3 globalPosition;

		public Double3 globalVelocity;

		public float rotation;

		public float angularVelocity;

		public Vessel.State state;

		public Vessel.Throttle throttle;

		public Part.Save[] parts;

		public Part.Joint.Save[] joints;

		public List<string> vesselArchivments;

		public bool RCS;
	}
}
