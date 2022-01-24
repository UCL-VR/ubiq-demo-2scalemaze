using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ubiq.XR
{
public class TorchOnUse : MonoBehaviour
{
    public GameObject torch_prefab;
    protected GameObject torch_current = null;
    private HandController controller;

    private void Awake()
    {
        controller = GetComponent<HandController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        controller.TriggerPress.AddListener(Use);
    }

    void Use(bool state)
    {
        if(torch_current == null){
            torch_current = Instantiate (torch_prefab, transform.position , transform.rotation);
            torch_current.transform.parent = this.gameObject.transform;
            controller.Vibrate(0.3f, 0.2f);
        }
        else{
            Destroy(torch_current);
            torch_current = null;
            controller.Vibrate(0.3f, 0.2f);
        }
    }

    void Update()
    {
        if(torch_current)
        {
            controller.Vibrate(Random.Range(0.0f, 0.3f), 0.1f);
        }
    }
}
}
