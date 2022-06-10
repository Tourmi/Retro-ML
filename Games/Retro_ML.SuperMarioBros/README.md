# Super Mario Kart Plugin
Plugin by Tourmi & Jaewongtongsoup
-------
Training AIs to beat Super Mario Bros levels automatically.

## Configuration

### Plugin Configuration
* **Use Vision Grid**
  * Whether or not to use the Vision Grid. If turned off, the AI will instead Raycast for its vision.
  * Recommended to leave this off for faster training.
* **View distance horizontal** `VDH`
  * The horizontal distance that the AI can see for, in tiles, not including the tile the AI is on. This means that if we set both the horizontal and vertical distances to 4, a 9x9 grid of inputs will be used.
* **View distance vertical** `VDV`
  * The vertical distance that the AI can see for, in tiles, not including the tile the AI is on. This means that if we set both the horizontal and vertical distances to 4, a 9x9 grid of inputs will be used.
* **View Ray Distance**
  * The total distance to cast rays, in tiles.
* **Vision ray count**
    * The amount of vision rays to cast.
    * Recommended value : 16
* **Internal Clock Tick Length (Frames)**
  * The time (in frames) that it takes for the internal clock to move to its next state.
* **Internal Clock Length**
  * The total amount of nodes the internal clock has.
  * The total time it takes for the clock to do a full cycle is equal to `2^Clock Length * Tick Length` frames.

### Objectives

* **Died**
  * The amount of points to attribute to an AI that died. 
  * It is recommended to set this value to a negative value to discourage AIs from killing themselves.
* **Distance travelled**
  * Points to attribute for each tile the AI traverses. This is based on the maximum distance, so going back and forth will not give more points.
  * The AI must be grounded for points to be attributed, so jumping down a pit will not give extra points.
  * East, West, Up, Down multipliers
    * Specific multipliers for distance traveled in the respective directions. These are multiplied with the objective's multiplier.
* **Stopped moving**
  * Stops the current level if the AI has stopped progressing through the level. This is based on the maximum distance reached so far, not the current position.
  * It is recommended to leave this enabled, but you may disable it if you want AIs to stay on the level for up to the maximum duration.
  * A negative amount of points is recommended to discourage the AI from idling, moving back and forth, and looping forever.
* **Time taken**
  * Gives points when the AI takes way too long to complete a level. Recommended to set to a negative value and leave enabled in case AIs decide to take way too much time on a level.
* **Won level**
  * The amount of points to attribute if the AI wins a level. Ideally, this should be a high value to encourage actually finishing levels.
* **Coins**
  * The amount of points to give the AI per coin collected.
* **High Score**
  * The amount of points to give the AI for its in-game high-score.
  * The formula is `(In-game High Score / 1000) * Multiplier`
  * So a high-score of 55200 with a multiplier of 2 will give a total amount of points of 110.4 to the AI.
* **Power Up**
  * Points given whenever the AI collects a power up it didn't have yet.
  * Mushroom, Flower multipliers, Star and 1-up
    * Additional multiplier for the type of power-up collected. If the global multiplier is 4, and the star multiplier is 2, then the AI is awarded 8 points for collecting a star.
* **Taken Damage**
  * Points applied whenever an AI takes damage, not counting dying. 

### Neural Configuration

* **Input nodes**
  * Tiles : The tiles the AI can stand on. `(VDH * 2 + 1) * (VDV * 2 + 1)` total nodes.
  * Dangers : The dangerous tiles around the AI. Includes dangerous tiles as well as dangerous sprites. `(VDH * 2 + 1) * (VDV * 2 + 1)` total nodes.
  * Goodies : The "good" tiles around the AI. Includes coins, powerups, blocks that contain items. `(VDH * 2 + 1) * (VDV * 2 + 1)` total nodes.
  * Water : The water tiles around the AI. `(VDH * 2 + 1) * (VDV * 2 + 1)` total nodes.
  * On ground : Whether or not the AI is touching the ground.
  * In water : Whether or not the AI is in water
  * Max Speed : Whether or not the AI has reached maximum speed.
  * Internal clock : Timed bias value. Alternates between on and off every couple frames.
  * Bias : Bias value. Always on.
* **Output Nodes**
  * The output nodes represent each button on an SNES controller.
  * `A` : Jump
  * `B` : Run or throw fireballs
  * `Left` and `Right` : Move left and right.
  * `Up` : Allows climbing on vines.
  * `Down` : Allows crouching if mario is in Big state. Also allows Mario to go down pipes or in water levels.
  * `Start` and `Select` : Useless
