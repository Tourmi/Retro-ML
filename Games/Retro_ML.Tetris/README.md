# Tetris Plugin
Plugin by Voltage
-------
Training AIs that learn how to play Tetris

### Special Thanks
* [DataCrystal](https://datacrystal.romhacking.net/wiki/Tetris_(Game_Boy):RAM_map) : Some addresses in game memory were easier to find thanks to their RAM map.

## Configuration

### Plugin Configuration
* **Visible Rows**
    * Number of rows the AI can see.
    * Recommended value : 4 rows
    * Also used to normalize column heights when `Use Normalized Heights` is enabled.
* **Number of attempts**
    * The number of attempts the AI will do for each save states selected
    * Recommended value : 2 attempts
    * The higher this value is, the risk of eliminating a good AI is going to be lower.
    * A high value will also lengthen the training of each generations.
* **Use Normalized Heights**
    * When this option is enabled, the AIs will be given the height of each columns normalized.

### Objectives
Note that for objectives, the term "Reward" also refers to negative rewards, or punishments.
Turning off an objective also turns off its stop condition.

* **Line Cleared**
    * Reward applied when the AI clears one or multiple lines.
    * Should be a positive number if the goal is for AIs to get better scores.
* **Time Taken**
    * Reward applied every frame of the level.
    * Should be a positive number to encourage AIs to survive for longer. This number is the reward applied over a second, which means the actual reward per frame is equal to `Multiplier / 60`
    * **Maximum Level Time**
        * Maximum time the level can go on for, in seconds.
        * Stops the training on the current level once this time is reached.
* **Game Over**
    * Reward when the AI is game over.
    * Should be a negative number.
* **Number of holes**
    * Reward applied whenever the AI creates a hole when placing blocks.
    * Should be a negative number.
    * **Stop after X holes**
        * Stops the current level if the AI creates X holes.
        * Set to 0 if creating holes shouldn't stop the current level.

### Neural Configuration

* **Input Nodes**
    * **Tiles**
        * Gives the AI the tiles that have already been placed. The number of rows given is based on `Visible Rows`.
        * When `Use normalized heights`, it instead gives the normalized heights of each columns.
    * **Current Block**
        * Gives the AI what type of block it is currently placing and their rotation.
    * **Current Pos X**
        * Gives the X position of the block being currently placed.
    * **Current Pos Y**
        * Gives the Y position of the block being currently placed.
    * **Bias**
        * A bias node that is always on.
        * It is not recommended to turn this node off.
* **Output Nodes**
    * The output nodes represent each button on an GameBoy controller.
    * `A` : Block rotates 90 degrees clockwise every two frames while this button is held.
    * `B` : Block rotates 90 degress counter-clockwise every two frames while this button is held.
    * `Left` and `Right` : Allows to move the block horizontally
    * `Up` and `Down` : Useless
    * `Left Shoulder` and `Right Shoulder` : Useless
    * `Start` and `Select` : Useless
