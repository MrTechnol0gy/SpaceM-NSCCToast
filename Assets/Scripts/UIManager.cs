using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class UIManager : MonoBehaviour
{
    public static UIManager get;
    UIDocument uiDocument;                  // reference to the UIDocument
    private VisualElement root;             // reference to the root visual element
    private VisualElement overlay;          // reference to the overlay screen visual element
    private VisualElement winloseScreen;    // reference to the winlose screen visual element
    public Label missionTimerLabel, speedLabel, intelTotalRequiredLabel, intelCurrentAmountLabel; //references the ingame UI Labels from the UIDocument
    public Label winText, loseText;         // references the win/lose UI Labels from the UIDocument
    public Toggle tractorBeamToggle;        // references the Toggles from the UIDocument
    public Button retryButton, quitButton;  // references buttons from the win/lose screen

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
        winText = root.Query<Label>("WinText");
        loseText = root.Query<Label>("LoseText");
        retryButton = root.Query<Button>("Retry");
        retryButton.clickable = new Clickable(DoRetryStuff);
        quitButton = root.Query<Button>("Quit");
        quitButton.clickable = new Clickable(QuitApplication);
    }

    void Start()
    {
        ShowOverlayScreen();
    }

    [ContextMenu("ShowWinLose")]
    public void ShowWinLoseScreen() 
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        winloseScreen.SetDisplayBasedOnBool(true);
        if (GameManager.get.gameoverVictorious)
        {
            winText.SetDisplayBasedOnBool(true);
            GameManager.get.StopTime();
        }
        else
        {
            loseText.SetDisplayBasedOnBool(true);
            GameManager.get.StopTime();
        }
    }

    [ContextMenu("HideWinLose")]
    public void HideWinLoseScreen() {
        winloseScreen.SetDisplayBasedOnBool(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    [ContextMenu("ShowOverlay")]
    public void ShowOverlayScreen() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
        overlay.SetDisplayBasedOnBool(true);
    }

    [ContextMenu("HideOverlay")]
    public void HideOverlayScreen() {
        overlay.SetDisplayBasedOnBool(false);
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

    public void DoRetryStuff()
    {
        Debug.Log("Clicked Retry.");
    }

    public void QuitApplication()
    {
        Debug.Log("Clicked Quit.");
        Application.Quit();
    }
}
