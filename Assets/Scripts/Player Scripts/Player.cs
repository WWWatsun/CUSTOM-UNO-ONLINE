using UnityEngine;
using Unity.Netcode;
using Managers;
using System;

namespace PlayerScripts
{
    [RequireComponent(typeof(PlayerController))]
    public class Player : NetworkBehaviour
    {
        public int playerID;  // playerID = -1 -> auto assign, playerID >= 0 -> 3 assigned by GameManager

        [SerializeField] private PlayerVisual playerVisual;
        private PlayerController playerController;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            playerController = GetComponent<PlayerController>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public override void OnNetworkSpawn()
        {

        }

        public int GetPlayerID()
        {
            return playerID;
        }

        public void SetPlayerTurn(bool isCurrentTurn)
        {
            playerVisual.SetPlayerTurnVisual(isCurrentTurn);
            //playerController.SetPlayerTurnControls(isCurrentTurn);
        }
    }
}
