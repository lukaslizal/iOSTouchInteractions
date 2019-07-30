/*
 * @author Lukáš Lízal
 */
using UnityEngine;
using System.Collections;
/// <summary>
/// Class offerening various extension methods used 
/// which are being used in this work.
/// </summary>
public static class ExtensionMethods
{
    public static Transform[] GetFirstChildren(this Transform parent)
    {
        Transform[] children = parent.GetComponentsInChildren<Transform>();
        Transform[] firstChildren = new Transform[parent.childCount];
        int index = 0;
        foreach (Transform child in children)
        {
            if (child.parent == parent)
            {
                firstChildren[index] = child;
                index++;
            }
        }
        return firstChildren;
    }
    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
