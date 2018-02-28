using SFSML;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
	public Text resumeButtonText;

	public Text disableMusicText;

	public Text disableSoundText;

	public Text disableScreenRotationText;

	public Text disable60fpsText;

	public CustomEvent openTutorial;

    public static MainMenu main;

	private void Start()
	{
		if (!MainMenu.CanResumeGame())
		{
			this.resumeButtonText.color = new Color(1f, 1f, 1f, 0.5f);
		}
		if (Ref.openTutorial)
		{
			this.openTutorial.InvokeEvenets();
			Ref.openTutorial = false;
		}
		this.SetMusicToggleText();
		this.SetSoundToggleText();
		this.SetFpsText();
		this.SetAutoRotationText();
        main = this;
    }

    public void FixedUpdate()
    {
    }

	public void BuildNewRocket()
	{
		Ref.LoadScene(Ref.SceneType.Build);
	}

	public void ResumeGame()
	{
		if (MainMenu.CanResumeGame())
		{
			Ref.LoadScene(Ref.SceneType.Game);
		}
	}

	private static bool CanResumeGame()
	{
		return Ref.FileExists(Saving.SaveKey.PersistantGameSave) && GameSaving.GameSave.LoadPersistant().mainVesselId != -1;
	}

	[Button(ButtonSizes.Small)]
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

	public void Toggle60fps()
	{
		Saving.ToggleSetting(Saving.SettingKey.disable60fps);
		Ref.inputController.SetFps();
		this.SetFpsText();
	}

	private void SetFpsText()
	{
		this.disable60fpsText.text = "Fps: " + ((!Saving.LoadSetting(Saving.SettingKey.disable60fps)) ? "60" : "30");
	}

	public void ToggleScreenRotation()
	{
		Saving.ToggleSetting(Saving.SettingKey.disableAutoRotate);
		this.SetAutoRotationText();
	}

	private void SetAutoRotationText()
	{
		this.disableScreenRotationText.text = "Screen rotation: " + ((!Saving.LoadSetting(Saving.SettingKey.disableAutoRotate)) ? "On" : "Off");
	}

	public void ToggleMusic()
	{
		Saving.ToggleSetting(Saving.SettingKey.disableMusic);
		this.SetMusicToggleText();
	}

	private void SetMusicToggleText()
	{
		Ref.inputController.soundtrackController.gameObject.SetActive(!Saving.LoadSetting(Saving.SettingKey.disableMusic));
		this.disableMusicText.text = "Music: " + ((!Saving.LoadSetting(Saving.SettingKey.disableMusic)) ? "On" : "Off");
	}

	public void ToggleSound()
	{
		Saving.ToggleSetting(Saving.SettingKey.disableSound);
		this.SetSoundToggleText();
	}

	private void SetSoundToggleText()
	{
		Ref.inputController.SetEnableSound();
		this.disableSoundText.text = "Sound: " + ((!Saving.LoadSetting(Saving.SettingKey.disableSound)) ? "On" : "Off");
	}

	public void QuitApplication()
	{
		Application.Quit();
	}
}
