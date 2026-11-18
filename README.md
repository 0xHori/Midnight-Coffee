# ☕ Midnight Coffee

**Midnight Coffee** is a cozy 2D idle clicker game built with Unity, combining coffee shop management, character collection (Gacha), affection progression, and Lo-Fi atmosphere.

![Unity](https://img.shields.io/badge/Unity-6.6%20(6000.6.8f1)-black?logo=unity)
![Platform](https://img.shields.io/badge/Platform-Android-3DDC84?logo=android)
![Language](https://img.shields.io/badge/Localization-EN%20%7C%20RU-blue)
![Version](https://img.shields.io/badge/Release-v1.0.0-orange)

## 🎮 Gameplay Features

* **Tap & Idle Economy:** Brew coffee manually with critical hits and Fever Mode multipliers, or automate production with café equipment upgrades.
* **Skill Tree & Prestige:** Reset progress to earn Reputation Crystals, unlock permanent upgrades across Click, Auto-Income, and Luck branches, and accelerate future runs.
* **Character Gacha & Affection System:** Recruit baristas and characters via single or 10x pulls (with a 35-roll pity system), equip them for active/passive bonuses, and increase their affection level through gameplay.
* **Offline Progression:** Earn passive income while away from the game (up to 8 hours) with a summary report upon return.
* **Built-in Lo-Fi Music Player & Localization:** Switch tracks, adjust audio/haptics independently, and toggle between **English** and **Russian** on the fly.

## 📱 Download & Installation

1. Go to the [**Releases**](https://github.com/0xHori/Midnight-Coffee/releases) page.
2. Download the latest `MidnightCoffee-v1.0.0.apk`.
3. Install the APK on your Android device (allow installation from unknown sources if prompted).

## 🏗️ Technical Architecture

The project is organized into modular Assembly Definitions (`Core`, `Data`, `Economy`, `Characters`, `Effects`, `UI`) with a decoupled event-driven design:

* **Event-Driven Communication (`GameEvents`):** C# `event` delegates decouple core economy logic from UI updates, audio cues, particle pools, and character emotion reactions.
* **Data-Driven Configuration (`ScriptableObjects`):** Game balance (`BalanceConfig`), characters (`CharacterData`), shop items (`UpgradeData`), and skill nodes (`SkillData`) are configured as isolated assets.
* **Atomic Save System (`SaveManager`):** JSON serialization with `.tmp` and `.bak` backup rotation to prevent save corruption, separate from `PlayerPrefs` user audio/language settings.
* **Object Pooling:** Custom pools (`FloatingTextPool`, `TapParticlePool`) eliminate runtime garbage collection spikes during rapid tapping.

## 🛠️ Building from Source

1. Clone the repository:
   ```bash
   git clone https://github.com/0xHori/Midnight-Coffee.git
   ```

2. Open the project in **Unity Hub** (Unity 6.6 (6000.6.8f1) or newer).
3. Open `Assets/_Project/Scenes/SampleScene.unity`.
4. Select **File → Build Settings → Android** (IL2CPP, ARM64) and click **Build**.

## 👤 Author

Developed by **zeroxhori** ([0xHori](https://t.me/zeroxhori)).
