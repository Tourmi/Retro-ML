using SharpNeat.BlackBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMW_ML.Game.SuperMarioWorld
{
    internal class InputSetter
    {
        private const bool USE_BIAS = true;
        private const bool USE_POS_X = false;
        private const bool USE_POS_Y = false;
        private const bool USE_IS_ON_GROUND = true;
        private const bool USE_IS_IN_WATER = true;
        private const bool USE_IS_RAISING = true;
        private const bool USE_IS_SINKING = true;
        private const bool USE_CAN_JUMP_OUT_OF_WATER = true;
        private const bool USE_IS_CARRYING_SOMETHING = true;
        private const bool USE_CAN_CLIMB = true;
        private const bool USE_IS_AT_MAX_SPEED = true;
        private const bool USE_INTERNAL_CLOCK = true;
        private const bool USE_WAS_DIALOG_TRIGGERED = true;

        private const bool USE_TILE_GRID = true;
        private const bool USE_ENEMY_GRID = false;

        /// <summary>
        /// Horizontal distance in tiles from Mario to check
        /// </summary>
        private const int GRID_X_DIST = 5;
        /// <summary>
        /// Vertical distance in tiles from Mario to check
        /// </summary>
        private const int GRID_Y_DIST = 5;
        private const int GRID_WIDTH = GRID_X_DIST * 2 + 1;
        private const int GRID_HEIGHT = GRID_Y_DIST * 2 + 1;

        private DataGetter dataReader;

        public InputSetter(DataGetter dataReader)
        {
            this.dataReader = dataReader;
        }

        public void SetInputs(IVector<double> inputs)
        {
            int currOffset = 0;
            if (USE_BIAS)
            {
                inputs[currOffset++] = 1; //Bias
            }
            if (USE_POS_X)
            {
                inputs[currOffset++] = dataReader.GetPositionX();
            }
            if (USE_POS_Y)
            {
                inputs[currOffset++] = dataReader.GetPositionY();
            }
            if (USE_IS_ON_GROUND)
            {
                inputs[currOffset++] = dataReader.IsOnGround() ? 1 : 0;
            }
            if (USE_IS_IN_WATER)
            {
                inputs[currOffset++] = dataReader.IsInWater() ? 1 : 0;
            }
            if (USE_IS_RAISING)
            {
                inputs[currOffset++] = dataReader.IsRaising() ? 1 : 0;
            }
            if (USE_IS_SINKING)
            {
                inputs[currOffset++] = dataReader.IsSinking() ? 1 : 0;
            }
            if (USE_CAN_JUMP_OUT_OF_WATER)
            {
                inputs[currOffset++] = dataReader.CanJumpOutOfWater() ? 1 : 0;
            }
            if (USE_IS_CARRYING_SOMETHING)
            {
                inputs[currOffset++] = dataReader.IsCarryingSomething() ? 1 : 0;
            }
            if (USE_CAN_CLIMB)
            {
                inputs[currOffset++] = dataReader.CanClimb() ? 1 : 0;
            }
            if (USE_IS_AT_MAX_SPEED)
            {
                inputs[currOffset++] = dataReader.IsAtMaxSpeed() ? 1 : 0;
            }
            if (USE_INTERNAL_CLOCK)
            {
                inputs[currOffset++] = dataReader.WasInternalClockTriggered() ? 1 : 0;
            }
            if (USE_WAS_DIALOG_TRIGGERED)
            {
                inputs[currOffset++] = dataReader.WasDialogBoxOpened() ? 1 : 0;
            }

            if (USE_TILE_GRID)
            {
                var walkableTiles = dataReader.GetWalkableTilesAroundPosition(GRID_X_DIST, GRID_Y_DIST);
                for (int i = 0; i < GRID_HEIGHT; i++)
                {
                    for (int j = 0; j < GRID_WIDTH; j++)
                    {
                        inputs[currOffset++] = walkableTiles[i, j] ? 1 : 0;
                    }
                }
            }
            if (USE_ENEMY_GRID)
            {
                for (int i = 0; i < GRID_HEIGHT; i++)
                {
                    for (int j = 0; j < GRID_WIDTH; j++)
                    {
                        //TODO : check if enemy at position
                        inputs[currOffset++] = 0;
                    }
                }
            }
        }

        public int GetInputCount()
        {
            int count = 0;
            if (USE_BIAS) count++;
            if (USE_POS_X) count++;
            if (USE_POS_Y) count++;
            if (USE_IS_ON_GROUND) count++;
            if (USE_TILE_GRID) count += GRID_WIDTH * GRID_HEIGHT;
            if (USE_ENEMY_GRID) count += GRID_WIDTH * GRID_HEIGHT;
            if (USE_IS_IN_WATER) count++;
            if (USE_IS_RAISING) count++;
            if (USE_IS_SINKING) count++;
            if (USE_CAN_JUMP_OUT_OF_WATER) count++;
            if (USE_IS_CARRYING_SOMETHING) count++;
            if (USE_CAN_CLIMB) count++;
            if (USE_IS_AT_MAX_SPEED) count++;
            if (USE_INTERNAL_CLOCK) count++;
            if (USE_WAS_DIALOG_TRIGGERED) count++;

            return count;
        }
    }
}
