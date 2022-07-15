namespace Retro_ML.PokemonGen1.Game;

/// <summary>
/// <br>RAM addresses used in Pokemon.</br>
/// <br/>
/// <br>Big thanks to </br>
/// <br>- DataCrystal Wiki <see href="https://datacrystal.romhacking.net/wiki/Pok%C3%A9mon_Red/Blue:RAM_map"/></br>
/// </summary>
internal static class Addresses
{
    public struct AddressData
    {
        public enum CacheDurations
        {
            NoCache,
            Turn,
            Battle
        }

        public AddressData(uint address, uint length = 1, CacheDurations cacheDuration = CacheDurations.Turn, bool isBigEndian = false)
        {
            Address = address;
            Length = length;
            CacheDuration = cacheDuration;
            IsBigEndian = isBigEndian;
        }

        public uint Address;
        public uint Length;
        public CacheDurations CacheDuration;
        public bool IsBigEndian;
    }

    /// <summary>
    /// When equals to 9: Fight is selected
    /// </summary>
    public static readonly AddressData FightCursor = new(0xCC26, cacheDuration: AddressData.CacheDurations.NoCache);

    /// <summary>
    /// if equals 0: Not in fight
    /// </summary>
    public static readonly AddressData GameState = new(0xC012);

    public static class WildEncounters
    {
        public static readonly AddressData Encounter1 = new(0xD889);
        public static readonly AddressData Encounter2 = new(0xD88B);
        public static readonly AddressData Encounter3 = new(0xD88D);
        public static readonly AddressData Encounter4 = new(0xD88F);
        public static readonly AddressData Encounter5 = new(0xD891);
        public static readonly AddressData Encounter6 = new(0xD893);
        public static readonly AddressData Encounter7 = new(0xD895);
        public static readonly AddressData Encounter8 = new(0xD897);
        public static readonly AddressData Encounter9 = new(0xD899);
        public static readonly AddressData Encounter10 = new(0xD89B);
    }

    public static class PlayerPokemons
    {
        /// <summary>
        /// Data for a single player pokemons.
        /// </summary>
        public static readonly AddressData SinglePlayerPokemon = new(0xD16B, 0x2C);
        /// <summary>
        /// Data for all player pokemons
        /// </summary>
        public static readonly AddressData AllPlayerPokemon = new(0xD16B, 0x0108);

        /// <summary>
        /// Used to detect version of Pokemon
        /// </summary>
        public static readonly AddressData EndOfList = new(0xD16A);
    }

    public static class CurrentPokemon
    {
        public static readonly AddressData CurrentHP = new(0xD015, 2, AddressData.CacheDurations.NoCache, isBigEndian: true);

        public static readonly AddressData StatusEffect = new(0xD018);

        public static readonly AddressData MaxHP = new(0xD023, 2, isBigEndian: true);

        /// <summary>
        /// Each byte represents one move's current PP
        /// </summary>
        public static readonly AddressData MovesCurrentPP = new(0xD02D, 4, cacheDuration: AddressData.CacheDurations.NoCache);

        /// <summary>
        /// Types Summary 
        /// <code>
        /// <br>0  : Normal</br>
        /// <br>1  : Fighting</br>
        /// <br>2  : Flying</br>
        /// <br>3  : Poison</br>
        /// <br>4  : Ground</br>
        /// <br>5  : Rock</br>
        /// <br>6  : Bird</br>
        /// <br>7  : Bug</br>
        /// <br>8  : Ghost</br>
        /// <br>20 : Fire</br>
        /// <br>21 : Water</br>
        /// <br>22 : Grass</br>
        /// <br>23 : Electric</br>
        /// <br>24 : Psychic</br>
        /// <br>25 : Ice</br>
        /// <br>26 : Dragon</br>
        /// </code>
        /// </summary>
        public static readonly AddressData SelectedMoveType = new(0xCFD5, cacheDuration: AddressData.CacheDurations.NoCache);

