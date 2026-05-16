using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

namespace Managers
{
    public class DeckManager : NetworkBehaviour
    {
        public static DeckManager Instance { get; private set; }

        [Header("Deck Settings")]
        [SerializeField] CardScriptables[] cardDeck;
        [SerializeField] List<CardScriptables> activeDeck = new List<CardScriptables>();
        [SerializeField] List<CardScriptables> discardPile = new List<CardScriptables>();

        [Header("References")]
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private Transform discardPilePosition;
        [SerializeField] private Transform drawPilePosition;

        private GameObject discardPileDisplay;
        private GameObject drawPileDisplay;
        NetworkVariable<ulong> drawpileNetwordId = new NetworkVariable<ulong>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
            }

            activeDeck.AddRange(cardDeck);

            for (int i = 0; i < activeDeck.Count; i++)
            {
                cardDeck[i].cardID = i;
            }

            Shuffle();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                InitDiscardPile();
                InitDrawPile();
            }
        }

        private void InitDiscardPile()
        {
            if (!IsServer) return;

            discardPileDisplay = Instantiate(cardPrefab, discardPilePosition.position, discardPilePosition.rotation);
            NetworkObject discardPileDisplayNetworkObject = discardPileDisplay.GetComponent<NetworkObject>();
            discardPileDisplayNetworkObject.Spawn();
            discardPileDisplayNetworkObject.TrySetParent(discardPilePosition);

            discardPile.Add(DrawCard());
            discardPileDisplay.GetComponent<Card>().SetCard(discardPile[0].cardID);

            if (discardPile[0].cardColor == CardColor.NEUTRAL)
            {
                // If the first card is neutral, we need to shuffle it back to the deck and draw another card
                activeDeck.AddRange(discardPile);
                discardPile.Clear();
                Shuffle();
                discardPile.Add(DrawCard());
                discardPileDisplay.GetComponent<Card>().SetCard(discardPile[0].cardID);
            }
            GameManager.Instance.SetCurrentColor(discardPile[0].cardColor);

            //discardPileDisplay.transform.localScale = Vector3.one;
        }

        private void InitDrawPile()
        {
            if (!IsServer) return;
            drawPileDisplay = Instantiate(cardPrefab, drawPilePosition.position, drawPilePosition.rotation);
            NetworkObject drawDisplayNetworkObject = drawPileDisplay.GetComponent<NetworkObject>();
            drawDisplayNetworkObject.Spawn();
            drawDisplayNetworkObject.TrySetParent(drawPilePosition);
            //drawPileDisplay.transform.localScale = Vector3.one;
            drawpileNetwordId.Value = drawDisplayNetworkObject.NetworkObjectId;
        }

        public bool IsDrawPile(ulong pileId)
        {
            return drawpileNetwordId.Value == pileId;
        }

        public void Shuffle()
        {
            Debug.Log("Start Shuffling");
            //Fisher - Yates
            int n = activeDeck.Count - 1;
            //Iterate from last card, switch with random card
            while (n > 0)
            {
                int k = UnityEngine.Random.Range(0, n);
                CardScriptables card = activeDeck[k];
                activeDeck[k] = activeDeck[n];
                activeDeck[n] = card;
                n--;
            }
        }

        [ContextMenu("DrawCard")]
        public CardScriptables DrawCard()
        {
            // Shuffle the discard pile if out of card
            if (activeDeck.Count <= 0)
            {
                CardScriptables topCard = discardPile[discardPile.Count - 1];
                discardPile.RemoveAt(discardPile.Count - 1);

                activeDeck.AddRange(discardPile);

                discardPile.Clear();
                discardPile.Add(topCard);

                Shuffle();
            }
            int pos = activeDeck.Count - 1;
            CardScriptables card = activeDeck[pos];
            activeDeck.RemoveAt(pos);

            Debug.Log($"Active: {activeDeck.Count}, Discard: {discardPile.Count}");
            return card;
        }

        //If a card is played, call this to append the card to the discard pile
        public void GetDiscarded(CardScriptables card)
        {
            discardPile.Add(card);

            discardPileDisplay.GetComponent<Card>().SetCard(card.cardID);
        }

        public CardScriptables GetTopDiscardPileCard()
        {
            if (discardPile.Count == 0)
            {
                return null;
            }
            return discardPile[discardPile.Count - 1];
        }

        public CardScriptables GetCardData(int cardID)
        {
            if (cardID < 0 || cardID >= cardDeck.Length)
            {
                Debug.LogError($"Invalid card ID: {cardID}");
                return null;
            }
            return cardDeck[cardID];
        }
    }
}
