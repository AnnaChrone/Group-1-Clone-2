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
    [SerializeField] private string deathLayerName = "Obstacle"; // Layer for obstacles that kill player

    private bool isDead = false;
    private Vector3 deathPosition;

    private void Start()
    {
        gameOverPanel.SetActive(false);
        isDead = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        // Check if the player collided with something on the Obstacle layer
        if (other.gameObject.layer == LayerMask.NameToLayer(deathLayerName))
        {
            Die();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;

        // Check if the player collided with something on the Obstacle layer
        if (collision.gameObject.layer == LayerMask.NameToLayer(deathLayerName))
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        deathPosition = playerCharacter.position;

        playerCharacter.gameObject.SetActive(false);

        gameManager.SetGameActive(false);

        ShowGameOver();
    }
    public void TriggerDeath()
    {
        if (!isDead)
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
