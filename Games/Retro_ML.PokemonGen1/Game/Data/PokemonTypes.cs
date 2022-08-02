using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Retro_ML.PokemonGen1.Game.Data;
internal static class PokemonTypes
{
    public enum Types
    {
        Normal = 0,
        Fighting = 1,
        Flying = 2,
        Poison = 3,
        Ground = 4,
        Rock = 5,
        Bug = 7,
        Ghost = 8,
        Fire = 20,
        Water = 21,
        Grass = 22,
        Electric = 23,
        Psychic = 24,
        Ice = 25,
        Dragon = 26
    }

    public struct Effectiveness
    {
        public Types Attacking;
        public Types Defending;
        public double Multiplier;

        public Effectiveness(Types attacking, Types defending, double multiplier)
        {
            Attacking = attacking;
            Defending = defending;
            Multiplier = multiplier;
        }
    }

    public static double GetMultiplier(Types att, Types def) => effectivenesses.Where(a => a.Attacking == att && a.Defending == def).ToList().Count == 0 ? 1 : effectivenesses.SingleOrDefault(a => a.Attacking == att && a.Defending == def).Multiplier;

    public static double GetMultiplier(Types att, Types def1, Types def2) => GetMultiplier(att, def1) * GetMultiplier(att, def2);

    private static readonly Effectiveness[] effectivenesses = new Effectiveness[]
    {
        new Effectiveness(Types.Water, Types.Fire, 2),
        new Effectiveness(Types.Fire, Types.Grass, 2),
        new Effectiveness(Types.Fire, Types.Ice, 2),
        new Effectiveness(Types.Grass, Types.Water, 2),
        new Effectiveness(Types.Electric, Types.Water, 2),
        new Effectiveness(Types.Water, Types.Rock, 2),
        new Effectiveness(Types.Ground, Types.Flying, 0),
        new Effectiveness(Types.Water, Types.Water, 0.5),
        new Effectiveness(Types.Fire, Types.Fire, 0.5),
        new Effectiveness(Types.Electric, Types.Electric, 0.5),
        new Effectiveness(Types.Ice, Types.Ice, 0.5),
        new Effectiveness(Types.Grass, Types.Grass, 0.5),
        new Effectiveness(Types.Psychic, Types.Psychic, 0.5),
        new Effectiveness(Types.Fire, Types.Water, 0.5),
        new Effectiveness(Types.Grass, Types.Fire, 0.5),
        new Effectiveness(Types.Water, Types.Grass, 0.5),
        new Effectiveness(Types.Electric, Types.Grass, 0.5),
        new Effectiveness(Types.Normal, Types.Rock, 0.5),
        new Effectiveness(Types.Normal, Types.Ghost, 0),
        new Effectiveness(Types.Ghost, Types.Ghost, 2),
        new Effectiveness(Types.Fire, Types.Bug, 2),
        new Effectiveness(Types.Fire, Types.Rock, 0.5),
        new Effectiveness(Types.Water, Types.Ground, 2),
        new Effectiveness(Types.Electric, Types.Ground, 0),
        new Effectiveness(Types.Electric, Types.Flying, 2),
        new Effectiveness(Types.Grass, Types.Ground, 2),
        new Effectiveness(Types.Grass, Types.Bug, 0.5),
        new Effectiveness(Types.Grass, Types.Poison, 0.5),
        new Effectiveness(Types.Grass, Types.Rock, 2),
        new Effectiveness(Types.Grass, Types.Flying, 0.5),
        new Effectiveness(Types.Ice, Types.Water, 0.5),
        new Effectiveness(Types.Ice, Types.Grass, 2),
        new Effectiveness(Types.Ice, Types.Ground, 2),
        new Effectiveness(Types.Ice, Types.Flying, 2),
        new Effectiveness(Types.Fighting, Types.Normal, 2),
        new Effectiveness(Types.Fighting, Types.Poison, 0.5),
        new Effectiveness(Types.Fighting, Types.Flying, 0.5),
        new Effectiveness(Types.Fighting, Types.Psychic, 0.5),
        new Effectiveness(Types.Fighting, Types.Bug, 0.5),
        new Effectiveness(Types.Fighting, Types.Rock, 2),
        new Effectiveness(Types.Fighting, Types.Ice, 2),
        new Effectiveness(Types.Fighting, Types.Ghost, 0),
        new Effectiveness(Types.Poison, Types.Grass, 2),
        new Effectiveness(Types.Poison, Types.Poison, 0.5),
        new Effectiveness(Types.Poison, Types.Ground, 0.5),
        new Effectiveness(Types.Poison, Types.Bug, 2),
        new Effectiveness(Types.Poison, Types.Rock, 0.5),
        new Effectiveness(Types.Poison, Types.Ghost, 0.5),
        new Effectiveness(Types.Ground, Types.Fire, 2),
        new Effectiveness(Types.Ground, Types.Electric, 2),
        new Effectiveness(Types.Ground, Types.Grass, 0.5),
        new Effectiveness(Types.Ground, Types.Bug, 0.5),
        new Effectiveness(Types.Ground, Types.Rock, 2),
        new Effectiveness(Types.Ground, Types.Poison, 2),
        new Effectiveness(Types.Flying, Types.Electric, 0.5),
        new Effectiveness(Types.Flying, Types.Fighting, 2),
        new Effectiveness(Types.Flying, Types.Bug, 2),
        new Effectiveness(Types.Flying, Types.Grass, 2),
        new Effectiveness(Types.Flying, Types.Rock, 0.5),
        new Effectiveness(Types.Psychic, Types.Fighting, 2),
        new Effectiveness(Types.Psychic, Types.Poison, 2),
        new Effectiveness(Types.Bug, Types.Fire, 0.5),
        new Effectiveness(Types.Bug, Types.Grass, 2),
        new Effectiveness(Types.Bug, Types.Fighting, 0.5),
        new Effectiveness(Types.Bug, Types.Flying, 0.5),
        new Effectiveness(Types.Bug, Types.Psychic, 2),
        new Effectiveness(Types.Bug, Types.Ghost, 0.5),
        new Effectiveness(Types.Bug, Types.Poison, 2),
        new Effectiveness(Types.Rock, Types.Fire, 2),
        new Effectiveness(Types.Rock, Types.Fighting, 0.5),
        new Effectiveness(Types.Rock, Types.Ground, 0.5),
        new Effectiveness(Types.Rock, Types.Flying, 2),
        new Effectiveness(Types.Rock, Types.Bug, 2),
        new Effectiveness(Types.Rock, Types.Ice, 2),
        new Effectiveness(Types.Ghost, Types.Normal, 0),
        new Effectiveness(Types.Ghost, Types.Psychic, 0),
        new Effectiveness(Types.Fire, Types.Dragon, 0.5),
        new Effectiveness(Types.Water, Types.Dragon, 0.5),
        new Effectiveness(Types.Electric, Types.Dragon, 0.5),
        new Effectiveness(Types.Grass, Types.Dragon, 0.5),
        new Effectiveness(Types.Ice, Types.Dragon, 2),
        new Effectiveness(Types.Dragon, Types.Dragon, 2)
    };
}
