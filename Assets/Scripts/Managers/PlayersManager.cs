using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;
using PlayerScripts;

namespace Managers
{
    public class PlayersManager : NetworkBehaviour
    {
        public static PlayersManager Instance { get; private set; }

        [Header("Player Spawn Points")]
        [SerializeField] Transform player1SpawnPoint;
        [SerializeField] Transform player2SpawnPoint;
        [SerializeField] Transform player3SpawnPoint;
        [SerializeField] Transform player4SpawnPoint;

        [Header("Player Hand Spawn Points")]
        [SerializeField] PlayerHand player1Hand;
        [SerializeField] PlayerHand player2Hand;
        [SerializeField] PlayerHand player3Hand;
        [SerializeField] PlayerHand player4Hand;

        List<Player> m_Players;
        int m_PlayersCount;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                // Create 4 empty player objects in the list
                m_Players = new List<Player>() { null, null, null, null };
                m_PlayersCount = 0;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                // Subscribe to the OnClientConnectedCallback event to handle player joining
                NetworkManager.Singleton.OnClientConnectedCallback += HandlePlayerJoined;

                GameManager.Instance.OnStartGame += DealCardAtStart;

                //Create 4 empty player objects in the list
                m_Players = new List<Player>() { null, null, null, null };
                m_PlayersCount = 0;
            }

            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= HandlePlayerJoined;
            }
        }

        private void HandlePlayerJoined(ulong clientId)
        {
            if (m_PlayersCount >= 4) return;

            m_Players[m_PlayersCount] = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<Player>();
            Player player = m_Players[m_PlayersCount];
            Transform newTransform = GetPlayerSpawnPoint(m_PlayersCount);

            if (IsServer && player != null)
            {
                player.transform.position = newTransform.position;
                player.transform.rotation = newTransform.rotation;
            }

            m_PlayersCount++;
            Debug.Log($"Player joined! Total: {m_PlayersCount}");
        }

        private void DealCardAtStart()
        {
            if (!IsServer) return;

            for (int i = 0; i < GameManager.Instance.startingCard; i++)
            {
                for (int j = 0; j < m_PlayersCount; j++)
                {
                    DealCardToPlayer(j);
                }
            }
        }

        private void DealCardToPlayer(int playerIndex)
        {
            if (!IsServer) return;

            PlayerHand playerHand = GetPlayerHand(playerIndex);

            if (playerHand != null)
            {
                playerHand.DrawCard(m_Players[playerIndex].OwnerClientId);
            }
        }

        private PlayerHand GetPlayerHand(int playerIndex)
        {
            return playerIndex switch
            {
                0 => player1Hand,
                1 => player2Hand,
                2 => player3Hand,
                3 => player4Hand,
                _ => throw new ArgumentException("Invalid player ID")
            };
        }

        public void BroadcastPlayerTurn(int playerIndex)
        {
            if (!IsServer) return;

            // The server knows the true list. Grab the NetworkObjectId of the active player.
            Player activePlayer = m_Players[playerIndex];
            if (activePlayer != null)
            {
                SetPlayerTurnRpc(activePlayer.NetworkObjectId);
            }
        }

        [Rpc(SendTo.Server)]
        public void UpdatePlayerLookInputRpc(ulong clientId, float rotation)
        {
            Player player = m_Players.FirstOrDefault(p => p != null && p.OwnerClientId == clientId);
            if (player != null)
            {
                // Update the player's transform on the server
                player.transform.Rotate(Vector3.up, rotation);
            }
        }

        [Rpc(SendTo.Server)]
        public void RequestCardActionRpc(ulong clientId, ulong cardNetworkId)
        {
            int index = m_Players.FindIndex(p => p != null && p.OwnerClientId == clientId);
            Player player = m_Players[index];
            if (player != null && player.OwnerClientId == clientId)
            {
                PlayerHand playerHand = GetPlayerHand(index);
                if (playerHand != null)
                {
                    playerHand.DiscardCard(cardNetworkId);
                }
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void SetPlayerTurnRpc(ulong activePlayerNetworkId)
        {
            Player[] allPlayers = FindObjectsByType<Player>(FindObjectsSortMode.None);

            foreach (Player p in allPlayers)
            {
                bool isTheirTurn = p.NetworkObjectId == activePlayerNetworkId;
                p.SetPlayerTurn(isTheirTurn);
            }
        }

        public int GetPlayerCount()
        {
            return m_PlayersCount;
        }

        private Transform GetPlayerSpawnPoint(int playerIndex)
        {
            return playerIndex switch
            {
                0 => player1SpawnPoint,
                1 => player2SpawnPoint,
                2 => player3SpawnPoint,
                3 => player4SpawnPoint,
                _ => throw new ArgumentException("Invalid player ID")
            };
        }
    }
}