using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerDeathManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Transform playerCharacter;
    [SerializeField] private string deathLayerName = "Obstacle";
    [SerializeField] private string waterLayerName = "Water";

    [Header("Death Effects")]
    [SerializeField] private float sinkSpeed = 2f;
    [SerializeField] private float sinkDuration = 1f;

    private bool isDead = false;
    private bool isSinking = false;
    private bool isOnRidable = false;

    private void Start()
    {
        gameOverPanel.SetActive(false);
        isDead = false;
        isSinking = false;
        isOnRidable = false;
    }

    private void Update()
    {
        if (isDead || isSinking)
            return;

        if (gameManager != null && gameManager.IsGameActive())
        {
            CheckOutOfView();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead || isSinking) return;

        // Check for ridable object (log)
        if (other.GetComponent<Ridable>() != null)
        {
            isOnRidable = true;
            return;
        }

        // Check for obstacle death
        if (other.gameObject.layer == LayerMask.NameToLayer(deathLayerName))
        {
            Die();
        }

        // Check for water/river death - ONLY if NOT on a ridable
        if (other.gameObject.layer == LayerMask.NameToLayer(waterLayerName) && !isOnRidable)
        {
            StartCoroutine(SinkAndDie());
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isDead || isSinking) return;

        // Keep checking if we're on a ridable
        if (other.GetComponent<Ridable>() != null)
        {
            isOnRidable = true;
            return;
        }

        // Check for water/river death - ONLY if NOT on a ridable
        if (other.gameObject.layer == LayerMask.NameToLayer(waterLayerName) && !isOnRidable)
        {
            StartCoroutine(SinkAndDie());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if we're still on the log
        if (other.GetComponent<Ridable>() != null)
        {
            StartCoroutine(DelayedRidableCheck());
        }
    }

    private IEnumerator DelayedRidableCheck()
    {
        yield return null; // Wait one frame

        isOnRidable = false;

        // Check all colliders currently touching the player
        Collider[] colliders = Physics.OverlapSphere(playerCharacter.position, 0.5f);
        foreach (Collider col in colliders)
        {
            if (col.GetComponent<Ridable>() != null)
            {
                isOnRidable = true;
                break;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDead || isSinking) return;

        // Check for ridable object (log)
        if (collision.gameObject.GetComponent<Ridable>() != null)
        {
            isOnRidable = true;
            return;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer(deathLayerName))
        {
            Die();
        }

        // Check for water/river death - ONLY if NOT on a ridable
        if (collision.gameObject.layer == LayerMask.NameToLayer(waterLayerName) && !isOnRidable)
        {
            StartCoroutine(SinkAndDie());
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (isDead || isSinking) return;

        // Keep checking if we're on a ridable
        if (collision.gameObject.GetComponent<Ridable>() != null)
        {
            isOnRidable = true;
            return;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer(waterLayerName) && !isOnRidable)
        {
            StartCoroutine(SinkAndDie());
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.GetComponent<Ridable>() != null)
        {
            StartCoroutine(DelayedRidableCheck());
        }
    }

    private void CheckOutOfView()
    {
        if (playerCharacter == null)
            return;

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Vector3 viewportPos = mainCamera.WorldToViewportPoint(playerCharacter.position);

            // Kill immediately if player is not in view
            if (viewportPos.z < 0 ||
                viewportPos.y < -0.1f ||
                viewportPos.y > 1.1f ||
                viewportPos.x < -0.1f ||
                viewportPos.x > 1.1f)
            {
                Die();
            }
        }
    }

    private IEnumerator SinkAndDie()
    {
        if (isDead || isSinking) yield break;

        isSinking = true;
        gameManager.SetGameActive(false);

        float elapsedTime = 0f;
        Vector3 startPos = playerCharacter.position;
        Quaternion startRot = playerCharacter.rotation;

        // Sink the player
        while (elapsedTime < sinkDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / sinkDuration;

            // Move down
            playerCharacter.position = new Vector3(startPos.x, startPos.y - progress * 2f, startPos.z);

            playerCharacter.rotation = Quaternion.Euler(progress * 30f, startRot.eulerAngles.y, 0);

            yield return null;
        }
        playerCharacter.gameObject.SetActive(false);
        isSinking = false;
        Die();
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        gameManager.SetGameActive(false);

        if (playerCharacter.gameObject.activeSelf)
        {
            playerCharacter.gameObject.SetActive(false);
        }

        ShowGameOver();
    }

    public void TriggerDeath()
    {
        if (!isDead && !isSinking)
        {
            Die();
        }
    }

    private void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(0);
    }

    public void Quit()
    {
        Application.Quit();
    }
}