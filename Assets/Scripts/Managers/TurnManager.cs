using System;
using UnityEngine;
using Unity.Netcode;

namespace Managers
{
    public class TurnManager : NetworkBehaviour
    {
        public static TurnManager Instance { get; private set; }
        public Action<int> OnNextPlayerTurn;
        
        int currentPlayerIndex;
        int direction = 1; // 1 for clockwise, -1 for counterclockwise
        int playerCount;

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

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            GameManager.Instance.OnStartGame += StartGame;
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void StartGame()
        {
            playerCount = PlayersManager.Instance.GetPlayerCount();
            currentPlayerIndex = 0;
            OnNextPlayerTurn?.Invoke(currentPlayerIndex);
            Debug.Log("Current Player Index: " + currentPlayerIndex);
        }

        public void MoveToNextPlayer()
        {
            currentPlayerIndex = (currentPlayerIndex + direction + playerCount) % playerCount;
            OnNextPlayerTurn?.Invoke(currentPlayerIndex);
            Debug.Log("Current Player Index: " + currentPlayerIndex);
        }
    }
}
