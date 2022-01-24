using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SetTransformOnSceneLoaded : MonoBehaviour
{
    public new string tag;

    // called second
    void Start ()
    {
        SetTransform();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetTransform();
    }

    void SetTransform()
    {
        var player = GameObject.FindGameObjectWithTag(tag);
        player.transform.position = transform.position;
        player.transform.rotation = transform.rotation;
    }
}
