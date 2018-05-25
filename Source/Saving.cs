using System;
using System.Collections;
using System.Collections.Generic;
using NewBuildSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class Saving : MonoBehaviour
{
    private void Start()
    {
        this.UpdateLoadButtonColor();
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
                    MsgController.ShowMsg("Cannot save while not controlling a rocket");
                    return;
                }
            }
        }
        else if (!Build.main.buildGrid.HasAnyParts())
        {
            return;
        }
        this.onOpenSaveMenu.InvokeEvenets();
    }

    public void CompleteSaveProcess()
    {
        string text = TouchKeyboard.main.GetText("Unnamed Save");
        Ref.SceneType currentScene = Ref.currentScene;
        if (currentScene != Ref.SceneType.Build)
        {
            if (currentScene == Ref.SceneType.Game)
            {
                GameSaving.Quicksaves.AddQuicksave(GameSaving.GetGameSaveData(text));
                MsgController.ShowMsg("Game Saved");
            }
        }
        else
        {
            Build.BuildQuicksaves.AddQuicksave(new Build.BuildSave(text, Camera.main.transform.position, Build.main.buildGrid.parts, 0));
            MsgController.ShowMsg("Design Saved");
        }
        this.onCloseSaveMenu.InvokeEvenets();
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
            Build.main.DisableUI();
        }
        else if (Ref.currentScene == Ref.SceneType.Game)
        {
            Ref.controller.HideArrowButtons();
            Ref.controller.fuelIconsHolder.gameObject.SetActive(false);
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
                    MsgController.ShowMsg("Game Loaded");
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
        Ref.SceneType currentScene = Ref.currentScene;
        if (currentScene != Ref.SceneType.Build)
        {
            if (currentScene == Ref.SceneType.Game)
            {
                Ref.inputController.leftArrow.parent.gameObject.SetActive(true);
                Ref.controller.fuelIconsHolder.gameObject.SetActive(true);
            }
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
        bool flag = Ref.inputController.canvasScalers[0].referenceResolution.x == 750f;
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

    private IEnumerator MoveSaveFilesIcons()
    {
        bool movingIcons = true;
        while (movingIcons)
        {
            yield return new WaitForEndOfFrame();
            movingIcons = false;
            int collumIndex = 0;
            int collumsCount = 0;
            for (int i = 0; i < this.saveFilesIcons.Count; i++)
            {
                if (i > 0 && this.saveFilesIcons[i].transform.localPosition.x != this.saveFilesIcons[i - 1].transform.localPosition.x)
                {
                    collumIndex = 0;
                    collumsCount++;
                }
                float num = (float)(collumIndex * -66);
                collumIndex++;
                float num2 = (25f + 5f * Mathf.Abs(this.saveFilesIcons[i].transform.localPosition.y - num)) * Time.deltaTime;
                if (Mathf.Abs(this.saveFilesIcons[i].transform.localPosition.y - num) > num2)
                {
                    this.saveFilesIcons[i].transform.localPosition = new Vector3(this.saveFilesIcons[i].transform.localPosition.x, this.saveFilesIcons[i].transform.localPosition.y - num2 * Mathf.Sign(this.saveFilesIcons[i].transform.localPosition.y - num));
                    movingIcons = true;
                }
                else
                {
                    this.saveFilesIcons[i].transform.localPosition = new Vector3(this.saveFilesIcons[i].transform.localPosition.x, num);
                }
            }
            float targetPositionX = -182.5f * (float)collumsCount;
            float diff = this.loadingMenuHolder.transform.GetChild(2).localPosition.x - targetPositionX;
            float moveAmountX = (40f + 5f * Mathf.Abs(diff)) * Time.deltaTime;
            if (Mathf.Abs(diff) > moveAmountX)
            {
                this.loadingMenuHolder.transform.GetChild(2).localPosition = new Vector3(this.loadingMenuHolder.transform.GetChild(2).localPosition.x - moveAmountX * Mathf.Sign(diff), this.loadingMenuHolder.transform.GetChild(2).localPosition.y);
                movingIcons = true;
            }
            else
            {
                this.loadingMenuHolder.transform.GetChild(2).localPosition = new Vector3(targetPositionX, this.loadingMenuHolder.transform.GetChild(2).localPosition.y);
            }
        }
        yield break;
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

    public GameObject savingMenuHolder;

    public GameObject loadButton;

    public GameObject loadingMenuHolder;

    public GameObject confirmMenuHolder;

    public int selectedSaveId;

    public List<BoxCollider2D> saveFilesIcons;

    public Transform saveFileIconPrefab;

    [BoxGroup]
    public CustomEvent onOpenSaveMenu;

    [BoxGroup]
    public CustomEvent onCloseSaveMenu;

    private IEnumerator iconsAnimation;

    public enum SettingKey
    {
        seenBuildInstructions,
        seenGameInstructions,
        seenMapInstructions,
        seenTimewarpInstructions,
        disableMusic,
        disableSound,
        disableAutoRotate,
        disable60fps,
        hasPartDLC,
        hasHackedDLC,
        infiniteFuel,
        noDrag,
        unbreakableParts,
        noGravity,
        orbitInfo,
        askedToRate1,
        askedToRate2,
        hasRated
    }

    public enum SaveKey
    {
        ToLaunch,
        PersistantGameSave,
        Settings,
        BuildQuicksaves,
        GameQuicksaves
    }
}
