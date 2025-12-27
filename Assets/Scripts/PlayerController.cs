using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float turnSpeed = 12f; // higher = snappier turning

    [Header("Stability")]
    public float groundStickAcceleration = 30f; // keeps player grounded
    public float maxUpwardVelocity = 0f;        // 0 = no upward physics pop

    [Header("References")]
    public Transform camTransform; // optional (leave empty to use Camera.main)
    public Animator animator;      // optional (auto-finds in children)
    public Transform visual;       // optional (auto-uses animator transform)

    Rigidbody rb;
    Vector3 moveDir;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (!camTransform && Camera.main) camTransform = Camera.main.transform;
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!visual && animator) visual = animator.transform;

        // Rigidbody setup for character-style movement
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.freezeRotation = true;
    }

    void Update()
    {
        // Keyboard input (New Input System)
        float h = 0f;
        float v = 0f;

        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.aKey.isPressed) h -= 1f;
            if (kb.dKey.isPressed) h += 1f;
            if (kb.sKey.isPressed) v -= 1f;
            if (kb.wKey.isPressed) v += 1f;
        }

        Vector3 input = new Vector3(h, 0f, v);
        input = Vector3.ClampMagnitude(input, 1f);

        // Camera-relative movement
        Vector3 forward = camTransform ? camTransform.forward : Vector3.forward;
        Vector3 right   = camTransform ? camTransform.right   : Vector3.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        moveDir = (right * input.x + forward * input.z);

        // Animator speed param (you already have this)
        if (animator) animator.SetFloat("Speed", input.magnitude);
    }

    void FixedUpdate()
    {
        // Keep player stuck to ground
        rb.AddForce(Vector3.down * groundStickAcceleration, ForceMode.Acceleration);

        // Move via Rigidbody
        Vector3 newPos = rb.position + moveDir * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);

        // Hard clamp upward velocity (prevents box collisions from lifting player)
        Vector3 vel = rb.linearVelocity;
        vel.y = Mathf.Min(vel.y, maxUpwardVelocity);
        rb.linearVelocity = vel;

        // Rotate visual toward movement direction
        if (visual && moveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            visual.rotation = Quaternion.Slerp(
                visual.rotation,
                targetRot,
                turnSpeed * Time.fixedDeltaTime
            );
        }
    }
}
