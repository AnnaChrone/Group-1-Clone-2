using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform playerTarget;
    [SerializeField] private float forwardSpeed = 0.5f;

    [Header("Camera Position (Set in Inspector)")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(4f, 8f, -5f);
    [SerializeField] private Vector3 cameraRotation = new Vector3(45f, 45f, 0f);

    [Header("Camera Follow Settings")]
    [SerializeField] private float maxZDistance = -5f;  

    [Header("References")]
    [SerializeField] private PlayerDeathManager deathManager;
    [SerializeField] private GameManager gameManager;

    [Header("Home Screen Settings")]
    [SerializeField] private Transform homeScreenLookTarget;
    [SerializeField] private Vector3 homeScreenOffset = new Vector3(4f, 8f, -5f);
    [SerializeField] private Vector3 homeScreenRotation = new Vector3(45f, 45f, 0f);

    private float currentZ;
    private bool isGameStarted = false;
    private Quaternion fixedRotation;

    private void Start()
    {
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTarget = player.transform;
        }

        fixedRotation = Quaternion.Euler(cameraRotation);
        SetupHomeScreenView();
    }

    private void Update()
    {
        if (!isGameStarted)
            return;

        if (playerTarget == null)
            return;
        currentZ += forwardSpeed * Time.deltaTime;

        float playerZ = playerTarget.position.z;
        float targetZ = playerZ + maxZDistance;

        float finalZ = Mathf.Max(currentZ, targetZ);

        Vector3 targetPos = new Vector3(
            playerTarget.position.x + cameraOffset.x,
            cameraOffset.y,
            finalZ
        );

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 8f);

        transform.rotation = fixedRotation;

        if (deathManager != null)
        {
            float actualDistanceBehind = transform.position.z - playerTarget.position.z;
            // If player is more than 7 steps behind, kill them
            if (actualDistanceBehind > 7f)
            {
                deathManager.TriggerDeath();
            }
        }
    }

    public void SetupHomeScreenView()
    {
        isGameStarted = false;

        if (homeScreenLookTarget != null)
        {
            Vector3 targetPos = homeScreenLookTarget.position + homeScreenOffset;
            transform.position = targetPos;
            transform.rotation = Quaternion.Euler(homeScreenRotation);
            currentZ = targetPos.z;
        }
        else if (playerTarget != null)
        {
            Vector3 targetPos = new Vector3(
                playerTarget.position.x + cameraOffset.x,
                cameraOffset.y,
                playerTarget.position.z + cameraOffset.z
            );
            transform.position = targetPos;
            transform.rotation = fixedRotation;
            currentZ = targetPos.z;
        }
    }

    public void StartGameplayCamera()
    {
        isGameStarted = true;

        if (playerTarget != null)
        {
            Vector3 targetPos = new Vector3(
                playerTarget.position.x + cameraOffset.x,
                cameraOffset.y,
                playerTarget.position.z + cameraOffset.z
            );
            transform.position = targetPos;
            transform.rotation = fixedRotation;
            currentZ = targetPos.z;
        }
    }

    public void ResetCameraPosition()
    {
        if (playerTarget != null)
        {
            Vector3 targetPos = new Vector3(
                playerTarget.position.x + cameraOffset.x,
                cameraOffset.y,
                playerTarget.position.z + cameraOffset.z
            );
            transform.position = targetPos;
            transform.rotation = fixedRotation;
            currentZ = targetPos.z;
        }
    }
}