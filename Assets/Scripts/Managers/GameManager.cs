using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using PlayerScripts;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public Action OnStartGame;

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
                foreach(PlayerHand p in players)
                {
                    Debug.Log($"Deal card number {i+1}");
                    p.DrawCard();
                }
            }
        }
    }
}