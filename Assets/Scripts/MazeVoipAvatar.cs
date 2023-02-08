using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Voip;
using Ubiq.Messaging;
using Ubiq.Avatars;

#if UNITY_EDITOR || !UNITY_WEBGL
using Ubiq.Voip.Implementations.Dotnet;
#endif

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
        voipAvatar = GetComponentInChildren<VoipAvatar>();
    }

    private void LateUpdate()
    {
#if UNITY_EDITOR || !UNITY_WEBGL
        if (voipAvatar && voipAvatar.peerConnection != null)
        {
            var pc = voipAvatar.peerConnection;
            var sink = pc.GetComponentInChildren<IDotnetVoipSink>() as AudioSourceDotnetVoipSink;
            if (sink != null)
            {
                var unitySink = sink.unityAudioSource;
                unitySink.dopplerLevel = 0;
                unitySink.minDistance = mazeAvatar.avatarIsBig
                    ? audioFalloffMinDistanceForBigAvatars
                    : audioFalloffMinDistance;
            }
        }
#endif
    }
}
