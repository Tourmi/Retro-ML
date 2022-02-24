using System;
using System.Drawing;

namespace SMW_ML.Game.SuperMarioWorld.Data
{
    internal struct Sprite
    {
        private const byte SPRITE_CLIPPING_MASK = 0b0011_1111;

        /// <summary>
        /// <br>Huge thanks to imamelia @ SMWCentral (<see href="https://www.smwcentral.net/?p=section&amp;a=details&amp;id=4749"/>). </br>
        /// <br>Ordered array which contains all the possible sprite clippings under this format</br>
        /// <br>Index 0: Width, 1: Height, 2: x offset, 3: y offset</br>
        /// <br>Since the values are in bytes, and the offsets can be negative, make sure to cast the offsets to signed bytes before use</br>
        /// </summary>
        private static readonly byte[][] SpriteClippings = new byte[][]
        {
            new byte[] { 0x0C, 0x0A, 0x02, 0x03 }, //00
            new byte[] { 0x0C, 0x15, 0x02, 0x03 }, //01
            new byte[] { 0x10, 0x12, 0x10, 0xFE },
            new byte[] { 0x08, 0x08, 0x14, 0x08 },
            new byte[] { 0x30, 0x0E, 0x00, 0xFE },
            new byte[] { 0x50, 0x0E, 0x00, 0xFE },
            new byte[] { 0x0E, 0x18, 0x01, 0x02 },
            new byte[] { 0x28, 0x30, 0x08, 0x08 },
            new byte[] { 0x20, 0x10, 0xF8, 0xFE },
            new byte[] { 0x14, 0x1E, 0xFE, 0x08 },
            new byte[] { 0x01, 0x02, 0x03, 0x07 },
            new byte[] { 0x03, 0x03, 0x06, 0x06 },
            new byte[] { 0x0D, 0x16, 0x01, 0xFE },
            new byte[] { 0x0F, 0x10, 0x00, 0xFC },
            new byte[] { 0x14, 0x14, 0x06, 0x06 },
            new byte[] { 0x24, 0x12, 0x02, 0xFE },
            new byte[] { 0x0F, 0x20, 0x00, 0xFE },
            new byte[] { 0x40, 0x40, 0xE8, 0xE8 },
            new byte[] { 0x08, 0x34, 0xFC, 0x10 },
            new byte[] { 0x08, 0x74, 0xFC, 0x10 },
            new byte[] { 0x18, 0x0C, 0x04, 0x02 },
            new byte[] { 0x0F, 0x0E, 0x00, 0xFE },
            new byte[] { 0x18, 0x18, 0xFC, 0xF4 },
            new byte[] { 0x0C, 0x45, 0x02, 0x08 },
            new byte[] { 0x0C, 0x3A, 0x02, 0x13 },
            new byte[] { 0x0C, 0x2A, 0x02, 0x23 },
            new byte[] { 0x0C, 0x1A, 0x02, 0x33 },
            new byte[] { 0x0C, 0x0A, 0x02, 0x43 },
            new byte[] { 0x0A, 0x30, 0x00, 0x0A },
            new byte[] { 0x1C, 0x1B, 0x02, 0xFD },
            new byte[] { 0x30, 0x20, 0xE0, 0xF8 },
            new byte[] { 0x30, 0x12, 0xF0, 0xFC },
            new byte[] { 0x08, 0x18, 0xFC, 0xE8 },
            new byte[] { 0x08, 0x18, 0xFC, 0x10 },
            new byte[] { 0x10, 0x10, 0x00, 0x00 },
            new byte[] { 0x20, 0x20, 0xF8, 0xE8 },
            new byte[] { 0x38, 0x38, 0xF4, 0x20 },
            new byte[] { 0x3C, 0x14, 0xF2, 0x04 },
            new byte[] { 0x20, 0x08, 0x00, 0x58 },
            new byte[] { 0x18, 0x18, 0xFC, 0xFC },
            new byte[] { 0x1C, 0x28, 0xF2, 0xE8 },
            new byte[] { 0x20, 0x1B, 0xF0, 0xFC },
            new byte[] { 0x0C, 0x13, 0x02, 0xF8 },
            new byte[] { 0x10, 0x4C, 0x00, 0x02 },
            new byte[] { 0x10, 0x10, 0xF8, 0xF8 },
            new byte[] { 0x08, 0x04, 0x04, 0x04 },
            new byte[] { 0x1C, 0x22, 0x02, 0xFE },
            new byte[] { 0x1C, 0x20, 0x02, 0xFE },
            new byte[] { 0x10, 0x1C, 0x08, 0xF2 },
            new byte[] { 0x30, 0x12, 0x00, 0xFE },
            new byte[] { 0x30, 0x12, 0x00, 0xFE },
            new byte[] { 0x40, 0x12, 0x00, 0xFE },
            new byte[] { 0x08, 0x08, 0xFC, 0xFC },
            new byte[] { 0x12, 0x20, 0x03, 0x00 },
            new byte[] { 0x34, 0x2E, 0x08, 0x08 },
            new byte[] { 0x0F, 0x14, 0x00, 0xF8 },
            new byte[] { 0x20, 0x28, 0x08, 0x10 },
            new byte[] { 0x08, 0x0A, 0x04, 0x03 },
            new byte[] { 0x20, 0x10, 0xF8, 0x10 },
            new byte[] { 0x10, 0x0D, 0x00, 0x00 }, //3B
        };

