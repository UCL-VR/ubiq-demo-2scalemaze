using UnityEngine;
using UnityEngine.SceneManagement;
using Ubiq.Rooms;
using Ubiq.Messaging;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class goToOutside : MonoBehaviour
{
    private RoomClient roomClient;
    private XRSimpleInteractable interactable;

    private void Start ()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        roomClient = NetworkScene.Find(this).
            GetComponentInChildren<RoomClient>();
        
        interactable.selectEntered.AddListener(Interactable_SelectEntered);
    }

    private void OnDestroy()
    {
        interactable.selectEntered.RemoveListener(Interactable_SelectEntered);
    }

    private void Interactable_SelectEntered(SelectEnterEventArgs eventArgs)
    {
        // Drop the object
        eventArgs.manager.SelectExit(eventArgs.interactorObject,interactable);

        // Inform our box that we are transitioning between scenes,
        // and that the next set of tiles to load will be asking for their original positions.
        Debug.Log("Scene2 loading: Outside Scene");
        eventArgs.interactorObject.transform
            .GetComponentInParent<HapticImpulsePlayer>()
            .SendHapticImpulse(1.0f, 0.3f);

        SceneManager.LoadScene("Assets/Scenes/Outside_scene.unity", LoadSceneMode.Single);
        roomClient.Me["maze.world"] = "Outside";
    }
}
