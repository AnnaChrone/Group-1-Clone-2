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

    [SerializeField] private float startupFollowTime = 2f;

    private float startupTimer = 0f;
    private bool startupFollowing = false;

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

        // Start on home screen view
        SetupHomeScreenView();
    }

    private void Update()
    {
        if (playerTarget == null)
            return;

        // Check if game is active from GameManager
        if (gameManager != null && gameManager.IsGameActive())
        {
            isGameStarted = true;
        }

        // Only move camera if game has started
        if (isGameStarted)
        {
            UpdateGameplayCamera();
        }
        else
        {
            // Keep camera at home screen position - no movement
            // But still look at the target if needed
            if (homeScreenLookTarget != null)
            {
                // Camera stays fixed, no movement
            }
        }
    }

    private void UpdateGameplayCamera()
    {
        // Follow the player during the initial startup period
        if (startupFollowing)
        {
            startupTimer += Time.deltaTime;

            // Calculate target camera position based on player
            Vector3 starttargetPos = new Vector3(
                playerTarget.position.x + cameraOffset.x,
                cameraOffset.y,
                playerTarget.position.z + cameraOffset.z
            );

            // Move camera smoothly
            transform.position = Vector3.Lerp(
                transform.position,
                starttargetPos,
                Time.deltaTime * 8f
            );

            // Keep fixed rotation
            transform.rotation = fixedRotation;

            // Stop startup following after the set amount of time
            if (startupTimer >= startupFollowTime)
            {
                startupFollowing = false;
                currentZ = transform.position.z;
            }

            return;
        }

        // ALWAYS move camera forward
        currentZ += forwardSpeed * Time.deltaTime;

        // Get the player's current Z
        float playerZ = playerTarget.position.z;

        // Calculate target camera Z: player position + offset (which is -5)
        float targetZ = playerZ + maxZDistance;

        // Camera should always try to be at targetZ, but also keep moving forward
        float finalZ = Mathf.Max(currentZ, targetZ);

        // Calculate camera position
        Vector3 targetPos = new Vector3(
            playerTarget.position.x + cameraOffset.x,
            cameraOffset.y,
            finalZ
        );

        // Move camera smoothly
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 8f);

        // Keep fixed rotation
        transform.rotation = fixedRotation;

        // Check if player is too far behind (out of view)
        if (deathManager != null)
        {
            float actualDistanceBehind = transform.position.z - playerTarget.position.z;
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
        startupFollowing = true;
        startupTimer = 0f;

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

    public void StopCamera()
    {
        isGameStarted = false;
    }
}