namespace Retro_ML.StreetFighter2Turbo.Game
{
    /// <summary>
    /// RAM addresses used in Street Fighter 2 Turbo
    /// 
    /// </summary>
    internal static class Addresses
    {
        public struct AddressData
        {

            public AddressData(uint address, uint length)
            {
                Address = address;
                Length = length;
            }

            public uint Address;
            public uint Length;
        }

        public static class GameAddresses
        {
            /// <summary>
            /// Round Timer, In Hex from 0x99 to 0x00. Represents seconds.
            /// </summary>
            public static readonly AddressData RoundTimer = new(0x18F3, 1);
        }

        public static class Player1Addresses
        {
            /// <summary>
            /// Player X position
            /// </summary>
            public static readonly AddressData XPos = new(0x506, 3);
            /// <summary>
            /// Player Y position
            /// </summary>
            public static readonly AddressData YPos = new(0x509, 2);
            /// <summary>
            /// Player state
            /// <code>
            /// 0x02 when crouching
            /// 0x04 when jumping
            /// 0x0A when attacking
            /// 0x0C when using a super attack
            /// 0x0E when hit
            /// 0x14 when staggered
            /// </code>
            /// </summary>
            public static readonly AddressData State = new(0x503, 1);
            /// <summary>
            /// Player input
            /// 0x03 when going backward (blocking)
            /// </summary>
            public static readonly AddressData Input = new(0x53A, 1);
            /// <summary>
            /// Player HP. Starts at 0xB0 and set to 0xFF when dead
            /// </summary>
            public static readonly AddressData HP = new(0x530, 1);
            /// <summary>
            /// Set to 1 if the round is over, could be either via K.O or run out of time.
            /// </summary>
            public static readonly AddressData EndRoundStatus = new(0x5F2, 1);
            /// <summary>
            /// Strength of the attack of Player 1
            /// <code>
            /// 0x0000 = Close Light Attack
            /// 0x0001 = Light Attack
            /// 0x0200 = Medium Grab
            /// 0x0202 = Close Medium Attack
            /// 0x0203 = Medium Attack
            /// 0x0402 = Strong Grab
            /// 0x0404 = Close Strong Attack
            /// 0x0405 = Strong Attack
            /// </code>
            /// </summary>
            public static readonly AddressData AttackStrength = new(0x5E5, 2);
            /// <summary>
            /// Is Player 1 attack a kick or a punch?
            /// 0x00 = Punch
            /// 0x02 = Kick
            /// </summary>
            public static readonly AddressData AttackType = new(0x5E7, 1);
        }

        public static class Player2Addresses
        {
            /// <summary>
            /// Player X position
            /// </summary>
            public static readonly AddressData XPos = new(0x706, 3);
            /// <summary>
            /// Player Y position
            /// </summary>
            public static readonly AddressData YPos = new(0x709, 2);
            /// <summary>
            /// Player state
            /// <code>
            /// 0x02 when crouching
            /// 0x04 when jumping
            /// 0x0A when attacking
            /// 0x0C when using a super attack
            /// 0x0E when hit
            /// 0x14 when staggered
            /// </code>
            /// </summary>
            public static readonly AddressData State = new(0x703, 1);
            /// Player input
            /// 0x03 when going backward (blocking)
            /// </summary>
            public static readonly AddressData Input = new(0x73A, 1);
            /// <summary>
            /// Player HP. Starts at 0xB0 and set to 0xFF when dead
            /// </summary>
            public static readonly AddressData HP = new(0x730, 1);
            /// <summary>
            /// Set to 1 if the round is over, could be either via K.O or run out of time.
            /// </summary>
            public static readonly AddressData EndRoundStatus = new(0x7F2, 1);
            /// <summary>
            /// Strength of the attack of Player 2
            /// <code>
            /// 0x0000 = Close Light Attack
            /// 0x0001 = Light Attack
            /// 0x0200 = Medium Grab
            /// 0x0202 = Close Medium Attack
            /// 0x0203 = Medium Attack
            /// 0x0402 = Strong Grab
            /// 0x0404 = Close Strong Attack
            /// 0x0405 = Strong Attack
            /// </code>
            /// </summary>
            public static readonly AddressData AttackStrength = new(0x7E5, 2);
            /// <summary>
            /// Is Player 2 attack a kick or a punch?
            /// 0x00 = Punch
            /// 0x02 = Kick
            /// </summary>
            public static readonly AddressData AttackType = new(0x7E7, 1);
        }
    }
}
