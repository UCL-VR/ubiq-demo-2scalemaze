using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

public class MazePlayerController : MonoBehaviour
{
    public List<ContinuousMoveProvider> moveProviders;
    public float insideMazeFlySpeed = 2.6f;
    public float outsideMazeFlySpeed = 1.2f;

    public GameObject ui;

    private void Start ()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(),LoadSceneMode.Single);
    }

    private void OnDestroy ()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
        // When the local player changes scene, update this static reference
        if (scene.name == "Inside_scene") {
            for (int i = 0; i < moveProviders.Count; i++)
            {
                moveProviders[i].moveSpeed = insideMazeFlySpeed;
            }
            ui.SetActive(false);
        } else {
            for (int i = 0; i < moveProviders.Count; i++)
            {
                moveProviders[i].moveSpeed = outsideMazeFlySpeed;
            }
            ui.SetActive(true);
        }
    }
}
