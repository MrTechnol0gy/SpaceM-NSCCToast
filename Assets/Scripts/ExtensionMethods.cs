using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

//It is common to create a class to contain all of your
//extension methods. This class must be static.
public static class ExtensionMethods
{
    //Even though they are used like normal methods, extension
    //methods must be declared static. Notice that the first
    //parameter has the 'this' keyword followed by a GameObject
    //variable. This variable denotes which class the extension
    //method becomes a part of.
    public static bool IsInLayerMask(this GameObject obj, LayerMask layerMask) 
    {
        // Convert the object's layer to a bitfield for comparison
        int objLayerMask = (1 << obj.layer);

        if ((layerMask.value & objLayerMask) > 0)  // Extra round brackets required!
            return true;
        else
            return false;
    }

    public static bool IntervalElapsedSince(this float thisTime, float interval) => !(thisTime + interval > Time.time);

    public static void SetDisplayBasedOnBool(this VisualElement vs, bool b)
    {
        vs.style.display =
            b ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex) : new StyleEnum<DisplayStyle>(DisplayStyle.None);
    }
    public static void SetDisplayAndOpacityBasedOnBool(this VisualElement vs, bool b)
    {
        vs.style.display =
            b ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex) : new StyleEnum<DisplayStyle>(DisplayStyle.None);
        vs.style.opacity = b ? 1 : 0;
    }
}