using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
namespace Ubiq.XR
{
[RequireComponent(typeof(DesktopHand))]
public class TorchOnUseDesktop : MonoBehaviour
{
    public GameObject torch_prefab;
    protected GameObject torch_current = null;
    private HandController controller;

    private DesktopHand hand;

    private void Awake()
    {
        hand = GetComponent<DesktopHand>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if(torch_current == null){
                torch_current = Instantiate(torch_prefab, transform.position, Quaternion.identity);
                torch_current.transform.parent = this.gameObject.transform;
            }
            else{
                Destroy(torch_current);
                torch_current = null;
            }
        }
    }
}
}
