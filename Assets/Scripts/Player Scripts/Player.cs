using UnityEngine;
using Unity.Netcode;
using Managers;
using System;

namespace PlayerScripts
{
    public class Player : NetworkBehaviour
    {
        public int playerID;  // playerID = -1 -> auto assign, playerID >= 0 -> 3 assigned by GameManager

        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerVisual playerVisual;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {

        }

        public override void OnNetworkSpawn()
        {
            TurnManager.Instance.OnNextPlayerTurn += SetPlayerTurn;
        }

        public int GetPlayerID()
        {
            return playerID;
        }

        private void SetPlayerTurn(int playerID)
        {
            if (playerID == this.playerID)
            {
                playerVisual.SetPlayerTurnVisual(true);
                playerController.SetPlayerTurnControls(true);
            }
            else
            {
                playerVisual.SetPlayerTurnVisual(false);
                playerController.SetPlayerTurnControls(false);
            }
        }
    }
}
