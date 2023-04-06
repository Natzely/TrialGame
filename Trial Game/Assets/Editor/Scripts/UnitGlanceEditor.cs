using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UnitGlance))]
public class UnitGlanceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        UnitGlance unitGlance = (UnitGlance)target;
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Cooldown", EditorStyles.miniButtonMid, GUILayout.Width(85)))
        {
            unitGlance.StartCooldown(5f);
        }
        if (GUILayout.Button("Hurt-G", EditorStyles.miniButtonMid, GUILayout.Width(85)))
        {
            unitGlance.TakeDamage(.8f, 1);
        }
        if (GUILayout.Button("Hurt-Y", EditorStyles.miniButtonMid, GUILayout.Width(85)))
        {
            unitGlance.TakeDamage(.55f, 3);
        }
        if (GUILayout.Button("Hurt-R", EditorStyles.miniButtonMid, GUILayout.Width(85)))
        {
            unitGlance.TakeDamage(.25f, 5);
        }
        if(GUILayout.Button("Kill", EditorStyles.miniButtonMid, GUILayout.Width(85)))
        {
            unitGlance.Death();
        }
        if (GUILayout.Button("Reset", EditorStyles.miniButtonMid, GUILayout.Width(85)))
        {
            unitGlance.Reset();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("CD/H-G", EditorStyles.miniButtonMid, GUILayout.Width(85)))
        {
            unitGlance.StartCooldown(3f);
            unitGlance.TakeDamage(.8f, 1);
        }
        if (GUILayout.Button("CD/H-Y", EditorStyles.miniButtonMid, GUILayout.Width(85)))
        {
            unitGlance.StartCooldown(3f);
            unitGlance.TakeDamage(.55f, 3);
        }
        if (GUILayout.Button("CD/H-R", EditorStyles.miniButtonMid, GUILayout.Width(85)))
        {
            unitGlance.StartCooldown(3f);
            unitGlance.TakeDamage(.25f, 5);
        }
        EditorGUILayout.EndHorizontal();
        DrawDefaultInspector();
    }
}
