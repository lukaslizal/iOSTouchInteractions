/*
*   @author Lukáš Lízal 2018
 */
using UnityEngine;
using TouchScript.Gestures;
using System;

/// <summary>
/// Part of menu elastic pan controller
/// Subscribes to Transform Gesture, listening to its events.
/// Based on recieved events of Transform Gesture, this class executes the main logic of
/// menu elastic pan process.
/// </summary>
[RequireComponent(typeof(MenuPanController))]
public class MenuPanHandler : MonoBehaviour
{
    [HideInInspector]
    [Tooltip("associated instance of MenuPanController")]
    public MenuPanController c;
    /// <summary>
    /// Called onece on initialization
    /// </summary>
    void Awake()
    {
        c = GetComponent<MenuPanController>();
        c.SetDifference(Vector3.zero);
        var cam = Camera.main;
        cam.transform.position = new Vector3(0, 0, cam.transform.position.z);
    }
    /// <summary>
    /// Subscribes "handling" functions to gesture events.
    /// </summary>
    private void OnEnable()
    {
        GetComponent<TransformGesture>().TransformStarted += panStartedHandler;
        GetComponent<TransformGesture>().Transformed += panHandler;
        GetComponent<TransformGesture>().TransformCompleted += panCompletedHandler;
    }
    /// <summary>
    /// Unsubscribes "handling" functions to gesture events.
    /// </summary>
    private void OnDisable()
    {
        GetComponent<TransformGesture>().TransformStarted -= panStartedHandler;
        GetComponent<TransformGesture>().Transformed -= panHandler;
        GetComponent<TransformGesture>().TransformCompleted -= panCompletedHandler;
    }
    /// <summary>
    /// Initializes elastic panning process based on newly
    /// registred Transform Gesture.
    /// Marks elastic pan phase as Started (Direct finger manipulation).
    /// </summary>
    /// <param name="sender">Gesture Object.</param>
    /// <param name="e">Event Data.</param>
    private void panStartedHandler(object sender, EventArgs e)
    {
        c.SetInputInterruptedTween(LeanTween.isTweening(gameObject));
        // disable ongoing tweens so finger has all control over movement
        LeanTween.cancel(gameObject);
        var gesture = sender as TransformGesture;
        c.SetDifference(gesture.DeltaPosition);
        c.SetGestureState(GestureState.Start);
        c.SetDirection();

        if (c.GetOccurrance() == NodeState.BumpZone)
        {
            c.SlowDown();
        }
    }
    /// <summary>
    /// Updates new Transform Gesture pointer position (delta of previous and new pointer position)
    /// </summary>
    /// <param name="sender">Gesture Object.</param>
    /// <param name="e"> Event Data.</param>
    /// <returns></returns>
    private void panHandler(object sender, EventArgs e)
    {
        var gesture = sender as TransformGesture;
        c.SetDifference(gesture.DeltaPosition);
        c.SetDirection();
        if (c.GetOccurrance() == NodeState.BumpZone)
        {
            c.SlowDown();
        }
    }
    /// <summary>
    /// Updates last Transform Gesture pointer position. Switches Elastic pan to End state (Inertia).
    /// /// </summary>
    /// <param name="sender">Gesture Object.</param>
    /// <param name="e"> Event Data.</param>
    /// <returns></returns>
    private void panCompletedHandler(object sender, EventArgs e)
    {
        // disable ongoing tweens so finger has all control over movement
        LeanTween.cancel(gameObject);
        var gesture = sender as TransformGesture;
        c.SetDifference(gesture.DeltaPosition);
        c.SetGestureState(GestureState.End);
        c.SetDirection();
        if (c.attractor &&
            c.GetPositionFromDifference(c.difference) < -c.GetItemPosition(0) &&
            c.GetPositionFromDifference(c.difference) > -c.GetItemPosition(c.itemCount - 1))
        {
            c.AttractTo(c.GetItemFromDifference(c.difference));
            c.SetGestureState(GestureState.Done);
        }
        if (c.GetOccurrance() == NodeState.BumpZone)
        {
            c.BumpBack();
            c.SetDifference(Vector3.zero);
            c.SetGestureState(GestureState.Done);
        }
    }
    /// <summary>
    /// Called every frame.
    /// Main Logic of Menu elastic panning controller.
    /// </summary>
    void Update()
    {
        switch (c.GetGestureState())
        {
            // Manual panning.
            case GestureState.Start:
                c.UpdateParams();
                c.BindDifference();
                c.SetDifference(Vector3.zero);
                break;
            // Inertia.
            case GestureState.End:
                c.UpdateParams();
                // If camera is in free moving area.
                if (c.GetOccurrance() == NodeState.FreeZoneOut
                    || c.GetOccurrance() == NodeState.FreeZoneIn)
                {
                    c.FreeMoveZoneInertia();
                }
                // If camera is inside bumping end.
                else if (c.GetOccurrance() == NodeState.BumpZone && c.ToTriggerBump() && !LeanTween.isTweening(gameObject))
                {
                    c.Bump();
                }
                c.BindDifference();
                if (c.FinishGesture() == true)
                {
                    c.SetGestureState(GestureState.Done);
                    c.SetDifference(Vector3.zero);
                }
                break;
            // Elastic pan process ended.
            case GestureState.Done:
                break;
        }
    }
}
