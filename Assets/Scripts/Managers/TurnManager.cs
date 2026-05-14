using System;
using UnityEngine;
using Unity.Netcode;

namespace Managers
{
    public class TurnManager : NetworkBehaviour
    {
        public static TurnManager Instance { get; private set; }
        public Action<int> OnNextPlayerTurn;

        NetworkVariable<int> currentPlayerIndex = new NetworkVariable<int>();
        int direction = 1;
        int playerCount;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {
            GameManager.Instance.OnStartGame += StartGame;
        }

        private void StartGame()
        {
            playerCount = PlayersManager.Instance.GetPlayerCount();
            currentPlayerIndex.Value = 0;
            Debug.Log("Current Player Index: " + currentPlayerIndex.Value);

            // Tell the PlayersManager to broadcast the turn to everyone
            PlayersManager.Instance.BroadcastPlayerTurn(currentPlayerIndex.Value);
        }

        public int GetCurrentPlayerIndex()
        {
            return currentPlayerIndex.Value;
        }

        public void MoveToNextPlayer()
        {
            if (!IsServer) return;

            currentPlayerIndex.Value = (currentPlayerIndex.Value + direction + playerCount) % playerCount;
            Debug.Log("Current Player Index: " + currentPlayerIndex.Value);

            // Tell the PlayersManager to broadcast the turn to everyone
            PlayersManager.Instance.BroadcastPlayerTurn(currentPlayerIndex.Value);
        }
    }
}