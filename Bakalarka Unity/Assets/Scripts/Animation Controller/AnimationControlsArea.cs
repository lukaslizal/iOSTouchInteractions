/*
 * @author Lukáš Lízal 2018
 */
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
/// <summary>
/// Controls Speed/Progress/Trigger of animated objects
/// in the scene, based on players position in this area.
/// This area is defined by a BoxCollider attached to the
/// same gameobject.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class AnimationControlsArea : MonoBehaviour
{
    [Tooltip("used for generating a name for animation area")]
    public string animatedObjectName;
    [Tooltip("Player camera reference")]
    public Camera mainCamera;
    [Tooltip("reference to animation(S)")]
    public GameObject animatedObject;
    [Tooltip("animation area")]
    public BoxCollider col;
    [Tooltip("list of animator(s) of controlled object")]
    public Animator[] animators;
    [Tooltip("track animation controls in X or Y direction")]
    public Axis controlsOrientation;
    [Tooltip("reverts the animation progress")]
    public bool revertAnimation;
    [Tooltip("animation starts at ")]
    [Range(0, 1)]
    public float progressOffset = 0f;
    [Tooltip("the lower speed of the two speed points")]
    [Range(0, 1)]
    public float minusSpeed;
    [Tooltip("the higher of the two speed points")]
    [Range(0, 1)]
    public float plusSpeed;
    [Tooltip("default speed of animation playback at start")]
    [Range(0, 5)]
    public float animatorSpeed = 1f;
    [Tooltip("the earlier progress of the two progress points")]
    [Range(0, 1)]
    public float minusProgress;
    [Tooltip("the latter progress of the two progress points")]
    [Range(0, 10)]
    public float plusProgress;
    [Tooltip("current state of progress of controlled animation(s)")]
    [Range(0, 10)]
    public float animatorProgress;
    [Tooltip("animation triggers everytime when entering the area")]
    public bool triggerEverytime;
    [Tooltip("animation starts at min speed")]
    public bool startSpeedAtMin;
    [Tooltip("wheter animation area animation single object or multiple parented objects")]
    public bool singleAnimator;
    private float minus;
    private float plus;
    private float distance;
    private int[] animationStateNames;
    private bool isColliding;
    public bool triggerAnim;
    public bool progressAnim;
    public bool speedAnim;
    private bool firstTimeTrigger;
    /// <summary>
    /// Initialize animation control area
    /// </summary>
    void Start()
    {
        mainCamera = Camera.main;
        if (animatedObject == null)
            Debug.LogWarning("Animator " + this.name + " has no Animation Object assigned");
        if (!Camera.main.gameObject.GetComponent<Collider>())
        {
            var bc = Camera.main.gameObject.AddComponent<BoxCollider>();
            bc.center = new Vector3(0, 0, 33.45f);
            bc.size = new Vector3(0.45f, 0.45f, 800f);
            bc.isTrigger = false;
        }
        if (!Camera.main.gameObject.GetComponent<Rigidbody>())
        {
            var rb = Camera.main.gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.freezeRotation = true;
        }
        if (triggerAnim)
        {
            mainCamera = Camera.main;
            col = GetComponent<BoxCollider>();
            if (animatedObject)
            {
                if(singleAnimator)
                    animators = new Animator[] {animatedObject.GetComponent<Animator>()};
                else
                    animators = animatedObject.GetComponentsInChildren<Animator>();
            }
            animationStateNames = new int[animators.Length];
            for (int i = 0; i < animators.Length; i++)
            {
                animationStateNames[i] = animators[i].GetNextAnimatorStateInfo(0).fullPathHash;
            }
            if (!col)
                Debug.LogWarning("No collider component on custom animation object (AnimationControls).");
            if (animators.Length < 1)
                Debug.LogWarning("No animator component on custom animation object (AnimationControls).");
                for (int i = 0; i < animators.Length; i++)
            {
                animators[i].speed = 0;
            }
            firstTimeTrigger = true;
        }
        else if (progressAnim || speedAnim)
        {
            mainCamera = Camera.main;
            col = GetComponent<BoxCollider>();
            if (animatedObject)
            {
                if(singleAnimator)
                    animators = new Animator[] {animatedObject.GetComponent<Animator>()};
                else
                    animators = animatedObject.GetComponentsInChildren<Animator>();
            }
            animationStateNames = new int[animators.Length];
            for (int i = 0; i < animators.Length; i++)
            {
                animationStateNames[i] = animators[i].GetNextAnimatorStateInfo(0).fullPathHash;
            }
            if (!col)
                Debug.LogWarning("No collider component on custom animation object (AnimationControls). Remove Animation Control Areas if any Menu Items are not animated by animation control area");
            if (animators.Length < 1)
                Debug.LogWarning("No animator component on custom animation object (AnimationControls). Remove Animation Control Areas if any Menu Items are not animated by animation control area");
                if (!revertAnimation)
            {
                animatorProgress = minusProgress;
            }
            else
            {
                animatorProgress = plusProgress;
            }
            if (!startSpeedAtMin)
            {
                animatorSpeed = minusSpeed;
            }
            else
            {
                animatorSpeed = plusSpeed;
            }
            for (int i = 0; i < animators.Length; i++)
            {
                if (progressAnim)
                {
                    animators[i].speed = 0;
                }
                if (speedAnim)
                {
                    if(startSpeedAtMin)
                        animators[i].speed = minusSpeed;
                    else
                        animators[i].speed = plusSpeed;
                }
                animators[i].Play(animationStateNames[i], 0, progressOffset * i + animatorProgress);
            }
        }
        if (col == null)
            Debug.LogWarning("Animator " + this.name + " has no controll box collider object assigned");
        else
            col.isTrigger = true;
    }
    /// <summary>
    /// When is player inside of the area control the animation(s) everyframe new.
    /// </summary>
    void Update()
    {
        if (isColliding && !triggerAnim)
        {
            if (progressAnim)
            {
                for (int i = 0; i < animators.Length; i++)
                {
                    animators[i].Play(animationStateNames[i], 0, progressOffset * i + animatorProgress);
                }
            }
            else
            {
                for (int i = 0; i < animators.Length; i++)
                {
                    animators[i].speed = animatorSpeed;
                }
            }
        }
    }
    /// <summary>
    /// OnCollisionEnter is called when this collider/rigidbody has begun
    /// touching another rigidbody/collider.
    /// </summary>
    /// <param name="other">The Collision data associated with this collision.</param>
    void OnTriggerEnter(Collider other)
    {
        if (triggerAnim && (firstTimeTrigger || triggerEverytime))
        {
            for (int i = 0; i < animators.Length; i++)
            {
                if (triggerEverytime)
                    animators[i].Play(animationStateNames[i], 0, 0);
                animators[i].speed = animatorSpeed;
                // Debug.Log(animators[i].speed);
            }
        }
        else if (progressAnim || speedAnim)
        {
            isColliding = true;
            float player = 0f;
            switch (controlsOrientation)
            {
                case Axis.X:
                    plus = col.bounds.center.x + col.bounds.extents.x;
                    minus = col.bounds.center.x - col.bounds.extents.x;
                    player = mainCamera.transform.position.y;
                    break;
                case Axis.Y:
                    minus = col.bounds.center.y - col.bounds.extents.y;
                    plus = col.bounds.center.y + col.bounds.extents.y;
                    player = mainCamera.transform.position.y;
                    break;
                case Axis.Z:
                    minus = col.bounds.center.z - col.bounds.extents.z;
                    plus = col.bounds.center.z + col.bounds.extents.z;
                    player = mainCamera.transform.position.y;
                    break;
                default:
                    minus = 0;
                    plus = 0;
                    player = 0;
                    break;
            }
            if (revertAnimation)
            {
                var m = minus;
                minus = plus;
                plus = m;
            }
            distance = plus - minus;
            player -= minus;
            animatorProgress = Mathf.Clamp01(player / distance).Remap(0, 1, minusProgress, plusProgress);
            animatorSpeed = Mathf.Clamp01(player / distance).Remap(0, 1, minusSpeed, plusSpeed);
        }
        firstTimeTrigger = false;
    }
    /// <summary>
    /// OnCollisionStay is called once per frame for every collider/rigidbody
    /// that is touching rigidbody/collider.
    /// </summary>
    /// <param name="other">The Collision data associated with this collision.</param>
    void OnTriggerStay(Collider other)
    {
        if (progressAnim || speedAnim)
        {
            float player = 0f;
            switch (controlsOrientation)
            {
                case Axis.X:
                    plus = col.bounds.center.x + col.bounds.extents.x;
                    minus = col.bounds.center.x - col.bounds.extents.x;
                    player = mainCamera.transform.position.y;
                    break;
                case Axis.Y:
                    minus = col.bounds.center.y - col.bounds.extents.y;
                    plus = col.bounds.center.y + col.bounds.extents.y;
                    player = mainCamera.transform.position.y;
                    break;
                case Axis.Z:
                    minus = col.bounds.center.z - col.bounds.extents.z;
                    plus = col.bounds.center.z + col.bounds.extents.z;
                    player = mainCamera.transform.position.y;
                    break;
                default:
                    minus = 0;
                    plus = 0;
                    player = 0;
                    break;
            }
            if (revertAnimation)
            {
                var m = minus;
                minus = plus;
                plus = m;
            }
            distance = plus - minus;
            player -= minus;
            animatorProgress = Mathf.Clamp01(player / distance).Remap(0, 1, minusProgress, plusProgress);
            animatorSpeed = Mathf.Clamp01(player / distance).Remap(0, 1, minusSpeed, plusSpeed);
        }
    }
    /// <summary>
    /// OnCollisionExit is called when this collider/rigidbody has
    /// stopped touching another rigidbody/collider.
    /// </summary>
    /// <param name="other">The Collision data associated with this collision.</param>
    void OnTriggerExit(Collider other)
    {
        isColliding = false;
    }
    public string SetName()
    {
        var areaType = "";
        if (triggerAnim)
            areaType = " Animation Trigger";
        if (progressAnim)
            areaType = " Progress Controller";
        if (speedAnim)
            areaType = " Speed Controller";
        if (animatedObject)
            animatedObjectName = animatedObject.name;
        return this.gameObject.name = animatedObjectName + areaType;
    }
}
