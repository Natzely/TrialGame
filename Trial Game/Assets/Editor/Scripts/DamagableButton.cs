using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Damageable))]
public class DamagableButton : Editor
{
    public override void OnInspectorGUI()
    {
        Damageable damageable = (Damageable)target;
        if(GUILayout.Button("Kill"))
        {
            damageable.Kill();
        }
        DrawDefaultInspector();
    }
}