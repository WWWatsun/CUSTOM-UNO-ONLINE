using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;
using PlayerScripts;

public class UIManager : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private TMP_Text penaltyText;

    [Header("Panels")]
    [SerializeField] GameObject panelRule0;
    [SerializeField] GameObject panelRule7;
    [SerializeField] GameObject panelRule8;
    [SerializeField] GameObject colorPicker;

    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        startGameButton.gameObject.SetActive(false); // Hide the start game button until the host starts the game

        startHostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            startHostButton.gameObject.SetActive(false);
            startClientButton.gameObject.SetActive(false);
        });

        startClientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            startHostButton.gameObject.SetActive(false);
            startClientButton.gameObject.SetActive(false);
        });

        penaltyText.text = ""; // Clear penalty text at the start
    }

    private void Update()
    {
        if (NetworkManager.Singleton.IsServer && NetworkManager.Singleton.ConnectedClients.Count >= 2)
        {
            startGameButton.gameObject.SetActive(true); // Show the start game button when at least 2 players are connected
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ShowColorPickerUIRpc(ulong playerNetworkId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkId, out var obj))
        {
            if (obj.IsOwner)
            {
                colorPicker.SetActive(true);
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ShowRule0UIRpc(ulong playerNetworkId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkId, out var obj))
        {
            if (obj.IsOwner)
            {
                panelRule0.SetActive(true);
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ShowRule7UIRpc(ulong playerNetworkId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkId, out var obj))
        {
            if (obj.IsOwner)
            {
                panelRule7.SetActive(true);
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ShowRule8UIRpc(ulong playerNetworkId)
    {
        panelRule8.SetActive(true);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void UpdatePenaltyRpc(int penalty)
    {
        if (penalty != 0)
        {
            penaltyText.text = $"Current penalty: +{penalty}";
        }
        else
        {
            penaltyText.text = "";
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void TurnOffUIRpc()
    {
        Player[] allPlayers = FindObjectsByType<Player>(FindObjectsSortMode.None);
        foreach (Player p in allPlayers)
        {
            colorPicker.SetActive(false);
            panelRule0.SetActive(false);
            panelRule7.SetActive(false);
            panelRule8.SetActive(false);
        }
    }
}
