using NewBuildSystem;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class Saving : MonoBehaviour
{
	public enum SettingKey
	{
		seenBuildInstructions,
		seenGameInstructions,
		seenMapInstructions,
		seenTimewarpInstructions,
		hidePartDescription,
		disableMusic,
		disableSound,
		disableAutoRotate,
		disable60fps
	}

	public enum SaveKey
	{
		ToLaunch,
		PersistantGameSave,
		Settings,
		BuildQuicksaves,
		GameQuicksaves
	}

	public Text keyboardText;

	private TouchScreenKeyboard keyboard;

	public GameObject savingMenuHolder;

	public GameObject loadButton;

	public GameObject loadingMenuHolder;

	public GameObject confirmMenuHolder;

	public int selectedSaveId;

	public List<BoxCollider2D> saveFilesIcons;

	public Transform saveFileIconPrefab;

	private IEnumerator iconsAnimation;

	[BoxGroup]
	public GameObject keyboardHolder;

	[BoxGroup]
	public BoxCollider2D[] keys;

	private void Start()
	{
		this.UpdateLoadButtonColor();
	}

	public void OnKeyboardClick(Vector2 clickPosPixel)
	{
		GameObject gameObject = Ref.inputController.PointCastUI(clickPosPixel, this.keys);
		if (gameObject == null)
		{
			return;
		}
		base.StartCoroutine(Ref.inputController.ClickGlow(gameObject.transform.GetChild(0).gameObject));
		Ref.inputController.PlayClickSound(0.4f);
		this.OnKey(gameObject.GetComponentInChildren<Text>().text);
	}

	public void ToggleCaps()
	{
		string text = this.keys[0].GetComponentInChildren<Text>().text;
		bool flag = text == text.ToLower();
		for (int i = 0; i < this.keys.Length; i++)
		{
			this.keys[i].GetComponentInChildren<Text>().text = ((!flag) ? this.keys[i].GetComponentInChildren<Text>().text.ToLower() : this.keys[i].GetComponentInChildren<Text>().text.ToUpper());
		}
	}

	public void OnKey(string key)
	{
		this.keyboardText.text = this.keyboardText.text.Replace("|", string.Empty) + key + ((Time.unscaledTime % 1f <= 0.5f) ? string.Empty : "|");
		string text = this.keys[0].GetComponentInChildren<Text>().text;
	}

	public void Backspace()
	{
		this.keyboardText.text = this.keyboardText.text.Replace("|", string.Empty);
		this.keyboardText.text = this.keyboardText.text.Substring(0, Mathf.Max(this.keyboardText.text.Length - 1, 0)) + ((Time.unscaledTime % 1f <= 0.5f) ? string.Empty : "|");
	}

	public void StartSaveProcess()
	{
		Ref.SceneType currentScene = Ref.currentScene;
		if (currentScene != Ref.SceneType.Build)
		{
			if (currentScene == Ref.SceneType.Game)
			{
				if (Ref.mainVessel == null)
				{
					Ref.controller.ShowMsg("Cannot save while not controlling a rocket");
					return;
				}
				Time.timeScale = 0f;
				if (Ref.inputController.menuHolder.activeSelf)
				{
					Ref.inputController.ToggleDropdownMenu();
				}
			}
		}
		else if (!Build.main.buildGrid.HasAnyParts())
		{
			return;
		}
		this.savingMenuHolder.SetActive(true);
		this.keyboardText.text = string.Empty;
		this.keyboardHolder.SetActive(true);
		this.keyboard = TouchScreenKeyboard.Open(string.Empty, TouchScreenKeyboardType.Default);
		this.keyboard.active = true;
	}

	public void CompleteSaveProcess()
	{
		string text = this.keyboardText.text.Replace("|", string.Empty);
		if (text == string.Empty)
		{
			text = "Unnamed Save";
		}
		Ref.SceneType currentScene = Ref.currentScene;
		if (currentScene != Ref.SceneType.Build)
		{
			if (currentScene == Ref.SceneType.Game)
			{
				GameSaving.Quicksaves.AddQuicksave(GameSaving.GetGameSaveData(text));
				Ref.controller.ShowMsg("Game Saved");
			}
		}
		else
		{
			Build.BuildQuicksaves.AddQuicksave(new Build.BuildSave(text, Camera.main.transform.position, Build.main.buildGrid.parts));
		}
		this.CloseMenus();
		this.UpdateLoadButtonColor();
	}

	private bool CanLoad()
	{
		return ((Ref.currentScene != Ref.SceneType.Build) ? GameSaving.Quicksaves.LoadQuicksaves().QuicksavesCount : Build.BuildQuicksaves.LoadBuildQuicksaves().QuicksavesCount) > 0;
	}

	private void UpdateLoadButtonColor()
	{
		this.loadButton.GetComponentInChildren<Text>().color = ((!this.CanLoad()) ? new Color(1f, 1f, 1f, 0.6f) : Color.white);
	}

	public void TryStartLoadProcess()
	{
		if (!this.CanLoad())
		{
			return;
		}
		this.StartLoadProcess();
	}

	private void StartLoadProcess()
	{
		this.loadingMenuHolder.SetActive(true);
		this.selectedSaveId = -1;
		this.LoadSaveFilesIcons();
		if (Ref.currentScene == Ref.SceneType.Build)
		{
			Build.main.DisableDescription();
		}
		else if (Ref.currentScene == Ref.SceneType.Game)
		{
			Ref.controller.HideArrowButtons();
			foreach (EngineModule.FuelIcon current in Ref.controller.fuelIcons)
			{
				if (current.icon != null)
				{
					current.icon.gameObject.SetActive(false);
				}
			}
			if (Ref.inputController.menuHolder.activeSelf)
			{
				Ref.inputController.ToggleDropdownMenu();
			}
		}
	}

	public void TrySelectLoad(Vector2 clickPosPixel)
	{
		if (this.selectedSaveId != -1 && this.selectedSaveId < this.saveFilesIcons.Count)
		{
			this.saveFilesIcons[this.selectedSaveId].transform.GetChild(0).gameObject.SetActive(false);
		}
		this.selectedSaveId = -1;
		GameObject gameObject = Ref.inputController.PointCastUI(clickPosPixel, this.saveFilesIcons.ToArray());
		for (int i = 0; i < this.saveFilesIcons.Count; i++)
		{
			if (this.saveFilesIcons[i].gameObject == gameObject)
			{
				this.selectedSaveId = i;
				gameObject.transform.GetChild(0).gameObject.SetActive(true);
				Ref.inputController.PlayClickSound(0.6f);
				break;
			}
		}
	}

	public void CompleteLoadProcess()
	{
		Ref.SceneType currentScene = Ref.currentScene;
		if (currentScene != Ref.SceneType.Build)
		{
			if (currentScene == Ref.SceneType.Game)
			{
				GameSaving.Quicksaves quicksaves = GameSaving.Quicksaves.LoadQuicksaves();
				if (this.selectedSaveId != -1 && this.selectedSaveId < quicksaves.QuicksavesCount)
				{
					GameSaving.LoadGame(quicksaves.quicksaves[this.selectedSaveId]);
					Ref.controller.ShowMsg("Game Loaded");
					this.CloseMenus();
				}
			}
		}
		else
		{
			Build.BuildQuicksaves buildQuicksaves = Build.BuildQuicksaves.LoadBuildQuicksaves();
			if (this.selectedSaveId != -1 && this.selectedSaveId < buildQuicksaves.QuicksavesCount)
			{
				Build.main.LoadSave(buildQuicksaves.buildSaves[this.selectedSaveId]);
				this.CloseMenus();
			}
		}
	}

	public void DeleteSelectedQuicksave()
	{
		if (this.selectedSaveId == -1)
		{
			return;
		}
		Ref.SceneType currentScene = Ref.currentScene;
		if (currentScene != Ref.SceneType.Build)
		{
			if (currentScene == Ref.SceneType.Game)
			{
				UnityEngine.Object.Destroy(this.saveFilesIcons[this.selectedSaveId].gameObject);
				this.saveFilesIcons.RemoveAt(this.selectedSaveId);
				GameSaving.Quicksaves.RemoveQuicksaveAt(this.selectedSaveId);
				this.iconsAnimation = this.MoveSaveFilesIcons();
				base.StartCoroutine(this.iconsAnimation);
			}
		}
		else
		{
			UnityEngine.Object.Destroy(this.saveFilesIcons[this.selectedSaveId].gameObject);
			this.saveFilesIcons.RemoveAt(this.selectedSaveId);
			Build.BuildQuicksaves.RemoveQuicksaveAt(this.selectedSaveId);
			this.iconsAnimation = this.MoveSaveFilesIcons();
			base.StartCoroutine(this.iconsAnimation);
		}
		this.selectedSaveId = -1;
	}

	public void CloseMenus()
	{
		Time.timeScale = 1f;
		this.savingMenuHolder.SetActive(false);
		this.loadingMenuHolder.SetActive(false);
		while (this.saveFilesIcons.Count > 0)
		{
			UnityEngine.Object.Destroy(this.saveFilesIcons[0].gameObject);
			this.saveFilesIcons.RemoveAt(0);
		}
		if (this.keyboard != null)
		{
			this.keyboard.active = false;
		}
		this.keyboardHolder.SetActive(false);
		this.keyboard = null;
		Ref.SceneType currentScene = Ref.currentScene;
		if (currentScene != Ref.SceneType.Build)
		{
			if (currentScene == Ref.SceneType.Game)
			{
				Ref.inputController.leftArrow.parent.gameObject.SetActive(true);
				foreach (EngineModule.FuelIcon current in Ref.controller.fuelIcons)
				{
					if (current.icon != null)
					{
						current.icon.gameObject.SetActive(true);
					}
				}
			}
		}
		else
		{
			Build.main.EnableDescription();
		}
		this.UpdateLoadButtonColor();
	}

	private void LoadSaveFilesIcons()
	{
		while (this.saveFilesIcons.Count > 0)
		{
			UnityEngine.Object.Destroy(this.saveFilesIcons[0].gameObject);
			this.saveFilesIcons.RemoveAt(0);
		}
		bool flag = Ref.inputController.canvasScaler.referenceResolution.x == 750f;
		int num = (!flag) ? 8 : 12;
		List<string> list = new List<string>();
		if (Ref.currentScene == Ref.SceneType.Build)
		{
			Build.BuildQuicksaves buildQuicksaves = Build.BuildQuicksaves.LoadBuildQuicksaves();
			for (int i = 0; i < Mathf.Min(buildQuicksaves.QuicksavesCount, 24); i++)
			{
				list.Add(buildQuicksaves.buildSaves[i].saveName);
			}
		}
		else if (Ref.currentScene == Ref.SceneType.Game)
		{
			GameSaving.Quicksaves quicksaves = GameSaving.Quicksaves.LoadQuicksaves();
			for (int j = 0; j < Mathf.Min(quicksaves.QuicksavesCount, 24); j++)
			{
				list.Add(quicksaves.quicksaves[j].saveName);
			}
		}
		Transform child = this.loadingMenuHolder.transform.GetChild(1);
		for (int k = 0; k < list.Count; k++)
		{
			Transform transform = UnityEngine.Object.Instantiate<Transform>(this.saveFileIconPrefab, child);
			transform.localPosition = new Vector3((float)(k / num * 365), (float)(k % num * -66), 0f);
			transform.GetChild(1).GetComponent<Text>().text = list[k];
			this.saveFilesIcons.Add(transform.GetComponent<BoxCollider2D>());
		}
		child.localPosition = new Vector3(-182.5f * (float)((list.Count - 1) / num), child.localPosition.y);
	}

	[DebuggerHidden]
	private IEnumerator MoveSaveFilesIcons()
	{
        bool movingIcons = true;

        while (movingIcons)
        {
            yield return new WaitForEndOfFrame();

            movingIcons = false;
            int collumIndex = 0; // Collum index of the icon
            int collumsCount = 0;

            // Creates new icons
            for (int i = 0; i < saveFilesIcons.Count; i++)
            {

                // Checks if icon is in a new collum
                if (i > 0)
                    if (saveFilesIcons[i].transform.localPosition.x != saveFilesIcons[i - 1].transform.localPosition.x)
                    {
                        collumIndex = 0; // New collum
                        collumsCount++;
                    }

                float targetPositionY = collumIndex * -66; // Target position of the icon
                collumIndex++;
                float moveAmountY = (25 + 5 * Mathf.Abs(saveFilesIcons[i].transform.localPosition.y - targetPositionY)) * Time.deltaTime; // By how much icon can be moved this frame            

                if (Mathf.Abs(saveFilesIcons[i].transform.localPosition.y - targetPositionY) > moveAmountY)
                {

                    // Will take more than 1 frame to move icon to position
                    saveFilesIcons[i].transform.localPosition = new Vector3(saveFilesIcons[i].transform.localPosition.x, saveFilesIcons[i].transform.localPosition.y - moveAmountY * Mathf.Sign(saveFilesIcons[i].transform.localPosition.y - targetPositionY));
                    movingIcons = true;
                }
                else
                {
                    // Moved icon to target position this frame
                    saveFilesIcons[i].transform.localPosition = new Vector3(saveFilesIcons[i].transform.localPosition.x, targetPositionY);
                }
            }

            float targetPositionX = -182.5f * collumsCount;
            float diff = loadingMenuHolder.transform.GetChild(2).localPosition.x - targetPositionX;
            float moveAmountX = (40 + 5 * Mathf.Abs(diff)) * Time.deltaTime;

            if (Mathf.Abs(diff) > moveAmountX)
            {

                loadingMenuHolder.transform.GetChild(2).localPosition = new Vector3(loadingMenuHolder.transform.GetChild(2).localPosition.x - moveAmountX * Mathf.Sign(diff), loadingMenuHolder.transform.GetChild(2).localPosition.y);
                movingIcons = true;
            }
            else
            {
                loadingMenuHolder.transform.GetChild(2).localPosition = new Vector3(targetPositionX, loadingMenuHolder.transform.GetChild(2).localPosition.y);
            }
        }
    }

	private void Update()
	{
		if (this.savingMenuHolder.activeSelf)
		{
			this.keyboardText.text = this.keyboardText.text.Replace("|", string.Empty) + ((Time.unscaledTime % 1f <= 0.5f) ? string.Empty : "|");
			if (Input.anyKeyDown)
			{
				this.OnKey(Input.inputString);
			}
			if (Input.GetKeyDown(KeyCode.Backspace))
			{
				this.Backspace();
			}
			this.keyboardText.transform.localPosition = new Vector3((Time.unscaledTime % 1f <= 0.5f) ? 0f : 4.5f, 30f, 0f);
		}
	}

	public static void SaveSetting(Saving.SettingKey settingKey, bool value)
	{
		PlayerPrefs.SetInt(settingKey.ToString(), (!value) ? 0 : 1);
		PlayerPrefs.Save();
	}

	public static bool LoadSetting(Saving.SettingKey settingKey)
	{
		return PlayerPrefs.GetInt(settingKey.ToString(), 0) == 1;
	}

	public static bool HasSetting(Saving.SettingKey settingKey)
	{
		return PlayerPrefs.HasKey(settingKey.ToString());
	}

	public static void ToggleSetting(Saving.SettingKey settingKey)
	{
		Saving.SaveSetting(settingKey, !Saving.LoadSetting(settingKey));
	}
}
