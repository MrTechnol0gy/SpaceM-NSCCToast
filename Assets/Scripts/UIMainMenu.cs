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
    public Button startButton, continueButton, quitButton;  // references buttons from the main menu screen
    void Awake()
    {
        get = this;
        uiDocument = GetComponent<UIDocument>();    // Get the reference to the UIDocument. This script should be in the same gameobject for this to work.
        root = uiDocument.rootVisualElement;        // Get a reference to the root visual element.
        mainmenu = root.Query<VisualElement>("ButtonMenu");
        startButton = root.Query<Button>("Start");
        startButton.clickable = new Clickable(StartGame);
        continueButton = root.Query<Button>("Continue");
        continueButton.clickable = new Clickable(Continue);
        quitButton = root.Query<Button>("Quit");
        quitButton.clickable = new Clickable(QuitApplication);
    }

    void Start()
    {

    }

    public bool isShowingMenu() {
        if (mainmenu.style.display.value == DisplayStyle.None) return false;
        return true;
    }
    [ContextMenu("ShowMainMenu")]
    public void ShowMainMenuScreen() 
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        mainmenu.SetDisplayBasedOnBool(true);
    }

    [ContextMenu("HideMainMenu")]
    public void HideMainMenuScreen() {
        mainmenu.SetDisplayBasedOnBool(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void StartGame()
    {
        Debug.Log("Clicked Start.");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Workshop");
    }

    public void Continue()
    {
        Debug.Log("Clicked Continue.");
        //UnityEngine.SceneManagement.SceneManager.LoadScene("Loading");
    }

    public void QuitApplication()
    {
        Debug.Log("Clicked Quit.");
        Application.Quit();
    }
}
