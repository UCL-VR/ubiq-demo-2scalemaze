using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Voip;
using Ubiq.Messaging;
using Ubiq.Avatars;

[RequireComponent(typeof(MazeAvatar))]
public class MazeVoipAvatar : MonoBehaviour
{
    public float audioFalloffMinDistance = 1.0f;
    public float audioFalloffMinDistanceForBigAvatars = 50.0f;

    private MazeAvatar mazeAvatar;
    private VoipAvatar voipAvatar;

    private void Awake()
    {
        mazeAvatar = GetComponent<MazeAvatar>();
        voipAvatar = GetComponent<VoipAvatar>();
    }

    private void LateUpdate()
    {
        if (voipAvatar && voipAvatar.peerConnection != null)
        {
            var audioSource = voipAvatar.peerConnection.audioSink.unityAudioSource;
            audioSource.dopplerLevel = 0;
            if (mazeAvatar.avatarIsBig)
            {
                audioSource.minDistance = audioFalloffMinDistanceForBigAvatars;
            }
            else
            {
                audioSource.minDistance = audioFalloffMinDistance;
            }
        }
    }
}
