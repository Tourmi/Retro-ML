using static Retro_ML.Metroid.Game.Addresses;

namespace Retro_ML.Metroid.Game.Data
{
    internal class Sprite
    {
        public byte XPos { get; }
        public byte YPos { get; }
        public byte Health { get; }
        public byte DespawnTimer { get; }

        public byte Status { get; }
        public bool IsInFirstScreen { get; }

        public Sprite(byte[] baseBytes, byte[] extraBytes)
        {
            XPos = baseBytes[Sprites.PosX.Address - Sprites.AllBaseSprites.Address];
            YPos = baseBytes[Sprites.PosY.Address - Sprites.AllBaseSprites.Address];
            Health = baseBytes[Sprites.Hitpoints.Address - Sprites.AllBaseSprites.Address];
            DespawnTimer = baseBytes[Sprites.DespawnTimer.Address - Sprites.AllBaseSprites.Address];

            Status = extraBytes[Sprites.Status.Address - Sprites.AllExtraSprites.Address];
            IsInFirstScreen = extraBytes[Sprites.NameTable.Address - Sprites.AllExtraSprites.Address] == 0;
        }

        public bool IsAlive()
        {
            return Health > 0 && Status != 0x00;
        }

        public bool IsPickup()
        {
            return Status != 0x00 && DespawnTimer > 1;
        }
    }
}
