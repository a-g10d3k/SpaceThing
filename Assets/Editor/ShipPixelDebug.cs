using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ShipPixel))]
public class ShipPixelDebug : Editor
{
    
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Destroy pixel"))
        {
            ShipPixel myTarget = target as ShipPixel;
            myTarget.DestroyPixel(new Vector2(0, 0));
        }
    }
}

