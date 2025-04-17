using UnityEngine;
using UnityEngine.Rendering;

public class EnableLightOnRender : MonoBehaviour
{
    public Light toDisable;
    
    private new Light light;
    private Camera localCamera;
    
    void Start()
    {
        light = GetComponent<Light>();
        localCamera = GetComponent<Camera>();
        Debug.Assert(localCamera);
        Debug.Assert(light);
        
        RenderPipelineManager.beginCameraRendering += Pipeline_BeginCameraRendering;
        RenderPipelineManager.endCameraRendering += Pipeline_EndCameraRendering;
    }
    
    void OnDestroy()
    {
        RenderPipelineManager.beginCameraRendering -= Pipeline_BeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= Pipeline_EndCameraRendering;
    }

    void Pipeline_BeginCameraRendering(ScriptableRenderContext _, Camera camera)
    {
        // Put the code that you want to execute before the camera renders here
        // If you are using URP or HDRP, Unity calls this method automatically
        // If you are writing a custom SRP, you must call RenderPipeline.BeginCameraRendering
        if (camera == localCamera)
        {
            if (light)
            {
                light.enabled = true;
                toDisable.enabled = false;
            }
        }
    }
    
    void Pipeline_EndCameraRendering(ScriptableRenderContext _, Camera camera)
    {
        if (camera == localCamera)
        {
            if (light)
            {
                light.enabled = false;
                toDisable.enabled = true;
            }
        }
    }
}
