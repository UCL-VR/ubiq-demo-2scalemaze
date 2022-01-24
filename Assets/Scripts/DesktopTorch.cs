using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesktopTorch : MonoBehaviour
{

    public GameObject torch_prefab;
    protected GameObject torch_current = null;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("t"))
        {
            if (torch_current == null)
            {
                torch_current = Instantiate(torch_prefab, transform.position, Quaternion.identity);
                torch_current.transform.parent = this.gameObject.transform;
            }
            else
            {
                Destroy(torch_current);
                torch_current = null;
            }
        }
    }
}
