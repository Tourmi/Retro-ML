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
        private const bool USE_POS_X = true;
        private const bool USE_POS_Y = true;
        private const bool USE_IS_ON_GROUND = true;

        private const bool USE_TILE_GRID = false;
        private const bool USE_ENEMY_GRID = false;

        private const int GRID_WIDTH = 11;
        private const int GRID_HEIGHT = 11;

        private DataReader dataReader;

        public InputSetter(DataReader dataReader)
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
                uint positionX = dataReader.GetPositionX();
                inputs[currOffset++] = positionX;
            }
            if (USE_POS_Y)
            {
                uint positionY = dataReader.GetPositionY();
                inputs[currOffset++] = positionY;
            }
            if (USE_IS_ON_GROUND)
            {
                bool isOnGround = dataReader.IsOnGround();
                inputs[currOffset++] = isOnGround ? 1 : 0;
            }

            if (USE_TILE_GRID)
            {
                for (int i = 0; i < GRID_WIDTH; i++)
                {
                    for (int j = 0; j < GRID_HEIGHT; j++)
                    {
                        // TODO : read tile at position
                        inputs[currOffset++] = 0;
                    }
                }
            }
            if (USE_ENEMY_GRID)
            {
                for (int i = 0; i < GRID_WIDTH; i++)
                {
                    for (int j = 0; j < GRID_HEIGHT; j++)
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

            return count;
        }
    }
}
