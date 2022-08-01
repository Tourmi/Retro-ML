# Super Mario 64 Plugin
Plugin by Tourmi
-------
Training AIs to progress through Super Mario 64 on the Nintendo 64.

### Special Thanks
* [STROOP contributors](https://github.com/SM64-TAS-ABC/STROOP): Without this tool, it would have been extremely painful to figure out most of this game's memory addresses and behaviours.
* [Hack64](https://hack64.net/wiki/doku.php?id=super_mario_64:ram_memory_map): Useful quick reference for in-game memory addresses.
* [pannenkoek2012](https://www.youtube.com/user/pannenkoek2012) ([2nd channel](https://www.youtube.com/user/pannenkeok2012)): Useful videos explaining how Super Mario 64 works behind the scenes.

## Configuration

### Plugin Configuration
* **View Distance**
  * Distance in units that Mario can see.
* **Solid Vertical Rays**
  * Amount of rows of rays to cast for Mario's vision, vertically.
* **Solid Vertical View Angle**
  * Total angle Mario can see in, vertically. Rays are spread evenly through this range, always passing through angle 0.
  * The maximum value is 180, allowing to see up to straight up and down.
  * Note that, currently, rays are mapped linearly, meaning that if this is set to 180, all rays for the top and bottom rows will be identical, due to all of them pointing straight up and down.
* **Solid Horizontal Rays**
  * Amount of rays to cast for Mario's vision, per row.
* **Solid Horizontal View Angle**
  * Radius Mario can see around him, in degrees.
  * Setting this to 180 allows Mario to see directly to the left and right of him
  * Setting this to 360 allows Mario to see directly behind him. However, 2 rays will both point to straight behind him for every row in that case.
* **Internal Clock Tick Length (Frames)**
  * The time (in frames) that it takes for the internal clock to move to its next state.
* **Internal Clock Length**
  * The total amount of nodes the internal clock has.
  * The total time it takes for the clock to do a full cycle is equal to `2^Clock Length * Tick Length` frames.
* **Frames to skip**
  * Amount of frames to skip after every evaluation.
  * Eg : setting this to 1 will skip every other frame.
  * For better performance in SM64, it is recommended to set this to somewhere in the range of `[2-5]`

### Objectives
* **Death**
  * Reward applied every time Mario dies.
  * Will jump the training to the next save state when Mario dies.
* **Stars Collected**
  * Reward applied every time Mario collects a **Yellow** star.
  * The application does not detect when Mario collects a blue star.
* **Distance to star**
  * Reward applied for each unit Mario gets closer to a star.
  * The chosen star is the nearest loaded  **yellow** or **blue** star to Mario
  * **Getting Further Away Multiplier**
    * If Mario moves away from the star, multiplies his distance delta reward by this value.
    * Recommended to set to a value between `]0-1[` to penalize Mario less than when he moves closer to the star.
    * Set to `1` for a perfect balance between getting closer than moving away
    * Set to a value higher than `1` to penalize Mario more than his reward for getting closer whenever he moves further away.
  * **Distance Timeout**
    * Time in seconds before timing out Mario for not getting closer to a star.
    * If a star was never loaded, then this timeout does not apply.
* **Exploration**
  * Reward to give Mario for exploring new territory.
  * When Mario is not grounded, his exploration is paused (but is still accumulated) until he touches the ground, meaning that jumping in a pit will never reward Mario since he dies before touching the ground.
  * **Distance to cross**
    * Distance in units that Mario needs to cross before rewarding him.
  * **Exploration Timer**
    * Time in seconds before timing out Mario if he hasn't explored a new region.
  * **Elimination Multiplier**
    * Multiplier to apply to the original reward multiplier, whenever Mario is timed out for not exploring a new region.
    * This multiplier should be negative.
* **Time Taken**
  * Reward applied every frame of the save state.
  * Should be a negative number. This number is the reward applied over a second, which means the actual reward per frame is equal to `Multiplier / 60`
  * **Maximum Training Time**
      * Maximum time the training can go on for the current save state, in seconds.
* **Coins Collected**
  * Reward applied for each coin Mario picks up.

### Neural Configuration
* **Input Nodes**
  * **Solid collision** : Collision around Mario. Uses raycasting with the parameters determined higher.
  * **Dangers** : Dangers around Mario. Uses raycasting with the parameters determined higher.
  * **Goodies** : Goodies around Mario. Uses raycasting with the parameters determined higher.
  * **Mario Angle** : Mario's current facing angle.
  * **Mario Speed** : Mario's current speed, in the X Y and Z directions.
  * **Camera Angle** : The angle the camera is currently facing. Needed for the AI to eventually adapt its inputs based on the camera.
  * **Mission star direction** : X Y Z direction the closest star is in.
  * **Is grounded** : Whether or not Mario is currently touching the ground. Note that if Mario dives, he won't be considered grounded until he stands back up.
  * **Is Swimming** : Whether or not Mario is currently swimming. Walking around with a metal cap does not count as swimming.
  * **Health** : Mario's current health
  * **Internal clock** : Timed bias value. See `Internal Clock Tick Length (Frames)` and `Internal Clock Length` above
  * **Bias** : Bias value. Always on.
* **Output Nodes**
  * `A` : Jump, Rollout, etc.
  * `B` : Punch, Kick, Dive, etc.
  * `C buttons` : Controls the camera.
  * `Dpad` : Useless
  * `Z` : Crouch, Ground Pound. Followed by a jump, allows for long jumps and back flips.
  * `Left Shoulder` : Useless
  * `Right Shoulder` : Depending on camera mode, switches between Mario Cam and Lakitu Cam, or holds the camera in place. Not very useful to the AI.
  * `Start` : Pauses the game. Useless
  * `Joystick` : Joystick output of the AI.
