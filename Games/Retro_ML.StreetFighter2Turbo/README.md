# Street Fighter 2 Turbo plugin
Plugin by Jaewongtongsoup
-------
Training AIs to beat Street Fighter 2 Turbo fights automatically.

## Configuration

### Plugin Configuration
* **Internal Clock Tick Length (Frames)**
  * The time (in frames) that it takes for the internal clock to move to its next state.
* **Internal Clock Length**
  * The total amount of nodes the internal clock has.
  * The total time it takes for the clock to do a full cycle is equal to `2^Clock Length * Tick Length` frames.
 * **Frame Skip**
  * Amount of frames to skip for every AI evaluation

### Objectives

* **Combat**
  * The amount of points to attribute to an AI for its combat prowess at the end of a round. 
  * The amount of points depends on the time of the fight and the HP delta between the AI and the enemy.
  * The score given is equal to ScoreMultiplier * HPDelta (can be negative) * (TimeOfTheFight * TimerInfluence + (1.0 - TimerInfluence)).
  * The TimerInfluence is a multiplier between 0 and 1 that represent the weight carried by the time of the fight in the score calculation.
* **End Round**
  * Points to attribute depending on the outcome of a round. Also influenced by a different multiplier for every outcome (Win, Lose, Draw)
* **Stopped Fighting**
  * Stops the current fight if the AI has not hit the enemy for a specific amount of frames. Used to speed up training by skipping AIs that don't fight.


### Neural Configuration

* **Input nodes**
  * Player Crouched : If the player is in a crouched stance.
  * Enemy Crouched : If the enemy is in a crouched stance.
  * Player Airborn : If the player is airborn.
  * Enemy Airborn : If the enemy is airborn.
  * Player Attacking : If the player is attacking.
  * Enemy Attacking : If the enemy is attacking.
  * Player Punching : If the player is attacking with a punch.
  * Enemy Punching : If the enemy is attacking with a punch.
  * Player Kicking : If the player is attacking with a kick.
  * Enemy Kicking : If the enemy is attacking with a kick.
  * Player Throwing : If the player is attacking with a throw attack.
  * Enemy Throwing : If the enemy is attacking with a throw attack.
  * Player Blocking : If the player is blocking.
  * Enemy Blocking : If the enemy is blocking.
  * Player Staggered : If the player is in a staggered state. Most of the time it is triggered after receiving an attack.
  * Enemy Staggered : If the enemy is in a staggered state. Most of the time it is triggered after receiving an attack.
  * Player Attack Strength : Value between 0 and 1 that represent the strength of the attack used by the player. 1/3 for a light attack, 2/3 for a medium attack and 1 for a strong attack.
  * Enemy Attack Strength : Value between 0 and 1 that represent the strength of the attack used by the enemy. 1/3 for a light attack, 2/3 for a medium attack and 1 for a strong attack.
  * Player X : Player X position on the stage.
  * Enemy X : Enemy X position on the stage.
  * X Delta : X position delta between the player and the enemy.
  * Y Delta : Y position delta between the player and the enemy.
  * Enemy Direction : Boolean set to 1 if the enemy is facing left. 0 if the enemy is facing right.
  * Player Health : Value between 1 and 0 representing the percentage of health remaining to the player.
  * Enemy Health : Value between 1 and 0 representing the percentage of health remaining to the enemy.
  * Time Left : Value between 1 and 0 representing the percentage of the clock remaining.
  * Internal clock : Timed bias value. See `Internal Clock Tick Length (Frames)` and `Internal Clock Length` above
  * Bias : Bias value. Always on.

* **Output Nodes**
  * The output nodes represent each button on an SNES controller.
  * `A` : Middle Kick
  * `B` : Low Kick
  * `X` : Middle Punch
  * `Y` : Low Punch
  * `Left Shoulder` : High Punch
  * `Right Shoulder` : High Kick
  * `Left` : Move Left, blocks if moving away from enemy
  * `Right` : Move Right, blocks if moving away from enemy
  * `Up` : Jump.
  * `Down` : Crouch.
  * `Start` and `Select` : Useless
