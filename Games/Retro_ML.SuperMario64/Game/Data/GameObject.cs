using Retro_ML.Utils.Game.Geometry3D;

namespace Retro_ML.SuperMario64.Game.Data;
/// <summary>
/// Class that represents a single Object in Super Mario 64.
/// </summary>
internal class GameObject
{
    private static readonly uint[] CoinScripts = new uint[]
    {
        0x13000830, //blue coin
        0x13000888, //boo coin
        0x130008D0, //premature yellow coin ???
        0x1300090C, //blue/yellow coin
        0x13000940, //temporary coin
        0x130009A4, //premature yellow coin ???
        0x13003068, //yellow coin
        0x13002588, //blue coin block coin
        0x130030A4, //blue coin (accelerates away)
        0x130030D4, //blue coin (runs away)
        0x13003104, //blue coin
        0x13003EAC, //red coin
        0x1300091C, //coin
    };

    private static readonly uint[] StarScripts = new uint[]
    {
        0x130007F8, //power star, offset 0x188 specifies mission
        0x1300080C, //power star, offset 0x188 specifies mission
        0x13002A20, //star?
        0x13001714, //grand star
        0x13003E3C, //star
        0x13003E64, //star
    };

    private static readonly uint[] EnemyScripts = new uint[]
    {
        0x13000054, //Mr. I main body, subtype 1 == giant Mr. I
        0x130000F8, //Mr. I projectile
        0x1300472C, //goomba
        0x130001F4, //King bob-omb
        0x13000528, //chuckya
        0x13000B58, //grindel (jumping solid enemy in desert pyramid)
        0x13000B8C, //thwomp
        0x13000BC8, //thwomp
        0x13000C84, //wall flame
        0x13001124, //Flame
        0x13001184, //orange podoboo flame
        0x130011D0, //shockwave
        0x130012B4, //spindrift
        0x13001548, //heave-ho
        0x1300179C, //bullet bill
        0x13001850, //bowser
        0x13001984, //flame breath
        0x130019C8, //falling flame
        0x13001A30, //blue podoboo flame
        0x13001A74, //flame breath
        0x13001AA4, //blown flame
        0x13001AE8, //landed flame
        0x13001DA8, //fire bar
        0x13001DCC, //flame
        0x13001E4C, //volcano flame
        0x13001F90, //desert Tox box
        0x13001FBC, //piranha plant
        0x1300220C, //Bub
        0x13002338, //sushi
        0x13002388, //sushi hitbox
        0x130023A4, //skull crate
        0x130026D4, //castle boo
        0x13002710, //cage boo
        0x13002768, //big balcony boo
        0x1300277C, //big merry-go-round boo
        0x13002790, //big lobby boo
        0x130027E4, //boo
        0x130027F4, //merry go round boo
        0x13002804, //boo that activates text
        0x13002B5C, //scuttlebug
        0x13002BB8, //king whomp
        0x13002BCC, //whomp
        0x13003174, //bob-omb
        0x13003354, //targeting amp
        0x13003388, //circle amp
        0x13003510, //bobomb explosion
        0x1300362C, //small bully
        0x13003660, //big bully
        0x13003694, //post triplets big bully
        0x130036C8, //small chill bully
        0x13003700, //chill bully
        0x130037EC, //floating mine
        0x1300381C, //floating mine explosion
        0x130039A0, //moneybag
        0x130039D4, //moneybag coin (trap)
        0x13003A08, //bowling ball (pit)
        0x13003A30, //bowling ball (still)
        0x13003A58, //spawned bowling ball
        0x13003B00, //desert spindel
        0x13003D4C, //snowman's rolling body
        0x13003D74, //rolling rock
        0x130043C4, //falling pillar hitbox
        0x13004580, //koopa
        0x13004668, //pokey
        0x13004698, //swooper
        0x130046DC, //fly guy
        0x1300478C, //chain chomp
        0x13004898, //wiggler head (hp: 0x184)
        0x130048E0, //wiggler body part
        0x13004918, //lakitu
        0x130049C8, //spiny
        0x13004A00, //monty mole
        0x13004A78, //pebble
        0x13004BA8, //water bomb
        0x13004DBC, //mr blizzard
        0x13004E08, //snowball
        0x13004F78, //unagi hitbox
        0x13004FD4, //haunted chair
        0x13005024, //mad piano
        0x1300506C, //bookend
        0x13005120, //tiny/huge pirahna plant
        0x13005158, //fire
        0x1300518C, //fire spitter
        0x130051AC, //residual fire
        0x130051E0, //snufit
        0x1300521C, //snufit bullet
        0x130052D0, //eyerok hand
        0x13005310, //klepto
        0x13005440, //clam
        0x13005468, //skeeter
        0x130055DC, //bubba
    };
    private static readonly uint[] GoodScripts = new uint[]
    {
        0x1300091C, //coin
        0x130001CC, //Cap switch button
        0x130003BC, //air bubble
        0x13000708, //water shell
        0x130007F8, //power star, offset 0x188 specifies mission
        0x1300080C, //power star, offset 0x188 specifies mission
        0x13000830, //blue coin
        0x13000888, //boo coin
        0x130008D0, //premature yellow coin ???
        0x1300090C, //blue/yellow coin
        0x13000940, //temporary coin
        0x130009A4, //premature yellow coin ???
        0x13001468, //! switch
        0x13001478, //! switch
        0x13001484, //! switch
        0x130014AC, //! switch
        0x13001650, //crazy box
        0x130016E4, //bowser key
        0x13001714, //grand star
        0x13001F3C, //koopa shell
        0x13002250, //cap/item/star block (offset 0x144 for contents)
        0x13002A20, //star?
        0x13003068, //yellow coin
        0x13002568, //blue coin block
        0x13002588, //blue coin block coin
        0x130030A4, //blue coin (accelerates away)
        0x130030D4, //blue coin (runs away)
        0x13003104, //blue coin
        0x13003750, //jet stream water ring
        0x13003798, //manta water ring
        0x13003DB8, //wing cap
        0x13003DD8, //metal cap
        0x13003DF8, //mario cap
        0x13003E1C, //vanish cap
        0x13003E3C, //star
        0x13003E64, //star
        0x13003EAC, //red coin
        0x13003F1C, //secret
        0x13003FDC, //1-up (ignores mario)
        0x13004010, //1-up (runs away)
        0x13004044, //1-up (runs away on slide)
        0x1300407C, //1-up stationary
        0x130040B4, //bouncing 1-up
        0x130040EC, //activator 1-up
        0x13004148, //following 1-up
        0x13004218, //grabbable cork box
        0x130044E0, //chest top
        0x130044FC, //MIPS
        0x13004EF8, //heart
        0x130050F4, //book switch
    };
    private static readonly uint[] SolidScripts = new uint[]
    {
        0x130001AC, //Cap switch base
        0x130001CC, //Cap switch button
        0x130005B4, //floating island/rotating platform
        0x130005D8, //whomp's fortress tower
        0x13000600, //bullet bill blaster
        0x1300066C, //falling plank
        0x13000624, //blastable corner with star
        0x13000638, //blastable corner without star
        0x130006A4, //whomp's fortress breakable tower block
        0x130006D8, //rotating bridge
        0x130006E0, //whomp's fortress rotating bridge
        0x13000B58, //grindel (jumping solid enemy in desert pyramid)
        0x13000B8C, //thwomp
        0x13000BC8, //thwomp
        0x13000C04, //falling block
        0x13000CFC, //rainbow ride elevator
        0x13000D30, //hazy maze cave elevator
        0x13000F48, //monkey cage
        0x13000F9C, //up-down 3-platforms
        0x13000FC8, //up-down mesh platform
        0x13001030, //tilting inverted pyramid platform
        0x13001064, //springy staircase
        0x130010D8, //rotating flame shooter platform
        0x13001318, //whomp fortress in-out tower platform
        0x13001340, //whomp fortress elevator tower platform
        0x13001368, //whomp fortress static tower platform
        0x13001408, //revolving diamond platform
        0x130014E0, //cork box
        0x13001518, //pushable block
        0x13001920, //bitfs bowser platform
        0x13001B88, //checkered elevator
        0x13001C58, //moat grills
        0x13001D14, //LLL rotating hex platform
        0x13001D40, //LLL sinking stone platform
        0x13001D78, //LLL cage platform
        0x13001E04, //up-down wood
        0x13001E6C, //rotating volcano platform
        0x13001E94, //up-down long platform
        0x13001EC4, //up-down 4 platform
        0x13001EF8, //LLL tilting inverted pyramid platform
        0x13002018, //LLL metal mesh platform
        0x13002038, //bowser puzzle pieces
        0x13002194, //moving express elevator
        0x130021C0, //static express elevator
        0x130022B8, //jolly rogers bay rock
        0x130022D8, //bowser sub gate
        0x13002308, //bowser sub
        0x130023EC, //floating ship hitbox
        0x1300241C, //ghost ship
        0x13002480, //sunken ship hitbox
        0x13002568, //blue coin block
        0x130025C0, //openable gate
        0x1300286C, //BBH steps
        0x13002898, //BBH rising steps
        0x130028CC, //loose floor panel
        0x130028FC, //bookshelf
        0x1300292C, //mesh elevator
        0x13002968, //merry go round
        0x13002A7C, //bitdw entrance trap door
        0x13002BB8, //king whomp
        0x13002BCC, //whomp
        0x13003274, //cannon lid
        0x130038E8, //LLL drawbridge
        0x13003910, //small bomp
        0x13003940, //large bomp
        0x13003970, //whomp's fortress in-out platform
        0x13003AE0, //cruiser wing
        0x13003B00, //desert spindel
        0x13003B30, //pyramid up-down steps
        0x13003B60, //pyramid elevator
        0x13003BB4, //pyramid top
        0x13003CB8, //cannon grills
        0x13003F40, //tall tall mountain log
        0x13003F78, //volcano crusher
        0x13003FA4, //LLL log
        0x130041BC, //directional elevator
        0x130041F0, //directional elevator buttons
        0x13004244, //moving snow mound
        0x13004284, //short wooden board
        0x130042E4, //jrb wooden board
        0x13004314, //arrow lift
        0x130043E0, //falling pillar base
        0x130044B8, //chest bottom
        0x1300481C, //wooden post
        0x13004868, //chain chomp gate
        0x13004AB0, //tracking platform (carpet, elevators, etc)
        0x13004B1C, //see-saw platform
        0x13004B44, //revolving elevator axle
        0x13004B6C, //revolving platform
        0x13004BF0, //ttc rotating block
        0x13004BF0, //ttc rotating triangular prism
        0x13004C24, //ttc pendulum
        0x13004C5C, //ttc treadmill
        0x13004C94, //ttc moving bar
        0x13004CCC, //ttc rotating cog/triangle
        0x13004CF8, //ttc pit block
        0x13004D28, //ttc elevator platform
        0x13004D64, //ttc rotating hand
        0x13004D90, //ttc red coin spinner
        0x13004E4C, //back and forth platform
        0x13004E78, //rotating octogonal platform
        0x13004EA0, //temp staircase/tricky triangles
        0x13004ECC, //elevator
        0x13004F90, //dorrie
        0x1300525C, //horizontal grindel
        0x130052D0, //eyerok hand
        0x13005414, //coffin
        0x130054B8, //swing
        0x13005504, //donut lift
    };
    private static readonly uint[] OtherScripts = new uint[]
    {
        0x13002EC0, //Mario
        0x13000000, //Star door
        0x13000118, //Whomp fortress big pole
        0x13000144, //pole
        0x1300075C, //teleporter
        0x13000780, //warp
        0x130007A0, //warp pipe
        0x13000AFC, //warp door
        0x13000B0C, //door
        0x13000F08, //ukiki
        0x13001000, //up-down pole
        0x130014BC, //hidden box
        0x1300167C, //boo cage
        0x13001C04, //poundable pillar
        0x13001CB0, //ukiki
        0x13002088, //mother penguin
        0x130020D8, //fake baby penguin (returned)
        0x130020E8, //baby penduin (offset 0x189, true if fake, false if real) (sub type 0: real, sub type 1: fake)
        0x130020E0, //real baby penguin (returned)
        0x1300227C, //exclamation mark ???
        0x1300246C, //sunken ship warp
        0x130025F8, //crystal tap (water level adjusters)
        0x13002650, //tornado
        0x13002AA4, //tree
        0x13002E58, //walking penguin
        0x13002EF8, //toad
        0x130031DC, //pink bob-omb
        0x13003228, //pink cannon bob-omb
        0x130032A8, //whirpool
        0x130032E0, //sign
        0x13003324, //bulletin board
        0x130033EC, //hoot (owl)
        0x13003738, //water ring spawner
        0x13003CE4, //snowman's body (talks)
        0x13003D0C, //snowman's head
        0x13004370, //manta ray
        0x13004538, //yoshi
        0x130045F8, //koopa the quick flag
        0x1300481C, //wooden post
        0x13004954, //camera lakitu
        0x13004A58, //monty mole hole
        0x13004F40, //unagi
        0x13005380, //racing penguin
        0x130053C4, //finish line checkpoint
        0x130053DC, //shortcut checkpoint
        0x13005528, //DDD pole
    };

