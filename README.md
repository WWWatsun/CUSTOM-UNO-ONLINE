# ASSIGNMENT 4: Custom UNO Online

A fully networked, 3D multiplayer turn-based card game built in Unity using **Netcode for GameObjects (NGO)**. This project brings the classic UNO gameplay into a first-person 3D environment with synchronized state updates and host-authoritative rule validation.

## 🚀 Game Overview

This game allows 2 to 4 players to join a host-created room and play a rule-heavy variant of UNO. Rather than a standard 2D menu, players sit at a virtual 3D table, interact with their cards using physics raycasts, and see dynamic visual indicators for turn order and game states.

### Core Features
* **Host Room Architecture:** Players can create a room as a Host or join as a Client. The Host is the absolute authority on game state.
* **Synchronized Multiplayer:** Card plays, draws, color changes, and hand swaps are synchronized in real-time across all clients.
* **Smart Deck Management:** Features a dynamic draw and discard pile. When the draw pile runs out, the discard pile is automatically safely reshuffled (retaining the top card) to keep the game going.
* **3D Visual Feedback:** * The main camera background synchronizes across the network to match the currently active card color (Red, Blue, Green, Yellow).
  * A 3D physical arrow dynamically points at the player whose turn it currently is.
  * A Crown spawns on the winning player's head when they successfully empty their hand.

## 📐 Networking Architecture & Validation

The networking layer is built using a **Lightweight Client-Server model** via Unity NGO, where the Host acts as both a player and the authoritative server. 

**Host Authority & Data Integrity:**
* Clients **cannot** force moves. All card clicks send an RPC request to the server.
* The `GameManager` and `UnoRuleEngine` run strictly on the server to validate moves.
* The server validates: correct turn order, legal card matching, valid targets for Rule 7, stacking legality, and win-condition legality.
* **Disconnections/Empty Decks:** Handled seamlessly by the `DeckManager` and `PlayersManager` to ensure data integrity.

## 📜 Custom House Rules

The game strictly enforces the required custom rule set via the `UnoRuleEngine`:

* **Rule of 0 (Directional Swap):** Playing a `0` triggers a UI prompt allowing the player to choose Clockwise or Counter-Clockwise. All players' hands are simultaneously passed in the chosen direction.
* **Rule 7 (Targeted Swap):** Playing a `7` opens a UI targeting menu. The player selects a specific opponent, and their entire hands are swapped.
* **Rule 8 (Reaction Event):** Playing an `8` triggers a global quick-time event. All players have a limited window (5 seconds) to click the Reaction button on their screen. The last player to react, or anyone who fails to react in time, is forced to draw 2 penalty cards.
* **Rule 4.4 (No Win with Action Card):** A player **cannot** win the game if their final card is a Skip, Reverse, +2, Wild, or +4. The `UnoRuleEngine` will flag this as an illegal move and block it. The player must draw a card if they have no other legal moves.
* **Rule 4.5 (Stacking +2 and +4):** * If hit with a `+2`, a player can stack another `+2` or a `+4` to pass the penalty down.
  * If hit with a `+4`, a player can **only** stack another `+4`.
  * If a player cannot (or chooses not to) stack, they are forced to draw the accumulated penalty and lose their turn.

**Note:** In a two-player environment, the reverse card is still treated as a reverse direction action instead of a skip action.

## 🎮 Controls & Interface

The game utilizes Unity's modern Input System, designed for a first-person 3D perspective:
* **Mouse Look:** Look around the 3D table to see opponents and your hand.
* **Left Click (Interact):** Fire a raycast to play a card from your hand or click the center deck to draw a card.
* **Hold Alt Button (Show Cursor):** Holding the designated cursor button locks your camera movement and frees the mouse cursor, allowing you to click on UI elements (like the Color Picker or Rule 0/7/8 menus). Releasing it locks the camera back to first-person mode.

**UI State Indicators:**
* The top card of the discard pile is always visible in the center of the table.
* The current play direction and active turn are represented by spinning 3D indicators.
* Active penalties (e.g., "+4") are tracked and displayed on the UI.

## 🛠️ Setup & Installation

1. Clone the repository.
2. Open the project in Unity 6 (6000.x or newer). Ensure the "Input System", "Netcode for GameObjects", and "Cinemachine" packages are installed.
3. Open the main Game scene.
4. **Player 1:** Click **Start Host** (Creates the room).
5. **Player 2-4:** Click **Start Client** (Joins the room).
6. Once all players have joined, the Host clicks **Start Game** to deal the initial 7 cards and begin the match.

## 📦 Asset Sources & Attribution

* **Models:**
  * **Arrow model:** https://skfb.ly/6ZMSV
  * **Crown model:** https://skfb.ly/ovRzo
