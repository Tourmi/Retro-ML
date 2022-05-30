namespace Retro_ML.Tetris.Game
{
    /// <summary>
    /// RAM addresses used in Tetris.
    /// 
    /// </summary>
    internal static class Addresses
    {
        public struct AddressData
        {
            public enum CacheDurations
            {
                Frame,
                Tiles
            }

            public AddressData(uint address, uint length, CacheDurations cacheDuration = CacheDurations.Frame, uint highByteLocation = 0)
            {
                Address = address;
                Length = length;
                CacheDuration = cacheDuration;
                HighByteAddress = highByteLocation;
            }

            public uint Address;
            public uint Length;
            public CacheDurations CacheDuration;
            public uint HighByteAddress;
        }

        /// <summary>
        /// Where background tiles start in VRAM
        /// </summary>
        public static readonly AddressData BackgroundTiles = new(0x9800, 1024, AddressData.CacheDurations.Tiles);

        /// <summary>
        /// Indicate the current state of the game
        /// <br>00 = in-game                        </br>
        /// <br>01 = shortly before game over screen</br>
        /// <br>04 = game over screen               </br>
        /// <br>0D = game lost animation            </br>
        /// <br></br>
        /// </summary>
        public static readonly AddressData GameStatus = new AddressData(0x00ffe1, 1);

        public class Score
        {
            public static readonly AddressData Single = new(0x00c0ac, 4);
            public static readonly AddressData Double = new(0x00c0b1, 4);
            public static readonly AddressData Triple = new(0x00c0b6, 4);
            public static readonly AddressData Tetris = new(0x00c0bb, 4);
        }

        public class CurrentBlock
        {
            public static readonly AddressData PosX = new(0x00c202, 1);

            public static readonly AddressData PosY = new(0x00c201, 1);

            /// <summary>
            /// Higher numbers mean counter-clockwise rotation.
            /// <br>0x00:L Shape </br>
            /// <br>0x01:L Shape </br>
            /// <br>0x02:L Shape </br>
            /// <br>0x03:L Shape </br>
            /// <br></br>
            /// <br>0x04:Reverse L Shape </br>
            /// <br>0x05:Reverse L Shape </br>
            /// <br>0x06:Reverse L Shape </br>
            /// <br>0x07:Reverse L Shape </br>
            /// <br></br>
            /// <br>0x08:I Shape </br>
            /// <br>0x09:I Shape </br>
            /// <br>0x0a:I Shape </br>
            /// <br>0x0b:I Shape </br>
            /// <br></br>
            /// <br>0x0C:Square Shape </br>
            /// <br>0x0D:Square Shape </br>
            /// <br>0x0E:Square Shape </br>
            /// <br>0x0F:Square Shape </br>
            /// <br></br>
            /// <br>0x10:Z Shape </br>
            /// <br>0x11:Z Shape </br>
            /// <br>0x12:Z Shape </br>
            /// <br>0x13:Z Shape </br>
            /// <br></br>
            /// <br>0x14:S Shape </br>
            /// <br>0x15:S Shape </br>
            /// <br>0x16:S Shape </br>
            /// <br>0x17:S Shape </br>
            /// <br></br>
            /// <br>0x18:T Shape </br>
            /// <br>0x19:T Shape </br>
            /// <br>0x1A:T Shape </br>
            /// <br>0x1B:T Shape </br>
            /// </summary>
            public static readonly AddressData Type = new(0x00c203, 1);

            public static readonly AddressData Visibility = new(0x00c200, 1);

        }

        public class NextBlock
        {
            /// <summary>
            /// Higher numbers mean counter-clockwise rotation.
            /// <br>0x00:L Shape </br>
            /// <br>0x01:L Shape </br>
            /// <br>0x02:L Shape </br>
            /// <br>0x03:L Shape </br>
            /// <br></br>
            /// <br>0x04:Reverse L Shape </br>
            /// <br>0x05:Reverse L Shape </br>
            /// <br>0x06:Reverse L Shape </br>
            /// <br>0x07:Reverse L Shape </br>
            /// <br></br>
            /// <br>0x08:I Shape </br>
            /// <br>0x09:I Shape </br>
            /// <br>0x0a:I Shape </br>
            /// <br>0x0b:I Shape </br>
            /// <br></br>
            /// <br>0x0c:Square Shape </br>
            /// <br>0x0d:Square Shape </br>
            /// <br>0x0e:Square Shape </br>
            /// <br>0x0f:Square Shape </br>
            /// <br></br>
            /// <br>0x10:Z Shape </br>
            /// <br>0x11:Z Shape </br>
            /// <br>0x12:Z Shape </br>
            /// <br>0x13:Z Shape </br>
            /// <br></br>
            /// <br>0x14:S Shape </br>
            /// <br>0x15:S Shape </br>
            /// <br>0x16:S Shape </br>
            /// <br>0x17:S Shape </br>
            /// <br></br>
            /// <br>0x18:T Shape </br>
            /// <br>0x19:T Shape </br>
            /// <br>0x1a:T Shape </br>
            /// <br>0x1b:T Shape </br>
            /// </summary>
            public static readonly AddressData Type = new(0x00c213, 1);
        }
    }
}
