using SMW_ML.Emulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMW_ML.Game.SuperMarioWorld
{
    internal class DataReader
    {
        private struct AddressData
        {
            public AddressData(uint address, uint length, AddressType addressType)
            {
                Address = address;
                Length = length;
                AddressType = addressType;
            }

            public uint Address;
            public uint Length;
            public AddressType AddressType;
        }

        private enum AddressType
        {
            Integer
        }

        private static class Addresses
        {
            public static readonly AddressData PositionX = new(0x000094, 2, AddressType.Integer);
            public static readonly AddressData PositionY = new(0x000096, 2, AddressType.Integer);
            public static readonly AddressData IsOnGround = new(0x0013EF, 1, AddressType.Integer);
            public static readonly AddressData PlayerAnimationState = new(0x000071, 1, AddressType.Integer);
        }

        private static class PlayerAnimationStates
        {
            public const byte NONE = 0x00;
            public const byte FLASHING = 0x01;
            public const byte GET_MUSHROOM = 0x02;
            public const byte GET_FEATHER = 0x03;
            public const byte GET_FIRE_FLOWER = 0x04;
            public const byte ENTER_HORIZONTAL_PIPE = 0x05;
            public const byte ENTER_VERTICAL_PIPE = 0x06;
            public const byte SHOOT_FROM_PIPE = 0x07;
            public const byte YOSHI_WINGS = 0x08;
            public const byte DYING = 0x09;
            public const byte CASTLE_ENTRANCE_MOVE = 0x0A;
            public const byte PLAYER_FROZEN = 0x0B;
            public const byte CASTLE_DESTRUCTIONS_MOVE = 0x0C;
            public const byte ENTER_DOOR = 0x0D;
        }

        private readonly IEmulatorAdapter emulator;

        public DataReader(IEmulatorAdapter emulator)
        {
            this.emulator = emulator;
        }

        public uint GetPositionX() => ToUnsignedInteger(Read(Addresses.PositionX));
        public uint GetPositionY() => ToUnsignedInteger(Read(Addresses.PositionY));
        public bool IsOnGround() => Read(Addresses.IsOnGround)[0] != 0;
        public bool CanAct() => Read(Addresses.PlayerAnimationState)[0] == PlayerAnimationStates.NONE;
        public bool IsDead() => Read(Addresses.PlayerAnimationState)[0] == PlayerAnimationStates.DYING;

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
