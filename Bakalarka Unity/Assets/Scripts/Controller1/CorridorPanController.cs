/*
*   @author Lukáš Lízal 2018
 */
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// Part of Corridor based elastic pan controller.
/// Represents state of corridor elastic pan controls.
/// Implements All necessery functions for this type of controls.
/// </summary>
[RequireComponent(typeof(CorridorPanHandler))]
public class CorridorPanController : MonoBehaviour
{
    [Tooltip("latest frame speed in main direction")]
    public float differenceOriented;
    [Tooltip("latest frame speed of input transform gesture")]
    public Vector3 difference;
    [Tooltip("camera start when level is loaded")]
    public Node mapStart;
    [Tooltip("represents node we came from when guiding between two nodes")]
    public Node startNode;
    [Tooltip("represents node we are guiding to when guiding between two nodes")]
    public Node destNode;
    [Tooltip("previous frame player position in map coo (just switch signs in world coo)")]
    public Vector3 prevPlayerPos;
    [Tooltip("latest frame player state in relation to position inside corridor map")]
    public NodeState state;
    [Tooltip("@state value in previous frame")]
    public NodeState prevState;
    [Tooltip("latest frame direction (Plus, Minus)")]
    public PanDirection direction;
    [Tooltip("latest frame main direction of pan")]
    public PanOrientation orientation;
    [Tooltip("revious frame main diretion of pan")]
    public PanOrientation prevOrientation;
    [Tooltip("latest frame elastic pan state")]
    public GestureState gestureState;
    [Tooltip("axis of horizontal pan (x) (might custom axis pan later)")]
    public Axis horizontal;
    [Tooltip("axis of vertical pan (y) (might custom axis pan later)")]
    public Axis vertical;
    [Range(0, 1)]
    [Tooltip("newSpeed = previousFrameSpeed*inertiaCoefficient")]
    public float inertiaCoefficient = 0.93f;
    [Tooltip("maximum pan speed")]
    [Range(0, 2)]
    public float maxPanSpeed = 2f;
    [Tooltip("maximum speed in inertia stage")]
    [Range(0, 2)]
    public float maxInertiaSpeed = 2f;
    [Tooltip("lambda value - when absolute speed of inertia is less, elastic pan stops ")]
    public float finishGestureThreshold;
    [Tooltip("exact position of player when he left last crossroad")]
    public Vector3 nodeExitPoint;
    [Tooltip("destination position when guiding between two nodes")]
    public Vector3 guideDestination;
    [Tooltip("player position when player leaves node or when player changes direction between two nodes")]
    public Vector3 guideStart;
    [Tooltip("distance between @guideStart and @guideDestination along axis of main direction")]
    public float guideDistance;
    [Tooltip("history of several last pan orientations")]
    public List<PanOrientation> orientationTrail;
    [Tooltip("@orientationTrail history length")]
    public int trailLength;
    [Tooltip("crossroad visualization game object reference")]
    public GameObject debugVizualizationObject;
    [Tooltip("enable crossroad visualization game object")]
    public bool enableDebugVizualization;
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        if(!mapStart)
            Debug.LogError("There is no mapStart node. Node will be initiated automaticaly");
        else
        {
            startNode = mapStart;
            destNode = mapStart;
            transform.position = -mapStart.transform.localPosition;
        }
        if(!debugVizualizationObject)
        {
            enableDebugVizualization = true;
        }
    }
    /// <summary>
    /// Starts is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        orientationTrail = new List<PanOrientation>();   
        trailLength = 3;
        gestureState = GestureState.Done;
        horizontal = Axis.X;
        vertical = Axis.Y;
    }
    /// <summary>
    /// Checks condition for initiating Bump routine.
    /// </summary>
    /// <returns>return true if conditions for initiating Bump routine are met</returns>
    public bool ToTriggerBump()
    {
        return (prevState == NodeState.FreeZoneIn || prevState == NodeState.FreeZoneOut) && state == NodeState.BumpZone;
    }
    /// <summary>
    /// Checks condition for initiating BumpBack routine.
    /// </summary>
    /// <returns>true if conditions for initiating BumpBack routine are met; false otherwise</returns>
    public bool ToTriggerBumpBack()
    {
        return (Mathf.Abs(differenceOriented) < 0.01f) && state == NodeState.BumpZone;
    }
    /// <summary>
    /// Executes Inertia movement routine (every frame)
    /// </summary>
    public void FreeMoveZoneInertia()
    {
        differenceOriented = differenceOriented * (inertiaCoefficient);
        difference = Vector3.zero;
    }
    /// <summary>
    /// Gets latest frame gesture elastic pan state (Start, End, Done).
    /// </summary>
    /// <returns>@gestureState</returns>
    public GestureState GetGestureState()
    {
        return gestureState;
    }
    /// <summary>
    /// Gets latest frame speed in main direction of pan.
    /// </summary>
    /// <returns>@differenceOriented</returns>
    public float GetDifferenceOriented()
    {
        return differenceOriented;
    }
    /// <summary>
    /// Guiding routine. Adjusts players position to not miss
    /// the node on the other end od the corridor.
    /// </summary>
    public void Guide()
    {
        Axis axis;
        Axis guideAxis;
        float currentDistance;
        float currentPosition;
        float startNodePosition;
        float guide;
        // guideStart = -transform.position;
        if (orientation == PanOrientation.Horizontal)
        {
            axis = horizontal;
            guideAxis = vertical;
        }
        else
        {
            axis = vertical;
            guideAxis = horizontal;
        }
        switch (axis)
        {
            case Axis.X:
                currentDistance = guideDestination.x + transform.position.x;
                currentPosition = -transform.position.x;
                startNodePosition = startNode.transform.localPosition.x;
                break;
            case Axis.Y:
                currentDistance = guideDestination.y + transform.position.y;
                currentPosition = -transform.position.y;
                startNodePosition = startNode.transform.localPosition.y;
                break;
            case Axis.Z:
                currentDistance = guideDestination.z + transform.position.z;
                currentPosition = -transform.position.z;
                startNodePosition = startNode.transform.localPosition.z;
                break;
            default:
                currentDistance = 0f;
                currentPosition = 0f;
                startNodePosition = 0f;
                break;
        }
        switch (startNode.guideType)
        {
            case GuideType.OneDirection:
                if ((differenceOriented < 0 && state == NodeState.FreeZoneOut && direction == PanDirection.Plus && currentPosition > startNodePosition) ||
                    (differenceOriented > 0 && state == NodeState.FreeZoneOut && direction == PanDirection.Minus && currentPosition < startNodePosition))
                {
                    switch (guideAxis)
                    {
                        case Axis.X:
                            guide = -Mathf.Lerp(guideDestination.x, guideStart.x, Mathf.Abs(currentDistance / guideDistance));
                            transform.position = new Vector3(guide, transform.position.y, transform.position.z);
                            break;
                        case Axis.Y:
                            guide = -Mathf.Lerp(guideDestination.y, guideStart.y, Mathf.Abs(currentDistance / guideDistance));
                            transform.position = new Vector3(transform.position.x, guide, transform.position.z);
                            break;
                        case Axis.Z:
                            guide = -Mathf.Lerp(guideDestination.z, guideStart.z, Mathf.Abs(currentDistance / guideDistance));
                            transform.position = new Vector3(transform.position.x, transform.position.y, guide);
                            break;
                        default:
                            break;
                    }
                }
                break;
            case GuideType.BothDirections:
                if (state == NodeState.FreeZoneOut && (currentPosition > startNodePosition || currentPosition < startNodePosition))
                {
                    switch (guideAxis)
                    {
                        case Axis.X:
                            guide = -Mathf.Lerp(guideDestination.x, nodeExitPoint.x, GetProgressFrom(nodeExitPoint.y, guideDestination.y, transform.position.y));
                            transform.position = new Vector3(guide, transform.position.y, transform.position.z);
                            break;
                        case Axis.Y:
                            guide = -Mathf.Lerp(guideDestination.y, nodeExitPoint.y, GetProgressFrom(nodeExitPoint.x, guideDestination.x, transform.position.x));
                            transform.position = new Vector3(transform.position.x, guide, transform.position.z);
                            break;
                        case Axis.Z:
                            guide = -Mathf.Lerp(guideDestination.z, nodeExitPoint.z, GetProgressFrom(nodeExitPoint.z, guideDestination.z, transform.position.z));
                            transform.position = new Vector3(transform.position.x, transform.position.y, guide);
                            break;
                        default:
                            break;
                    }
                }
                break;
            case GuideType.None:
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// Calculates progress in [0,1], where 0 progress
    /// is start position and 1 is the destination position
    /// </summary>
    /// <param name="start">start position</param>
    /// <param name="end">destination position</param>
    /// <param name="point"></param>
    /// <returns>progress value</returns>
    public float GetProgressFrom(float start, float end, float point)
    {
        var distance = Mathf.Abs(end - start);
        var normalizePoint = Mathf.Abs(Mathf.Abs(point) - Mathf.Abs(start));
        if(distance == 0f)
        {
            distance = 0.000001f;
        }
        var distanceNormalizeCoeficient = 1/distance;
        return 1f-Mathf.Clamp01(normalizePoint*distanceNormalizeCoeficient);
    }
    /// <summary>
    /// Resets guiding destination node to a default value of @startNode position
    /// </summary>
    public void ResetGuideDestination()
    {
        guideDestination = startNode.transform.localPosition;
    }
    /// <summary>
    /// Recalculates new guide destination point.
    /// </summary>
    public void SetGuideDestination()
    {
        Axis axis;
        Vector3 extents;
        if (orientation == PanOrientation.Horizontal)
        {
            axis = horizontal;
        }
        else
        {
            axis = vertical;
        }
        if (orientation == PanOrientation.Horizontal)
        {

            switch (axis)
            {
                case Axis.X:
                    extents = new Vector3(destNode.hSize/2f, 0, 0);
                    break;
                case Axis.Y:
                    extents = new Vector3(0, destNode.hSize/2f, 0);
                    break;
                case Axis.Z:
                    extents = new Vector3(0, 0, destNode.hSize/2f);
                    break;
                default:
                    extents = Vector3.zero;
                    break;
            }
            if (direction == PanDirection.Plus)
            {
                guideDestination = destNode.transform.localPosition - extents;
            }
            else if (direction == PanDirection.Minus)
            {
                guideDestination = destNode.transform.localPosition + extents;
            }
        }
        else if (orientation == PanOrientation.Vertical)
        {
            switch (axis)
            {
                case Axis.X:
                    extents = new Vector3(destNode.vSize/2f, 0, 0);
                    break;
                case Axis.Y:
                    extents = new Vector3(0, destNode.vSize/2f, 0);
                    break;
                case Axis.Z:
                    extents = new Vector3(0, 0, destNode.vSize/2f);
                    break;
                default:
                    extents = Vector3.zero;
                    break;
            }
            if (direction == PanDirection.Plus)
            {
                guideDestination = destNode.transform.localPosition - extents;
            }
            else if (direction == PanDirection.Minus)
            {
                guideDestination = destNode.transform.localPosition + extents;
            }
        }
    }
    /// <summary>
    /// Recalculates parameters that are being used for guiding purposes.
    /// </summary>
    public void SetNewGuideParams()
    {
        Axis axis;
        guideStart = -transform.position;
        if (orientation == PanOrientation.Horizontal)
        {
            axis = horizontal;
        }
        else
        {
            axis = vertical;
        }
        switch (axis)
        {
            case Axis.X:
                guideDistance = guideDestination.x - guideStart.x;
                break;
            case Axis.Y:
                guideDistance = guideDestination.y - guideStart.y;
                break;
            case Axis.Z:
                guideDistance = guideDestination.z - guideStart.z;
                break;
        }
    }
    /// <summary>
    /// Sets new speed of pan.
    /// </summary>
    /// <param name="difference">New pan speed.</param>
    public void SetDifference(Vector3 difference)
    {
        this.difference = difference;
    }
    /// <summary>
    /// Clamps given speed to fit inside @maxInertiaSpeed boundaries. 
    /// </summary>
    /// <param name="speed">Speed to clamp</param>
    /// <returns>Speed clamped to @maxInertiaSpeed.</returns>
    public Vector3 ClampInertialSpeed(Vector3 speed)
    {
        return new Vector3(Mathf.Clamp(speed.x, -maxInertiaSpeed, maxInertiaSpeed),
        Mathf.Clamp(speed.y, -maxInertiaSpeed, maxInertiaSpeed),
        Mathf.Clamp(speed.z, -maxInertiaSpeed, maxInertiaSpeed)
        );
    }
    /// <summary>
    /// Sets new @differenceOriented.
    /// </summary>
    /// <param name="diff">new @differenceOriented</param>
    public void SetDifferenceOriented(float diff)
    {
        this.differenceOriented = diff;
    }
    /// <summary>
    /// Sets new current main direction of pan if conditions are met.
    /// </summary>
    public void SetOrientation()
    {
        Vector2 diff = new Vector2(difference.x,difference.y);
        int horizontalCount = 0;
        int verticalCount = 0;
        Axis axis;
        if (state == NodeState.FreeZoneIn || state == NodeState.BumpZone)
        {
            if(orientation == PanOrientation.Horizontal)
            {
                if(Mathf.Abs(Vector2.Dot(diff.normalized, Vector2.right.normalized))>0.5f)
                    orientationTrail.Add(PanOrientation.Horizontal);
                else
                    orientationTrail.Add(PanOrientation.Vertical);
            }
            else if(orientation == PanOrientation.Vertical)
            {
                if(Mathf.Abs(Vector2.Dot(diff.normalized, Vector2.up.normalized))>0.5f)
                    orientationTrail.Add(PanOrientation.Vertical);
                else
                    orientationTrail.Add(PanOrientation.Horizontal);
            }

            if(orientationTrail.Count > trailLength)
            {
                orientationTrail.RemoveAt(0);
            }

            for(int i = 0; i<orientationTrail.Count;i++)
            {
                if(orientationTrail[i]==PanOrientation.Horizontal)
                    horizontalCount += 1;
                else if(orientationTrail[i]==PanOrientation.Vertical)
                    verticalCount += 1;
            }

            if(orientation == PanOrientation.Horizontal)
            {
                if(horizontalCount == 0 && (startNode.vMinus != null || startNode.vPlus != null))
                {
                    prevOrientation = orientation;
                    orientation = PanOrientation.Vertical;
                }
            }
            else if(orientation == PanOrientation.Vertical)
            {
                if(verticalCount == 0 && (startNode.hMinus != null || startNode.hPlus != null))
                {
                    prevOrientation = orientation;
                    orientation = PanOrientation.Horizontal;
                }
            }
        }
        if (orientation == PanOrientation.Horizontal)
        {
            axis = horizontal;
        }
        else
        {
            axis = vertical;
        }
        switch (axis)
        {
            case Axis.X:
                differenceOriented = difference.x;
                break;
            case Axis.Y:
                differenceOriented = difference.y;
                break;
            case Axis.Z:
                differenceOriented = difference.z;
                break;
            default:
                differenceOriented = 0f;
                break;
        }
        differenceOriented = Mathf.Clamp(differenceOriented, -maxPanSpeed, maxPanSpeed);
    }
    /// <summary>
    /// Recalculates and set current direction of pan.
    /// </summary>
    public void SetDirection()
    {
        Axis axis;
        float currentPosition;
        float startNodePosition;
        if (orientation == PanOrientation.Horizontal)
        {
            axis = horizontal;
        }
        else
        {
            axis = vertical;
        }
        switch (axis)
        {
            case Axis.X:
                currentPosition = -transform.position.x;
                startNodePosition = startNode.transform.localPosition.x;
                break;
            case Axis.Y:
                currentPosition = -transform.position.y;
                startNodePosition = startNode.transform.localPosition.y;
                break;
            case Axis.Z:
                currentPosition = -transform.position.z;
                startNodePosition = startNode.transform.localPosition.z;
                break;
            default:
                currentPosition = 0f;
                startNodePosition = 0f;
                break;
        }
        if (differenceOriented < 0)
        {
            if (state == NodeState.FreeZoneOut && direction != PanDirection.Plus && currentPosition < startNodePosition)
            {
                SetNewGuideParams();
            }
            direction = PanDirection.Plus;
        }
        if (differenceOriented > 0)
        {
            if (state == NodeState.FreeZoneOut && direction != PanDirection.Minus && currentPosition > startNodePosition)
            {
                SetNewGuideParams();
            }
            direction = PanDirection.Minus;
        }
    }
    /// <summary>
    /// Sets current stage of elastic pan.
    /// </summary>
    /// <param name="gesture">new elastic pan stage</param>
    public void SetGestureState(GestureState gesture)
    {
        gestureState = gesture;
    }
    /// <summary>
    /// Routine for decreasing speed inside BumpZone area.
    /// Which is the space defined as are behind the end of viewd content.
    /// </summary>
    public void SlowDown()
    {
        var extentsDirection = 0f;
        var distanceFromEdgeDirectionPlus = 0f;
        var distanceFromEdgeDirectionMinus = 0f;
        Node Minus = null;
        Node Plus = null;
        Axis axis;
        if (orientation == PanOrientation.Horizontal)
        {
            axis = horizontal;
            extentsDirection = ((float)startNode.hSize) / 2;
            Minus = startNode.hMinus;
            Plus = startNode.hPlus;
        }
        else
        {
            axis = vertical;
            extentsDirection = ((float)startNode.vSize) / 2;
            Minus = startNode.vMinus;
            Plus = startNode.vPlus;
        }
        switch (axis)
        {
            case Axis.X:
                distanceFromEdgeDirectionPlus = startNode.transform.localPosition.x + extentsDirection + transform.position.x;
                distanceFromEdgeDirectionMinus = startNode.transform.localPosition.x - extentsDirection + transform.position.x;
                break;
            case Axis.Y:
                distanceFromEdgeDirectionPlus = startNode.transform.localPosition.y + extentsDirection + transform.position.y;
                distanceFromEdgeDirectionMinus = startNode.transform.localPosition.y - extentsDirection + transform.position.y;
                break;
            case Axis.Z:
                distanceFromEdgeDirectionPlus = startNode.transform.localPosition.z + extentsDirection + transform.position.z;
                distanceFromEdgeDirectionMinus = startNode.transform.localPosition.z - extentsDirection + transform.position.z;
                break;
        }
        // difference.x < 0 for not slowing down motion when moving in direction back to freezone
        if (Plus == null && differenceOriented <= 0f)
        {
            var normalizedDistance = Math.Abs(distanceFromEdgeDirectionPlus / extentsDirection);

            //sometimes when swiping furiously back and forward normalizedDistance can get out of range <0,1> into high very numbers so we need to clip these cases
            if (normalizedDistance > 1f || normalizedDistance < 0.01f)
            {
                normalizedDistance = 0f;
            }

            differenceOriented *= normalizedDistance;
        }
        if (Minus == null && differenceOriented >= 0f)
        {
            var normalizedDistance = Math.Abs(distanceFromEdgeDirectionMinus / extentsDirection);
            //sometimes when swiping furiously back and forward normalizedDistance can get out of range <0,1> into high very numbers so we need to clip these cases
            if (normalizedDistance > 1f || normalizedDistance < 0.01f)
            {
                normalizedDistance = 0f;
            }
            differenceOriented *= normalizedDistance;
        }
    }
    /// <summary>
    /// Makes changes to player position based on latest
    /// calculated speed
    /// </summary>
    public void BindDifference()
    {
        Vector3 diff = Vector3.zero;
        Axis axis;
        if (orientation == PanOrientation.Horizontal)
        {
            axis = horizontal;
        }
        else
        {
            axis = vertical;
        }
        switch (axis)
        {
            case Axis.X:
                diff = new Vector3(differenceOriented, 0f, 0f);
                break;
            case Axis.Y:
                diff = new Vector3(0f, differenceOriented, 0f);
                break;
            case Axis.Z:
                diff = new Vector3(0f, 0f, differenceOriented);
                break;
        }
        prevPlayerPos = transform.position;
        transform.position += diff;
    }
    /// <summary>
    /// Bump routine covers the proces of entering BumpZone area
    /// when in inertial movement state. It simulates springy bouncing back
    /// movement. This process calls two tweens. first tween slows movment down
    /// second tween function then brings player back to the edge of the
    /// Freezone  area.
    /// </summary>
    public void Bump()
    {
        float duration1 = .1f;
        float duration2 = .7f;
        float from = differenceOriented;
        switch (orientation)
        {
            case PanOrientation.Horizontal:
                LeanTween.cancel(gameObject);
                switch (horizontal)
                {
                    case Axis.X:
                        LeanTween.value(gameObject, from, 0f, duration1)
                        .setOnUpdate((float val) =>
                        {
                            differenceOriented = val;
                            SlowDown();
                            BindDifference();
                        })
                        .setEase(LeanTweenType.linear);
                        //speed of tween is higher when speed of inertia was higher..
                        LeanTween.moveX(gameObject, -startNode.transform.localPosition.x, duration2) 
                        .setEase(LeanTweenType.easeOutCubic)
                        .setDelay(duration1);
                        break;
                    case Axis.Y:
                        LeanTween.value(gameObject, from, 0f, duration1)
                        .setOnUpdate((float val) =>
                        {
                            differenceOriented = val;
                            SlowDown();
                            BindDifference();
                        })
                        .setEase(LeanTweenType.linear);
                        //speed of tween is higher when speed of inertia was higher..
                        LeanTween.moveY(gameObject, -startNode.transform.localPosition.y, duration2)
                        .setEase(LeanTweenType.easeOutCubic)
                        .setDelay(duration1);
                        break;
                    case Axis.Z:
                        LeanTween.value(gameObject, from, 0f, duration1)
                        .setOnUpdate((float val) =>
                        {
                            differenceOriented = val;
                            SlowDown();
                            BindDifference();
                        })
                        .setEase(LeanTweenType.linear);
                        //speed of tween is higher when speed of inertia was higher..
                        LeanTween.moveZ(gameObject, -startNode.transform.localPosition.z, duration2)
                        .setEase(LeanTweenType.easeOutCubic)
                        .setDelay(duration1);
                        break;
                }
                break;
            case PanOrientation.Vertical:
                LeanTween.cancel(gameObject);
                switch (vertical)
                {
                    case Axis.X:
                        LeanTween.value(gameObject, from, 0f, duration1).setOnUpdate((float val) =>
                        {
                            differenceOriented = val;
                            SlowDown();
                            BindDifference();
                        })
                        .setEase(LeanTweenType.linear);
                        //speed of tween is higher when speed of inertia was higher..
                        LeanTween.moveX(gameObject, -startNode.transform.localPosition.x, duration2)
                        .setEase(LeanTweenType.easeOutCubic)
                        .setDelay(duration1);
                        break;
                    case Axis.Y:
                        LeanTween.value(gameObject, from, 0f, duration1).setOnUpdate((float val) =>
                        {
                            differenceOriented = val;
                            SlowDown();
                            BindDifference();
                        })
                        .setEase(LeanTweenType.linear);
                        //speed of tween is higher when speed of inertia was higher..
                        LeanTween.moveY(gameObject, -startNode.transform.localPosition.y, duration2)
                        .setEase(LeanTweenType.easeOutCubic)
                        .setDelay(duration1);
                        break;
                    case Axis.Z:
                        LeanTween.value(gameObject, from, 0f, duration1)
                        .setOnUpdate((float val) =>
                        {
                            differenceOriented = val;
                            SlowDown();
                            BindDifference();
                        })
                        .setEase(LeanTweenType.linear);
                        //speed of tween is higher when speed of inertia was higher..
                        LeanTween.moveZ(gameObject, -startNode.transform.localPosition.z, duration2)
                        .setEase(LeanTweenType.easeOutCubic)
                        .setDelay(duration1);
                        break;
                }
                break;
        }        
        differenceOriented = 0f;
    }
    /// <summary>
    /// BumpBack routine covers the proces of releasing finger inside of a
    /// BumpZone area. It simulates springy bouncing back movement. 
    /// This process calls one tween to bring player back to the edge of the
    /// Freezone area.
    /// </summary>
    public void BumpBack()
    {
        float duration = .7f;
        switch (orientation)
        {
            case PanOrientation.Horizontal:
                LeanTween.cancel(gameObject);
                switch (horizontal)
                {
                    case Axis.X:
                        LeanTween.moveX(gameObject, -startNode.transform.localPosition.x, duration)
                        .setEase(LeanTweenType.easeOutQuint);
                        break;
                    case Axis.Y:
                        LeanTween.moveY(gameObject, -startNode.transform.localPosition.y, duration)
                        .setEase(LeanTweenType.easeOutQuint);
                        break;
                    case Axis.Z:
                        LeanTween.moveZ(gameObject, -startNode.transform.localPosition.z, duration)
                        .setEase(LeanTweenType.easeOutQuint);
                        break;
                }
                break;
            case PanOrientation.Vertical:
                LeanTween.cancel(gameObject);
                switch (vertical)
                {
                    case Axis.X:
                        LeanTween.moveX(gameObject, -startNode.transform.localPosition.x, duration)
                        .setEase(LeanTweenType.easeOutQuint);
                        break;
                    case Axis.Y:
                        LeanTween.moveY(gameObject, -startNode.transform.localPosition.y, duration)
                        .setEase(LeanTweenType.easeOutQuint);
                        break;
                    case Axis.Z:
                        LeanTween.moveZ(gameObject, -startNode.transform.localPosition.z, duration)
                        .setEase(LeanTweenType.easeOutQuint);
                        break;
                }
                break;
        }
    }
    /// <summary>
    /// Determines wheter inertial ovment stage has decreased
    /// speed below @finishGestureThreshold value (lambda).
    /// </summary>
    /// <returns>True if current speed is lower then @finishGestureThreshold; False otherwise</returns>
    public bool FinishGesture()
    {
        float player = 0f;
        float prevPlayer = 0f;
        Axis axis;
        if (orientation == PanOrientation.Horizontal)
        {
            axis = horizontal;
        }
        else
        {
            axis = vertical;
        }
        switch (axis)
        {
            case Axis.X:
                player = transform.position.x;
                prevPlayer = prevPlayerPos.x;
                break;
            case Axis.Y:
                player = transform.position.y;
                prevPlayer = prevPlayerPos.y;
                break;
            case Axis.Z:
                player = transform.position.z;
                prevPlayer = prevPlayerPos.z;
                break;
        }
        return Mathf.Abs(player - prevPlayer) < finishGestureThreshold;
    }
    /// <summary>
    /// Gets area type in which player currently is
    /// in context of corridor map (FreezoneIn, FreezoneOut, BumpZone) 
    /// </summary>
    /// <returns>@state</returns>
    public NodeState GetOccurrance()
    {
        return state;
    }
    /// <summary>
    /// Checks if start node has at least one neighbour.
    /// </summary>
    /// <returns>True if start node has at least one neighbout. False otherwise. </returns>
    public bool FunctionalNode()
    {
        return startNode.hPlus || startNode.hMinus || startNode.vPlus || startNode.vMinus;
    }
    /// <summary>
    /// Updates data related to navigation trough the corridor map.
    /// </summary>
    public void UpdateParamsLate(){
        float player = 0f;
        float start = 0f;
        float startSize = 0f;
        Node plus;
        Node minus;
        Axis axis;
        if (orientation == PanOrientation.Horizontal)
        {
            startSize = startNode.hSize;
            axis = horizontal;
            plus = startNode.hPlus;
            minus = startNode.hMinus;
        }
        else
        {
            startSize = startNode.vSize;
            axis = vertical;
            plus = startNode.vPlus;
            minus = startNode.vMinus;
        }
        switch (axis)
        {
            case Axis.X:
                player = -transform.position.x;
                start = startNode.transform.localPosition.x;
                break;
            case Axis.Y:
                player = -transform.position.y;
                start = startNode.transform.localPosition.y;
                break;
            case Axis.Z:
                player = -transform.position.z;
                start = startNode.transform.localPosition.z;
                break;
        }
        if (player < start + startSize / 2f && player > start - startSize / 2f)
        {
            if(player < start)
            {
                if(minus != null)
                    state = NodeState.FreeZoneIn;
                else
                    state = NodeState.BumpZone;
            }
            if(player > start)
            {
                if(plus != null)
                    state = NodeState.FreeZoneIn;
                else
                    state = NodeState.BumpZone;
            }
        }
        if(prevOrientation != orientation)
        {
            ResetStartDest(startNode);
        }
    }
    /// <summary>
    /// Updates data related to navigation through the corridor map.
    /// Triggers functions to be called during special situations:
    /// OnNodeEnter - player entered a crossroad area
    /// OnNodeExit - player left current crossroad area
    /// OnPlusEnter - player crossed the center of a crossroad to the Plus half
    /// OnMinusExit - player crossed the center of a crossroad to the Minus half
    /// </summary>
    public void UpdateParams()
    {
        float player = 0f;
        float prevPlayer = 0f;
        float start = 0f;
        float dest = 0f;
        float startSize = 0f;
        float destSize = 0f;
        Axis axis;
        if (orientation == PanOrientation.Horizontal)
        {
            startSize = startNode.hSize;
            destSize = destNode.hSize;
            axis = horizontal;
        }
        else
        {
            startSize = startNode.vSize;
            destSize = destNode.vSize;
            axis = vertical;
        }
        switch (axis)
        {
            case Axis.X:
                player = -transform.position.x;
                prevPlayer = -prevPlayerPos.x;
                start = startNode.transform.localPosition.x;
                dest = destNode.transform.localPosition.x;
                break;
            case Axis.Y:
                player = -transform.position.y;
                prevPlayer = -prevPlayerPos.y;
                start = startNode.transform.localPosition.y;
                dest = destNode.transform.localPosition.y;
                break;
            case Axis.Z:
                player = -transform.position.z;
                prevPlayer = -prevPlayerPos.z;
                start = startNode.transform.localPosition.z;
                dest = destNode.transform.localPosition.z;
                break;
        }
        if (player < dest + destSize / 2f && player >= dest - destSize / 2f)
        {
            if (!(prevPlayer < dest + destSize / 2f && prevPlayer >= dest - destSize / 2f))
            {
                ResetStartDest(destNode);
                if (orientation == PanOrientation.Horizontal)
                {
                    startSize = startNode.hSize;
                    destSize = destNode.hSize;
                    axis = horizontal;
                }
                else
                {
                    startSize = startNode.vSize;
                    destSize = destNode.vSize;
                    axis = vertical;
                }

                switch (axis)
                {
                    case Axis.X:
                        player = -transform.position.x;
                        prevPlayer = -prevPlayerPos.x;
                        start = startNode.transform.localPosition.x;
                        dest = destNode.transform.localPosition.x;
                        break;
                    case Axis.Y:
                        player = -transform.position.y;
                        prevPlayer = -prevPlayerPos.y;
                        start = startNode.transform.localPosition.y;
                        dest = destNode.transform.localPosition.y;
                        break;
                    case Axis.Z:
                        player = -transform.position.z;
                        prevPlayer = -prevPlayerPos.z;
                        start = startNode.transform.localPosition.z;
                        dest = destNode.transform.localPosition.z;
                        break;
                }
            }
        }
        if (player < start + startSize / 2f && player > start - startSize / 2f)
        {
            if (!(prevPlayer < start + startSize / 2f && prevPlayer >= start - startSize / 2f))
            {
                OnNodeEnter();
            }
        }
        else
        {
            if (prevPlayer < start + startSize / 2f && prevPlayer >= start - startSize / 2f)
            {
                OnNodeExit();
            }
        }

        if (player > start && prevPlayer <= start)
        {
            OnPlusEnter();
        }
        if (player < start && prevPlayer >= start)
        {
            OnMinusEnter();
        }
    }
    /// <summary>
    /// Sets new startpoint and destination nodes parameters for
    /// purposes of guiding between two nodes, when leaving area
    /// of a crossroad zone.
    /// </summary>
    /// <param name="active"></param>
    public void ResetStartDest(Node active)
    {
        float player = 0f;
        float start = 0f;
        Node minus = null;
        Node plus = null;
        Axis axis;
        if (orientation == PanOrientation.Horizontal)
        {
            minus = active.hMinus;
            plus = active.hPlus;
            axis = horizontal;
        }
        else
        {
            minus = active.vMinus;
            plus = active.vPlus;
            axis = vertical;
        }
        switch (axis)
        {
            case Axis.X:
                player = -transform.position.x;
                start = active.transform.localPosition.x;
                break;
            case Axis.Y:
                player = -transform.position.y;
                start = active.transform.localPosition.y;
                break;
            case Axis.Z:
                player = -transform.position.z;
                start = active.transform.localPosition.z;
                break;
        }
        startNode = active;
        if (player > start)
        {
            destNode = plus;
            if (destNode == null)
                destNode = startNode;
        }
        else
        {
            destNode = minus;
            if (destNode == null)
                destNode = startNode;
        }
    }
    /// <summary>
    /// Gets called everytime player enters
    /// minus side of a node.
    /// </summary>
    private void OnMinusEnter()
    {
        Node minusNode = null;
        ResetStartDest(startNode);
        switch (orientation)
        {
            case PanOrientation.Horizontal:
                minusNode = startNode.hMinus;
                break;
            case PanOrientation.Vertical:
                minusNode = startNode.vMinus;
                break;
        }
        if (minusNode == null)
        {
            state = NodeState.BumpZone;
        }
        else
        {
            state = NodeState.FreeZoneIn;
        }
    }
    /// <summary>
    /// Gets called everytime player enters
    /// plus side of a node.
    /// </summary>
    private void OnPlusEnter()
    {
        Node plusNode = null;
        ResetStartDest(startNode);
        switch (orientation)
        {
            case PanOrientation.Horizontal:
                plusNode = startNode.hPlus;
                break;
            case PanOrientation.Vertical:
                plusNode = startNode.vPlus;
                break;
        }
        if (plusNode == null)
        {
            state = NodeState.BumpZone;
        }
        else
        {
            state = NodeState.FreeZoneIn;
        }
    }
    /// <summary>
    /// Called everytime player leaves area of a crossroad
    /// </summary>
    private void OnNodeExit()
    {
        state = NodeState.FreeZoneOut;
        nodeExitPoint = -transform.position;
        SetGuideDestination();
        SetNewGuideParams();
        startNode.HideNavigation();
    }
    /// <summary>
    /// Called everytime player enters a crossroad area.
    /// </summary>
    private void OnNodeEnter()
    {
        state = NodeState.FreeZoneIn;
        ResetGuideDestination();
        startNode.RevealNavigation();
    }
}
