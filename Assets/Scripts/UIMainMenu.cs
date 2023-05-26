using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class UIMainMenu : MonoBehaviour
{
    public static UIMainMenu get;
    UIDocument uiDocument;                  // reference to the UIDocument
    private VisualElement root;             // reference to the root visual element
    private VisualElement mainmenu;          // reference to the overlay screen visual element
    private VisualElement credits;
    public Button startButton, resetButton, quitButton, creditsButton;  // references buttons from the main menu screen
    private bool isCreditsActive = false;
    void Awake()
    {
        get = this;
        uiDocument = GetComponent<UIDocument>();    // Get the reference to the UIDocument. This script should be in the same gameobject for this to work.
        root = uiDocument.rootVisualElement;        // Get a reference to the root visual element.
        mainmenu = root.Query<VisualElement>("ButtonMenu");
        startButton = root.Query<Button>("Start");
        startButton.clickable = new Clickable(StartGame);
        resetButton = root.Query<Button>("Reset");
        resetButton.clickable = new Clickable(Reset);
        quitButton = root.Query<Button>("Quit");
        quitButton.clickable = new Clickable(QuitApplication);
        creditsButton = root.Query<Button>("CreditsButton");
        creditsButton.clickable = new Clickable(ShowCredits);
        credits = root.Query<VisualElement>("Credits");
    }

    void Start()
    {

    }

    public bool isShowingMenu() 
    {
        if (mainmenu.style.display.value == DisplayStyle.None) return false;
        else if (credits.style.display.value == DisplayStyle.None) return false;
        return true;
    }
    [ContextMenu("ShowMainMenu")]
    public void ShowMainMenuScreen() 
    {
        UIMainMenu.get.UnlockCursor();
        mainmenu.SetDisplayBasedOnBool(true);
    }

    [ContextMenu("HideMainMenu")]
    public void HideMainMenuScreen() {
        mainmenu.SetDisplayBasedOnBool(false);
        UIMainMenu.get.LockCursor();
    }
    [ContextMenu("ShowCredits")]
    public void ShowCreditsScreen() 
    {
        UIMainMenu.get.UnlockCursor();
        credits.SetDisplayBasedOnBool(true);
    }

    [ContextMenu("HideCredits")]
    public void HideCreditsScreen() {
        credits.SetDisplayBasedOnBool(false);
        UIMainMenu.get.LockCursor();
    }
    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Level Select");
    }

    public void Reset()
    {
        //Debug.Log("Clicked Reset.");
        AudioManager.get.PlayClick();
        GameManager.TotalIntelCollectedOverTime = 0;
    }

    public void QuitApplication()
    {
        //Debug.Log("Clicked Quit.");
        AudioManager.get.PlayClick();
        Application.Quit();
    }
    public void ShowCredits()
    {
        AudioManager.get.PlayClick();
        if (!isCreditsActive)
        {
            isCreditsActive = true;
            HideMainMenuScreen();
            ShowCreditsScreen();
        }
        else
        {
            isCreditsActive = false;
            HideCreditsScreen();
            ShowMainMenuScreen();
        }
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
}
