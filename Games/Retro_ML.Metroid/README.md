# Metroid Plugin
Plugin by Tourmi
-------
Training AIs to progress through Metroid for the NES.

## Configuration

### Plugin Configuration
* **Use Vision Grid**
  * Whether or not to use the Vision Grid. If turned off, the AI will instead Raycast for its vision.
  * Recommended to leave this off for faster training.
  * Avoid enabling this with `Double Precision`, as the amount of tiles in the grid will be doubled, making the UI extremely slow.
* **Double Precision**
  * Doubles the precision of tile/sprite positions. Will use an `8x8` tile precision instead of an `16x16` one.
* **Use Direction to Goodie**
  * Whether or not to use 2 inputs instead of a grid/raycast for goodies. The 2 inputs will always point to the nearest pickup/powerup, priotizing the nearest powerup before pickups.
* **View Distance**
  * The distance that the AI can see for, in tiles, not including the tile the AI is on.
  * Doubled internally if `Double Precision` is used.
* **Vision ray count**
    * The amount of vision rays to cast.
    * Recommended value : 8-16
* **Internal Clock Tick Length (Frames)**
  * The time (in frames) that it takes for the internal clock to move to its next state.
* **Internal Clock Length**
  * The total amount of nodes the internal clock has.
  * The total time it takes for the clock to do a full cycle is equal to `2^Clock Length * Tick Length` frames.
* **Frames to skip**
  * Amount of frames to skip for every AI evaluation
* **Equipment Toggles**
  * The equipment toggles allow to choose which powerups the AI is aware it has. 
  * Allows to reduce the total amount of neural inputs when disabling equipments that shouldn't change the AI's behaviour, such as the Varia Suit.

### Objectives
* **Died**
  * Reward given to an AI that died. 
  * It is recommended to set this value to a negative value.
* **Time Taken**
    * Reward applied every frame of the save state.
    * Should be a negative number. This number is the reward applied over a second, which means the actual reward per frame is equal to `Multiplier / 60`
    * **Maximum Training Time**
        * Maximum time the training can go on for the current save state, in seconds.
* **Objective Complete**
  * Reward given when the AI kills a boss or collects an item.
  * **Stop on objective reached**
    * Stops the current save state once a boss is killed, or a powerup picked up.
  * **Item Multiplier**
    * Multiplier applied on top of the regular one when collecting an item.
    * Total reward will be `Multiplier * Item Multiplier`
  * **Boss Multiplier**
    * Multiplier applied on top of the regular one when killing a boss.
    * Total reward will be `Multiplier * Boss Multiplier`
  * **Damaged Boss Multiplier**
    * Multiplier applied on top of the regular one for each point of damage done to the boss.
    * Total reward will be `Damage dealt * Multiplier * Damaged Boss Multiplier`
* **Progression**
  * Reward given for each tile traversed towards the current navigation direction.
  * **Maximum time without progress**
    * Stops the current save state if the AI hasn't made progress in this amount of seconds.
    * Disabled if set to 0.
* **Health**
  * Reward given when losing/gaining health.
  * **Lost Health Multiplier**
    * Multiplier applied on top of the regular one for each health point lost.
    * Total reward will be `HP lost * Multiplier * Lost Health Multiplier`
    * Recommended to set this to a negative value to discourage AIs from taking damage.
  * **Gained Health Multiplier**
    * Multiplier applied on top of the regular one for each health point gained.
    * Total reward will be `HP Gained * Multiplier * Gained Health Multiplier`
    * Recommended to set this to a positive value to encourage AIs to heal when they can.

### Neural Configuration
* **Input nodes**
  * Solid Tiles : The tiles the AI can stand on.
  * Dangers : The dangerous tiles around the AI.
  * Goodies : The "good" tiles around the AI.
  * Health : Ratio of health remaining for the AI out of the current maximum health.
  * Missiles : Ratio of missiles remaining for the AI out of the current maximum missiles.
  * X Speed : Ratio of the AI's current X speed out of its maximum horizontal speed.
  * Y Speed : Ratio of the AI's current Y speed out of its maximum vertical speed.
  * Look Direction : The direction the AI is currently looking in. Black : left, White : right.
  * Grounded : Whether or not the AI is touching the ground.
  * In Morph Ball : Whether or not the AI is in morph ball(maru mari) mode
  * Invincible : Whether or not the AI is currently invincible, due to having taken damage from an enemy.
  * Can pass under : Whether or not the AI can pass under the wall in front on it.
  * Door : Whether or not the AI is in front of a door.
  * Missile Door : Whether or not the AI is in front of an unopened missile door.
  * On Elevator : Whether or not the AI in on top of an elevator.
  * Using Missiles : Whether or not the AI is currently using missiles.
  * Equipment : The equipment the AI current has. See `Equipment Toggles` above for more information.
  * Navigation : The direction the AI should be taking from its current position and game state, as well as the objective in the current room (picking up an item/killing a boss)
  * Internal clock : Timed bias value. See `Internal Clock Tick Length (Frames)` and `Internal Clock Length` above
  * Bias : Bias value. Always on.
* **Output Nodes**
  * The output nodes represent each button on an NES controller.
  * `A` : Shoot, use bombs
  * `B` : Jump, disable morph ball
  * `Left` and `Right` : Move left and right.
  * `Up` : Aim up, disable morph ball.
  * `Down` : Toggle morph ball
  * `Start` : Useless
  * `Select` : Toggle missiles
