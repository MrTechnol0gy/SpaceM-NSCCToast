using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;
using UnityEngine.SceneManagement;

public class UILevelSelect : MonoBehaviour
{
    public static UILevelSelect get;
    UIDocument uiDocument;                  // reference to the UIDocument
    private VisualElement root;             // reference to the root visual element
    private VisualElement levelselect;          // reference to the overlay screen visual element
    private Label planetNameLabel;
    public Button nextButton, selectButton, previousButton;  // references buttons from the main menu screen
    void Awake()
    {
        get = this;
        uiDocument = GetComponent<UIDocument>();    // Get the reference to the UIDocument. This script should be in the same gameobject for this to work.
        root = uiDocument.rootVisualElement;        // Get a reference to the root visual element.
        planetNameLabel = root.Query<Label>("PlanetName");
        levelselect = root.Query<VisualElement>("Buttons");
        nextButton = root.Query<Button>("Next");
        nextButton.clickable = new Clickable(Next);
        previousButton = root.Query<Button>("Previous");
        previousButton.clickable = new Clickable(Previous);
        selectButton = root.Query<Button>("Select");
        selectButton.clickable = new Clickable(Select);
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public bool isShowingMenu() {
        if (levelselect.style.display.value == DisplayStyle.None) return false;
        return true;
    }
    [ContextMenu("ShowLevelSelect")]
    public void ShowLevelSelectScreen() 
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        levelselect.SetDisplayBasedOnBool(true);
    }

    [ContextMenu("HideLevelSelect")]
    public void HideLevelSelectScreen() {
        levelselect.SetDisplayBasedOnBool(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public static void SetPlanetName(string name)
    {
        get.planetNameLabel.text = $"{name}";      // other scripts can set this using "UIManager.SetPlanetName(name);"
    }
    public void Next()
    {
        PlanetarySelection.get.Rotate();
    }

    public void Previous()
    {
        PlanetarySelection.get.ReverseRotate();
    }

    public void Select()
    {
        Debug.Log("Clicked Select.");
        SceneManager.LoadScene("Workshop");
    }
}