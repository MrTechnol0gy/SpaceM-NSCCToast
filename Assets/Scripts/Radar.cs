using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Radar : MonoBehaviour
{
    public Transform playerObj;
    public Texture2D playerSprite,enemySprite,pickupSprite;

    public float size = 200;
    public static Radar instance;
    public UIDocument uiDocument;

    public Color backgroundColor;
    private VisualElement root,manager,radarElement,playerElement;

    public Dictionary<Transform,VisualElement> targetDictionary = new Dictionary<Transform, VisualElement>();
    
    //This multiplication will determine how far the objects show away from the player center. It works as a "zoom".
    public float multiplyAmount = 1;
    private void Awake()
    {
        instance = this;
        //I didn't really want to commit to making a UXML for it so I created the elements manually here, and hijacked my actual menu to display it.
        root = uiDocument.rootVisualElement;
        //This is a 100% screen container I have that I normally use for screen alerts and such, I just placed it under it.
        manager = root.Query("RadarContainer");
        
        //Creating the radar element.
        radarElement = new VisualElement();
        //I want the position to be absolute for it, and I'll place it in the bottom corner
        radarElement.style.position = new StyleEnum<Position>(Position.Absolute);
        //Setting the size to be 360x360
        radarElement.style.minHeight = new StyleLength(size);
        radarElement.style.minWidth= radarElement.style.minHeight;
        //Setting my background color to white.
        radarElement.style.backgroundColor = new StyleColor(backgroundColor);
        //This will make it be on the bottom right corner, just a bit away from the edge (10pixels away to be exact.
        radarElement.style.right = 10;
        radarElement.style.bottom = 10;
        //Now i'm setting the border radius to be the same value of my width/height so it'll end up being a perfect circle.
        radarElement.style.borderBottomLeftRadius = radarElement.style.minHeight;
        radarElement.style.borderBottomRightRadius = radarElement.style.minHeight;
        radarElement.style.borderTopLeftRadius = radarElement.style.minHeight;
        radarElement.style.borderTopRightRadius = radarElement.style.minHeight;
        //I want to hide elements that aren't inside my circle, so I'm setting overflow to hidden.
        radarElement.style.overflow = new StyleEnum<Overflow>(Overflow.Hidden);
        //Now I'm adding it to my container panel.
        manager.Add(radarElement);
        //The player element I'm just placing a sprite in the center of the radar.
        playerElement = new VisualElement();
        //Absolute position.
        playerElement.style.position = new StyleEnum<Position>(Position.Absolute);
        //I decided size will be 42 so I'm setting that to min width and height.
        playerElement.style.minHeight = new StyleLength(42);
        playerElement.style.minWidth = playerElement.style.minHeight;
        //Now for the bottom and right position I'm using half of the radar size minus half of this sprite's size, that should place it smack dab in the center.
        playerElement.style.bottom = new StyleLength((radarElement.style.minHeight.value.value * 0.5f) - (playerElement.style.minHeight.value.value * 0.5f));
        playerElement.style.right = playerElement.style.bottom;
        //My sprite is looking to the right and I wanted it to look up so I'm rotating it -90 degrees.
        playerElement.style.rotate = new StyleRotate(new Rotate(0));
        //Here I'm setting the sprite image.
        playerElement.style.backgroundImage = new StyleBackground(playerSprite);
        //The sprite image is white which won't work on my white radar... So I'm tinting it blue.
        playerElement.style.unityBackgroundImageTintColor = new StyleColor(Color.blue);
        //Added it to the radar element.
        radarElement.Add(playerElement);
    }

    public void CreateTarget(Transform target, bool isPickup) {
        var enemyElement = new VisualElement();
        enemyElement.style.position = new StyleEnum<Position>(Position.Absolute);
        enemyElement.style.minHeight = new StyleLength(42);
        enemyElement.style.minWidth = playerElement.style.minHeight;
        enemyElement.style.backgroundImage = new StyleBackground( isPickup ? pickupSprite : enemySprite );
        enemyElement.style.unityBackgroundImageTintColor = new StyleColor(isPickup ? Color.yellow : Color.red);
        radarElement.Add(enemyElement);
        targetDictionary[target] = enemyElement;
    }

    public void RemoveTarget(Transform target)
    {
        if (targetDictionary.TryGetValue(target, out VisualElement visualElement)) {
            targetDictionary.Remove(target);
            visualElement.RemoveFromHierarchy();
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (KeyValuePair<Transform,VisualElement> target in targetDictionary)
        {
            //I'm using InverseTransformPoint to find a local space offset position based on the player's position and rotation for the enemy world position. I'm using the camera as my player transform.
            Vector3 offsetEnemyPos = playerObj.transform.InverseTransformPoint(target.Key.position);
            //Then for the enemy element right I take the player element's right position and I subtract the local space offset I found multiplied by the multiplyAmount which acts as my "zoom"
            target.Value.style.right = playerElement.style.right.value.value - (offsetEnemyPos.x * multiplyAmount);
            //same for the bottom position
            target.Value.style.bottom = playerElement.style.bottom.value.value + (offsetEnemyPos.z * multiplyAmount);
        }
    }
}