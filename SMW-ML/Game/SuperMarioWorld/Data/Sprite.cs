using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMW_ML.Game.SuperMarioWorld.Data
{
    internal struct Sprite
    {
        /// <summary>
        /// Sprite ID. See <see href="https://docs.google.com/spreadsheets/d/1YuEyTkBXl-BvXyAf6C7EPXo20CdVlbwUttp2RXHoY2U/edit#gid=0"/> for more information
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
    }
}
