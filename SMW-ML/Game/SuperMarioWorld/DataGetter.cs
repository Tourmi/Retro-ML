using SMW_ML.Emulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SMW_ML.Game.SuperMarioWorld.Addresses;

namespace SMW_ML.Game.SuperMarioWorld
{
    internal class DataGetter
    {
        private const int INTERNAL_CLOCK_LENGTH = 3;

        private readonly IEmulatorAdapter emulator;
        private readonly Dictionary<uint, byte[]> frameCache;
        private readonly Dictionary<uint, byte[]> levelCache;

        private int internal_clock_timer = INTERNAL_CLOCK_LENGTH;

        public DataGetter(IEmulatorAdapter emulator)
        {
            this.emulator = emulator;
            frameCache = new();
            levelCache = new();
        }

        /// <summary>
        /// Needs to be called every frame to reset the memory cache
        /// </summary>
        public void NextFrame()
        {
            frameCache.Clear();
            internal_clock_timer--;
            if (internal_clock_timer < 0)
            {
                internal_clock_timer = INTERNAL_CLOCK_LENGTH;
            }
        }

        /// <summary>
        /// Needs to be called every level to reset the level cache.
        /// </summary>
        public void NextLevel()
        {
            levelCache.Clear();
            internal_clock_timer = INTERNAL_CLOCK_LENGTH;
        }

        public uint GetPositionX() => ToUnsignedInteger(Read(Player.PositionX));
        public uint GetPositionY() => ToUnsignedInteger(Read(Player.PositionY));
        public bool IsOnGround() => ReadSingle(Player.IsOnGround) != 0 || ReadSingle(Player.IsOnSolidSprite) != 0;
        public bool CanAct() => ReadSingle(Player.PlayerAnimationState) == Player.PlayerAnimationStates.NONE;
        public bool IsDead() => ReadSingle(Player.PlayerAnimationState) == Player.PlayerAnimationStates.DYING;
        public bool IsInWater() => ReadSingle(Player.IsInWater) != 0;
        public bool CanJumpOutOfWater() => ReadSingle(Player.CanJumpOutOfWater) != 0;
        public bool IsSinking() => ReadSingle(Player.AirFlag) == 0x24;
        public bool IsCarryingSomething() => ReadSingle(Player.IsCarryingSomething) != 0;
        public bool CanClimb() => (ReadSingle(Player.CanClimb) & 0b00001011 | ReadSingle(Player.CanClimbOnAir)) != 0;
        public bool IsAtMaxSpeed() => ReadSingle(Player.DashTimer) == 0x70;
        public bool WasInternalClockTriggered() => internal_clock_timer == 0;
        public bool WasDialogBoxOpened() => ReadSingle(Level.TextBoxTriggered) != 0;

        private byte ReadSingle(AddressData addressData) => Read(addressData)[0];

        private byte[] Read(AddressData addressData)
        {
            var cacheToUse = frameCache;
            if (addressData.CacheDuration == AddressData.CacheDurations.Level)
            {
                cacheToUse = levelCache;
            }
            if (!cacheToUse.ContainsKey(addressData.Address))
            {
                cacheToUse[addressData.Address] = emulator.ReadMemory(addressData.Address, addressData.Length);
            }

            return cacheToUse[addressData.Address];
        }

        private static uint ToUnsignedInteger(byte[] bytes)
        {
            uint value = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                value += (uint)bytes[i] << i * 8;
            }
            return value;
        }
    }
}
