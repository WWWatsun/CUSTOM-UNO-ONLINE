using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

namespace PlayerScripts
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : NetworkBehaviour
    {
        [Header("Look Settings")]
        [SerializeField] private CinemachineCamera playerCamera;
        [SerializeField] private float mouseSensitivity = 1f;
        [SerializeField] private float maxLookAngle = 85.0f;

        private PlayerInput playerInput;

        private Vector2 lookInput;
        private float verticalRotation = 0f;
        private bool isPlaying = true; // Flag to track if the game has started

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            playerInput.enabled = false;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            HandleLook();
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                playerInput.enabled = true; // Enable input for the owning player
                playerCamera.Priority = 20; // Set camera priority to ensure the owning player's camera is active
            }

            base.OnNetworkSpawn();
        }

        public void OnLook(InputValue context)
        {
            if (!isPlaying) return; // Ignore input if the game hasn't started
            lookInput = context.Get<Vector2>();
        }

        private void HandleLook()
        {
            float mouseX = lookInput.x * mouseSensitivity;
            float mouseY = lookInput.y * mouseSensitivity;

            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);

            playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);
        }

        public void SetPlayerTurnControls(bool isCurrentTurn)
        {
            isPlaying = isCurrentTurn; // Update the flag based on whether it's the player's turn
            //playerCamera.Priority = isCurrentTurn ? 20 : 10; // Set camera priority to switch between players
        }
<<<<<<< Updated upstream
=======

        private void FireRaycast()
        {
            Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * 100f, Color.red, 1f);

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, cardLayer))
            {
                NetworkObject hitObject = hit.collider.GetComponent<NetworkObject>();

                if (hitObject != null)
                {
                    Debug.Log($"hit object {hitObject.NetworkObjectId}, {DeckManager.Instance.IsDrawPile(hitObject.NetworkObjectId)}");
                    if (DeckManager.Instance.IsDrawPile(hitObject.NetworkObjectId))
                    {
                        Debug.Log("Request a drawing card");
                        PlayersManager.Instance.RequestCardDrawRpc(NetworkManager.Singleton.LocalClientId);
                    }
                    // Check if THIS player owns the card they clicked
                    else if (hitObject.IsOwner)
                    {
                        Debug.Log("Hit my card! Requesting deletion...");
                        PlayersManager.Instance.RequestCardActionRpc(NetworkManager.Singleton.LocalClientId, hitObject.NetworkObjectId);
                    }
                    else
                    {
                        Debug.Log("That's not your card!");
                    }
                }
            }
        }
>>>>>>> Stashed changes
    }
}

