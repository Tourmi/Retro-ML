using SMW_ML.Emulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SMW_ML.Game.SuperMarioWorld.Addresses;

namespace SMW_ML.Game.SuperMarioWorld
{
    internal class DataReader
    {

        private readonly IEmulatorAdapter emulator;

        public DataReader(IEmulatorAdapter emulator)
        {
            this.emulator = emulator;
        }

        public uint GetPositionX() => ToUnsignedInteger(Read(Player.PositionX));
        public uint GetPositionY() => ToUnsignedInteger(Read(Player.PositionY));
        public bool IsOnGround() => Read(Player.IsOnGround)[0] != 0;
        public bool CanAct() => Read(Player.PlayerAnimationState)[0] == Player.PlayerAnimationStates.NONE;
        public bool IsDead() => Read(Player.PlayerAnimationState)[0] == Player.PlayerAnimationStates.DYING;

        private byte[] Read(AddressData addressData) => emulator.ReadMemory(addressData.Address, addressData.Length);

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
