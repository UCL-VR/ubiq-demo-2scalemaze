using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.XR;
using Ubiq.Messaging;
using Ubiq.Avatars;
using Ubiq.Dictionaries;

public class Tile : MonoBehaviour, IGraspable //,IUseable
{
    public int TileId;

    public Box box;
    public Vector2Int PositionInBox;
    public AudioSource DragSound = null;
    public float maxVolume = 0.2f;
    public float volumeRegulator = 0.4f;

    private Hand follow = null;
    private Rigidbody body;
    private Vector3[] possibleMovePoints;

    private Vector3? lastHandPosition = null;

    public Transform boxTrans()
    {
        return transform.parent;
    }

    void Start()
    {
        // Get a reference to the box manager.
        // The box manager keeps our game state and is never destroyed.
        box = Box.rootBox;

        // Ask the box manager for our current position.
        // If the game has just loaded, sets our current position.
        box.declarePosition(this);
    }

    void Vibrate(Hand hand, float amplitude, float duration)
    {
        var controller = hand as HandController;
        if (controller)
        {
            controller.Vibrate(amplitude, duration);
        }
    }

    public void Grasp(Hand hand)
    {
        // Block the tile from moving if there are no free slots.
        possibleMovePoints = box.getMovable(this);
        if (possibleMovePoints == null)
        {
            Debug.Log("Tile blocked by other tiles.");
            Vibrate(hand,1.0f, 0.1f);
            return;
        }

        // Prevent grabbing a tile if somebody's head is inisde it.
        MazeAvatar[] avatars = NetworkScene.Find(this).GetComponentsInChildren<MazeAvatar>();
        foreach (MazeAvatar avatar in avatars)
        {
            if (GetComponent<Collider>().bounds.Contains(avatar.headPosition))
            {
                Debug.Log("Tile is blocked - another player's head is inside.");
                Vibrate(hand,1.0f, 0.1f);
                return;
            }
        }

        // Tell the box manager to begin broadcasting.
        box.TileGrabbed();

        // Follow our hand
        Debug.Log("Grasped " + transform.position);
        follow = hand;
        lastHandPosition = follow.transform.position;
    }

    public void Release(Hand hand)
    {


        // Snap the tile in place if it can be moved.
        if (possibleMovePoints != null){
            // Final short small haptic update
            Vibrate(hand,0.1f, 0.1f);
            transform.position = click_to_edge(possibleMovePoints[0], possibleMovePoints[1], transform.position);
        }

        // Reset the possible move points
        possibleMovePoints = null;
        lastHandPosition = null;
        follow = null;
        box.update_tileOccupation(this);
        if (DragSound) DragSound.volume = 0;

        // Tell the box manager to stop broadcasting.
        box.TileReleased();
        Debug.Log("Released " + transform.position);
    }

    private Vector3 move_In_Range(Vector3 start, Vector3 end, Vector3 point)
    {
        //Keep the point alongside the segment of start and end
        Vector3 projection = start + Vector3.Project(point - start, end - start);

        var toStart = (projection - start).sqrMagnitude;
        var toEnd = (projection - end).sqrMagnitude;
        var segment = (start - end).sqrMagnitude;

        if (toStart > segment || toEnd > segment)
        {
            return toStart > toEnd ? end : start;
        }

        return projection;
    }

    private Vector3 click_to_edge(Vector3 start, Vector3 end, Vector3 point)
    {
        //Keep point inside the segment between start and end
        Vector3 projection = start + Vector3.Project(point - start, end - start);

        var toStart = (projection - start).sqrMagnitude;
        var toEnd = (projection - end).sqrMagnitude;

        return toStart > toEnd ? end : start;
    }

    private void playSound(float velocity){
        if(DragSound) { DragSound.volume = Mathf.Min(volumeRegulator * velocity, maxVolume); }
    }

    // Update is called once per frame
    void Update()
    {
        if (follow != null)
        {
            Vector3 handDelta = follow.transform.position - (Vector3)lastHandPosition;
            Vector3 newHandPos = this.transform.position + handDelta;
            transform.position = move_In_Range(possibleMovePoints[0], possibleMovePoints[1], newHandPos);
            playSound(handDelta.magnitude/Time.deltaTime);
            lastHandPosition = follow.transform.position;
            Vibrate(follow,Mathf.Min(handDelta.magnitude/Time.deltaTime, 1.0f), 0.1f);
        }
    }
}