    private AABB? cachedAABB;
    private uint bankStart;

    public uint BehaviourScript { get; private set; }
    public uint BankAddress => ToBankAddress(BehaviourScript, bankStart);
    public float X { get; private set; }
    public float Y { get; private set; }
    public float Z { get; private set; }
    public float Radius { get; private set; }
    public float Height { get; private set; }
    public float DownOffset { get; private set; }

    public Vector Pos => new(X, Y - DownOffset, Z);
    public Vector Size => new(Radius * 2, Height, Radius * 2);

    public AABB AABB
    {
        get
        {
            if (!cachedAABB.HasValue) cachedAABB = new AABB(Pos, Size, isStatic: false);
            return cachedAABB!.Value;
        }
    }

    public GameObject(byte[] bytes, uint bankStart)
    {
        this.bankStart = bankStart;

        BehaviourScript = GetUint(bytes[0..4]);
        X = GetFloat(bytes[4..8]);
        Y = GetFloat(bytes[8..12]);
        Z = GetFloat(bytes[12..16]);
        Radius = GetFloat(bytes[16..20]);
        Height = GetFloat(bytes[20..24]);
        DownOffset = GetFloat(bytes[24..28]);
    }

    public bool IsEnemy() => EnemyScripts.Contains(ToBankAddress(BehaviourScript, bankStart));
    public bool IsGoodie() => GoodScripts.Contains(ToBankAddress(BehaviourScript, bankStart));
    public bool IsStar() => StarScripts.Contains(ToBankAddress(BehaviourScript, bankStart));

    public static bool IsGoodie(uint behaviourScript, uint bankStart) => GoodScripts.Contains(ToBankAddress(behaviourScript, bankStart));
    private static uint ToBankAddress(uint behaviourScript, uint bankStart) => ((uint)Math.Max((behaviourScript & ~0x8000_0000) - (long)bankStart, 0)) + 0x13000000;
    private static uint GetUint(byte[] bytes)
    {
        if (BitConverter.IsLittleEndian) bytes = bytes.Reverse().ToArray();
        return BitConverter.ToUInt32(bytes, 0);
    }
    private static float GetFloat(byte[] bytes)
    {
        if (BitConverter.IsLittleEndian) bytes = bytes.Reverse().ToArray();
        return BitConverter.ToSingle(bytes, 0);
    }
}
