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
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }
        public Action OnStartGame;

        [Header("Environment Settings")]
        [SerializeField] GameObject tablePrefab;
        [SerializeField] Transform tableSpawnPosition;

        [Header("Game Settings")]
        public int startingCard { get; private set; } = 7;

        private int currentPenalty = 0;

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

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                SpawnTable();
            }
        }

        public void StartGame()
        {
            if (!IsServer) return;

            OnStartGame?.Invoke();
            //DealingOnStart();
        }

        public bool IsLegalMove(Player player, CardScriptables card)
        {
            if (!IsServer) return false; // Only the server should validate moves

            if (player == null || card == null) return false;

            if (player.GetPlayerIndex() != TurnManager.Instance.GetCurrentPlayerIndex())
            {
                return false;
            }

            return UnoRuleEngine.IsLegalMove(
                playedCard: card,
                topCard: DeckManager.Instance.GetTopDiscardPileCard(),
                currentColor: DeckManager.Instance.GetTopDiscardPileCard().cardColor,
                playerCardCount: PlayersManager.Instance.GetPlayerCardCount(player.GetPlayerIndex()),
                pendingPenalty: currentPenalty
            );
        }

        private void SpawnTable()
        {
            // 1. Create the object in the Unity Scene on the Server's machine
            GameObject spawnedTable = Instantiate(tablePrefab, tableSpawnPosition.position, tableSpawnPosition.rotation);

            // 2. Tell Netcode to sync this object to all connected clients
            NetworkObject tableNetworkObject = spawnedTable.GetComponent<NetworkObject>();
            if (tableNetworkObject != null)
            {
                tableNetworkObject.Spawn(); // This is the magic word!
            }
        }
    }
}