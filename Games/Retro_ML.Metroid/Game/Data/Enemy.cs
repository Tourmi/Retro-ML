using static Retro_ML.Metroid.Game.Addresses;

namespace Retro_ML.Metroid.Game.Data
{
    internal class Enemy
    {
        public byte XPos { get; }
        public byte YPos { get; }
        public byte Health { get; }

        public byte Status { get; }
        public bool IsInFirstScreen { get; }

        public Enemy(byte[] baseBytes, byte[] extraBytes)
        {
            XPos = baseBytes[Enemies.EnemyPosX.Address - Enemies.AllBaseEnemies.Address];
            YPos = baseBytes[Enemies.EnemyPosY.Address - Enemies.AllBaseEnemies.Address];
            Health = baseBytes[Enemies.EnemyHitpoints.Address - Enemies.AllBaseEnemies.Address];

            Status = extraBytes[Enemies.Status.Address - Enemies.AllExtraEnemies.Address];
            IsInFirstScreen = extraBytes[Enemies.NameTable.Address - Enemies.AllExtraEnemies.Address] == 0;
        }

        public bool IsAlive()
        {
            return Health > 0 && Status != 0x00;
        }
    }
}
