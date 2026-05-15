using NUnit.Framework;
using PlayerScripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
<<<<<<< Updated upstream
using PlayerScripts;
=======
>>>>>>> Stashed changes

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public Action OnStartGame;

<<<<<<< Updated upstream
=======
        [Header("Environment Settings")]
        [SerializeField] GameObject tablePrefab;
        [SerializeField] Transform tableSpawnPosition;

        [Header("Game Settings")]
        public int startingCard { get; private set; } = 7;

        private Player lastPlus4Player = null; // Biến để lưu người chơi đã đánh lá +4 cuối cùng

        private int currentPenalty = 0;
        private CardColor currentColor = CardColor.RED; // Mặc định màu đỏ khi bắt đầu

        [Header("Rule 8")]
        private List<int> rule8Clickers = new List<int>();


>>>>>>> Stashed changes
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
            Instance.OnStartGame += DealingOnStart;
        }

        public void StartGame()
        {
            OnStartGame?.Invoke();
            //DealingOnStart();
        }


        [SerializeField] int startingCard = 7;
        public void DealingOnStart()
        {
            PlayerHand[] players = FindObjectsByType<PlayerHand>(FindObjectsSortMode.None);
            for (int i=0; i<startingCard; i++)
            {
                return false;
            }

            Debug.Log($"{card.CardName()}, player: {player.GetPlayerIndex()}, top: {DeckManager.Instance.GetTopDiscardPileCard()}, color: {currentColor}");

            return UnoRuleEngine.IsLegalMove(
                playedCard: card,
                topCard: DeckManager.Instance.GetTopDiscardPileCard(),
                currentColor: currentColor/*DeckManager.Instance.GetTopDiscardPileCard().cardColor*/,
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
                    ApplyBasicCardEffect(card, player);
                }
                else
                {
                    // LÁ WILD THƯỜNG: Dừng lượt, bật UI chọn màu ngay lập tức
                    //ShowColorPickerUI(card);
                    UIManager.Instance.ShowColorPickerUIRpc(player.NetworkObjectId);
                }
            }
            else
            {
                // NẾU ĐÁNH LÁ BÌNH THƯỜNG (Hoặc +2)
                currentColor = card.cardColor;
                ApplyBasicCardEffect(card, player);
            }
        }

        public void SetCurrentColor(CardColor color)
        {
            if (!IsServer) return;
            currentColor = color;
            Debug.Log($"Current color set to {currentColor}");
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void SetCurrentColorByIntServerRpc(int color)
        {
            //RED:0, BLUE:1, GREEN:2, YELLOW:3
            SetCurrentColor((CardColor)color);
            UIManager.Instance.TurnOffUIClientRpc();
            TurnManager.Instance.MoveToNextPlayer();
        }

        private void ApplyBasicCardEffect(CardScriptables card, Player player)
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
                    TurnManager.Instance.MoveToNextPlayer();
                    break;

                case CardValue.PLUS4:
                    currentPenalty += 4;
                    TurnManager.Instance.MoveToNextPlayer();
                    break;

                // --- CUSTOM RULES HOOKS (4.1, 4.2, 4.3) ---
                case CardValue.ZERO:
                    Debug.Log("Rule of 0: Trigger UI to choose Direction.");
                    // TODO: Dừng turn logic ở đây, hiển thị UI chọn chiều. 
                    // Sau khi user chọn, gọi hàm ExecuteRule0(int direction)
                    //panelRule0.SetActive(true);
                    UIManager.Instance.ShowRule0UIRpc(player.NetworkObjectId);
                    break;

                case CardValue.SEVEN:
                    Debug.Log("Rule of 7: Trigger UI to choose Target Player.");
                    // TODO: Dừng turn, hiển thị UI chọn mục tiêu.
                    // Sau khi chọn, gọi hàm ExecuteRule7(PlayerController target)
                    //panelRule7.SetActive(true);
                    //ShowRule7UI(turnManager.CurrentPlayerIndex);
                    UIManager.Instance.ShowRule7UIRpc(player.NetworkObjectId);
                    break;

                case CardValue.EIGHT:
                    Debug.Log("Rule of 8: Trigger Reaction Event!");
                    UIManager.Instance.ShowRule8UIRpc(player.NetworkObjectId);
                    StartCoroutine(Rule8ReactionEventRoutine());
                    //TurnManager.Instance.MoveToNextPlayer();
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
            for (int i=0; i<count; i++)
            {
                hands[i] = PlayersManager.Instance.GetPlayerHand(i).HandList;
            }

            for(int i=0; i<count; i++)
            {
                int destinationIndex = (i + direction + count) % count;
                PlayerHand destinationHand = PlayersManager.Instance.GetPlayerHand(destinationIndex);
                destinationHand.HandList = hands[i];
                destinationHand.UpdateSwappedHand(PlayersManager.Instance.GetPlayerHand(destinationIndex).OwnerClientId);
            }

            UIManager.Instance.TurnOffUIClientRpc();
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

            if(playerHand != null && targetHand != null)
            {
                Debug.Log($"Swap Player {playerId} to Player {targetId}");
                List<CardScriptables> playerHandList = playerHand.HandList;
                List<CardScriptables> targetHandList = targetHand.HandList;
                playerHand.HandList = targetHandList;
                playerHand.UpdateSwappedHand(PlayersManager.Instance.GetPlayer(playerId).OwnerClientId);
                targetHand.HandList = playerHandList;
                targetHand.UpdateSwappedHand(PlayersManager.Instance.GetPlayer(targetId).OwnerClientId);
                UIManager.Instance.TurnOffUIClientRpc();
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
            if(playerId != -1 && !rule8Clickers.Contains(playerId))
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
                foreach(int i in nonClicker)
                {
                    PlayersManager.Instance.DealCardToPlayer(i);
                    PlayersManager.Instance.DealCardToPlayer(i);
                }
            }
            //If all clicked, last to click draw
            else
            {
                int lastToClick = rule8Clickers[rule8Clickers.Count-1];
                PlayersManager.Instance.DealCardToPlayer(lastToClick);
                PlayersManager.Instance.DealCardToPlayer(lastToClick);
            }
                UIManager.Instance.TurnOffUIClientRpc();
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
    }
}