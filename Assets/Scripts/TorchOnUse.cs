using Ubiq;
using Ubiq.Avatars;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Random = UnityEngine.Random;

public class TorchOnUse : MonoBehaviour
{
    public GameObject torch;
    
    public InputActionReference leftSelect;
    public InputActionReference rightSelect;
    
    private HapticImpulsePlayer leftImpulsePlayer;
    private HapticImpulsePlayer rightImpulsePlayer;
    
    private bool isInLeftHand;
    private bool isInRightHand;
    private XRInteractionManager interactionManager;
    private AvatarManager avatarManager;

    // Start is called before the first frame update
    void Start()
    {
        interactionManager = FindAnyObjectByType<XRInteractionManager>(
            FindObjectsInactive.Include);
        avatarManager = FindAnyObjectByType<AvatarManager>();

        var origin = FindAnyObjectByType<XROrigin>();
        var modalityManager = origin
            .GetComponentInChildren<XRInputModalityManager>(includeInactive:true);
        
        leftImpulsePlayer = modalityManager.leftController
            .GetComponentInChildren<HapticImpulsePlayer>(includeInactive:true);
        rightImpulsePlayer = modalityManager.rightController
            .GetComponentInChildren<HapticImpulsePlayer>(includeInactive:true);
    }

    private void LateUpdate()
    {
        if (leftSelect.action.inProgress 
            && !interactionManager.IsHandSelecting(InteractorHandedness.Left))
        {
            if (isInLeftHand)
            {
                leftImpulsePlayer.SendHapticImpulse(Random.Range(0.0f, 0.3f), 0.1f);
            }
            else
            {
                leftImpulsePlayer.SendHapticImpulse(0.3f, 0.2f);
            }
            
            isInLeftHand = true;
            if (avatarManager.input.TryGet(out IHeadAndHandsInput input)
                && input.leftHand.valid)
            {
                torch.SetActive(true);
                torch.transform.SetPositionAndRotation(
                    input.leftHand.value.position,
                    input.leftHand.value.rotation);
            }
            return;
        }
        isInLeftHand = false;
        
        
        if (rightSelect.action.inProgress 
            && !interactionManager.IsHandSelecting(InteractorHandedness.Right))
        {
            if (isInRightHand)
            {
                rightImpulsePlayer.SendHapticImpulse(Random.Range(0.0f, 0.3f), 0.1f);
            }
            else
            {
                rightImpulsePlayer.SendHapticImpulse(0.3f, 0.2f);
            }
            
            isInRightHand = true;
            if (avatarManager.input.TryGet(out IHeadAndHandsInput input)
                && input.rightHand.valid)
            {
                torch.SetActive(true);
                torch.transform.SetPositionAndRotation(
                    input.rightHand.value.position,
                    input.rightHand.value.rotation);
            }
            return;
        }
        isInRightHand = false;
        
        torch.SetActive(false);
    }
}
