/*
*   @author Lukáš Lízal 2018
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
/// <summary>
/// Part of Menu elastic pan controller.
/// Represents state of menu elastic pan controls.
/// Implements All necessery functions for this type of controls.
/// </summary>
[RequireComponent(typeof(MenuPanHandler))]
public class MenuPanController : MonoBehaviour
{
    [Tooltip("reference to the parent of items of the menu")]
    public GameObject itemContainer;
    [Tooltip("reference to the parent of item animation controllers")]
	public GameObject animationTriggerContainer;
    [Tooltip("latest frame speed of input transform gesture")]
    public float difference;
    [Tooltip("latest frame main direction of pan (Vertical menu might be implemented in future)")]
    public PanOrientation orientation;
    [Tooltip("starting position of a player inside of this menu.")]
    public float start;
    [Tooltip("spacing between items of the menu.")]
    public float intervalSpacing;
    [Range(2, 50)]
    [Tooltip("number of menu items.")]
    public int itemCount;
    [Tooltip("size of Bumpzone length.")]
    public float bumpExtents;
    [Tooltip("previous frame players position")]
    public float prevPlayerPos;
    [Tooltip("latest frame player occurence in menu area type")]
    public NodeState state;
    [Tooltip("@state value in previous frame")]
    public NodeState prevState;
    [Tooltip("latest frame direction (Plus, Minus)")]
    public PanDirection direction;
    [Tooltip("latest frame elastic pan state")]
    public GestureState gestureState;
    [Range(0, 1)]
    [Tooltip("newSpeed = previousFrameSpeed*inertiaCoefficient")]
    public float inertiaCoefficient = 0.67f;
    [Range(0, 2)]
    [Tooltip("maximum pan speed")]
    public float maxPanSpeed = 2f;
    [Tooltip("automatically generate animation controller are for every menu item")]
    public bool autoAnimationAreas;
    [Tooltip("lambda value - when absolute speed of inertia is less, elastic pan stops ")]
    public float finishGestureThreshold;
    [Tooltip("enables magnetic menu items")]
    public bool attractor;
    [Tooltip("menu item being in focus of the controller")]
    public int targetItem;
    [Tooltip("gesture start interupted tween")]
    public bool inputInterruptedTween;
    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        if (itemContainer)
        {
            if (itemContainer.GetComponentsInChildren<Transform>().Length <= 1)
            {
                if(!itemContainer)
                {
                    itemContainer= new GameObject();
                    itemContainer.transform.parent = this.transform;
                    itemContainer.name = "Menu Items";
                }
                var go = new GameObject();
                go.transform.parent = itemContainer.transform;
                Debug.Log("There are no Menu items in Menu Pan Controller.");
            }
            var itemList = itemContainer.transform.GetFirstChildren();
            itemCount = itemList.Length;
        }
        else
        {
            Debug.LogError("There is no items parent container reference at SimplePanController.");
        }
        Vector3 pos;
        if (orientation == PanOrientation.Horizontal)
        {
            pos = new Vector3(-start, 0, transform.position.z);
        }
        else
        {
            pos = new Vector3(0, -start, transform.position.z);
        }
        transform.localPosition = pos;
        targetItem = -1;
    }
    /// <summary>
    /// Set flag, that gesture input start interupted a tween in progress.
    /// </summary>
    /// /// <param name="interupted">button controlls interuption trigger</param>
    public void SetInputInterruptedTween(bool interupted)
    {
        inputInterruptedTween = interupted;
    }
    /// <summary>
    /// Switch focus to next menu item
    /// </summary>
    public void IncreseTargetItem()
    {
        if(targetItem == -1)
            targetItem = GetIndexAt(transform.position.x);
        targetItem = Mathf.Clamp(targetItem+1,0,itemCount-1);
        float duration = 0.7f;
		float point = -GetItemPosition(targetItem);
        LeanTween.cancel(gameObject);
        LeanTween.moveLocalX(gameObject, point, duration)
            .setEase(LeanTweenType.easeOutQuint)
            .setOnComplete(()=>{targetItem = -1;});
    
    }
    /// <summary>
    /// Switch focus to previous menu item
    /// </summary>
    public void DecreaseTargetItem()
    {
        if(targetItem == -1)
            targetItem = GetIndexAt(transform.position.x);
        targetItem = Mathf.Clamp(targetItem-1,0,itemCount-1);
        float duration = 0.7f;
		float point = -GetItemPosition(targetItem);
        LeanTween.cancel(gameObject);
        LeanTween.moveLocalX(gameObject, point, duration)
            .setEase(LeanTweenType.easeOutQuint)
            .setOnComplete(()=>{targetItem = -1;});
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
        return (Mathf.Abs(difference) < 0.01f) && state == NodeState.BumpZone;
    }
    /// <summary>
    /// Executes Inertia movement routine (every frame)
    /// </summary>
    public void FreeMoveZoneInertia()
    {
        difference = difference * (inertiaCoefficient);
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
    /// Sets new speed of pan.
    /// </summary>
    /// <param name="difference">New pan speed.</param>
    public void SetDifference(Vector3 difference)
    {
        if (orientation == PanOrientation.Horizontal)
            this.difference = Mathf.Clamp(difference.x, -maxPanSpeed, maxPanSpeed);
        else
            this.difference = Mathf.Clamp(difference.y, -maxPanSpeed, maxPanSpeed);
    }
    /// <summary>
    /// Recalculates and set current direction of pan.
    /// </summary>
    public void SetDirection()
    {
        if (difference < 0)
        {
            direction = PanDirection.Plus;
        }
        if (difference > 0)
        {
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
    /// Gets position of a item at @index 
    /// </summary>
    /// <param name="index">index of desired item</param>
    /// <returns></returns>
    public float GetItemPosition(int index)
    {
        return start + index * intervalSpacing;
    }
    /// <summary>
    /// Predicts, at which position would current inertial pan routine
    /// ended. 
    /// </summary>
    /// <param name="diff">speed from latest update</param>
    /// <returns></returns>
	public float GetPositionFromDifference(float diff)
	{
		float acc = 0f;
		float originalDiff = diff;
		float pos = 0f;
		if(orientation == PanOrientation.Horizontal)
			pos = transform.localPosition.x;
		else
			pos = transform.localPosition.y;
		while(Mathf.Abs(diff)>finishGestureThreshold)
		{
			acc += diff;
			diff = diff*inertiaCoefficient;
		}
		return pos+acc-originalDiff;
	}
    /// <summary>
    /// Predicts, at which item area would current inertial pan routine
    /// ended. 
    /// </summary>
    /// <param name="diff">speed from latest update</param>
    /// <returns></returns>
	public int GetItemFromDifference(float diff)
	{
		diff = Mathf.Clamp(diff, -maxPanSpeed, maxPanSpeed);
		int index = 0;
		float pos = 0f;
		if(orientation == PanOrientation.Horizontal)
			pos = transform.localPosition.x;
		else
			pos = transform.localPosition.y;
		index = GetIndexAt(GetPositionFromDifference(diff));
		if(index==GetIndexAt(pos) && !inputInterruptedTween)
		{
			if(-pos>GetItemPosition(index))
			{
				if(direction == PanDirection.Plus)
				{
					index++;
				}
			}
			else
			{
				if(direction == PanDirection.Minus)
				{
					index--;
				}
			}
		}
        if(index==GetIndexAt(pos) && inputInterruptedTween)
		{
				if(direction == PanDirection.Plus)
				{
					index++;
				}
				if(direction == PanDirection.Minus)
				{
					index--;
				}
		}
		return index;
	}
    /// <summary>
    /// Retrieves item number at given position
    /// </summary>
    /// <param name="pos">queried position</param>
    /// <returns></returns>
	public int GetIndexAt(float pos)
	{
		float plusBorder = -(start-intervalSpacing/2+intervalSpacing);
		int index = 0;
		while(pos<plusBorder && index < itemCount)
		{
			index++;
			plusBorder -= intervalSpacing;
		}
		return Mathf.Clamp(index,0,itemCount-1);
	}
    /// <summary>
    /// Tweens player to menu item @index
    /// </summary>
    /// <param name="index">index of item to tween to</param>
	public void AttractTo(int index)
	{
        index = Mathf.Clamp(index,0,itemCount-1);
		float duration = 0.7f;
		float point = -GetItemPosition(index);
		if (orientation == PanOrientation.Horizontal)
        {
            LeanTween.cancel(gameObject);
            LeanTween.moveLocalX(gameObject, point, duration)
                .setEase(LeanTweenType.easeOutQuint)
                .setOnComplete(()=>{targetItem = -1;});
        }
        else
        {
            LeanTween.cancel(gameObject);
            LeanTween.moveLocalY(gameObject, point, duration)
                .setEase(LeanTweenType.easeOutQuint)
                .setOnComplete(()=>{targetItem = -1;});
        }
	}
    /// <summary>
    /// Routine for decreasing speed inside BumpZone area.
    /// Which is the space defined as are behind the end of viewd content.
    /// </summary>
    public void SlowDown()
    {
        if (state == NodeState.BumpZone)
        {
            float player;
            float distanceFromEdge = 0f;
            float start = GetItemPosition(0);
            float end = GetItemPosition(itemCount - 1);
			float point = 0f;
            if (orientation == PanOrientation.Horizontal)
            {
                player = -transform.localPosition.x;
            }
            else
            {
                player = -transform.localPosition.y;
            }
            if (player < start)
                point = -start + bumpExtents;
            if (player > end)
                point = -end - bumpExtents;
            distanceFromEdge = point + player;
            if ((direction == PanDirection.Plus && player > end) ||
                (direction == PanDirection.Minus && player < start))
            {
                var normalizedDistance = Math.Abs(distanceFromEdge / bumpExtents);
                if (normalizedDistance > 1f || normalizedDistance < 0.01f)
                {
                    normalizedDistance = 0f;
                }
                difference *= normalizedDistance;
            }
        }
    }
    /// <summary>
    /// Makes changes to player position based on latest
    /// calculated speed
    /// </summary>
    internal void BindDifference()
    {
        if (orientation == PanOrientation.Horizontal)
        {
            prevPlayerPos = -transform.localPosition.x;
            transform.localPosition += new Vector3(difference, 0, 0);
        }
        else
        {
            prevPlayerPos = -transform.localPosition.y;
            transform.localPosition += new Vector3(0, difference, 0);
        }
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
        float from = difference;
        float point = 0f;
        float player = 0f;
        float start = GetItemPosition(0);
        float end = GetItemPosition(itemCount - 1);
        if (orientation == PanOrientation.Horizontal)
        {
            player = -transform.localPosition.x;
        }
        else
        {
            player = -transform.localPosition.y;
        }
        if (player < start)
            point = -start;
        if (player > end)
            point = -end;

        if (orientation == PanOrientation.Horizontal)
        {
            LeanTween.cancel(gameObject);
            LeanTween.value(gameObject, from, 0f, duration1)
                .setOnUpdate((float val) =>
                {
                    difference = val;
                    SlowDown();
                    BindDifference();
                })
                .setEase(LeanTweenType.linear);
            LeanTween.moveLocalX(gameObject, point, duration2)
                .setEase(LeanTweenType.easeOutCubic)
                .setDelay(duration1);
        }
        else
        {
            LeanTween.cancel(gameObject);
            LeanTween.value(gameObject
                , from, 0f, duration1).setOnUpdate((float val) =>
                {
                    difference = val;
                    SlowDown();
                    BindDifference();
                }).setEase(LeanTweenType.linear);
            LeanTween.moveLocalY(gameObject, point, duration2)
                .setEase(LeanTweenType.easeOutCubic)
                .setDelay(duration1);
        }
        difference = 0f;
    }
    /// <summary>
    /// BumpBack routine covers the proces of releasing finger inside of a
    /// BumpZone area. It simulates springy bouncing back movement. 
    /// This process calls one tween to bring player back to the edge of the
    /// Freezone area.
    /// </summary>
    internal void BumpBack()
    {
        float duration = .7f;
        float point = 0f;
        float player = 0f;
        float start = GetItemPosition(0);
        float end = GetItemPosition(itemCount - 1);
        if (orientation == PanOrientation.Horizontal)
        {
            player = -transform.localPosition.x;
        }
        else
        {
            player = -transform.localPosition.y;
        }
        if (player < start)
            point = -start;
        if (player > end)
            point = -end;
        if (orientation == PanOrientation.Horizontal)
        {
            LeanTween.cancel(gameObject);
            LeanTween.moveLocalX(gameObject, point, duration)
                .setEase(LeanTweenType.easeOutQuint);
        }
        else
        {
            LeanTween.cancel(gameObject);
            LeanTween.moveLocalY(gameObject, point, duration)
                .setEase(LeanTweenType.easeOutQuint);
        }
    }
    /// <summary>
    /// Determines wheter inertial ovment stage has decreased
    /// speed below @finishGestureThreshold value (lambda).
    /// </summary>
    /// <returns>True if current speed is lower then @finishGestureThreshold; False otherwise</returns>
    internal bool FinishGesture()
    {
        float player;
        if (orientation == PanOrientation.Horizontal)
        {
            player = -transform.localPosition.x;
        }
        else
        {
            player = -transform.localPosition.y;
        }
        return Mathf.Abs(player - prevPlayerPos) < finishGestureThreshold;
    }
    /// <summary>
    /// Gets area type in which player currently is
    /// in context of corridor map (FreezoneIn, FreezoneOut, BumpZone) 
    /// </summary>
    /// <returns>type of area that player currently occures in</returns>
    internal NodeState GetOccurrance()
    {
        float player = 0f;
        float start = GetItemPosition(0);
        float end = GetItemPosition(itemCount - 1);

        if (orientation == PanOrientation.Horizontal)
        {
            player = -transform.localPosition.x;
        }
        else
        {
            player = -transform.localPosition.y;
        }
        if (player < start || player > end)
            return NodeState.BumpZone;
        else
            return NodeState.FreeZoneOut;
    }
    /// <summary>
    /// Updates data related to navigation through the  menu.
    /// Triggrs functions to be called during special situations:
    /// OnBumpzoneEnter - player entered the end of the menu list area
    /// OnBumpzoneExit - player left the end of the menu list area
    /// </summary>
    internal void UpdateParams()
    {
        float player = 0f;
        float start = GetItemPosition(0);
        float end = GetItemPosition(itemCount - 1);
        if (orientation == PanOrientation.Horizontal)
        {
            player = -transform.localPosition.x;
        }
        else
        {
            player = -transform.localPosition.y;
        }
        if ((player < start && prevPlayerPos >= start)||(player > end && prevPlayerPos <= end))
            OnBumpzoneEnter();
        if ((player > start && prevPlayerPos <= start)||(player < end && prevPlayerPos >= end))
            OnBumpzoneExit();
    }
    /// <summary>
    /// Gets called everytime player enters
    /// the end of the menu list (Bumpzone)
    /// </summary>
    private void OnBumpzoneEnter()
    {
        state = NodeState.BumpZone;
    }
    /// <summary>
    /// Gets called everytime player leaves
    /// the end of the menu list (Bumpzone)
    /// </summary>
    private void OnBumpzoneExit()
    {
        state = NodeState.FreeZoneOut;
    }
}
