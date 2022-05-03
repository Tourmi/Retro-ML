using System;
using System.Linq;

namespace Retro_ML.Game.SuperMarioWorld.Data
{
    /// <summary>
    /// Extended sprites consist of small sprites often used as projectiles.
    /// </summary>
    internal struct ExtendedSprite
    {
        private static readonly byte[] DANGEROUS_SPRITES = new byte[] { 0x02, 0x03, 0x04, 0x06, 0x08, 0x0B, 0x0C, 0x0D };

        /// <summary>
        /// <br>00	(empty)                                        </br>
        /// <br>01	Smoke puff                                     </br>
        /// <br>02	Reznor fireball                                </br>
        /// <br>03	Flame left by hopping flame                    </br>
        /// <br>04	Hammer                                         </br>
        /// <br>05	Player fireball                                </br>
        /// <br>06	Bone from Dry Bones                            </br>
        /// <br>07	Lava splash                                    </br>
        /// <br>08	Torpedo Ted shooter's arm                      </br>
        /// <br>09	Unknown flickering object                      </br>
        /// <br>0A  Coin from coin cloud game                      </br>
        /// <br>0B	Piranha Plant fireball                         </br>
        /// <br>0C  Lava Lotus's fiery objects                     </br>
        /// <br>0D	Baseball                                       </br>
        /// <br>0E	Wiggler's flower                               </br>
        /// <br>0F	Trail of smoke(from Yoshi stomping the ground) </br>
        /// <br>10	Spinjump stars                                 </br>
        /// <br>11	Yoshi fireballs                                </br>
        /// <br>12	Water bubble                                   </br>
        /// </summary>
        public byte Number { get; set; }
        /// <summary>
        /// X position of extended sprite
        /// </summary>
        public ushort XPos { get; set; }
        /// <summary>
        /// Y position of extended sprite
        /// </summary>
        public ushort YPos { get; set; }

        /// <summary>
        /// Returns whether or not the current extended sprite is considered dangerous.
        /// </summary>
        /// <returns></returns>
        public bool IsDangerous() => DANGEROUS_SPRITES.Contains(Number);
    }
}
