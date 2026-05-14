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

        private Player lastPlus4Player = null; // Biến để lưu người chơi đã đánh lá +4 cuối cùng

        private int currentPenalty = 0;
        private CardColor currentColor = CardColor.RED; // Mặc định màu đỏ khi bắt đầu

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
                Debug.Log($"Player {player.GetPlayerIndex()} đã đánh hết bài!");
                //CheckGameEndCondition(); // Gọi hàm kiểm tra kết thúc
            }

            if (card.cardColor != CardColor.NEUTRAL)
            {
                currentColor = card.cardColor;
            }

            Debug.Log($"Player {player.GetPlayerIndex()} played {card.CardName()}");

            // KIỂM TRA NẾU LÀ LÁ ĐEN (WILD / +4)
            if (card.cardColor == CardColor.NEUTRAL)
            {
                if (card.cardValue == CardValue.PLUS4)
                {
                    // LÁ +4: Ghi nhận người đánh cuối cùng, cộng dồn phạt, NHƯNG CHƯA BẬT UI CHỌN MÀU
                    lastPlus4Player = player;
                    ApplyBasicCardEffect(card);
                }
                else
                {
                    // LÁ WILD THƯỜNG: Dừng lượt, bật UI chọn màu ngay lập tức
                    //ShowColorPickerUI(card);
                }
            }
            else
            {
                // NẾU ĐÁNH LÁ BÌNH THƯỜNG (Hoặc +2)
                currentColor = card.cardColor;
                ApplyBasicCardEffect(card);
            }
        }

        public void SetCurrentColor(CardColor color)
        {
            if (!IsServer) return;
            currentColor = color;
            Debug.Log($"Current color set to {currentColor}");
        }

        private void ApplyBasicCardEffect(CardScriptables card)
        {
            switch (card.cardValue)
            {
                case CardValue.REVERSE:
                    TurnManager.Instance.ReverseDirection();
                    break;

                case CardValue.SKIP:
                    TurnManager.Instance.MoveToNextPlayer();
                    TurnManager.Instance.MoveToNextPlayer();
                    break;

                // --- CẬP NHẬT LUẬT STACKING (4.5) ---
                case CardValue.PLUS2:
                    currentPenalty += 2;
                    break;

                case CardValue.PLUS4:
                    currentPenalty += 4;
                    break;

                // --- CUSTOM RULES HOOKS (4.1, 4.2, 4.3) ---
                case CardValue.ZERO:
                    Debug.Log("Rule of 0: Trigger UI to choose Direction.");
                    // TODO: Dừng turn logic ở đây, hiển thị UI chọn chiều. 
                    // Sau khi user chọn, gọi hàm ExecuteRule0(int direction)
                    //panelRule0.SetActive(true);
                    break;

                case CardValue.SEVEN:
                    Debug.Log("Rule of 7: Trigger UI to choose Target Player.");
                    // TODO: Dừng turn, hiển thị UI chọn mục tiêu.
                    // Sau khi chọn, gọi hàm ExecuteRule7(PlayerController target)
                    //panelRule7.SetActive(true);
                    //ShowRule7UI(turnManager.CurrentPlayerIndex);
                    break;

                case CardValue.EIGHT:
                    Debug.Log("Rule of 8: Trigger Reaction Event!");
                    //StartCoroutine(Rule8ReactionEventRoutine());
                    break;

                default:
                    TurnManager.Instance.MoveToNextPlayer();
                    break;
            }
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