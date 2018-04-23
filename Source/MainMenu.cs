using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
	private void Start()
	{
		bool flag = !MainMenu.CanResumeGame();
		if (flag)
		{
			this.resumeButtonText.color = new Color(1f, 1f, 1f, 0.5f);
		}
		bool flag2 = Ref.openTutorial;
		if (flag2)
		{
			this.openTutorial.InvokeEvenets();
			Ref.openTutorial = false;
		}
		Ref.inputController.soundtrackController.gameObject.SetActive(!Saving.LoadSetting(Saving.SettingKey.disableMusic));
		this.SetText(this.disableMusicText, "Music: " + ((!Saving.LoadSetting(Saving.SettingKey.disableMusic)) ? "On" : "Off"));
		Ref.inputController.SetEnableSound();
		this.SetText(this.disableSoundText, "Sound: " + ((!Saving.LoadSetting(Saving.SettingKey.disableSound)) ? "On" : "Off"));
		Ref.inputController.SetFps();
		this.SetText(this.disable60fpsText, "Fps: " + ((!Saving.LoadSetting(Saving.SettingKey.disable60fps)) ? "60" : "30"));
		this.SetText(this.disableScreenRotationText, "Screen rotation: " + ((!Saving.LoadSetting(Saving.SettingKey.disableAutoRotate)) ? "On" : "Off"));
		this.SetText(this.infiniteFuelText, "Infinite Fuel: " + ((!Saving.LoadSetting(Saving.SettingKey.disableSound)) ? "On" : "Off"));
		this.SetText(this.noDragText, "No Drag: " + ((!Saving.LoadSetting(Saving.SettingKey.disableSound)) ? "On" : "Off"));
		bool flag3 = !Saving.LoadSetting(Saving.SettingKey.seenBuildInstructions) || Saving.LoadSetting(Saving.SettingKey.hasPartDLC);
		if (flag3)
		{
			this.buyPartsExpansionButton.SetActive(false);
		}
		this.SetCheatSettings();
	}

	public void SetCheatSettings()
	{
		this.infiniteFuelText.color = new Color(1f, 1f, 1f, (!Ref.hasPartsExpansion) ? 0.3f : 1f);
		this.noDragText.color = new Color(1f, 1f, 1f, (!Ref.hasPartsExpansion) ? 0.3f : 1f);
	}

	public void BuildNewRocket()
	{
		Ref.LoadScene(Ref.SceneType.Build);
	}

	public void ResumeGame()
	{
		bool flag = MainMenu.CanResumeGame();
		if (flag)
		{
			Ref.LoadScene(Ref.SceneType.Game);
		}
	}

	private static bool CanResumeGame()
	{
		return Ref.FileExists(Saving.SaveKey.PersistantGameSave) && GameSaving.GameSave.LoadPersistant().mainVesselId != -1;
	}

	[Button(0)]
	public void DeleteGameSave()
	{
		GameSaving.GameSave.DeletePersistant();
		UnityEngine.Object.FindObjectOfType<MainMenu>().resumeButtonText.color = new Color(1f, 1f, 1f, 0.5f);
	}

	public void OpenLinkLiftoff()
	{
		Application.OpenURL("https://www.youtube.com/watch?v=5uorANMdB60");
	}

	public void OpenLinkMoonLanding()
	{
		Application.OpenURL("https://www.youtube.com/watch?v=5RgPDlBNaN8&feature=youtu.be");
	}

	public void OpenLink(string link)
	{
		Application.OpenURL(link);
	}

	public void Toggle60fps()
	{
		Saving.ToggleSetting(Saving.SettingKey.disable60fps);
		Ref.inputController.SetFps();
		this.SetText(this.disable60fpsText, "Fps: " + ((!Saving.LoadSetting(Saving.SettingKey.disable60fps)) ? "60" : "30"));
	}

	public void ToggleScreenRotation()
	{
		Saving.ToggleSetting(Saving.SettingKey.disableAutoRotate);
		this.SetText(this.disableScreenRotationText, "Screen rotation: " + ((!Saving.LoadSetting(Saving.SettingKey.disableAutoRotate)) ? "On" : "Off"));
	}

	public void ToggleMusic()
	{
		Saving.ToggleSetting(Saving.SettingKey.disableMusic);
		Ref.inputController.soundtrackController.gameObject.SetActive(!Saving.LoadSetting(Saving.SettingKey.disableMusic));
		this.SetText(this.disableMusicText, "Music: " + ((!Saving.LoadSetting(Saving.SettingKey.disableMusic)) ? "On" : "Off"));
	}

	public void ToggleSound()
	{
		Saving.ToggleSetting(Saving.SettingKey.disableSound);
		Ref.inputController.SetEnableSound();
		this.SetText(this.disableSoundText, "Sound: " + ((!Saving.LoadSetting(Saving.SettingKey.disableSound)) ? "On" : "Off"));
	}

	public void ToggleInfiniteFuel()
	{
		bool flag = !Ref.hasPartsExpansion;
		if (!flag)
		{
			Ref.inputController.PlayClickSound(0.6f);
			Saving.ToggleSetting(Saving.SettingKey.infiniteFuel);
			this.SetText(this.infiniteFuelText, "Infinite Fuel: " + ((!Saving.LoadSetting(Saving.SettingKey.infiniteFuel)) ? "Off" : "On"));
		}
	}

	public void ToggleNoDrag()
	{
		bool flag = !Ref.hasPartsExpansion;
		if (!flag)
		{
			Ref.inputController.PlayClickSound(0.6f);
			Saving.ToggleSetting(Saving.SettingKey.noDrag);
			this.SetText(this.noDragText, "No Drag: " + ((!Saving.LoadSetting(Saving.SettingKey.noDrag)) ? "Off" : "On"));
		}
	}

	private void SetText(Text textUI, string text)
	{
		textUI.text = text;
	}

	public void QuitApplication()
	{
		Application.Quit();
	}

	public MainMenu()
	{
	}

	public Text resumeButtonText;

	public Text disableMusicText;

	public Text disableSoundText;

	public Text disableScreenRotationText;

	public Text disable60fpsText;

	public Text infiniteFuelText;

	public Text noDragText;

	public CustomEvent openTutorial;

	public GameObject buyPartsExpansionButton;
}
