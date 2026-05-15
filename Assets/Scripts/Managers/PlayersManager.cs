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

        [SerializeField] Transform player1SpawnPoint;
        [SerializeField] Transform player2SpawnPoint;
        [SerializeField] Transform player3SpawnPoint;
        [SerializeField] Transform player4SpawnPoint;
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

        public int GetPlayerCount()
        {
            return m_PlayersCount;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                // Subscribe to the OnClientConnectedCallback event to handle player joining
                NetworkManager.Singleton.OnClientConnectedCallback += HandlePlayerJoined;

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
            m_Players[m_PlayersCount] = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<Player>();

            // Set player position to the corresponding spawn point
            Player player = m_Players[m_PlayersCount];
            Transform newTransform = GetPlayerSpawnPoint(m_PlayersCount);
            if (IsServer && player != null)
            {
                // Create parameters to target ONLY the client that just joined
                ClientRpcParams clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { clientId }
                    }
                };

                // Tell that specific client to move to the spawn point
                TeleportPlayerClientRpc(newTransform.position, newTransform.rotation, clientRpcParams);
            }

            // Increment player count
            m_PlayersCount++;
            Debug.Log("Player " + player.playerID + " joined the game. Total players: " + m_PlayersCount);
        }

        [ClientRpc]
        private void TeleportPlayerClientRpc(Vector3 spawnPosition, Quaternion spawnRotation, ClientRpcParams clientRpcParams = default)
        {
            // Find our local player and move it
            if (NetworkManager.Singleton.LocalClient.PlayerObject != null)
            {
                Transform localPlayerTransform = NetworkManager.Singleton.LocalClient.PlayerObject.transform;
                localPlayerTransform.position = spawnPosition;
                localPlayerTransform.rotation = spawnRotation;
            }
        }

        public void BroadcastPlayerTurn(int playerIndex)
        {
            if (!IsServer) return;

            // The server knows the true list. Grab the NetworkObjectId of the active player.
            Player activePlayer = m_Players[playerIndex];
            if (activePlayer != null)
            {
                SetPlayerTurnClientRpc(activePlayer.NetworkObjectId);
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
                    GameManager.Instance.TryPlayCard(player, cardNetworkId);
                }
            }
        }

        [Rpc(SendTo.Server)]
        public void RequestCardDrawRpc(ulong clientId)
        {
            int index = m_Players.FindIndex(p => p != null && p.OwnerClientId == clientId);
            Player player = m_Players[index];
            if (player != null && player.OwnerClientId == clientId)
            {
                if (player.GetPlayerIndex() == TurnManager.Instance.GetCurrentPlayerIndex())
                {
                    DealCardToPlayer(player.GetPlayerIndex());
                }
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void SetPlayerTurnRpc(ulong activePlayerNetworkId)
        {
            // Because clients don't have the m_Players list, we just find all Player objects in the scene
            Player[] allPlayers = FindObjectsByType<Player>(FindObjectsSortMode.None);

            foreach (Player p in allPlayers)
            {
                // If this player's ID matches the one the server sent, turn their visual on!
                bool isTheirTurn = p.NetworkObjectId == activePlayerNetworkId;
                p.SetPlayerTurn(isTheirTurn);
            }
        }

        public Player GetPlayer(int index)
        {
            return m_Players[index];
        }

        public int GetPlayerIndexFromClientId(ulong clientId)
        {
            return m_Players.FindIndex(p => p != null && p.OwnerClientId == clientId);
        }

        public int GetPlayerCount()
        {
            return m_PlayersCount;
        }

        public int GetPlayerCardCount(int playerIndex)
        {
            PlayerHand playerHand = GetPlayerHand(playerIndex);
            return playerHand != null ? playerHand.GetCardCount() : 0;
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