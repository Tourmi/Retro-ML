# Pokemon Red/Blue/Yellow Plugin
Plugin by Voltage
-------
Training AIs that learn how to win fights in Pokemon

### Special Thanks
* [Data Crystal](https://datacrystal.romhacking.net/wiki/Pok%C3%A9mon_Red/Blue:RAM_map): Reference for in-game memory addresses.
* [pokered contributors](https://github.com/pret/pokered): For the Pokemon disassembly 

## Configuration

### Plugin Configuration
* **Number of fights**
    * The number of fights the AI will do for each save states selected
    * Recommended value : 5 attempts
    * The higher this value is, the risk of eliminating a good AI is going to be lower.
    * A high value will also lengthen the training of each generations.

### Objectives
Note that for objectives, the term "Reward" also refers to negative rewards, or punishments.
Turning off an objective also turns off its stop condition.

* **Won Fight**
    * Reward applied when the AI wins a fight.
    * Should be a positive number.
* **Lost Fight**
    * Reward applied when the AI loses a fight.
    * Should be a negative number.
* **Fight Cancelled**
    * Reward the AI/enemy uses a move that cancels the fight(ex: Teleport, Whirlwind, Roar, etc.).
    * Should be a negative number if we want the AI to avoid to force the AI to fight or to stop the enemy pokemon from leaving the fight.

### Neural Configuration

* **Input Nodes**
    * **Move super effective**
        * Tells the AI if the current move is super effective.
    * **Move not very effective**
        * Tells the AI if the current move is not very effective.
    * **Move Power**
        * Normalized power of the current move.
    * **STAB**
        * Tells the AI if the current move is STAB(Same type attack bonus).
        * A STAB move has an effectiveness of 1.5 its power.
    * **HP**
        * Tells the AI its current HP normalized.
    * **Opponent HP**
        * Tells the AI its opponent current HP normalized.
    * **Attack Stat**
        * Tells the AI its attack stat normalized.
    * **Defense Stat**
        * Tells the AI its defense stat normalized.
    * **Speed Stat**
        * Tells the AI its speed stat normalized.
    * **Special Stat**
        * Tells the AI its special stat normalized.
    * **Attack Modifier**
        * Tells the AI its attack modifier normalized between -1 and 1.
        * A negative value(black circle) means its attack is nerfed and a positive value means it's buffed.
    * **Defense Modifier**
        * Tells the AI its defense modifier normalized between -1 and 1.
        * A negative value(black circle) means its defense is nerfed and a positive value means it's buffed.
    * **Speed Modifier**
        * Tells the AI its speed modifier normalized between -1 and 1.
        * A negative value(black circle) means its speed is nerfed and a positive value means it's buffed.
    * **Special Modifier**
        * Tells the AI its special modifier normalized between -1 and 1.
        * A negative value(black circle) means its special is nerfed and a positive value means it's buffed.
    * **Accuracy Modifier**
        * Tells the AI its accuracy modifier normalized between -1 and 1.
        * A negative value(black circle) means its accuracy is nerfed and a positive value means it's buffed.
    * **Evasion Modifier**
        * Tells the AI its evasion modifier normalized between -1 and 1.
        * A negative value(black circle) means its evasion is nerfed and a positive value means it's buffed.
    * **Opponent Attack Stat**
        * Tells the AI the opponent attack stat normalized.
    * **Opponent Defense Stat**
        * Tells the AI the opponent defense stat normalized.
    * **Opponent Speed Stat**
        * Tells the AI the opponent speed stat normalized.
    * **Opponent Special Stat**
        * Tells the AI the opponent special stat normalized.
    * **Opponent Attack Modifier**
        * Tells the AI the opponent attack modifier normalized between -1 and 1.
        * A negative value(black circle) means the opponent attack is nerfed and a positive value means it's buffed.
    * **Opponent Defense Modifier**
        * Tells the AI the opponent defense modifier normalized between -1 and 1.
        * A negative value(black circle) means the opponent defense is nerfed and a positive value means it's buffed.
    * **Opponent Speed Modifier**
        * Tells the AI the opponent speed modifier normalized between -1 and 1.
        * A negative value(black circle) means the opponent speed is nerfed and a positive value means it's buffed.
    * **Opponent Special Modifier**
        * Tells the AI the opponent special modifier normalized between -1 and 1.
        * A negative value(black circle) means the opponent special is nerfed and a positive value means it's buffed.
    * **Opponent Accuracy Modifier**
        * Tells the AI the opponent accuracy modifier normalized between -1 and 1.
        * A negative value(black circle) means the opponent accuracy is nerfed and a positive value means it's buffed.
    * **Opponent Evasion Modifier**
        * Tells the AI the opponent evasion modifier normalized between -1 and 1.
        * A negative value(black circle) means the opponent evasion is nerfed and a positive value means it's buffed.
    * **Opponent Sleeping**
        * Tells the AI if the opponent is asleep.
    * **Opponent Paralyzed**
        * Tells the AI if the opponent is paralyzed.
    * **Opponent Frozen**
        * Tells the AI if the opponent is frozen.
    * **Opponent Burned**
        * Tells the AI if the opponent is burned.
    * **Opponent Poisoned**
        * Tells the AI if the opponent is poisoned.
    * **Bias**
        * A bias node that is always on.
        * It is not recommended to turn this node off.
* **Output Nodes**
    * `Current Move Score` : Represents the score given to the move currently selected by the AI.
