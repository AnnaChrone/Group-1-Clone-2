using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HomeScreenManager : MonoBehaviour
{

    [Header("UI References")]
    [SerializeField] private GameObject homeScreenPanel;

    [Header("References")]
    [SerializeField] private GameManager gameManager;

    private bool gameStarted = false;

    private void Start()
    {
        homeScreenPanel.SetActive(true);
        gameManager.SetGameActive(false);
    }

    private void Update()
    {
        if (!gameStarted && AnyKeyPressed())
        {
            StartGame();
        }
    }

    private bool AnyKeyPressed()
    {
        return Keyboard.current.anyKey.wasPressedThisFrame;
    }
    private void StartGame()
    {
        gameStarted = true;
        homeScreenPanel.SetActive(false);
        gameManager.StartGameLevel();
    }
}

