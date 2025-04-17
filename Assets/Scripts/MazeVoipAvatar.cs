using System.Collections;
using System.Collections.Generic;
using Ubiq.Avatars;
using UnityEngine;
using Ubiq.Voip;
using Ubiq.Messaging;
using Avatar = Ubiq.Avatars.Avatar;

[RequireComponent(typeof(MazeAvatar))]
public class MazeVoipAvatar : MonoBehaviour
{
    public float audioFalloffMinDistance = 1.0f;
    public float audioFalloffMinDistanceForBigAvatars = 50.0f;

    private Avatar avatar;
    private MazeAvatar mazeAvatar;
    private VoipAvatar voipAvatar;
    private VoipAvatar localVoipAvatar;
    private AvatarManager avatarManager;

    private void Awake()
    {
        avatarManager = GetComponentInParent<AvatarManager>();
        avatar = GetComponent<Avatar>();
        mazeAvatar = GetComponent<MazeAvatar>();
        voipAvatar = GetComponentInChildren<VoipAvatar>();
    }

    private void LateUpdate()
    {
        if (!voipAvatar || !voipAvatar.peerConnection)
        {
            return;
        }
        
        if (!avatarManager || !avatarManager || !avatarManager.LocalAvatar)
        {
            return;
        }
        
        if (!localVoipAvatar)
        {
            if (avatar && avatar.IsLocal)
            {
                localVoipAvatar = voipAvatar;
            }
            else
            {
                localVoipAvatar = avatarManager.LocalAvatar
                    .GetComponentInChildren<VoipAvatar>();
            }
        }

        if (!localVoipAvatar || localVoipAvatar == voipAvatar)
        {
            return;
        }

        var source = voipAvatar.audioSourcePosition;
        var listener = localVoipAvatar.audioSourcePosition;
        var listenerPosition = !mazeAvatar.avatarIsBig
            ? listener.position
            : source.position + 
              (listener.position - source.position) 
              * (1/audioFalloffMinDistanceForBigAvatars);

        voipAvatar.peerConnection.UpdateSpatialization(
            source.position,source.rotation,
            listenerPosition,listener.rotation);
    }
}
