using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }
    [SerializeField] CardScriptables[] cardDeck;

    [SerializeField] List<CardScriptables> activeDeck = new List<CardScriptables>();
    [SerializeField] List<CardScriptables> discardPile = new List<CardScriptables>();
    private void Awake()
    {
<<<<<<< Updated upstream
        if (Instance != null && Instance != this)
=======
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
>>>>>>> Stashed changes
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

        activeDeck.AddRange(cardDeck);
        Shuffle();
    }

    public void Shuffle()
    {
        Debug.Log("Start Shuffling");
        //Fisher - Yates
        int n = activeDeck.Count - 1;
        //Iterate from last card, switch with random card
        while (n > 0)
        {
<<<<<<< Updated upstream
            int k = Random.Range(0, n);
            CardScriptables card = activeDeck[k];
            activeDeck[k] = activeDeck[n];
            activeDeck[n] = card;
            n--;
=======
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

            discardPileDisplay.transform.localScale = Vector3.one * 0.5f;
        }

        private void InitDrawPile()
        {
            if (!IsServer) return;
            drawPileDisplay = Instantiate(cardPrefab, drawPilePosition.position, drawPilePosition.rotation);
            NetworkObject drawDisplayNetworkObject = drawPileDisplay.GetComponent<NetworkObject>();
            drawDisplayNetworkObject.Spawn();
            drawDisplayNetworkObject.TrySetParent(drawPilePosition);
            drawPileDisplay.transform.localScale = Vector3.one * 0.5f;
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
                activeDeck.Clear();
                activeDeck.AddRange(discardPile);
                discardPile.Clear();
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
>>>>>>> Stashed changes
        }
    }

    [ContextMenu("DrawCard")]
    public CardScriptables DrawCard()
    {
        // Shuffle the discard pile if out of card
        if (activeDeck.Count <= 0)
        {
            activeDeck.Clear();
            activeDeck.AddRange(discardPile);
            discardPile.Clear();
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
    }

}
