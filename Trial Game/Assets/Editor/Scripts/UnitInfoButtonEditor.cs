using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(UnitInfoButton), true)]
[CanEditMultipleObjects]
public class UnitInfoButtonEditor : Editor
{
    internal const float k_DefaultElementHeight = 25f;
    internal const float k_PaddingBetweenRules = 5f;
    internal const float k_SingleLineHeight = 20f;
    internal const float k_LabelWidth = 80f;

    private UnitInfoButton UIText { get { return (target as UnitInfoButton); } }
    private List<string> _languageTexts;
    private SerializedProperty _type;
    private SerializedProperty _parent;
    private SerializedProperty _animator;
    private SerializedProperty _vector4;
    private SerializedProperty _flip;
    private SerializedProperty _hp;
    private SerializedProperty _movement;
    private SerializedProperty _speed;
    private SerializedProperty _attack;
    private SerializedProperty _defense;
    private SerializedProperty _range;
    private SerializedProperty _cooldown;
    private ReorderableList _reorderableList;

    void OnEnable()
    {
        if (UIText.LanguageTexts == null || UIText.LanguageTexts.Count == 0)
        {
            Debug.Log("new");
            UIText.LanguageTexts = new List<string>();
            _languageTexts = new List<string>();

            CreateLanguageList();
        }
        else if (UIText.LanguageTexts.Count < Utility.GetValues<Enums.Language>().Count())
        {
            UpdateLanguageList();
        }
        else if (UIText.LanguageTexts.Count > Utility.GetValues<Enums.Language>().Count())
        {
            RemoveLanguageFromList();
        }
        else
            _languageTexts = new List<string>(UIText.LanguageTexts);

        _type = serializedObject.FindProperty("Type");
        _parent = serializedObject.FindProperty("Parent");
        _animator = serializedObject.FindProperty("Animator");
        _vector4 = serializedObject.FindProperty("ImageSize");
        _flip = serializedObject.FindProperty("Flip");
        _hp = serializedObject.FindProperty("HP");
        _movement = serializedObject.FindProperty("Movement");
        _speed = serializedObject.FindProperty("Speed");
        _attack = serializedObject.FindProperty("Attack");
        _defense = serializedObject.FindProperty("Defense");
        _range = serializedObject.FindProperty("Range");
        _cooldown = serializedObject.FindProperty("Cooldown");

        _reorderableList = new ReorderableList(_languageTexts, typeof(string), true, true, true, true);
        _reorderableList.drawHeaderCallback = OnDrawHeader;
        _reorderableList.drawElementCallback = OnDrawElement;
        _reorderableList.elementHeightCallback = GetElementHeight;
        _reorderableList.onReorderCallback = ListUpdated;
        _reorderableList.draggable = false;
        _reorderableList.displayAdd = false;
        _reorderableList.displayRemove = false;
    }

    private void CreateLanguageList()
    {
        var values = Utility.GetValues<Enums.Language>();
        foreach (var value in values)
        {

            UIText.LanguageTexts.Add("");
            _languageTexts.Add("");
        }
    }

    private void UpdateLanguageList()
    {
        var values = Utility.GetValues<Enums.Language>();
        foreach (var value in values.Skip(UIText.LanguageTexts.Count))
        {
            UIText.LanguageTexts.Add("");
            _languageTexts.Add("");
        }
    }

    private void RemoveLanguageFromList()
    {
        var values = Utility.GetValues<Enums.Language>().Count();
        UIText.LanguageTexts.RemoveRange(values, UIText.LanguageTexts.Count - values);
        _languageTexts.RemoveRange(values, _languageTexts.Count - values);
    }

    private void ListUpdated(ReorderableList list)
    {
        SaveTile();
    }

    private float GetElementHeight(int index)
    {
        return k_DefaultElementHeight;
    }

    private void OnDrawElement(Rect rect, int index, bool isactive, bool isfocused)
    {
        var languageText = UIText.LanguageTexts[index];

        float yPos = rect.yMin;
        float height = rect.height - 26;
        float matrixWidth = 48;

        Rect inspectorRect = new Rect(rect.xMin, yPos, rect.width - matrixWidth * 2f - 25f, height);

        EditorGUI.BeginChangeCheck();
        LanguageInspectorOnGUI(inspectorRect, index, languageText);
        if (EditorGUI.EndChangeCheck())
            SaveTile();
    }

    private void LanguageInspectorOnGUI(Rect rect, int index, string languageText)
    {
        float y = rect.yMin;
        EditorGUI.BeginChangeCheck();
        GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), Enum.GetName(typeof(Enums.Language), index));
        UIText.LanguageTexts[index] = EditorGUI.TextField(new Rect(rect.xMin + k_LabelWidth, y, rect.width, k_SingleLineHeight), languageText);
    }

    private void SaveTile()
    {
        EditorUtility.SetDirty(target);
        SceneView.RepaintAll();

        UpdateOverrideTiles();
    }

    private void UpdateOverrideTiles()
    {
        string[] overrideTileGuids = AssetDatabase.FindAssets("t:RuleOverrideTile");
        foreach (string overrideTileGuid in overrideTileGuids)
        {
            string overrideTilePath = AssetDatabase.GUIDToAssetPath(overrideTileGuid);
            RuleOverrideTile overrideTile = AssetDatabase.LoadAssetAtPath<RuleOverrideTile>(overrideTilePath);
            if (overrideTile.m_Tile == target)
                overrideTile.Override();
        }
    }

    private void OnDrawHeader(Rect rect)
    {
        GUI.Label(rect, "Tutorial Texts");
    }

    public override void OnInspectorGUI()
    {
        // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.Space();

        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((UnitInfoButton)target), typeof(UnitInfoButton), false);
        GUI.enabled = true;

        EditorGUILayout.PropertyField(_type);
        EditorGUILayout.PropertyField(_parent);
        EditorGUILayout.PropertyField(_animator);
        EditorGUILayout.PropertyField(_vector4);
        EditorGUILayout.PropertyField(_flip);
        EditorGUILayout.PropertyField(_hp);
        EditorGUILayout.PropertyField(_movement);
        EditorGUILayout.PropertyField(_speed);
        EditorGUILayout.PropertyField(_attack);
        EditorGUILayout.PropertyField(_defense);
        EditorGUILayout.PropertyField(_range);
        EditorGUILayout.PropertyField(_cooldown);
        serializedObject.ApplyModifiedProperties();

        if (_reorderableList != null && UIText.LanguageTexts != null)
            _reorderableList.DoLayoutList();
    }
}

