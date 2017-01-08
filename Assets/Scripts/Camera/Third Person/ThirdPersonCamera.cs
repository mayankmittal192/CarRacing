using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    // Public Variables
    public Transform target = null;
    public float height = 3f;
    public float distance = 3f;
    public float positionDamping = 3f;
    public float velocityDamping = 3f;
    public LayerMask ignoreLayers = -1;

    // Private Variables
    private RaycastHit hit = new RaycastHit();
    private LayerMask raycastLayers = -1;
    private Vector3 previousVelocity = Vector3.zero;
    private Vector3 currentVelocity = Vector3.zero;

    // Cached Components
    private Transform trans;
    private Camera cam;
    private Transform targetTrans;
    private Rigidbody targetBody;


    void Start()
    {
        raycastLayers = ~ignoreLayers;
        trans = GetComponent<Transform>();
        cam = GetComponent<Camera>();
        targetTrans = target.GetComponent<Transform>();
        targetBody = target.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Calculate the target's current velocity by interpolating it with its
        // previous velocity so as to keep things pretty smooth
        currentVelocity = Vector3.Lerp(previousVelocity, targetBody.velocity, velocityDamping * Time.deltaTime);
        currentVelocity.y = 0;
        previousVelocity = currentVelocity;
    }

    void LateUpdate()
    {
        // Calculate speed factor and adjust the camera's distance
        // and field of view according to the target's speed
        float speedFactor = Mathf.Clamp01(targetBody.velocity.magnitude / 70.0f);
        cam.fieldOfView = Mathf.Lerp(55, 75, speedFactor);
        float currentDistance = Mathf.Lerp(7.5f, 6.5f, speedFactor);
        
        // Calculate the target's current velocity direction and
        // adjust the camera's position according to that
        Vector3 currentVelocityDirection = currentVelocity.normalized;
        if (currentVelocity.sqrMagnitude < 0.01f)
        {
            currentVelocityDirection = target.forward.normalized;
        }

        // Calculate the camera's new position and new look direction
        Vector3 newTargetPosition = targetTrans.position + Vector3.up * height;
        Vector3 newPosition = newTargetPosition - (currentVelocityDirection * currentDistance);
        newPosition.y = newTargetPosition.y;

        // Check if the camera is colliding with any object. If it is, then
        // adjust its position so as to avoid the situation of blocked view
        Vector3 targetDirection = newPosition - newTargetPosition;
        if (Physics.Raycast(newTargetPosition, targetDirection, out hit, currentDistance, raycastLayers))
        {
            newPosition = hit.point;
        }

        // Finally set the camera's updated position and look direction
        trans.position = newPosition;
        trans.LookAt(newTargetPosition);
    }
}