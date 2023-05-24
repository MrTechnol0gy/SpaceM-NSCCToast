using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;
using UnityEngine.SceneManagement;


public class UIVictory : MonoBehaviour
{
    public static UIVictory get;
    UIDocument uiDocument;                  // reference to the UIDocument
    private VisualElement root;             // reference to the root visual element
    private VisualElement victory;          // reference to the overlay screen visual element
    private Label victoryTextLabel;
    public Button mainMenu;  // references buttons from the level select screen
    void Awake()
    {
        get = this;
        uiDocument = GetComponent<UIDocument>();    // Get the reference to the UIDocument. This script should be in the same gameobject for this to work.
        root = uiDocument.rootVisualElement;        // Get a reference to the root visual element.
        victoryTextLabel = root.Query<Label>("VictoryText");
        mainMenu = root.Query<Button>("MainMenu");
        mainMenu.clickable = new Clickable(MainMenu);
        
    }

    void Start()
    {
        UnlockCursor();
        SetVictoryText("Congratulations! You've finished the demo! We appreciate your time, and hope you enjoyed trying it out!");
    }

    public bool isShowingMenu() {
        if (victory.style.display.value == DisplayStyle.None) return false;
        return true;
    }
    [ContextMenu("ShowVictory")]
    public void ShowLevelSelectScreen() 
    {
        UnlockCursor();
        victory.SetDisplayBasedOnBool(true);
    }

    [ContextMenu("HideVictory")]
    public void HideLevelSelectScreen() 
    {
        victory.SetDisplayBasedOnBool(false);
        LockCursor();
    }
    
    public void MainMenu()
    {
        //Debug.Log("Clicked MainMenu.");
        SceneManager.LoadScene("Main Menu");
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
    public static void SetVictoryText(string text)
    {
        get.victoryTextLabel.text = $"{text}";      // other scripts can set this using "UIManager.SetTime(time);"
    }
}