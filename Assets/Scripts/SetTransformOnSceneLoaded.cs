using Unity.XR.CoreUtils;
using UnityEngine;

public class SetTransformOnSceneLoaded : MonoBehaviour
{
    void Start ()
    {
        SetTransform();
    }

    void SetTransform()
    {
        var player = FindAnyObjectByType<XROrigin>();
        player.transform.SetPositionAndRotation(
            transform.position,transform.rotation);
    }
}
