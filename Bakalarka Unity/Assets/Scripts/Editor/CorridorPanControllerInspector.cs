/*
*   @author Lukáš Lízal 2018
 */
using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(CorridorPanController))]
public class CorridorPanControllerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        CorridorPanController myTarget = (CorridorPanController)target;
        GUILayout.Space(10);
        GUILayout.Label("Corridor Map Settings");
        GUILayout.BeginVertical("HelpBox");
        GUILayout.BeginHorizontal();
        myTarget.mapStart = (Node)EditorGUILayout.ObjectField("Spawn Player At Node", myTarget.mapStart, typeof(Node), true);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        myTarget.enableDebugVizualization = EditorGUILayout.Toggle("Enable Debug Vizualization", myTarget.enableDebugVizualization);
        if (EditorGUI.EndChangeCheck())
        {
            if (myTarget.debugVizualizationObject)
                myTarget.debugVizualizationObject.SetActive(myTarget.enableDebugVizualization);
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.Label("Elastic Scroll Settings");
        GUILayout.BeginVertical("HelpBox");
        GUILayout.BeginHorizontal();
        myTarget.inertiaCoefficient = EditorGUILayout.Slider("Inertia Coefficient", myTarget.inertiaCoefficient, 0f, 1f);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        myTarget.maxInertiaSpeed = EditorGUILayout.Slider("Max Inertia Speed", myTarget.maxInertiaSpeed, 0f, 2f);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        myTarget.maxPanSpeed = EditorGUILayout.Slider("Max Pan Speed", myTarget.maxPanSpeed, 0f, 2f);
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.Space(10);   
    }
}