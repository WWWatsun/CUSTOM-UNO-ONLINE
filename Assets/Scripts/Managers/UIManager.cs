using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;
    [SerializeField] private Button startGameButton;

    private void Start()
    {
        startGameButton.gameObject.SetActive(false); // Hide the start game button until the host starts the game

        startHostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
        });

        startClientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });
    }

    private void Update()
    {
        if (NetworkManager.Singleton.ConnectedClients.Count >= 2)
        {
            startGameButton.gameObject.SetActive(true); // Show the start game button when at least 2 players are connected
        }
    }
}
