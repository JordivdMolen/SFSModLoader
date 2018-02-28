using SFSML;
using Microsoft.Win32.SafeHandles;
using SFSML;
using Sirenix.OdinInspector;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using SFSML.GameManager.Hooks.UnityRelated;
using SFSML.GameManager.Hooks.FrameRelated;

public class Ref : MonoBehaviour
{
	public enum SceneType
	{
		MainMenu,
		Build,
		Game
	}

	[BoxGroup("All Scenes", true, false, 0), Space]
	public Camera Camera;

	public static Camera cam;

	[BoxGroup("All Scenes", true, false, 0)]
	public InputController InputController;

	public static InputController inputController;

	[BoxGroup("All Scenes", true, false, 0)]
	public Warning Warning;

	public static Warning warning;

	[BoxGroup("All Scenes", true, false, 0), Space]
	public Ref.SceneType CurrentScene;

	public static Ref.SceneType currentScene;

	public static Ref.SceneType lastScene;

	[BoxGroup("Game Scene", true, false, 0), Space]
	public Controller Controller;

	public static Controller controller;

	[BoxGroup("Game Scene", true, false, 0)]
	public PlanetManager PlanetManager;

	public static PlanetManager planetManager;

	[BoxGroup("Game Scene", true, false, 0)]
	public Map Map;

	public static Map map;

	[BoxGroup("Game Scene", true, false, 0), Space]
	public CelestialBodyData SolarSystemRoot;

	public static CelestialBodyData solarSystemRoot;

	public static Vessel mainVessel;

	public static Vessel selectedVessel;

	public static bool timeWarping;

	public static bool mapView;

	public static double mainVesselHeight;

	public static double mainVesselTerrainHeight;

	public static float mainVesselAngleToPlanet;

	public static Double3 positionOffset;

	public static Double3 velocityOffset;

	[BoxGroup("Some Scenes", true, false, 0), Space]
	public Saving Saving;

	public static Saving saving;

	[BoxGroup("Some Scenes", true, false, 0), Space]
	public Material PartShader;

	public static Material partShader;

	public static int connectionCheckId;

	public static bool openTutorial;

	public static bool loadLaunchedRocket;

    public static ModLoader myModLoader = null;

    public static Ref r;

	public AudioListener MainAudioListener;

	public static AudioListener mainAudioListener;

	[BoxGroup("All Scenes", true, false, 0), ReadOnly, ShowInInspector]
	private Ref.SceneType LastScene
	{
		get
		{
			return Ref.lastScene;
		}
		set
		{
		}
	}

	[BoxGroup("Game Scene", true, false, 0), ShowInInspector, Title("", null, TitleAlignments.Left, false, false)]
	private Vessel MainVessel
	{
		get
		{
			return Ref.mainVessel;
		}
		set
		{
		}
	}

	[BoxGroup("Game Scene", true, false, 0), ShowInInspector]
	private Vessel SelectedVessel
	{
		get
		{
			return Ref.selectedVessel;
		}
		set
		{
		}
	}

	[BoxGroup("Game Scene", true, false, 0), ShowInInspector]
	private bool TimeWarping
	{
		get
		{
			return Ref.timeWarping;
		}
		set
		{
		}
	}

	[BoxGroup("Game Scene", true, false, 0), ShowInInspector]
	private bool MapView
	{
		get
		{
			return Ref.mapView;
		}
		set
		{
		}
	}

	[BoxGroup("Game Scene", true, false, 0), ShowInInspector]
	private double MainVesselHeight
	{
		get
		{
			return Ref.mainVesselHeight;
		}
		set
		{
		}
	}

	[BoxGroup("Game Scene", true, false, 0), ShowInInspector]
	private double MainVTerrainHeight
	{
		get
		{
			return Ref.mainVesselTerrainHeight;
		}
		set
		{
		}
	}

	[BoxGroup("Game Scene", true, false, 0), ShowInInspector]
	private float MainVAngleToPlanet
	{
		get
		{
			return Ref.mainVesselAngleToPlanet;
		}
		set
		{
		}
	}

	[BoxGroup("Game Scene", true, false, 0), ShowInInspector]
	private Double3 PositionOffset
	{
		get
		{
			return Ref.positionOffset;
		}
		set
		{
			Ref.positionOffset = value;
		}
	}

	[BoxGroup("Game Scene", true, false, 0), ShowInInspector]
	private Double3 VelocityOffset
	{
		get
		{
			return Ref.velocityOffset;
		}
		set
		{
			Ref.velocityOffset = value;
		}
	}
    