        /// <summary>
        /// Sprite ID. See <see href="https://docs.google.com/spreadsheets/d/1YuEyTkBXl-BvXyAf6C7EPXo20CdVlbwUttp2RXHoY2U/edit#gid=0"/> for more information.
        /// </summary>
        public byte Number { get; set; }
        /// <summary>
        /// Sprite status
        /// Anything equal or above 0x08 is alive
        /// <br>0x00	Free slot, non-existent sprite.</br>
        /// <br>0x01	Initial phase of sprite.</br>
        /// <br>0x02	Killed, falling off screen.</br>
        /// <br>0x03	Smushed. Rex and shell-less Koopas can be in this state.</br>
        /// <br>0x04	Killed with a spinjump.</br>
        /// <br>0x05	Burning in lava; sinking in mud.</br>
        /// <br>0x06	Turn into coin at level end.</br>
        /// <br>0x07	Stay in Yoshi's mouth.</br>
        /// <br>0x08	Normal routine.</br>
        /// <br>0x09	Stationary / Carryable.</br>
        /// <br>0x0A	Kicked.</br>
        /// <br>0x0B	Carried.</br>
        /// <br>0x0C	Powerup from being carried past goaltape.</br>
        /// </summary>
        public byte Status { get; set; }
        /// <summary>
        /// X position of sprite
        /// </summary>
        public ushort XPos { get; set; }
        /// <summary>
        /// Y position of sprite
        /// </summary>
        public ushort YPos { get; set; }

        /// <summary>
        /// <br>Format: sSjJcccc</br>
        /// <br>s=Disappear in cloud of smoke.</br>
        /// <br>S=Hop in/kick shells.</br>
        /// <br>j=Dies when jumped on.</br>
        /// <br>J=Can be jumped on (false = player gets hurt if they jump on the sprite, but can bounce off with a spin jump).</br>
        /// <br>cccc=Object clipping.</br>
        /// </summary>
        public byte Properties1 { get; set; }
        /// <summary>
        /// <br>Format: dscccccc</br>
        /// <br>d=Falls straight down when killed</br>
        /// <br>s=Use shell as death frame</br>
        /// <br>cccccc=Sprite clipping</br>
        /// </summary>
        public byte Properties2 { get; set; }
        /// <summary>
        /// <br>Format: lwcfpppg</br>
        /// <br>l=Don't interact with layer 2 (or layer 3 tides)</br>
        /// <br>w=Disable water splash</br>
        /// <br>c=Disable cape killing</br>
        /// <br>f=Disable fireball killing</br>
        /// <br>ppp=Palette</br>
        /// <br>g=Use second graphics page</br>
        /// </summary>
        public byte Properties3 { get; set; }
        /// <summary>
        /// <br>Format: dpmksPiS</br>
        /// <br>d=Don't use default interaction with player</br>
        /// <br>p=Gives power-up when eaten by Yoshi</br>
        /// <br>m=Process interaction with player every frame</br>
        /// <br>k=Can't be kicked like a shell</br>
        /// <br>s=Don't change into a shell when stunned</br>
        /// <br>P=Process while off screen</br>
        /// <br>i=Invincible to star/cape/fire/bouncing bricks</br>
        /// <br>S=Don't disable clipping when killed with star</br>
        /// </summary>
        public byte Properties4 { get; set; }
        /// <summary>
        /// <br>Format: dnctswye</br>
        /// <br>d=Don't interact with objects</br>
        /// <br>n=Spawns a new sprite</br>
        /// <br>c=Don't turn into a coin when goal passed</br>
        /// <br>t=Don't change direction if touched</br>
        /// <br>s=Don't interact with other sprites</br>
        /// <br>w=Weird ground behavior</br>
        /// <br>y=Stay in Yoshi's mouth</br>
        /// <br>e=Inedible</br>
        /// </summary>
        public byte Properties5 { get; set; }
        /// <summary>
        /// <br>Format: wcdj5sDp</br>
        /// <br>w=Don't get stuck in walls (carryable sprites)</br>
        /// <br>c=Don't turn into a coin with silver POW</br>
        /// <br>d=Death frame 2 tiles high</br>
        /// <br>j=Can be jumped on with upward Y speed</br>
        /// <br>5=Takes 5 fireballs to kill. Clear means it's killed by one. The hit counter is at $7E:1528.</br>
        /// <br>s=Can't be killed by sliding</br>
        /// <br>D=Don't erase when goal passed</br>
        /// <br>p=Make platform passable from below</br>
        /// </summary>
        public byte Properties6 { get; set; }

