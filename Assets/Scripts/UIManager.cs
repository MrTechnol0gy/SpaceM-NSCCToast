using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager get;
    UIDocument uiDocument;                  // reference to the UIDocument
    private VisualElement root;             // reference to the root visual element
    private VisualElement overlay;          // reference to the overlay screen visual element
    private VisualElement winloseScreen;    // reference to the winlose screen visual element
    private VisualElement loadingScreen;    // reference to the loading screen visual element
    private VisualElement leaveByChoiceScreen;      // reference to the leave by choice screen visual element
    public Label missionTimerLabel, speedLabel, intelTotalRequiredLabel, intelCurrentAmountLabel; //references the ingame UI Labels from the UIDocument
    public Label timeoutText;               // references the timeout UI Label from the UIDocument
    public Label loadText;                  // reference the Loading screen text label from the UIDocument
    public Label leaveText;                 // reference to the leave by choice text label from the UIDocument
    public Toggle tractorBeamToggle;        // references the Toggles from the UIDocument
    public Button levelSelectButton;        // references buttons from the timeout screen
    public Button stayButton, leaveButton;  // references buttons from the leave by choice screen

    void Awake()
    {
        get = this;
        uiDocument = GetComponent<UIDocument>();    // Get the reference to the UIDocument. This script should be in the same gameobject for this to work.
        root = uiDocument.rootVisualElement;        // Get a reference to the root visual element.
        overlay = root.Query<VisualElement>("Overlay");
        winloseScreen = root.Query<VisualElement>("WinLoseScreen");
        missionTimerLabel = root.Query<Label>("MissionTimer");
        speedLabel = root.Query<Label>("Speed");
        tractorBeamToggle = root.Query<Toggle>("TractorBeam");
        intelTotalRequiredLabel = root.Query<Label>("IntelTotalAmount");
        intelCurrentAmountLabel = root.Query<Label>("IntelCurrentAmount");
        timeoutText = root.Query<Label>("TImeoutText");
        levelSelectButton = root.Query<Button>("ReturnToLevelSelect");
        levelSelectButton.clickable = new Clickable(LevelSelect);
        loadingScreen = root.Query<VisualElement>("LoadingScreen");
        loadText = root.Query<Label>("LoadText");
        leaveByChoiceScreen = root.Query<VisualElement>("LeaveScreen");
        leaveText = root.Query<Label>("LeaveText");
        leaveButton = root.Query<Button>("LeaveButton");
        leaveButton.clickable = new Clickable(LeaveByChoice);
        stayButton = root.Query<Button>("StayButton");
        stayButton.clickable = new Clickable(StayAsChoice);
    }

    public bool isShowingMenu() 
    {
        if (winloseScreen.style.display.value == DisplayStyle.None) 
        {
            return false;
        }
        else if (leaveByChoiceScreen.style.display.value == DisplayStyle.None)
        {
            return false;
        }
        else if (winloseScreen.style.display.value == DisplayStyle.None)
        {
            return false;
        }
        else
        return true;
    }
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    [ContextMenu("ShowWinLose")]
    public void ShowWinLoseScreen() 
    {
        UnlockCursor();
        winloseScreen.SetDisplayBasedOnBool(true);
        GameManager.get.StopTime();
    }

    [ContextMenu("HideWinLose")]
    public void HideWinLoseScreen() 
    {
        winloseScreen.SetDisplayBasedOnBool(false);
        LockCursor();
    }
    [ContextMenu("ShowOverlay")]
    public void ShowOverlayScreen() 
    {
        overlay.SetDisplayBasedOnBool(true);
    }

    [ContextMenu("HideOverlay")]
    public void HideOverlayScreen() {
        overlay.SetDisplayBasedOnBool(false);
    }

    [ContextMenu("ShowLoadScreen")]
    public void ShowLoadScreen() 
    {
        loadingScreen.SetDisplayBasedOnBool(true);
        LockCursor();
    }

    [ContextMenu("HideLoadScreen")]
    public void HideLoadScreen() {
        loadingScreen.SetDisplayBasedOnBool(false);
    }
    [ContextMenu("ShowLeaveByChoiceScreen")]
    public void ShowLeaveByChoiceScreen() 
    {
        HideOverlayScreen();
        leaveByChoiceScreen.SetDisplayBasedOnBool(true);
        UnlockCursor();
        GameManager.get.StopTime();
    }

    [ContextMenu("HideLeaveByChoiceScreen")]
    public void HideLeaveByChoiceScreen() 
    {
        ShowOverlayScreen();
        leaveByChoiceScreen.SetDisplayBasedOnBool(false);
        GameManager.get.StartTime();
    }

    public static void SetSpeed(int speed)
    {
        get.speedLabel.text = $"Speed {speed}";     // other scripts can set this using "UIManager.SetSpeed(speed);"
    }

    public static void SetTime(string time)
    {
        get.missionTimerLabel.text = $"{time}";      // other scripts can set this using "UIManager.SetTime(time);"
    }

    public static void SetTractorBeamActive(bool active)
    {
        get.tractorBeamToggle.value = active;       // other scripts can set this using "UIManager.SetTractorBeamActive(bool);"
    }

    public static void SetIntelTotalRequired(int amount)
    {
        get.intelTotalRequiredLabel.text = $"Intel Req: {amount}";
    }
    public static void SetIntelCurrentAmount(int amount)
    {
        get.intelCurrentAmountLabel.text = $"Intel: {amount}";
    }

    public void LevelSelect()
    {
        GameManager.get.UpdateTotalIntelOverTime();
        SceneManager.LoadScene("Level Select");
    }

    public static void SetLoadText(string text)
    {
        get.loadText.text = $"{text}";
    }
    public void LeaveByChoice()
    {
        GameManager.get.UpdateTotalIntelOverTime();
        SceneManager.LoadScene("Level Select");
        HideLeaveByChoiceScreen();
    }
    public void StayAsChoice()
    {
        LockCursor();
        HideLeaveByChoiceScreen();
    }
}
