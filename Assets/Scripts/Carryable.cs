using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Carryable : MonoBehaviour
{
    Rigidbody rb;
    Collider col;
    BoxCollider boxCol;

    Transform holdPoint;
    bool isCarried;

    Vector3 halfExtentsLocal; // cached from BoxCollider size

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        boxCol = col as BoxCollider;

        // Cache size (works even if collider gets disabled while carried)
        if (boxCol != null)
        {
            halfExtentsLocal = boxCol.size * 0.5f;
        }
        else
        {
            // Fallback if not a BoxCollider (shouldn't happen for Unity Cube)
            halfExtentsLocal = Vector3.one * 0.5f;
        }

        // Option A: immobile on ground
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    // World-space half-extents (accounts for scaling)
    public Vector3 HalfExtentsWorld
    {
        get
        {
            var s = transform.lossyScale;
            return new Vector3(
                Mathf.Abs(halfExtentsLocal.x * s.x),
                Mathf.Abs(halfExtentsLocal.y * s.y),
                Mathf.Abs(halfExtentsLocal.z * s.z)
            );
        }
    }

    public void PickUp(Transform newHoldPoint)
    {
        holdPoint = newHoldPoint;
        isCarried = true;

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.detectCollisions = false;

        col.enabled = false;

        // Snap immediately
        transform.position = holdPoint.position;
        transform.rotation = holdPoint.rotation;
    }

    public void Drop(Vector3 dropPos, Quaternion dropRot)
    {
        isCarried = false;
        holdPoint = null;

        transform.SetPositionAndRotation(dropPos, dropRot);

        col.enabled = true;
        rb.detectCollisions = true;

        // Stay immobile (cannot be shoved)
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void LateUpdate()
    {
        if (!isCarried || holdPoint == null) return;

        // Hard lock to hold point (prevents drift)
        transform.position = holdPoint.position;
        transform.rotation = holdPoint.rotation;
    }
}
