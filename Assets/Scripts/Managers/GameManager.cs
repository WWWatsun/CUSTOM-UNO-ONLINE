using NUnit.Framework;
using PlayerScripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace Managers
{
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }
        public Action OnStartGame;

        [Header("Environment Settings")]
        [SerializeField] GameObject tablePrefab;
        [SerializeField] Transform tableSpawnPosition;
        [SerializeField] GameObject crownPrefab;
        [SerializeField] Camera mainCamera;

        [Header("Game Settings")]
        public int startingCard { get; private set; } = 7;

        private int currentPenalty = 0;
        NetworkVariable<CardColor> currentColor = new NetworkVariable<CardColor>();

        [Header("Rule 8")]
        private List<int> rule8Clickers = new List<int>();

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
            currentColor.OnValueChanged += OncurrentColorChanged;

            SetCameraColor(currentColor.Value); // Ensure camera starts with the correct color
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
                currentColor: currentColor.Value,
                playerCardCount: PlayersManager.Instance.GetPlayerCardCount(player.GetPlayerIndex()),
                pendingPenalty: currentPenalty
            );
        }

        public void TryPlayCard(Player player, ulong cardNetworkId)
        {
            CardScriptables card = PlayersManager.Instance.GetPlayerHand(player.GetPlayerIndex()).GetCardByNetworkID(cardNetworkId);
            if (!IsLegalMove(player, card))
            {
                Debug.Log($"Illegal move: {card.CardName()}");
                return;
            }

            PlayerHand playerHand = PlayersManager.Instance.GetPlayerHand(player.GetPlayerIndex());
            playerHand.DiscardCard(cardNetworkId);
            DeckManager.Instance.GetDiscarded(card);

            // 1. KIỂM TRA NGƯỜI VỪA ĐÁNH CÓ HẾT BÀI CHƯA
            if (playerHand.GetCardCount() == 0)
            {
                if (IsServer)
                {
                    // Spawn crown prefab above the player who has no cards left
                    SpawnCrown(player);

                    Debug.Log($"Player {player.GetPlayerIndex()} đã đánh hết bài!");
                    return; // Game should end, so skip the rest of the logic
                }
            }

            if (card.cardColor != CardColor.NEUTRAL)
            {
                currentColor.Value = card.cardColor;
            }

            Debug.Log($"Player {player.GetPlayerIndex()} played {card.CardName()}");

            // KIỂM TRA NẾU LÀ LÁ ĐEN (WILD / +4)
            if (card.cardColor == CardColor.NEUTRAL)
            {
                if (card.cardValue == CardValue.PLUS4)
                {
                    currentPenalty += 4;
                }
                UIManager.Instance.ShowColorPickerUIRpc(player.NetworkObjectId);
            }
            else
            {
                // NẾU ĐÁNH LÁ BÌNH THƯỜNG (Hoặc +2)
                currentColor.Value = card.cardColor;
                ApplyBasicCardEffect(card, player);
            }
        }

        public void TryDrawCard(Player player)
        {
            if (player == null) return;

            if (player.GetPlayerIndex() != TurnManager.Instance.GetCurrentPlayerIndex())
            {
                Debug.LogWarning("Not your turn!");
                return;
            }

            // FIX: Process penalty first, skipping normal draw logic
            if (currentPenalty > 0)
            {
                AcceptDrawPenalty(player);
                return;
            }

            // Base Uno Draw Logic
            CardScriptables drawnCard = DeckManager.Instance.DrawCard();
            PlayersManager.Instance.DealCardToPlayer(player.GetPlayerIndex(), drawnCard);

            if (IsLegalMove(player, drawnCard))
            {
                Debug.Log("Drawn card is playable. Player retains turn decision.");
                return;
            }

            // Not playable? Advance turn cleanly
            TurnManager.Instance.MoveToNextPlayer();
        }

        public void SetCurrentColor(CardColor color)
        {
            if (!IsServer) return;
            currentColor.Value = color;
            SetCameraColor(color);
            Debug.Log($"Current color set to {currentColor}");
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void SetCurrentColorByIntServerRpc(int color)
        {
            //RED:0, BLUE:1, GREEN:2, YELLOW:3
            SetCurrentColor((CardColor)color);
            UIManager.Instance.TurnOffUIRpc();

            // Once the color is safely locked in on the server, hand over the turn
            TurnManager.Instance.MoveToNextPlayer();
        }

        private void ApplyBasicCardEffect(CardScriptables card, Player player)
        {
            switch (card.cardValue)
            {
                case CardValue.REVERSE:
                    TurnManager.Instance.ReverseDirection();
                    TurnManager.Instance.MoveToNextPlayer(); // FIX: Pass the turn in the new direction!
                    break;

                case CardValue.SKIP:
                    TurnManager.Instance.MoveToNextPlayer();
                    TurnManager.Instance.MoveToNextPlayer();
                    break;

                // --- CẬP NHẬT LUẬT STACKING (4.5) ---
                case CardValue.PLUS2:
                    currentPenalty += 2;
                    TurnManager.Instance.MoveToNextPlayer(); // FIX: Pass the penalty threat to the next player!
                    break;

                case CardValue.PLUS4:
                    currentPenalty += 4;
                    TurnManager.Instance.MoveToNextPlayer();
                    break;

                // --- CUSTOM RULES HOOKS (4.1, 4.2, 4.3) ---
                case CardValue.ZERO:
                    Debug.Log("Rule of 0: Trigger UI to choose Direction.");
                    UIManager.Instance.ShowRule0UIRpc(player.NetworkObjectId);
                    break;

                case CardValue.SEVEN:
                    Debug.Log("Rule of 7: Trigger UI to choose Target Player.");
                    UIManager.Instance.ShowRule7UIRpc(player.NetworkObjectId);
                    break;

                case CardValue.EIGHT:
                    Debug.Log("Rule of 8: Trigger Reaction Event!");
                    UIManager.Instance.ShowRule8UIRpc(player.NetworkObjectId);
                    StartCoroutine(Rule8ReactionEventRoutine());
                    break;

                default:
                    TurnManager.Instance.MoveToNextPlayer();
                    break;
            }
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void ExecuteRule0Rpc(int direction)
        {
            //Clockwise: 1, Anticlockwise: -1
            //E.g 3 people clockwise, p0 gives p1, p1 gives p2, p2 gives p0
            int count = PlayersManager.Instance.GetPlayerCount();
            List<CardScriptables>[] hands = new List<CardScriptables>[count];
            for (int i = 0; i < count; i++)
            {
                hands[i] = new List<CardScriptables>(PlayersManager.Instance.GetPlayerHand(i).HandList);
            }

            for (int i = 0; i < count; i++)
            {
                int destinationIndex = (i + direction + count) % count;
                PlayerHand destinationHand = PlayersManager.Instance.GetPlayerHand(destinationIndex);
                destinationHand.HandList = hands[i];
                destinationHand.UpdateSwappedHand(PlayersManager.Instance.GetPlayer(destinationIndex).OwnerClientId);
            }

            UIManager.Instance.TurnOffUIRpc();
            TurnManager.Instance.MoveToNextPlayer();
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void ExecuteRule7Rpc(int targetId)
        {
            int playerId = TurnManager.Instance.GetCurrentPlayerIndex();
            //If the player choose self, do nothing
            if (playerId == targetId) return;
            PlayerHand playerHand = PlayersManager.Instance.GetPlayerHand(playerId);
            PlayerHand targetHand = PlayersManager.Instance.GetPlayerHand(targetId);

            if (playerHand != null && targetHand != null)
            {
                Debug.Log($"Swap Player {playerId} to Player {targetId}");

                List<CardScriptables> playerHandList = new List<CardScriptables>(playerHand.HandList);
                List<CardScriptables> targetHandList = new List<CardScriptables>(targetHand.HandList);

                playerHand.HandList = targetHandList;
                playerHand.UpdateSwappedHand(PlayersManager.Instance.GetPlayer(playerId).OwnerClientId);

                targetHand.HandList = playerHandList;
                targetHand.UpdateSwappedHand(PlayersManager.Instance.GetPlayer(targetId).OwnerClientId);

                UIManager.Instance.TurnOffUIRpc();
                TurnManager.Instance.MoveToNextPlayer();
            }
        }

        public void SubmitRule8Click()
        {
            SubmitRule8Rpc();
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void SubmitRule8Rpc(RpcParams rpcParams = default)
        {
            ulong senderId = rpcParams.Receive.SenderClientId;
            int playerId = PlayersManager.Instance.GetPlayerIndexFromClientId(senderId);
            Debug.Log($"Player {playerId} clicked!!!");
            if (playerId != -1 && !rule8Clickers.Contains(playerId))
            {
                rule8Clickers.Add(playerId);
            }
        }

        private IEnumerator Rule8ReactionEventRoutine()
        {
            float rule8duration = 5f;
            yield return new WaitForSeconds(rule8duration);
            //Check for non-clicker
            List<int> nonClicker = new List<int>();
            for (int i = 0; i < PlayersManager.Instance.GetPlayerCount(); i++)
            {
                if (!rule8Clickers.Contains(i)) nonClicker.Add(i);
            }

            if (nonClicker.Count > 0)
            {
                //Each Non-clicker draw 2 cards
                foreach (int i in nonClicker)
                {
                    PlayersManager.Instance.DealCardToPlayer(i, DeckManager.Instance.DrawCard());
                    PlayersManager.Instance.DealCardToPlayer(i, DeckManager.Instance.DrawCard());
                }
            }
            //If all clicked, last to click draw
            else
            {
                int lastToClick = rule8Clickers[rule8Clickers.Count - 1];
                PlayersManager.Instance.DealCardToPlayer(lastToClick, DeckManager.Instance.DrawCard());
                PlayersManager.Instance.DealCardToPlayer(lastToClick, DeckManager.Instance.DrawCard());
            }
            UIManager.Instance.TurnOffUIRpc();
            TurnManager.Instance.MoveToNextPlayer();
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

        private void SpawnCrown(Player player)
        {
            GameObject crown = Instantiate(crownPrefab, player.transform.position + Vector3.up, Quaternion.Euler(-90, 0, 0));
            NetworkObject crownNetworkObject = crown.GetComponent<NetworkObject>();
            if (crownNetworkObject != null)
            {
                crownNetworkObject.Spawn();
                crownNetworkObject.TrySetParent(player.NetworkObject, true);
            }
        }

        private void AcceptDrawPenalty(Player player)
        {
            Debug.Log($"Player {player.GetPlayerIndex()} got {currentPenalty} cards and lost turn.");
            for (int i = 0; i < currentPenalty; i++)
            {
                PlayersManager.Instance.DealCardToPlayer(player.GetPlayerIndex(), DeckManager.Instance.DrawCard());
            }
            currentPenalty = 0; // Reset penalty
            TurnManager.Instance.MoveToNextPlayer();
        }

        private void OncurrentColorChanged(CardColor previoudColor, CardColor newColor)
        {
            SetCameraColor(newColor);
        }

        private void SetCameraColor(CardColor color)
        {
            if (mainCamera == null) return;

            switch (color)
            {
                case CardColor.RED:
                    mainCamera.backgroundColor = Color.red;
                    break;
                case CardColor.BLUE:
                    mainCamera.backgroundColor = Color.blue;
                    break;
                case CardColor.GREEN:
                    mainCamera.backgroundColor = Color.green;
                    break;
                case CardColor.YELLOW:
                    mainCamera.backgroundColor = Color.yellow;
                    break;
                default:
                    mainCamera.backgroundColor = Color.white;
                    break;
            }
        }
    }
}