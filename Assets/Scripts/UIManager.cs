using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager get;
    UIDocument uiDocument;          // reference to the UIDocument
    private VisualElement root;     // reference to the root visual element
    public Label missionTimerLabel, speedLabel, intelTotalRequiredLabel, intelCurrentAmountLabel; //references the Labels from the UIDocument
    public Toggle tractorBeamToggle;      // references the Toggles from the UIDocument

    void Awake()
    {
        get = this;
        uiDocument = GetComponent<UIDocument>();    // Get the reference to the UIDocument. This script should be in the same gameobject for this to work.
        root = uiDocument.rootVisualElement;        // Get a reference to the root visual element.
        missionTimerLabel = root.Query<Label>("MissionTimer");
        speedLabel = root.Query<Label>("Speed");
        tractorBeamToggle = root.Query<Toggle>("TractorBeam");
        intelTotalRequiredLabel = root.Query<Label>("IntelTotalAmount");
        intelCurrentAmountLabel = root.Query<Label>("IntelCurrentAmount");
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
}
