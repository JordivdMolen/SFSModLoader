using System;
using NewBuildSystem;
using SFSML;
using SFSML.HookSystem.ReWork;
using SFSML.HookSystem.ReWork.BaseHooks.FrameHooks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Ref : MonoBehaviour
{
	[BoxGroup("All Scenes", true, false, 0)]
	[ShowInInspector]
	[ReadOnly]
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

	[BoxGroup("Game Scene", true, false, 0)]
	[ShowInInspector]
	[Title("", null, 0, false, false)]
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

	[BoxGroup("Game Scene", true, false, 0)]
	[ShowInInspector]
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

	[BoxGroup("Game Scene", true, false, 0)]
	[ShowInInspector]
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

	[BoxGroup("Game Scene", true, false, 0)]
	[ShowInInspector]
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

	[BoxGroup("Game Scene", true, false, 0)]
	[ShowInInspector]
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

	[BoxGroup("Game Scene", true, false, 0)]
	[ShowInInspector]
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

	[BoxGroup("Game Scene", true, false, 0)]
	[ShowInInspector]
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

	[BoxGroup("Game Scene", true, false, 0)]
	[ShowInInspector]
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

	[BoxGroup("Game Scene", true, false, 0)]
	[ShowInInspector]
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
		MySceneChangeHook mySceneChangeHook = MyHookSystem.executeHook<MySceneChangeHook>(new MySceneChangeHook(Ref.lastScene, sceneToLoad));
		bool flag = mySceneChangeHook.isCanceled();
		if (!flag)
		{
			sceneToLoad = mySceneChangeHook.newScene;
			Ref.SceneType current = Ref.lastScene;
			Ref.lastScene = Ref.currentScene;
			SceneManager.LoadScene(sceneToLoad.ToString(), LoadSceneMode.Single);
			MyHookSystem.executeHook<MySceneChangedHook>(new MySceneChangedHook(current, sceneToLoad));
		}
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
		bool flag = num > 0;
		if (flag)
		{
			str = str + num.ToString() + "d ";
			bool flag2 = num2 < 10;
			if (flag2)
			{
				str += "0";
			}
		}
		bool flag3 = num2 > 0 || num > 0;
		if (flag3)
		{
			str = str + num2.ToString() + "h ";
			bool flag4 = num3 < 10;
			if (flag4)
			{
				str += "0";
			}
		}
		bool flag5 = num3 > 0 || num2 > 0 || num > 0;
		if (flag5)
		{
			str = str + num3.ToString() + "m ";
			bool flag6 = num4 < 10;
			if (flag6)
			{
				str += "0";
			}
		}
		return str + num4.ToString() + "s";
	}

	private void Awake()
	{
		SFSMLBase.initialize();
		Ref.hasPartsExpansion = Saving.LoadSetting(Saving.SettingKey.hasPartDLC);
		Ref.hasHackedExpansion = Saving.LoadSetting(Saving.SettingKey.hasHackedDLC);
		Ref.infiniteFuel = Saving.LoadSetting(Saving.SettingKey.infiniteFuel);
		Ref.noDrag = Saving.LoadSetting(Saving.SettingKey.noDrag);
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
	}

	private void Update()
	{
		bool keyDown = Input.GetKeyDown("t");
		if (keyDown)
		{
			this.Toggle();
		}
		bool keyDown2 = Input.GetKeyDown("y");
		if (keyDown2)
		{
			Saving.ToggleSetting(Saving.SettingKey.hasHackedDLC);
		}
	}

	public void Toggle()
	{
		Saving.ToggleSetting(Saving.SettingKey.hasPartDLC);
		Ref.hasPartsExpansion = Saving.LoadSetting(Saving.SettingKey.hasPartDLC);
		bool flag = Build.main != null;
		if (flag)
		{
			Build.main.buildGrid.SetBuildGridSize();
			Build.main.pickGrid.SelectPickList(Build.main.pickGrid.selectedListId);
			Build.main.PositionCameraStart();
		}
		MonoBehaviour.print(Saving.LoadSetting(Saving.SettingKey.hasPartDLC));
	}

	public static void Label(float x, float y, float width, float height, Texture2D texture)
	{
		GUI.Label(new Rect(x, y, width, height), texture);
	}

	public static void Box(float x, float y, float width, float height, Texture2D texture)
	{
		GUI.Box(new Rect(x, y, width, height), texture);
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
		bool flag = Ref.FileExists(fileName);
		if (flag)
		{
			PlayerPrefs.DeleteKey(fileName.ToString());
		}
	}

	public static CelestialBodyData GetPlanetByName(string adress)
	{
		bool flag = adress == Ref.solarSystemRoot.bodyName;
		CelestialBodyData result;
		if (flag)
		{
			result = Ref.solarSystemRoot;
		}
		else
		{
			foreach (CelestialBodyData celestialBodyData in Ref.solarSystemRoot.satellites)
			{
				bool flag2 = celestialBodyData.bodyName == adress;
				if (flag2)
				{
					return celestialBodyData;
				}
				foreach (CelestialBodyData celestialBodyData2 in celestialBodyData.satellites)
				{
					bool flag3 = celestialBodyData2.bodyName == adress;
					if (flag3)
					{
						return celestialBodyData2;
					}
				}
			}
			MonoBehaviour.print(" Could not find a celestial body with this adress");
			result = null;
		}
		return result;
	}

	public static float GetSizeOfWord(TextMesh text, string word)
	{
		float num = 0f;
		foreach (char ch in word)
		{
			CharacterInfo characterInfo;
			text.font.GetCharacterInfo(ch, out characterInfo, text.fontSize);
			num += (float)characterInfo.advance;
		}
		return num;
	}

	public Ref()
	{
	}

	[BoxGroup("All Scenes", true, false, 0)]
	[Space]
	public Camera Camera;

	public static Camera cam;

	[BoxGroup("All Scenes", true, false, 0)]
	public InputController InputController;

	public static InputController inputController;

	[BoxGroup("All Scenes", true, false, 0)]
	public Warning Warning;

	public static Warning warning;

	[BoxGroup("All Scenes", true, false, 0)]
	[Space]
	public Ref.SceneType CurrentScene;

	public static Ref.SceneType currentScene;

	public static Ref.SceneType lastScene;

	[BoxGroup("Game Scene", true, false, 0)]
	[Space]
	public Controller Controller;

	public static Controller controller;

	[BoxGroup("Game Scene", true, false, 0)]
	public PlanetManager PlanetManager;

	public static PlanetManager planetManager;

	[BoxGroup("Game Scene", true, false, 0)]
	public Map Map;

	public static Map map;

	[BoxGroup("Game Scene", true, false, 0)]
	[Space]
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

	[BoxGroup("Some Scenes", true, false, 0)]
	[Space]
	public Saving Saving;

	public static Saving saving;

	[BoxGroup("Some Scenes", true, false, 0)]
	[Space]
	public Material PartShader;

	public static Material partShader;

	public static int connectionCheckId;

	public static bool openTutorial;

	public static bool loadLaunchedRocket;

	public AudioListener MainAudioListener;

	public static AudioListener mainAudioListener;

	public static bool hasPartsExpansion;

	public static bool hasHackedExpansion;

	public static bool infiniteFuel;

	public static bool noDrag;

	public enum SceneType
	{
		MainMenu,
		Build,
		Game
	}
}
