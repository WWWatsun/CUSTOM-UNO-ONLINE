using UnityEngine;
using Unity.Netcode;
using Managers;
using System;

namespace PlayerScripts
{
    [RequireComponent(typeof(PlayerController))]
    public class Player : NetworkBehaviour
    {
        [SerializeField] private PlayerVisual playerVisual;
        [SerializeField] private SpriteRenderer playerFace;
        private PlayerController playerController;
        private int playerIndex;

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
            if (IsOwner)
            {
                playerFace.enabled = false;
            }
        }

        public int GetPlayerIndex()
        {
            return playerIndex;
        }

        public int SetPlayerIndex(int index)
        {
            playerIndex = index;
            return playerIndex;
        }

        public void SetPlayerTurn(bool isCurrentTurn)
        {
            playerVisual.SetPlayerTurnVisual(isCurrentTurn);
            //playerController.SetPlayerTurnControls(isCurrentTurn);
        }
    }
}
