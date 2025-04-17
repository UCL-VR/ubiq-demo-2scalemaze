using System.Collections;
using System.Collections.Generic;
using Ubiq;
using UnityEngine;
using Ubiq.Voip;
using Ubiq.Messaging;
using Ubiq.Avatars;
using UnityEngine.SceneManagement;
using Ubiq.Samples;

public class MazeAvatar : MonoBehaviour
{
    public bool avatarIsBig { get; private set; }
    public Vector3 headPosition { get; private set; }

    public enum World {
        OutsideWorld,
        InsideWorld,
    }

    // Which world is the local avatar in?
    public static World localWorld = World.OutsideWorld;

    private Transform sameTransform;
    private Transform insideTransform;
    private Transform outsideTransform;

    private Ubiq.Avatars.Avatar avatar;
    private FloatingAvatar floatingAvatar;
    private HeadAndHandsAvatar headAndHandsAvatar;

    private bool ignoringEvents;

    private void Awake()
    {
        avatar = GetComponent<Ubiq.Avatars.Avatar>();
        floatingAvatar = GetComponentInChildren<FloatingAvatar>();
        headAndHandsAvatar = GetComponent<HeadAndHandsAvatar>();
    }

    private void Start()
    {
        // Grab a reference to the necessary transforms
        sameTransform = NetworkScene.Find(this).transform;
        insideTransform = GameObject.Find("InsideOrigin").transform;
        outsideTransform = GameObject.Find("OutsideOrigin").transform;

        // Update the local origin when the local player changes scene
        if(avatar.IsLocal) {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        headAndHandsAvatar.OnHeadUpdate.AddListener(OnHeadUpdate);
        headAndHandsAvatar.OnLeftHandUpdate.AddListener(OnLeftHandUpdate);
        headAndHandsAvatar.OnRightHandUpdate.AddListener(OnRightHandUpdate);
    }

    private void OnDestroy()
    {
        if (headAndHandsAvatar)
        {
            headAndHandsAvatar.OnHeadUpdate.RemoveListener(OnHeadUpdate);
            headAndHandsAvatar.OnLeftHandUpdate.RemoveListener(OnLeftHandUpdate);
            headAndHandsAvatar.OnRightHandUpdate.RemoveListener(OnRightHandUpdate);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // When the local player changes scene, update this static reference
        if (scene.name == "Inside_scene") {
            localWorld = World.InsideWorld;
        } else {
            localWorld = World.OutsideWorld;
        }
    }

    private void OnHeadUpdate(InputVar<Pose> head)
    {
        if (ignoringEvents || !head.valid)
        {
            return;
        }

        var transform = GetWorldTransform();

        var pos = transform.TransformPoint(head.value.position);
        var rot = transform.rotation * head.value.rotation;

        ignoringEvents = true;
        headAndHandsAvatar.OnHeadUpdate.Invoke(
            new InputVar<Pose>(new Pose(pos,rot)));
        ignoringEvents = false;

        floatingAvatar.head.localScale = transform.localScale;
        floatingAvatar.torso.localScale = transform.localScale;

        avatarIsBig = transform == outsideTransform;
        headPosition = pos;
    }

    private void OnLeftHandUpdate(InputVar<Pose> leftHand)
    {
        if (ignoringEvents || !leftHand.valid)
        {
            return;
        }

        var transform = GetWorldTransform();

        var pos = transform.TransformPoint(leftHand.value.position);
        var rot = transform.rotation * leftHand.value.rotation;

        ignoringEvents = true;
        headAndHandsAvatar.OnLeftHandUpdate.Invoke(
            new InputVar<Pose>(new Pose(pos,rot)));
        ignoringEvents = false;

        floatingAvatar.leftHand.localScale = transform.localScale;
    }

    private void OnRightHandUpdate(InputVar<Pose> rightHand)
    {
        if (ignoringEvents || !rightHand.valid)
        {
            return;
        }

        var transform = GetWorldTransform();

        var pos = transform.TransformPoint(rightHand.value.position);
        var rot = transform.rotation * rightHand.value.rotation;

        ignoringEvents = true;
        headAndHandsAvatar.OnRightHandUpdate.Invoke(
            new InputVar<Pose>(new Pose(pos,rot)));
        ignoringEvents = false;

        floatingAvatar.rightHand.localScale = transform.localScale;
    }

    private Transform GetWorldTransform()
    {
        var remoteWorld = avatar.Peer["maze.world"] == "Inside"
            ? World.InsideWorld
            : World.OutsideWorld;

        if (remoteWorld == localWorld)
        {
            // We are in the same world - do nothing
            return sameTransform;
        }
        
        if (remoteWorld == World.InsideWorld)
        {
            // They are inside, we are outside.
            // Use the inside scene transform.
            return insideTransform;
        }
        
        // They are outside, we are inside.
        // Use the inverse of the inside scene transform.
        return outsideTransform;
    }

}
