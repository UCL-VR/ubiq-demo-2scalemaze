using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Voip;
using Ubiq.Messaging;
using Ubiq.Avatars;
using UnityEngine.SceneManagement;
using Ubiq.Samples;

[RequireComponent(typeof(Ubiq.Avatars.Avatar))]
[RequireComponent(typeof(FloatingAvatar))]
[RequireComponent(typeof(ThreePointTrackedAvatar))]
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
    private ThreePointTrackedAvatar threePointTrackedAvatar;

    private bool ignoringEvents;

    private void Awake()
    {
        avatar = GetComponent<Ubiq.Avatars.Avatar>();
        floatingAvatar = GetComponent<FloatingAvatar>();
        threePointTrackedAvatar = GetComponent<ThreePointTrackedAvatar>();
    }

    private void Start()
    {
        // Grab a reference to the necessary transforms
        sameTransform = NetworkScene.FindNetworkScene(this).transform;
        insideTransform = GameObject.Find("InsideOrigin").transform;
        outsideTransform = GameObject.Find("OutsideOrigin").transform;

        // Update the local origin when the local player changes scene
        if(avatar.IsLocal) {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        threePointTrackedAvatar.OnHeadUpdate.AddListener(OnHeadUpdate);
        threePointTrackedAvatar.OnLeftHandUpdate.AddListener(OnLeftHandUpdate);
        threePointTrackedAvatar.OnRightHandUpdate.AddListener(OnRightHandUpdate);
    }

    private void OnDestroy()
    {
        if (threePointTrackedAvatar)
        {
            threePointTrackedAvatar.OnHeadUpdate.RemoveListener(OnHeadUpdate);
            threePointTrackedAvatar.OnLeftHandUpdate.RemoveListener(OnLeftHandUpdate);
            threePointTrackedAvatar.OnRightHandUpdate.RemoveListener(OnRightHandUpdate);
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

    private void OnHeadUpdate(Vector3 pos, Quaternion rot)
    {
        if (ignoringEvents)
        {
            return;
        }

        var transform = GetWorldTransform();

        pos = transform.TransformPoint(pos);
        rot = transform.rotation * rot;

        ignoringEvents = true;
        threePointTrackedAvatar.OnHeadUpdate.Invoke(pos,rot);
        ignoringEvents = false;

        floatingAvatar.head.localScale = transform.localScale;
        floatingAvatar.torso.localScale = transform.localScale;

        avatarIsBig = transform == outsideTransform;
        headPosition = pos;
    }

    private void OnLeftHandUpdate(Vector3 pos, Quaternion rot)
    {
        if (ignoringEvents)
        {
            return;
        }

        var transform = GetWorldTransform();

        pos = transform.TransformPoint(pos);
        rot = transform.rotation * rot;

        ignoringEvents = true;
        threePointTrackedAvatar.OnLeftHandUpdate.Invoke(pos,rot);
        ignoringEvents = false;

        floatingAvatar.leftHand.localScale = transform.localScale;
    }

    private void OnRightHandUpdate(Vector3 pos, Quaternion rot)
    {
        if (ignoringEvents)
        {
            return;
        }

        var transform = GetWorldTransform();

        pos = transform.TransformPoint(pos);
        rot = transform.rotation * rot;

        ignoringEvents = true;
        threePointTrackedAvatar.OnRightHandUpdate.Invoke(pos,rot);
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
        else
        {
            if (remoteWorld == World.InsideWorld)
            {
                // They are inside, we are outside.
                // Use the inside scene transform.
                return insideTransform;
            }
            else
            {
                // They are outside, we are inside.
                // Use the inverse of the inside scene transform.
                return outsideTransform;
            }
        }
    }

}
