using UnityEngine;
using Ubiq.Messaging;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Tile : MonoBehaviour 
{
    private class Filter : IXRSelectFilter, IXRHoverFilter
    {
        bool IXRHoverFilter.canProcess => true;
        bool IXRHoverFilter.Process(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
        {
            return Process(interactor); 
        }

        bool IXRSelectFilter.canProcess => true;
        bool IXRSelectFilter.Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
        {
            return Process(interactor);
        }
        
        private bool Process(IXRInteractor interactor)
        {
            // Block the tile from moving if it's already being interacted with (locally).
            if (owner.interactor != null && owner.interactor != interactor)
            {
                return false;
            }
            
            // Block the tile from moving if there are no free slots.
            if (!owner.box.TryGetMovable(owner, out _, out _))
            {
                return false;
            }

            // Prevent grabbing a tile if somebody's head is inside it.
            var avatars = NetworkScene.Find(owner).GetComponentsInChildren<MazeAvatar>();
            var collider = owner.GetComponent<Collider>();
            foreach (MazeAvatar avatar in avatars)
            {
                if (collider.bounds.Contains(avatar.headPosition))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        public Tile owner;
        private bool _canProcess;
    }
    
    public int TileId;

    public Box box;
    public Vector2Int PositionInBox;
    public AudioSource DragSound = null;
    public float maxVolume = 0.2f;
    public float volumeRegulator = 0.4f;

    private Transform follow;
    private HapticImpulsePlayer haptic;
    private Rigidbody body;
    private Vector3 possibleMoveA;
    private Vector3 possibleMoveB;

    private Vector3? lastHandPosition = null;
    
    private XRSimpleInteractable interactable;
    private IXRInteractor interactor;
    private Filter filter;

    public Transform boxTrans()
    {
        return transform.parent;
    }

    void Start()
    {
        filter = new Filter();
        filter.owner = this;
        
        interactable = GetComponent<XRSimpleInteractable>();

        // Get a reference to the box manager.
        // The box manager keeps our game state and is never destroyed.
        box = Box.rootBox;

        // Ask the box manager for our current position.
        // If the game has just loaded, sets our current position.
        box.declarePosition(this);
        
        interactable.hoverFilters.Add(filter);
        interactable.selectFilters.Add(filter);
        interactable.selectEntered.AddListener(Interactable_SelectEntered);
        interactable.selectExited.AddListener(Interactable_SelectExited);
    }

    void OnDestroy()
    {
        if (interactable)
        {
            interactable.selectEntered.RemoveListener(Interactable_SelectEntered);
            interactable.selectExited.RemoveListener(Interactable_SelectExited);
            interactable.selectFilters.Remove(filter);
            interactable.hoverFilters.Remove(filter);
        }
    }

    void Interactable_SelectEntered(SelectEnterEventArgs eventArgs)
    {
        if (follow)
        {
            return;
        }
        
        if (!box.TryGetMovable(this, out possibleMoveA, out possibleMoveB))
        {
            return;
        }

        // Tell the box manager to begin broadcasting.
        box.TileGrabbed();

        // Follow our hand
        Debug.Log("Grasped " + transform.position);
        interactor = eventArgs.interactorObject;
        follow = eventArgs.interactorObject.GetAttachTransform(interactable);
        haptic = eventArgs.interactorObject.transform
            .GetComponentInParent<HapticImpulsePlayer>();
        lastHandPosition = follow.position;
    }
    
    void Interactable_SelectExited(SelectExitEventArgs eventArgs)
    {
        if (!follow)
        {
            return;
        }
        
        // Final short small haptic update
        haptic.SendHapticImpulse(0.1f, 0.1f);

        // Snap the tile in place if it can be moved.
        transform.position = click_to_edge(possibleMoveA, possibleMoveB, transform.position);

        // Reset the possible move points
        lastHandPosition = null;
        interactor = null;
        follow = null;
        haptic = null;
        box.update_tileOccupation(this);
        if (DragSound) DragSound.volume = 0;

        // Tell the box manager to stop broadcasting.
        box.TileReleased();
        Debug.Log("Released " + transform.position);
    }

    private Vector3 move_In_Range(Vector3 start, Vector3 end, Vector3 point)
    {
        //Keep the point alongside the segment of start and end
        Vector3 projection = start + Vector3.Project(point - start, end - start);

        var toStart = (projection - start).sqrMagnitude;
        var toEnd = (projection - end).sqrMagnitude;
        var segment = (start - end).sqrMagnitude;

        if (toStart > segment || toEnd > segment)
        {
            return toStart > toEnd ? end : start;
        }

        return projection;
    }

    private Vector3 click_to_edge(Vector3 start, Vector3 end, Vector3 point)
    {
        //Keep point inside the segment between start and end
        Vector3 projection = start + Vector3.Project(point - start, end - start);

        var toStart = (projection - start).sqrMagnitude;
        var toEnd = (projection - end).sqrMagnitude;

        return toStart > toEnd ? end : start;
    }

    private void playSound(float velocity){
        if(DragSound) { DragSound.volume = Mathf.Min(volumeRegulator * velocity, maxVolume); }
    }

    // Update is called once per frame
    void Update()
    {
        if (follow)
        {
            Vector3 handDelta = follow.position - (Vector3)lastHandPosition;
            Vector3 newHandPos = this.transform.position + handDelta;
            transform.position = move_In_Range(possibleMoveA, possibleMoveB, newHandPos);
            playSound(handDelta.magnitude/Time.deltaTime);
            lastHandPosition = follow.position;
            haptic.SendHapticImpulse(Mathf.Min(handDelta.magnitude/Time.deltaTime, 1.0f), 0.1f);
        }
    }
}