        /// <summary>
        /// View <see href="https://docs.google.com/spreadsheets/d/1YuEyTkBXl-BvXyAf6C7EPXo20CdVlbwUttp2RXHoY2U/edit#gid=0"/> for information.
        /// </summary>
        public byte Misc151C { get; set; }
        /// <summary>
        /// View <see href="https://docs.google.com/spreadsheets/d/1YuEyTkBXl-BvXyAf6C7EPXo20CdVlbwUttp2RXHoY2U/edit#gid=0"/> for information.
        /// </summary>
        public byte Misc1528 { get; set; }
        /// <summary>
        /// View <see href="https://docs.google.com/spreadsheets/d/1YuEyTkBXl-BvXyAf6C7EPXo20CdVlbwUttp2RXHoY2U/edit#gid=0"/> for information.
        /// </summary>
        public byte Misc1602 { get; set; }
        /// <summary>
        /// View <see href="https://docs.google.com/spreadsheets/d/1YuEyTkBXl-BvXyAf6C7EPXo20CdVlbwUttp2RXHoY2U/edit#gid=0"/> for information.
        /// </summary>
        public byte Misc187B { get; set; }

        /// <summary>
        /// Returns this sprite's bounding rectangle
        /// </summary>
        /// <returns></returns>
        public Rectangle GetSpriteClipping()
        {
            byte index = (byte)(Properties2 & SPRITE_CLIPPING_MASK);
            var clip = SpriteClippings[index];

            return new Rectangle(unchecked((sbyte)clip[2]), unchecked((sbyte)clip[3]), clip[0], clip[1]);
        }

        public Point GetSpriteRotationOffset()
        {
            if (Number == SpriteNumbers.CHAINED_GREY_PLATFORM)
            {
                const double radianRatio = Math.Tau / 512.0;
                //HIGH  LOW    REAL     DIR     VEC     XCalc       Ycalc
                //0     0      0        DOWN     0   1  Sin(0)      Cos(0)
                //0     128    128      RIGHT    1   0  Sin(90)     Cos(90)
                //1     0      256      UP       0  -1  Sin(180)    Cos(180)
                //1     128    384      LEFT    -1   0  Sin(270)    Cos(270)
                ushort angle = (ushort)(Misc151C << 8 | Misc1602); //Misc151C is high angle byte, Misc1602 is low angle byte
                byte length = Misc187B; //Misc187B is the total length of the chain, in pixels
                (var sin, var cos) = Math.SinCos(radianRatio * angle);
                int offsetX = (int)(length * sin);
                int offsetY = (int)(length * cos);

                return new Point(offsetX, offsetY);
            }
            else if (Number == SpriteNumbers.REVOLVING_PLATFORM)
            {
                //HIGH  LOW    REAL     DIR     VEC     XCalc       Ycalc
                //EVEN  0      0        RIGHT    1   0  
                //EVEN  128    128      DOWN     0   1  
                //ODD   0      256      LEFT    -1   0  
                //ODD   128    384      UP       0  -1  

                //Revolving platforms are HELL
                //TODO : Do revolving platforms as well
            }

            return Point.Empty;
        }
    }
}
