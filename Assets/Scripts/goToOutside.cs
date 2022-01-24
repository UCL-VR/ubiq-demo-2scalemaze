using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Ubiq.XR;
using Ubiq.Rooms;
using Ubiq.Messaging;

public class goToOutside : MonoBehaviour, IGraspable
{
    private RoomClient roomClient;

    private void Start ()
    {
        roomClient = NetworkScene.FindNetworkScene(this).
            GetComponentInChildren<RoomClient>();
    }

    public void Grasp(Hand hand)
    {
        // Inform our box that we are transitioning between scenes,
        // and that the next set of tiles to load will be asking for their original positions.
        Box box = (Box)GameObject.Find("Boxmanager").GetComponent("Box");
        Debug.Log("Scene2 loading: Outside Scene");
        var controller = hand as HandController;
        if (controller)
        {
            controller.Vibrate(1.0f, 0.3f);
        }

        SceneManager.LoadScene("Assets/Scenes/Outside_scene.unity", LoadSceneMode.Single);
        roomClient.Me["maze.world"] = "Outside";
    }

    public void Release(Hand hand)
    {
    }
}
