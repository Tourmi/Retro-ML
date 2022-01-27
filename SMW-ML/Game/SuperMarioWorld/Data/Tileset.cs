using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMW_ML.Game.SuperMarioWorld.Data
{
    internal class Tileset
    {
        private static readonly IEnumerable<ushort> defaultSolidTiles = Enumerable.Range(0x100, 0x1FA - 0x100 + 1).Select(i => (ushort)i).ToList();
        private static readonly IEnumerable<ushort> defaultDangerousTiles = Enumerable.Range(0x1FB, 0x1FF - 0x1FB + 1).Union(new int[] { 0x12F, 0x05 }).Select(i => (ushort)i).ToList();

        private static readonly ReadOnlyDictionary<byte, Tileset> tilesets = new(new Dictionary<byte, Tileset>()
        {
            [0x0] = new Tileset(defaultSolidTiles, defaultDangerousTiles),
            [0x1] = new Tileset(defaultSolidTiles, defaultDangerousTiles.Union(new ushort[] { 0x159, 0x15A, 0x15B, 0x15C, 0x1C1, 0x1C2, 0x1C3, 0x1C4})),
            [0x2] = new Tileset(defaultSolidTiles, defaultDangerousTiles),
            [0x3] = new Tileset(defaultSolidTiles, defaultDangerousTiles),
            [0x4] = new Tileset(defaultSolidTiles, defaultDangerousTiles),
            [0x5] = new Tileset(defaultSolidTiles, defaultDangerousTiles),
            [0x6] = new Tileset(defaultSolidTiles, defaultDangerousTiles),
            [0x7] = new Tileset(defaultSolidTiles, defaultDangerousTiles),
            [0x8] = new Tileset(defaultSolidTiles, defaultDangerousTiles),
            [0x9] = new Tileset(defaultSolidTiles, defaultDangerousTiles),
            [0xA] = new Tileset(defaultSolidTiles, defaultDangerousTiles),
            [0xB] = new Tileset(defaultSolidTiles, defaultDangerousTiles),
            [0xC] = new Tileset(defaultSolidTiles, defaultDangerousTiles),
            [0xD] = new Tileset(defaultSolidTiles, defaultDangerousTiles),
            [0xE] = new Tileset(defaultSolidTiles, defaultDangerousTiles)
        });

        private readonly HashSet<ushort> walkableTiles;
        private readonly HashSet<ushort> dangerousTiles;

        private Tileset(IEnumerable<ushort> walkableTiles, IEnumerable<ushort> dangerousTiles)
        {
            this.walkableTiles = new HashSet<ushort>(walkableTiles);

            this.dangerousTiles = new HashSet<ushort>(dangerousTiles);
        }

        public static IEnumerable<ushort> GetWalkableTiles(byte tileset) => tilesets[tileset].walkableTiles;
        public static IEnumerable<ushort> GetDangerousTiles(byte tileset) => tilesets[tileset].dangerousTiles;
    }
}
