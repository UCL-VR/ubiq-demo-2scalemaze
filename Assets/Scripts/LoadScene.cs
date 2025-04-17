using Ubiq.Messaging;
using Ubiq.Rooms;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    private void Awake()
    {        
        var roomClient = FindAnyObjectByType<RoomClient>();
        roomClient.Me["maze.world"] = "Outside";
        SceneManager.LoadScene("Assets/Scenes/Outside_scene.unity", LoadSceneMode.Single);
    }
}
