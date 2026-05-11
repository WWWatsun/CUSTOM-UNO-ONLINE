using UnityEngine;
using Managers;
using System;

namespace PlayerScripts
{
    public class Player : MonoBehaviour
    {
        public int playerID;  // playerID = -1 -> auto assign, playerID >= 0 -> 3 assigned by GameManager

        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerVisual playerVisual;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            PlayersManager.Instance.JoinPlayer(this, playerID);
            TurnManager.Instance.OnNextPlayerTurn += SetPlayerTurn;
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnEnable()
        {
            
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
