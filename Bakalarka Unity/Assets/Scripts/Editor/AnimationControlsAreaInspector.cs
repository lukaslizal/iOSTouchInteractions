/*
 * @author Lukáš Lízal 2018
 */
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AnimationControlsArea))]
public class AnimationControlsInspector : Editor
{
    public int selected;
    public int selectedStartingSpeed;
    public int selectedSingleMultiple;
    public string[] options = new string[]
    {
        "Animation Trigger", "Progress Controller", "Speed Controller"
    };
    public string[] optionsSingleMultimple = new string[]
    {
        "Single Animator", "All Children Animators"
    };
    public string[] optionsMinMax = new string[]
{
        "Min Speed", "Max Speed"
};
    [MenuItem("GameObject/Animation Control Area", false, 12)]
    private static void CreateAnimationControlArea()
    {
        var go = Create();
        if (Selection.activeGameObject && Selection.activeGameObject.transform.parent)
        {
            go.transform.parent = Selection.activeGameObject.transform.parent.transform;
            Selection.activeGameObject = go;
        }
    }
    public override void OnInspectorGUI()
    {
        AnimationControlsArea myTarget = (AnimationControlsArea)target;
        EditorGUI.BeginChangeCheck();
        GUILayout.Space(10);
        myTarget.mainCamera = (Camera)EditorGUILayout.ObjectField("Player Camera", myTarget.mainCamera, typeof(Camera), true);
        if (myTarget.singleAnimator)
            selectedSingleMultiple = 0;
        else
            selectedSingleMultiple = 1;
        var selSM = EditorGUILayout.Popup("Control: ", selectedSingleMultiple, optionsSingleMultimple);
        myTarget.singleAnimator = (selSM == 0);
        myTarget.animatedObject = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Of GameObject", "Select GameObject with animator attached to it. Or select a parent of multiple GameObjects with Animators attached to it."), myTarget.animatedObject, typeof(GameObject), true);

        EditorGUI.BeginDisabledGroup(myTarget.singleAnimator);
        myTarget.progressOffset = EditorGUILayout.Slider(new GUIContent("Progress Offset:", "If Controlling multiple Animators, an animation time offset may be introduced to them so none of them is set to the same playback time."), myTarget.progressOffset, 0f, 1f);
        EditorGUI.EndDisabledGroup();
        GUILayout.Space(10);
        if (myTarget.triggerAnim)
            selected = 0;
        if (myTarget.progressAnim)
            selected = 1;
        if (myTarget.speedAnim)
            selected = 2;
        var sel = EditorGUILayout.Popup("Controls Type", selected, options);

        if (sel != selected)
        {
            selected = sel;
            ChangeAnimationControlType(myTarget);
            myTarget.SetName();
        }
        GUILayout.Space(10);
        EditorGUI.BeginDisabledGroup(selected != 0);
        GUILayout.Label("Animation Trigger");
        GUILayout.BeginVertical("HelpBox");
        myTarget.triggerEverytime = !EditorGUILayout.Toggle("Trigger Once", myTarget.triggerEverytime);
        myTarget.animatorSpeed = EditorGUILayout.Slider("Animation Speed:", myTarget.animatorSpeed, 0f, 10f);
        GUILayout.EndVertical();
        EditorGUI.EndDisabledGroup();
        EditorGUI.BeginDisabledGroup(selected != 1);
        GUILayout.Label("Progress Controller");
        GUILayout.BeginVertical("HelpBox");
        myTarget.controlsOrientation = (Axis)EditorGUILayout.EnumPopup("Controls orientation", myTarget.controlsOrientation);
        if (myTarget.controlsOrientation == Axis.Z)
            myTarget.controlsOrientation = Axis.X;
        myTarget.revertAnimation = EditorGUILayout.Toggle("Reverse Animation", myTarget.revertAnimation);
        EditorGUILayout.LabelField("Min Progress: " + myTarget.minusProgress.ToString(), "Max Progress: " + myTarget.plusProgress.ToString());
        GUILayout.BeginHorizontal();
        GUILayout.Space(30);
        EditorGUILayout.MinMaxSlider(ref myTarget.minusProgress, ref myTarget.plusProgress, 0f, 1f);
        GUILayout.Space(30);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(selected != 2);
        GUILayout.Label("Speed Controller");
        GUILayout.BeginVertical("HelpBox");
        myTarget.controlsOrientation = (Axis)EditorGUILayout.EnumPopup("Controls orientation", myTarget.controlsOrientation);
        if (myTarget.controlsOrientation == Axis.Z)
            myTarget.controlsOrientation = Axis.X;
        myTarget.progressOffset = EditorGUILayout.Slider("Starting Progress:", myTarget.progressOffset, 0f, 1f);
        if (myTarget.startSpeedAtMin)
            selectedStartingSpeed = 0;
        else
            selectedStartingSpeed = 1;
        var selMinMax = EditorGUILayout.Popup("Initialize Speed At", selectedStartingSpeed, optionsMinMax);
        myTarget.startSpeedAtMin = (selMinMax == 0);
        EditorGUILayout.LabelField("Min Speed: " + myTarget.minusSpeed.ToString(), "Max Speed: " + myTarget.plusSpeed.ToString());
        GUILayout.BeginHorizontal();
        GUILayout.Space(30);
        EditorGUILayout.MinMaxSlider(ref myTarget.minusSpeed, ref myTarget.plusSpeed, 0f, 1f);
        GUILayout.Space(30);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        EditorGUI.EndDisabledGroup();
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(myTarget, "Set Field Value");
        }
        // DrawDefaultInspector();
    }
    public void ChangeAnimationControlType(AnimationControlsArea myTarget)
    {
        switch (selected)
        {
            case 0:
                myTarget.triggerAnim = true;
                myTarget.progressAnim = false;
                myTarget.speedAnim = false;
                break;
            case 1:
                myTarget.triggerAnim = false;
                myTarget.progressAnim = true;
                myTarget.speedAnim = false;
                break;
            case 2:
                myTarget.triggerAnim = false;
                myTarget.progressAnim = false;
                myTarget.speedAnim = true;
                break;
            default:
                break;
        }
    }
    public static GameObject Create()
    {
        var go = new GameObject();
        var bc = go.AddComponent<BoxCollider>();
        var aca = go.AddComponent<AnimationControlsArea>();
        bc.isTrigger = true;
        bc.size = new Vector3(5, 5, 1);
        aca.triggerAnim = true;
        aca.minusProgress = 0f;
        aca.plusProgress = 1f;
        aca.minusSpeed = 0.5f;
        aca.plusSpeed = 1f;
        aca.SetName();
        Undo.RegisterCreatedObjectUndo(go, "Create Animation Controls Area");
        return go;
    }
}