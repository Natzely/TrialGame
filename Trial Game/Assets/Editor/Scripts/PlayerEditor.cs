using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Tilemaps;
using UnityEditor.Build.Content;

[CustomEditor(typeof(PlayerManager), true)]
[CanEditMultipleObjects]
public class PlayerEditor : Editor
{
    internal const float k_DefaultElementHeight = 48f;
    internal const float k_PaddingBetweenRules = 26f;
    internal const float k_SingleLineHeight = 16f;
    internal const float k_LabelWidth = 80f;

    private PlayerManager _playerManager { get { return (target as PlayerManager); } }
    private SerializedProperty _actionTimer;
    private SerializedProperty _debugOn;
    private SerializedProperty _pauseScreen;
    private ReorderableList _reorderableList;

    void OnEnable()
    {
        if (_playerManager.PlayerList == null)
            _playerManager.PlayerList = new List<PlayerManager.PlayerInfo>();

        _actionTimer = serializedObject.FindProperty("ActionTimer");
        _debugOn = serializedObject.FindProperty("DebugOn");
        _pauseScreen = serializedObject.FindProperty("PauseScreen");

        _reorderableList = new ReorderableList(_playerManager.PlayerList, typeof(PlayerManager.PlayerInfo), true, true, true, true);
        _reorderableList.drawHeaderCallback = OnDrawHeader;
        _reorderableList.drawElementCallback = OnDrawElement;
        _reorderableList.elementHeightCallback = GetElementHeight;
        _reorderableList.onReorderCallback = ListUpdated;
        _reorderableList.onAddCallback = OnAddElement;
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
        PlayerManager.PlayerInfo playerInfo = _playerManager.PlayerList[index];

        float yPos = rect.yMin;
        float height = rect.height - 26;
        float matrixWidth = 48;

        Rect inspectorRect = new Rect(rect.xMin, yPos, rect.width - matrixWidth * 2f - 25f, height);

        EditorGUI.BeginChangeCheck();
        PlayerInspectorOnGUI(inspectorRect, playerInfo);
        if (EditorGUI.EndChangeCheck())
            SaveTile();
    }

    private static void PlayerInspectorOnGUI(Rect rect, PlayerManager.PlayerInfo playerInfo)
    {
        float y = rect.yMin;
        EditorGUI.BeginChangeCheck();
        GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), "Player");
        playerInfo.Player = (Enums.Player)EditorGUI.EnumPopup(new Rect(rect.xMin + k_LabelWidth + 40, y, rect.width, k_SingleLineHeight), playerInfo.Player);
        y += k_SingleLineHeight;
        playerInfo.DeleteMoveSpace = EditorGUI.Toggle(new Rect(rect.xMin, y, rect.width, k_SingleLineHeight), "Delete Move Space", playerInfo.DeleteMoveSpace);
        y += k_SingleLineHeight;
    }

    private void OnAddElement(ReorderableList list)
    {
        PlayerManager.PlayerInfo player = new PlayerManager.PlayerInfo();
        player.Player = (Enums.Player)_playerManager.PlayerList.Count + 11;
        player.DeleteMoveSpace = false;
        _playerManager.PlayerList.Add(player);
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
        GUI.Label(rect, "Player Info");
    }

    public override void OnInspectorGUI()
    {
        // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(_debugOn);
        EditorGUILayout.PropertyField(_actionTimer);
        EditorGUILayout.PropertyField(_pauseScreen);
        serializedObject.ApplyModifiedProperties();
        if (_reorderableList != null && _playerManager.PlayerList != null)
            _reorderableList.DoLayoutList();
    }
}