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
            startGameButton.gameObject.SetActive(true); // Show the start game button for the host
        });

        startClientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });
    }
}
