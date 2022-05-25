using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ResultText))]
public class ResultTextButton : Editor
{
    public override void OnInspectorGUI()
    {
        ResultText text = (ResultText)target;
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Victory", EditorStyles.miniButtonMid, GUILayout.Width(85)))
        {
            text.Show(true, text.sides);
        }
        if (GUILayout.Button("Defeat", EditorStyles.miniButtonMid, GUILayout.Width(85)))
        {
            text.Show(false, text.sides);
        }
        EditorGUILayout.EndHorizontal();
        DrawDefaultInspector();
    }
}
