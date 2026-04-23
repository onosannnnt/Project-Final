using UnityEngine;

public class BillboardToCamera : MonoBehaviour
{
    public enum BillboardMode
    {
        MatchCameraForward,
        FaceCameraPosition
    }

    [Header("Target Camera")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private bool useMainCameraIfTargetMissing = true;

    [Header("Billboard Settings")]
    [SerializeField] private BillboardMode mode = BillboardMode.MatchCameraForward;
    [SerializeField] private bool updateInLateUpdate = true;
    [SerializeField] private Vector3 rotationOffsetEuler;

    [Header("Axis Lock")]
    [SerializeField] private bool lockX;
    [SerializeField] private bool lockY;
    [SerializeField] private bool lockZ;

    private Vector3 initialEulerAngles;

    private void Awake()
    {
        initialEulerAngles = transform.rotation.eulerAngles;
    }

    private void Update()
    {
        if (!updateInLateUpdate)
        {
            ApplyBillboard();
        }
    }

    private void LateUpdate()
    {
        if (updateInLateUpdate)
        {
            ApplyBillboard();
        }
    }

    [ContextMenu("Apply Billboard Now")]
    public void ApplyBillboard()
    {
        Camera cam = ResolveCamera();
        if (cam == null)
        {
            return;
        }

        Quaternion baseRotation;
        if (mode == BillboardMode.FaceCameraPosition)
        {
            Vector3 lookDirection = transform.position - cam.transform.position;
            if (lookDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            baseRotation = Quaternion.LookRotation(lookDirection, cam.transform.up);
        }
        else
        {
            baseRotation = Quaternion.LookRotation(cam.transform.forward, cam.transform.up);
        }

        Quaternion finalRotation = baseRotation * Quaternion.Euler(rotationOffsetEuler);
        Vector3 finalEulerAngles = finalRotation.eulerAngles;

        if (lockX)
        {
            finalEulerAngles.x = initialEulerAngles.x;
        }

        if (lockY)
        {
            finalEulerAngles.y = initialEulerAngles.y;
        }

        if (lockZ)
        {
            finalEulerAngles.z = initialEulerAngles.z;
        }

        transform.rotation = Quaternion.Euler(finalEulerAngles);
    }

    public void SetTargetCamera(Camera newCamera)
    {
        targetCamera = newCamera;
    }

    public void SetMode(BillboardMode newMode)
    {
        mode = newMode;
    }

    public void SetUpdateInLateUpdate(bool useLateUpdate)
    {
        updateInLateUpdate = useLateUpdate;
    }

    public void SetRotationOffset(Vector3 offsetEuler)
    {
        rotationOffsetEuler = offsetEuler;
    }

    public void SetAxisLock(bool shouldLockX, bool shouldLockY, bool shouldLockZ)
    {
        lockX = shouldLockX;
        lockY = shouldLockY;
        lockZ = shouldLockZ;
    }

    private Camera ResolveCamera()
    {
        if (targetCamera != null)
        {
            return targetCamera;
        }

        if (useMainCameraIfTargetMissing)
        {
            return Camera.main;
        }

        return null;
    }
}