	public static void LoadScene(Ref.SceneType sceneToLoad)
	{
        MySceneChangeHook res = ModLoader.manager.castHook<MySceneChangeHook>(new MySceneChangeHook(Ref.lastScene, sceneToLoad));
        if (res.isCanceled()) return;
        sceneToLoad = res.targetScene;
        Ref.SceneType oldScene = Ref.lastScene;
        Ref.lastScene = Ref.currentScene;
		SceneManager.LoadScene(sceneToLoad.ToString(), LoadSceneMode.Single);
        ModLoader.manager.castHook<MySceneChangedHook>(new MySceneChangedHook(oldScene, sceneToLoad));
    }

	public static int GetFigure(double value)
	{
		int num = 0;
		double num2 = 1.0;
		while (value >= num2)
		{
			num2 *= 10.0;
			num++;
		}
		return num;
	}

	public static string GetTimeString(double time)
	{
		int num = (int)(time / 86400.0);
		int num2 = (int)(time / 3600.0) % 24;
		int num3 = (int)(time / 60.0) % 60;
		int num4 = (int)time % 60;
		string str = string.Empty;
		if (num > 0)
		{
			str = str + num.ToString() + "d ";
			if (num2 < 10)
			{
				str += "0";
			}
		}
		if (num2 > 0 || num > 0)
		{
			str = str + num2.ToString() + "h ";
			if (num3 < 10)
			{
				str += "0";
			}
		}
		if (num3 > 0 || num2 > 0 || num > 0)
		{
			str = str + num3.ToString() + "m ";
			if (num4 < 10)
			{
				str += "0";
			}
		}
		return str + num4.ToString() + "s";
	}

    

    private void Awake()
    {
        r = this;
        Ref.cam = this.Camera;
		Ref.inputController = this.InputController;
		Ref.saving = this.Saving;
		Ref.warning = this.Warning;
		Ref.currentScene = this.CurrentScene;
		Ref.lastScene = this.LastScene;
		Ref.controller = this.Controller;
		Ref.planetManager = this.PlanetManager;
		Ref.map = this.Map;
		Ref.solarSystemRoot = this.SolarSystemRoot;
		Ref.partShader = this.PartShader;
        if (Ref.myModLoader == null)
        {
            Ref.myModLoader = new ModLoader();
            Ref.myModLoader.startLoadProcedure();
        }
    }

    private static bool kd = false;
    void Update()
    {
        if (Input.GetKey(KeyCode.F12) && !kd)
        {
            Ref.myModLoader.myConsole.toggleConsole();
            kd = true;
        } 
        if (!Input.GetKey(KeyCode.F12) && kd)
        {
            kd = false;
        }
        if (Ref.myModLoader!=null)
        Ref.myModLoader.RunUpdate();
    }
   

	public static void SaveJsonString(string jsonString, Saving.SaveKey fileName)
	{
		PlayerPrefs.SetString(fileName.ToString(), jsonString);
		PlayerPrefs.Save();
	}

	public static string LoadJsonString(Saving.SaveKey fileName)
	{
		return PlayerPrefs.GetString(fileName.ToString(), string.Empty);
	}

	public static bool FileExists(Saving.SaveKey fileName)
	{
		return PlayerPrefs.HasKey(fileName.ToString());
	}

	public static void DeleteFile(Saving.SaveKey fileName)
	{
		if (Ref.FileExists(fileName))
		{
			PlayerPrefs.DeleteKey(fileName.ToString());
		}
	}

	public static CelestialBodyData GetPlanetByName(string adress)
	{
		if (adress == Ref.solarSystemRoot.bodyName)
		{
			return Ref.solarSystemRoot;
		}
		CelestialBodyData[] satellites = Ref.solarSystemRoot.satellites;
		for (int i = 0; i < satellites.Length; i++)
		{
			CelestialBodyData celestialBodyData = satellites[i];
			if (celestialBodyData.bodyName == adress)
			{
				return celestialBodyData;
			}
			CelestialBodyData[] satellites2 = celestialBodyData.satellites;
			for (int j = 0; j < satellites2.Length; j++)
			{
				CelestialBodyData celestialBodyData2 = satellites2[j];
				if (celestialBodyData2.bodyName == adress)
				{
					return celestialBodyData2;
				}
			}
		}
		MonoBehaviour.print(" Could not find a celestial body with this adress");
		return null;
	}

    private void OnGUI()
    {
        if (ModLoader.manager != null)
        ModLoader.manager.castHook<MyGeneralOnGuiHook>(new MyGeneralOnGuiHook(this.CurrentScene));
    }

	public static float GetSizeOfWord(TextMesh text, string word)
	{
		float num = 0f;
		for (int i = 0; i < word.Length; i++)
		{
			char ch = word[i];
			CharacterInfo characterInfo;
			text.font.GetCharacterInfo(ch, out characterInfo, text.fontSize);
			num += (float)characterInfo.advance;
		}
		return num;
	}
}
