using PlayerScripts;
using System;
using Unity.Netcode;
using UnityEngine;

namespace Managers
{
    public class TurnManager : NetworkBehaviour
    {
        public static TurnManager Instance { get; private set; }
        public Action<int> OnNextPlayerTurn;

        [SerializeField] private GameObject turnDirectionPrefab;
        [SerializeField] private Transform turnDirectionSpawnPoint;
        [SerializeField] private GameObject turnIndicatorPrefab;
        private GameObject turnDirectionObject;
        private GameObject turnIndicatorObject;

        NetworkVariable<int> currentPlayerIndex = new NetworkVariable<int>(-1);
        int direction = 1;
        int playerCount;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {

        }

        private void Update()
        {
            // Spin the turn direction indicator if it exists
            if (turnDirectionObject != null)
            {
                turnDirectionObject.transform.Rotate(Vector3.forward, 90 * Time.deltaTime);
            }

            if (turnIndicatorObject != null)
            {
                Player targetPlayer = PlayersManager.Instance.GetPlayer(currentPlayerIndex.Value);

                if (targetPlayer != null)
                {
                    // Get the player's true position
                    Vector3 targetPosition = targetPlayer.transform.position;

                    // OVERRIDE the target's height (Y) to match the arrow's exact height.
                    // This forces the arrow to only rotate left/right, staying flat on the table!
                    targetPosition.y = turnIndicatorObject.transform.position.y;

                    // Look at the flattened position
                    turnIndicatorObject.transform.LookAt(targetPosition);

                    // --- 3D MODEL ALIGNMENT FIX ---
                    // LookAt makes the local Z-axis (forward) point at the target.
                    // If your arrow 3D model was modeled sideways, it will point its side at the player.
                    // If your arrow is sideways right now, uncomment the line below and change 90f 
                    // to -90f or 180f until the tip of the arrow points at the player:
                    //
                    turnIndicatorObject.transform.Rotate(0, 90f, 0, Space.Self);
                }
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            GameManager.Instance.OnStartGame += StartGame;
            GameManager.Instance.OnStartGame += SpawnTurnDirection;
            GameManager.Instance.OnStartGame += SpawnTurnIndicator;
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

        public void ReverseDirection()
        {
            direction *= -1;

            // Flip the indicator exactly once
            if (turnDirectionObject != null)
            {
                Vector3 currentRotation = turnDirectionObject.transform.localEulerAngles;
                turnDirectionObject.transform.localRotation = Quaternion.Euler(direction == 1 ? -90 : 90, currentRotation.y, currentRotation.z);
            }
        }

        private void SpawnTurnDirection()
        {
            if (turnDirectionObject != null) return; // Already exists
            turnDirectionObject = Instantiate(turnDirectionPrefab, turnDirectionSpawnPoint.position, Quaternion.Euler(-90, 0, 0));
            NetworkObject netObj = turnDirectionObject.GetComponent<NetworkObject>();
            netObj.Spawn();
        }

        private void SpawnTurnIndicator()
        {
            if (turnIndicatorObject != null) return; // Already exists
            turnIndicatorObject = Instantiate(turnIndicatorPrefab, new Vector3(0, 1, 0), Quaternion.Euler(90, 0, 0));
            NetworkObject netObj = turnIndicatorObject.GetComponent<NetworkObject>();
            netObj.Spawn();
        }
    }
}