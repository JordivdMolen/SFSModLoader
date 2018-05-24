using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
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
        Ref.inputController.soundtrackController.gameObject.SetActive(!Saving.LoadSetting(Saving.SettingKey.disableMusic));
        this.SetText(this.disableMusicText, "Music: " + ((!Saving.LoadSetting(Saving.SettingKey.disableMusic)) ? "On" : "Off"));
        Ref.inputController.SetEnableSound();
        this.SetText(this.disableSoundText, "Sound: " + ((!Saving.LoadSetting(Saving.SettingKey.disableSound)) ? "On" : "Off"));
        Ref.inputController.SetFps();
        this.SetText(this.disable60fpsText, "Fps: " + ((!Saving.LoadSetting(Saving.SettingKey.disable60fps)) ? "60" : "30"));
        this.SetText(this.disableScreenRotationText, "Screen rotation: " + ((!Saving.LoadSetting(Saving.SettingKey.disableAutoRotate)) ? "On" : "Off"));
        this.SetText(this.infiniteFuelText, "Infinite Fuel: " + ((!Saving.LoadSetting(Saving.SettingKey.infiniteFuel)) ? "Off" : "On"));
        this.SetText(this.noDragText, "No Drag: " + ((!Saving.LoadSetting(Saving.SettingKey.noDrag)) ? "Off" : "On"));
        this.SetText(this.unbreakablePartsText, "Unbreakable Parts: " + ((!Saving.LoadSetting(Saving.SettingKey.unbreakableParts)) ? "Off" : "On"));
        this.SetText(this.noGravityText, "No Gravity: " + ((!Saving.LoadSetting(Saving.SettingKey.noGravity)) ? "Off" : "On"));
        this.SetText(this.orbitInfoText, "Orbit Info: " + ((!Saving.LoadSetting(Saving.SettingKey.orbitInfo)) ? "Off" : "On"));
        if (!Saving.LoadSetting(Saving.SettingKey.seenBuildInstructions) || Saving.LoadSetting(Saving.SettingKey.hasPartDLC))
        {
            this.buyPartsExpansionButton.SetActive(false);
        }
        this.SetCheatSettings();
    }

    private void LateUpdate()
    {
        if (Ref.openSalePage)
        {
            this.openSalePage.InvokeEvenets();
            Ref.openSalePage = false;
        }
    }

    public void SetCheatSettings()
    {
        this.infiniteFuelText.color = new Color(1f, 1f, 1f, (!Ref.hasPartsExpansion) ? 0.3f : 1f);
        this.noDragText.color = new Color(1f, 1f, 1f, (!Ref.hasPartsExpansion) ? 0.3f : 1f);
        this.unbreakablePartsText.color = new Color(1f, 1f, 1f, (!Ref.hasPartsExpansion) ? 0.3f : 1f);
        this.noGravityText.color = new Color(1f, 1f, 1f, (!Ref.hasPartsExpansion) ? 0.3f : 1f);
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
        if (!Ref.hasPartsExpansion)
        {
            return;
        }
        Ref.inputController.PlayClickSound(0.6f);
        Saving.ToggleSetting(Saving.SettingKey.infiniteFuel);
        this.SetText(this.infiniteFuelText, "Infinite Fuel: " + ((!Saving.LoadSetting(Saving.SettingKey.infiniteFuel)) ? "Off" : "On"));
    }

    public void ToggleNoDrag()
    {
        if (!Ref.hasPartsExpansion)
        {
            return;
        }
        Ref.inputController.PlayClickSound(0.6f);
        Saving.ToggleSetting(Saving.SettingKey.noDrag);
        this.SetText(this.noDragText, "No Drag: " + ((!Saving.LoadSetting(Saving.SettingKey.noDrag)) ? "Off" : "On"));
    }

    public void ToggleUnbreakableParts()
    {
        if (!Ref.hasPartsExpansion)
        {
            return;
        }
        Ref.inputController.PlayClickSound(0.6f);
        Saving.ToggleSetting(Saving.SettingKey.unbreakableParts);
        this.SetText(this.unbreakablePartsText, "Unbreakable Parts: " + ((!Saving.LoadSetting(Saving.SettingKey.unbreakableParts)) ? "Off" : "On"));
    }

    public void ToggleNoGravity()
    {
        if (!Ref.hasPartsExpansion)
        {
            return;
        }
        Ref.inputController.PlayClickSound(0.6f);
        Saving.ToggleSetting(Saving.SettingKey.noGravity);
        this.SetText(this.noGravityText, "No Gravity: " + ((!Saving.LoadSetting(Saving.SettingKey.noGravity)) ? "Off" : "On"));
    }

    public void ToggleOrbitInfo()
    {
        Ref.inputController.PlayClickSound(0.6f);
        Saving.ToggleSetting(Saving.SettingKey.orbitInfo);
        this.SetText(this.orbitInfoText, "Orbit Info: " + ((!Saving.LoadSetting(Saving.SettingKey.orbitInfo)) ? "Off" : "On"));
    }

    private void SetText(Text textUI, string text)
    {
        textUI.text = text;
    }

    public void QuitApplication()
    {
        Application.Quit();
    }

    public Text resumeButtonText;

    public Text disableMusicText;

    public Text disableSoundText;

    public Text disableScreenRotationText;

    public Text disable60fpsText;

    public Text infiniteFuelText;

    public Text noDragText;

    public Text unbreakablePartsText;

    public Text noGravityText;

    public Text orbitInfoText;

    public CustomEvent openTutorial;

    public CustomEvent openSalePage;

    public GameObject buyPartsExpansionButton;
}
