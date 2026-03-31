# FinishZone

FinishZone is a **server-side** V Rising mod that allows administrators to create finish zones for events such as races, mazes, or puzzles, and includes commands for manually giving reward sets to players.

## Features
- Create finish zones using simple in-game chat commands.
- Broadcast global completion messages.
- Grant rewards only on a player’s **first completion** of each zone.
- Apply a cooldown to prevent the same player from repeatedly triggering the same zone too quickly.
- Finish zones track players only. Admin-authenticated users are excluded and will not trigger zones.
- Log all completions to a CSV file for history tracking.
- Display recorded winners using an in-game command.
- Reload zone data from file without restarting the server.
- Includes commands for manually giving reward sets to players.

## Requirements
1. [BepInEx 1.733.2](https://thunderstore.io/c/v-rising/p/BepInEx/BepInExPack_V_Rising/)
2. [VampireCommandFramework 0.10.4](https://thunderstore.io/c/v-rising/p/deca/VampireCommandFramework/)

## Installation
1. Install the required dependencies.
2. Place `FinishZone.dll` into your server's BepInEx plugins folder.
3. Start the server once to generate the config files.
4. Create your finish zone with `.finish add <id> <radius>`
5. Edit `finishzones.json` if you want to customize rewards, messages, or vertical settings.
6. Use `.finish reload` or restart the server after making file changes.

## How It Works
- FinishZone continuously checks player positions against all enabled finish zones.
- The checking frequency is controlled by `LoopIntervalSeconds` in `finishzones.json`. Use `0.2–1.0` for precision-based activities and `1.0–2.0` for less precision-sensitive activities.
- When a player enters a finish zone, the server broadcasts a completion message.
- Rewards are granted only the **first time** a player completes a finish zone.
- A cooldown prevents repeated triggering in a short period.
- Every completion is recorded in `finish_log.csv`.
- **`AdminAuth` users do not trigger finish zones. Please use `AdminDeAuth` before testing.**

## Commands

### Player Commands
- `.finishers` or `.winners`
  - Show the recorded finishers from finish_log.csv

### Admin Commands
- `.finish add <id> <radius>`
  - Create a new finish zone at your current position.
  - Shortcut: *.finish a <id> <radius>*
  - Example: *.finish add Maze 5*

- `.finish update <id> <radius>`
  - Update the position and radius of an existing finish zone at your current position.
  - Shortcut: *.finish u <id> <radius>*
  - Example: *.finish update Maze 8*

- `.finish remove <id>`
  - Remove a finish zone.
  - Shortcut: *.finish rm <id>*
  - Example: *.finish remove maze*

- `.finish reload`
  - Reloads all finish zones from finishzones.json
  - Shortcut: *.finish rl*

- `.finish list`
  - Displays all finish zones, status, and mod settings.  
  - Shortcut: *.finish l*

- `.finish on <id>`
  - Enable a specific finish zone.
  - Example: *.finish on maze*

- `.finish off <id>`
  - Disable a specific finish zone.
  - Example: *.finish off maze*
  
- `.finish enable`
  - Enables the FinishZone mod globally.

- `.finish disable`
  - Disables the FinishZone mod globally.

- `.rewards add <setname> <prefabguid> <amount>`
  - Add an item to a reward set.
  - Shortcut: *.rewards a <setname> <prefabguid> <amount>*
  - Example: *.rewards add Potion 429052660 10*

- `.rewards remove <setname>`
  - Remove a reward set.
  - Shortcut: *.rewards rm <setname>*
  - Example: *.rewards remove potion*
  
- `.rewards give <setname> [player]`
  - Give a reward set to a player. If no player is provided, the reward set is given to yourself.
  - Shortcut: *.rewards g <setname> [player]*
  - Example: *.rewards give potion Del*

- `.rewards reload`
  - Reload rewards.json from disk.
  - Shortcut: *.rewards rl*

- `.rewards list`
  - Show all reward sets and their items.
  - Shortcut: *.rewards l*

## Config Files
After the first server start, the following files will be created:
- `BepInEx/config/FinishZone/finishzones.json`
- `BepInEx/config/FinishZone/finish_log.csv`
- `BepInEx/config/FinishZone/rewards.json`
- `BepInEx/config/FinishZone/rewards_log.csv`

### finishzones.json
Example:

```json
{
  "ModEnabled": true,
  "LoopIntervalSeconds": 1,
  "Zones": {
    "The Maze": {
      "Position": [
        -424.5694,
        9.700005,
        -312.78476
      ],
      "Radius": 5,
      "VerticalLimit": 1,
      "Message": "Find the way to the top of the castle! Type .tp 1 to join",
      "RewardPrefab": 576389135,
      "RewardName": "Greater Stygian Shards",
      "RewardAmount": 1000,
      "ZoneEnabled": true
    }
  }
}
```

### rewards.json
Example:

```json
{
  "Winner": [
    {
      "Item": -1461326411,
      "Amount": 20
    },
    {
      "Item": -1021407417,
      "Amount": 20
    }
  ],
  "Potion": [
    {
      "Item": 429052660,
      "Amount": 10
    },
    {
      "Item": 800879747,
      "Amount": 10
    }
  ]
}
```

## Credits
- **Odjit** and **zfolmt** for the original code and assistance with this mod.
- **V Rising modding community**

## License
This project is licensed under the AGPL-3.0 license.

## Notes
> - This mod was first made for my own server and originally ran through KindredCommands. It has now been separated into a standalone mod so that everyone can use it.
> - If you have any problems or run into bugs, please report them to me in the [V Rising Modding Community](https://discord.com/invite/QG2FmueAG9)
> **Del** (delta_663)