        /// <summary>
        /// Types Summary 
        /// <code>
        /// <br>0  : Normal</br>
        /// <br>1  : Fighting</br>
        /// <br>2  : Flying</br>
        /// <br>3  : Poison</br>
        /// <br>4  : Ground</br>
        /// <br>5  : Rock</br>
        /// <br>6  : Bird</br>
        /// <br>7  : Bug</br>
        /// <br>8  : Ghost</br>
        /// <br>20 : Fire</br>
        /// <br>21 : Water</br>
        /// <br>22 : Grass</br>
        /// <br>23 : Electric</br>
        /// <br>24 : Psychic</br>
        /// <br>25 : Ice</br>
        /// <br>26 : Dragon</br>
        /// </code>
        /// </summary>
        public static readonly AddressData Type1 = new(0xD019);
        /// <summary>
        /// Types Summary 
        /// <code>
        /// <br>0  : Normal</br>
        /// <br>1  : Fighting</br>
        /// <br>2  : Flying</br>
        /// <br>3  : Poison</br>
        /// <br>4  : Ground</br>
        /// <br>5  : Rock</br>
        /// <br>6  : Bird</br>
        /// <br>7  : Bug</br>
        /// <br>8  : Ghost</br>
        /// <br>20 : Fire</br>
        /// <br>21 : Water</br>
        /// <br>22 : Grass</br>
        /// <br>23 : Electric</br>
        /// <br>24 : Psychic</br>
        /// <br>25 : Ice</br>
        /// <br>26 : Dragon</br>
        /// </code>
        /// </summary>
        public static readonly AddressData Type2 = new(0xD01A);

        public static readonly AddressData SelectedMovePower = new(0xCFD4, cacheDuration:AddressData.CacheDurations.NoCache);

        public static readonly AddressData Move1ID = new(0xD01C);
        public static readonly AddressData Move2ID = new(0xD01D);
        public static readonly AddressData Move3ID = new(0xD01E);
        public static readonly AddressData Move4ID = new(0xD01F);

    }

    public static class OpposingPokemon
    {
        public static readonly AddressData CurrentHP = new(0xCFE6, 2, AddressData.CacheDurations.NoCache, isBigEndian: true);

        public static readonly AddressData MaxHP = new(0xCFF4, 2, isBigEndian: true);

        public static readonly AddressData Level = new(0xCFF3);

        /// <summary>
        /// Types Summary 
        /// <code>
        /// <br>0  : Normal</br>
        /// <br>1  : Fighting</br>
        /// <br>2  : Flying</br>
        /// <br>3  : Poison</br>
        /// <br>4  : Ground</br>
        /// <br>5  : Rock</br>
        /// <br>6  : Bird</br>
        /// <br>7  : Bug</br>
        /// <br>8  : Ghost</br>
        /// <br>20 : Fire</br>
        /// <br>21 : Water</br>
        /// <br>22 : Grass</br>
        /// <br>23 : Electric</br>
        /// <br>24 : Psychic</br>
        /// <br>25 : Ice</br>
        /// <br>26 : Dragon</br>
        /// </code>
        /// </summary>
        public static readonly AddressData Type1 = new(0xCFEA);
        /// <summary>
        /// Types Summary 
        /// <code>
        /// <br>0  : Normal</br>
        /// <br>1  : Fighting</br>
        /// <br>2  : Flying</br>
        /// <br>3  : Poison</br>
        /// <br>4  : Ground</br>
        /// <br>5  : Rock</br>
        /// <br>6  : Bird</br>
        /// <br>7  : Bug</br>
        /// <br>8  : Ghost</br>
        /// <br>20 : Fire</br>
        /// <br>21 : Water</br>
        /// <br>22 : Grass</br>
        /// <br>23 : Electric</br>
        /// <br>24 : Psychic</br>
        /// <br>25 : Ice</br>
        /// <br>26 : Dragon</br>
        /// </code>
        /// </summary>
        public static readonly AddressData Type2 = new(0xCFEB);

        /// <summary>
        /// 
        /// </summary>
        public static readonly AddressData StatusEffect = new(0xCFE9);

        public static readonly AddressData PokemonIDToLoad = new(0xCFD8);

    }
}
