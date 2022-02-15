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
        private static readonly IEnumerable<ushort> defaultSolidTiles = Enumerable.Range(0x100, 0x1FA - 0x100 + 1)
            .Select(i => (ushort)i).ToList();
        private static readonly IEnumerable<ushort> defaultDangerousTiles = Enumerable.Range(0x1FB, 0x1FF - 0x1FB + 1)
            .Union(new int[] { 0x12F, 0x05, 0x1D2, 0x1D3 })
            .Select(i => (ushort)i).ToList();
        private static readonly IEnumerable<ushort> defaultGoodTiles = Enumerable.Range(0x11F, 0x128 - 0x11F + 1)
            .Union(Enumerable.Range(0x117, 0x11D - 0x117 + 1))
            .Union(new int[] { 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x38, 0x6E, 0x114, 0x12D, 0x16A, 0x16B, 0x16C, 0x16D })
            .Select(i => (ushort)i).ToList();

        private static readonly ReadOnlyDictionary<byte, Tileset> tilesets = new(new Dictionary<byte, Tileset>()
        {
            [0x0] = new Tileset(defaultSolidTiles, defaultDangerousTiles, defaultGoodTiles),
            [0x1] = new Tileset(defaultSolidTiles, defaultDangerousTiles.Union(new ushort[] { 0x159, 0x15A, 0x15B, 0x15C, 0x1C1, 0x1C2, 0x1C3, 0x1C4, 0x166, 0x167, 0x168, 0x169 }), defaultGoodTiles),
            [0x2] = new Tileset(defaultSolidTiles, defaultDangerousTiles, defaultGoodTiles),
            [0x3] = new Tileset(defaultSolidTiles.Except(new ushort[] { 0x159, 0x15A, 0x15B, 0x1D2, 0x1D3, 0x1D4, 0x1D5, 0x1D6, 0x1D7 }), defaultDangerousTiles.Union(new ushort[] { 0x159, 0x15A, 0x15B, 0x1D2, 0x1D3, 0x1D4, 0x1D5, 0x1D6, 0x1D7 }), defaultGoodTiles),
            [0x4] = new Tileset(defaultSolidTiles, defaultDangerousTiles, defaultGoodTiles),
            [0x5] = new Tileset(defaultSolidTiles, defaultDangerousTiles, defaultGoodTiles),
            [0x6] = new Tileset(defaultSolidTiles, defaultDangerousTiles, defaultGoodTiles),
            [0x7] = new Tileset(defaultSolidTiles, defaultDangerousTiles, defaultGoodTiles),
            [0x8] = new Tileset(defaultSolidTiles, defaultDangerousTiles, defaultGoodTiles),
            [0x9] = new Tileset(defaultSolidTiles, defaultDangerousTiles, defaultGoodTiles),
            [0xA] = new Tileset(defaultSolidTiles, defaultDangerousTiles, defaultGoodTiles),
            [0xB] = new Tileset(defaultSolidTiles, defaultDangerousTiles, defaultGoodTiles),
            [0xC] = new Tileset(defaultSolidTiles, defaultDangerousTiles, defaultGoodTiles),
            [0xD] = new Tileset(defaultSolidTiles, defaultDangerousTiles, defaultGoodTiles),
            [0xE] = new Tileset(defaultSolidTiles, defaultDangerousTiles, defaultGoodTiles)
        });

        private readonly HashSet<ushort> walkableTiles;
        private readonly HashSet<ushort> dangerousTiles;
        private readonly HashSet<ushort> goodTiles;

        private Tileset(IEnumerable<ushort> walkableTiles, IEnumerable<ushort> dangerousTiles, IEnumerable<ushort> goodTiles)
        {
            this.walkableTiles = new HashSet<ushort>(walkableTiles);
            this.dangerousTiles = new HashSet<ushort>(dangerousTiles);
            this.goodTiles = new HashSet<ushort>(goodTiles);
        }

        public static IEnumerable<ushort> GetWalkableTiles(byte tileset) => tilesets[tileset].walkableTiles;
        public static IEnumerable<ushort> GetDangerousTiles(byte tileset) => tilesets[tileset].dangerousTiles;
        public static IEnumerable<ushort> GetGoodTiles(byte tileset) => tilesets[tileset].goodTiles;
    }
}
