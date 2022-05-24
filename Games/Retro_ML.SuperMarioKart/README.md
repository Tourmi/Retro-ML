# Super Mario Kart Plugin
Plugin by Tourmi
-------
Training AIs to beat Super Mario Kart racetracks automatically, and even potentially beat Grand Prix mode.

## Configuration

### Plugin Configuration
* **View distance**
    * Distance in tiles that the AI can see.
    * Recommended value : 16
* **View Angle (Degrees)**
    * The angle at which the AI can see.
    * Recommended value : 150 degrees
    * Note that for values that aren't 360, the forward vision ray will be duplicated due to limitations.
* **Raycast count**
    * The amount of vision rays to use.
    * Recommended value : 8-16
    * The rays will be spread evenly across the `View Angle`
* **Internal Clock Tick Length (Frames)**
    * The time (in frames) that it takes for the internal clock to move to its next state.
* **Internal Clock Length**
    * The total amount of nodes the internal clock has.
    * The total time it takes for the clock to do a full cycle is equal to `2^Clock Length * Tick Length` frames.

### Objectives
Note that for objectives, the term "Reward" also refers to negative rewards, or punishments.
Turning off an objective also turns off its stop condition.

* **Finished Race**
    * Reward applied when the driver finishes a race.
    * Stops training on the current track when reached.
    * Should be a large positive number if the goal is for AIs to finish a race.
    * **Ranking Multiplier**
        * Gives additional points in Grand Prix mode depending on the AI's placing at the end of the race.
        * Bonus points equal to this formula : `(8 - (Ranking - 1)) * Ranking Multiplier * Multiplier`
* **Stopped progressing**
    * Reward applied whenever the driver stops progressing in the racetrack for a certain amount of time.
    * Stops training on the current track when reached.
    * Should be a negative number, or else, the AI might just never move.
    * **Max time w/o progress**
        * The time in seconds before eliminating the AI for not making progress.
        * A value between 4 to 10 seconds is recommended
        * Note that time spent during the initial race countdown does not count as not making progress.
* **Checkpoint Reached**
    * Reward applied when the driver reaches a new checkpoint.
* **Time Taken**
    * Reward applied every frame of the race.
    * Should be a negative number. This number is the reward applied over a second, which means the actual reward per frame is equal to `Multiplier / 60`
    * **Maximum Race Time**
        * Maximum time the race can go on for, in seconds.
        * Stops the training on the current racetrack once this time is reached.
* **Offroad**
    * Reward applied for every frame the driver spends offroad
    * Should be a negative number. This number is the reward applied over a second, which means the actual reward per frame is equal to `Multiplier / 60`
    * **Stop after**
        * Stops the training on the current racetrack after the AI spends this amount of seconds offroad consecutively.
* **Lakitu**
    * Reward applied whenever the AI puts itself in a situation to get picked up by Lakitu
    * Should be a negative number.
    * **Stop after X falls**
        * Stops the current race if the AI falls off-track this amount of times.
        * Set to 0 if falling off shouldn't stop the current race.
* **Collision**
    * Reward applied whenever the AI collides with a wall, another racer, etc.
    * Should be a negative number.
    * **Maximum collisions**
        * Stops the current race if the total collisions reach this number.
        * Set to 0 if colliding shouldn't stop the current race.
* **Coins**
    * Reward applied whenever the AI picks up coins.
    * Useless in time trial
    * Should be a positive number.
    * **Losing coins mult.**
        * Multiplier to apply if the AI loses coins.
        * Reward applied is equal to `Multiplier * Losing coins mult.`
        * Should be a negative value.

### Neural Configuration

* **Input Nodes**
    * **Flowmap direction**
        * Feeds the track's intended path to the AI, corrected by the AI's own heading direction
        * This video by MrL314 at timestamp 3:43 goes into more details as to how flowmaps work. https://youtu.be/oq4Wu1PjukI?t=222
    * **Obstacles**
        * Casts rays that detect obstacles (such as pipes, moles, thwomps, etc) around or in front of the driver.
    * **Racers**
        * Casts rays that detect other racers around the AI.
        * Useless in Time Trial mode
    * **Offroad**
        * Casts rays that detect offroad tiles.
        * Pits and walls also count as offroad tiles, so if there's no need to be specific between the different types of tiles, this node can be enabled, and the other two, left off.
    * **Solid**
        * Casts rays that detect solid tiles. (Walls)
        * Can be left off if we don't need to specifically detect walls, and the Offroad node is enabled.
    * **Pit**
        * Casts rays that detect pits. (Includes deep water and lava)
        * Can be left off if we don't need to specifically detect pits, and the Offroad node is enabled.
    * **Goodies**
        * Casts rays that detect item and coin tiles.
        * Useless in Time Trial mode.
    * **Current Item**
        * The current item the driver is holding.
        * The possible items are, in order :
            * Mushroom
            * Feather
            * Star
            * Banana
            * Green shell
            * Red shell
            * Boo (Battle mode only)
            * Coins
            * Thunder
        * Useless in Time Trial mode.
    * **Speed**
        * The driver's current speed, compared to its current maximum speed.
    * **Internal Clock**
        * A binary clock which allows the AI to do actions on a cycle.
        * This node can easily lead to overfitting, so be careful in its usage.
    * **Bias**
        * A bias node that is always on.
        * It is not recommended to turn this node off.
* **Output Nodes**
    * The output nodes represent each button on an SNES controller.
    * `A` : Useless in Time Trial mode. Allows to use items.
    * `B` : Acceleration
    * `X` : Toggles rear-view mirror, useless for AIs
    * `Y` : Brake
    * `Left` and `Right` : Allows steering
    * `Up` and `Down` : Useless in Time Trial mode. Allows throwing shells backwards and throwing bananas forward.
    * `Left Shoulder` and `Right Shoulder` : Only one is needed. Allows drifting.
    * `Start` and `Select` : Useless
