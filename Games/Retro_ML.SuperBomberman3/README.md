# Super Bomberman 3
Plugin by Jaewongtongsoup
-------
Training AIs to beat Super Bomberman 3 on SNES

To be noted that this plugin is compatible with the european version of the rom as I wasnt able to find a good US version. The compatibility is not assured with other rom versions.

Right now, the plugin only supports battle mode (there is a single player campaign mode) on stages 1-2-5-8-9. We uses 4 players and 2 minutes timers with other default options.

## Special Thanks

* [Gamefaqs](https://gamefaqs.gamespot.com/snes/564897-super-bomberman-3/faqs/32363): Special thanks to Bryan008. His item FAQ was super useful to scope every powerups and outputs.

## What's left to do / could be improved?

* **Find a better way to track bombs and explosions**
  * Right now, if an enemy and the AI places a bomb on the same frame and the enemy bomb blow up a wall or another enemy, the AI will get credit for the kill / destruction. It should be possible knowing the AI / ennemies explosion expander level to associate dangerous positions on the map when a bomb is dropped. It should fix the problem but would compexify the dataFetcher.
* **Continue to research ram addresses**
  * Right now, many useful addresses in ram have been found. However, lots of work can still be done on this front in order to find better addresses to use. A good example would be the deathTimer addresses used to track when a player dies. There is probably a better addresse to use that keeps track of the number of player alive in the round, but I haven't been able to find it yet.
* **Add support for unconventional map and assets**
  * Right now, we only use the classic 11 x 13 playable tile map for our savestates, but some map uses 13 x 15 playable tiles. The dataFetcher needs to be modified in order to use those special maps. I also discovered that some maps uses special assets like teleporters that can be mapped using the staticTilesMap in the addresses. It would be nice to support these special assets.

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

* **Bomb**
  * Points to attribute when the AI destroy walls or eliminate enemies with its bomb usage. These points are rewarded at the end of the round, it the AI dies or wins.
  * The AI will be rewarded for every walls it destroys and every enemy it kills eliminates.
* **End Round**
  * Points to attribute depending on the outcome of a round. Also influenced by a different multiplier for every outcome (Win, Lose, Draw)
* **Idle**
  * Stops the current round if the AI has not moved for a specific amount of frames. Used to speed up training by skipping AIs that don't move / stays idle.
* **Powerups**
  * Points to attribute when the AI collect a powerup or an upgrade that will help its proficiency during the round.
* **Time Taken**
  * Rewards the AI for its time alive if it lost the round, and reward it even more depending on the time taken if it won the round.
  * Ideally, the AI will be pushed to survived longer in the case of a lost, and win faster in the case of a win.

### Neural Configuration

* **Input nodes**
  * Tile Map : A 11 x 13 Tile Map that mirors the game tile map. Every position is a double value ranging between 0.0 and 1.0. 0.0 represents an empty tile, 0.5 represents a destructible tile and 1.0 represents an undestructible tile. 
  * Dangers: A 11 x 13 Tile Map that mirors the game dangerous tiles. Every position is a double value ranging between 0.0 and 1.0. 1.0 represents explosions / other dangers when the tile is letal to the AI. When a bomb is dropped, the value starts at 0.0 and slowly increment to 0.99 when its explosion coutdown gets closer to the end.
  * Player X : The normalized X position of the player on the map. The value is represented by a double that vary between 0 and 1.
  * Player Y : The normalized X position of the player on the map. The value is represented by a double that vary between 0 and 1.
  * Enemies X Distance : The normalized X distance between the player and every ennemies. The value is represented by a double that vary between -1 and 1. The value is equal to 0.0 if an enemy is dead.
  * Enemies Y Distance : The normalized Y distance between the player and every ennemies. The value is represented by a double that vary between -1 and 1. The value is equal to 0.0 if an enemy is dead.
  * Closest Powerup X Distance : The normalized X distance between the player and the closest Powerup. The value is represented by a double that vary between -1 and 1.
  * Closest Powerup Y Distance : The normalized Y distance between the player and the closest Powerup. The value is represented by a double that vary between -1 and 1.
  * Number Of Bombs Planted : The number of bomb active planted by the AI.
  * Round Timer : Normalized value that represend the round timer remaining.
  * Extra Bomb Level : The number of extra bomb powerup the AI acquired, normalized.
  * Explosion Expander Level : The level of explosion expander of the AI, normalized.
  * Accelerator Level : The accelerator level of the AI, normalized.
  * Has Kick : Boolean that represent if the AI has the kick upgrade.
  * Has Gloves : Boolean that represent if the AI has the glove upgrade.
  * Has Sticky Bomb : Boolean that represent if the AI has the sticky bomb upgrade.
  * Has Power Bomb : Boolean that represent if the AI has the power bomb upgrade.
  * On a Louie : Boolean that represent if the AI is on a Louie.
  * Louie colour : Double that represent the colour of the Louie the AI is riding. Each Louie have different abilities depending on their coloue. 0.2 for a yellow one, 0.4 for a brown one, 0.6 for a pink one, 0.8 for a green one and 1.0 for a blue one.
  * Internal clock : Timed bias value. See `Internal Clock Tick Length (Frames)` and `Internal Clock Length` above
  * Bias : Bias value. Always on.

* **Output Nodes**
  * The output nodes represent each button on an SNES controller.
  * `A` : Used to drop bombs. When you lay a bomb you can press A to pick it up with the glove upgrade, and by releasing A you will throw the bomb over blocks.
  * `X` : If you press yourself against a bomb with the kick upgrade, the bomb will slide until it hits an obstacle or you press the X button.
  * `Y` : The Y button is used when riding a Louie to activate its ability.
  * `Left` : Move Left.
  * `Right` : Move Right.
  * `Up` : Move Up.
  * `Down` : Move Down.



