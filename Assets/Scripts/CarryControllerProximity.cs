using UnityEngine;
using UnityEngine.InputSystem;

public class CarryControllerProximity : MonoBehaviour
{
    [Header("Refs")]
    public Transform holdPoint;
    public Animator animator;
    public PlayerInput unityPlayerInput;

    [Header("Input System")]
    public string actionMapName = "Player";
    public string interactActionName = "Interact";

    [Header("Pickup")]
    public float pickupRadius = 1.25f;
    public Vector3 pickupOffset = new Vector3(0f, 0.6f, 0f);
    public LayerMask carryableMask = ~0;

    [Header("Drop")]
    public float dropForward = 1.0f;
    public float dropCastStartHeight = 2.0f;
    public float dropCastDistance = 6.0f;
    public LayerMask groundMask = ~0;
    public float groundPadding = 0.02f;

    Carryable carried;
    InputAction interactAction;

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!unityPlayerInput) unityPlayerInput = GetComponent<PlayerInput>();

        interactAction = unityPlayerInput.actions
            .FindActionMap(actionMapName, true)
            .FindAction(interactActionName, true);
    }

    void Update()
    {
        if (interactAction != null && interactAction.WasPressedThisFrame())
        {
            if (carried) Drop();
            else TryPickup();
        }

        if (animator) animator.SetBool("Equipped", carried != null);
    }

    void TryPickup()
    {
        Vector3 center = transform.position + pickupOffset;
        Collider[] hits = Physics.OverlapSphere(center, pickupRadius, carryableMask, QueryTriggerInteraction.Ignore);

        float best = float.PositiveInfinity;
        Carryable bestCarryable = null;

        for (int i = 0; i < hits.Length; i++)
        {
            var c = hits[i].GetComponentInParent<Carryable>();
            if (!c) continue;

            float d = (c.transform.position - transform.position).sqrMagnitude;
            if (d < best)
            {
                best = d;
                bestCarryable = c;
            }
        }

        if (!bestCarryable) return;

        carried = bestCarryable;
        carried.PickUp(holdPoint);
    }

    void Drop()
    {
        Quaternion dropRot = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

        // The BoxCast start position is the CENTER of the box, above + in front of the player.
        Vector3 castStart = transform.position
                          + transform.forward * dropForward
                          + Vector3.up * dropCastStartHeight;

        // Fallback if cast misses (should be rare)
        Vector3 dropCenter = transform.position + transform.forward * dropForward + Vector3.up * 0.75f;

        // Slightly shrink extents so “touching” doesn't count as penetrating
        Vector3 half = carried.HalfExtentsWorld * 0.98f;

        // BoxCast DOWN; hit.distance is how far the BOX CENTER can move before first contact.
        if (Physics.BoxCast(
                castStart,
                half,
                Vector3.down,
                out RaycastHit hit,
                dropRot,
                dropCastDistance,
                groundMask,
                QueryTriggerInteraction.Ignore))
        {
            dropCenter = castStart + Vector3.down * hit.distance;
        }

        // Place by CENTER (not by hit.point)
        Vector3 dropPos = dropCenter + Vector3.up * groundPadding;

        carried.Drop(dropPos, dropRot);
        carried = null;
    }
}
