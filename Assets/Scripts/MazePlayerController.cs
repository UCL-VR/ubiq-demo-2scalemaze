using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Ubiq.XR;

[RequireComponent(typeof(XRPlayerController))]
public class MazePlayerController : MonoBehaviour
{
    public float insideMazeFlySpeed = 2.6f;
    public float outsideMazeFlySpeed = 1.2f;

    public GameObject ui;

    private XRPlayerController playerController;

    private void Awake ()
    {
        playerController = GetComponent<XRPlayerController>();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy ()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!playerController)
        {
            return;
        }

        // When the local player changes scene, update this static reference
        if (scene.name == "Inside_scene") {
            playerController.joystickFlySpeed = insideMazeFlySpeed;
            ui.SetActive(false);
        } else {
            playerController.joystickFlySpeed = outsideMazeFlySpeed;
            ui.SetActive(true);
        }
    }
}
