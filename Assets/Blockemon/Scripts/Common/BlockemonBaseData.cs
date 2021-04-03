using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blockemon
{
    public struct BlockemonBaseData
    {
        public GameObject prefab;
        public BlockemonType type;
        public int baseHealth;
        public int baseAttack;
        public int baseSpecialAttack;
        public int baseDefense;
        public int baseSpecialDefense;
        public int baseSpeed;

        /// <summary>
        /// Base experience multiplier used to determine how much experience is gained after defeating a blockemon
        /// </summary>
        public int baseExperienceYield;

        /// <summary>
        /// Multiplier used to determine how much experience is required to reach another level
        /// </summary>
        public int experienceMultiplier;
        
        // see bulbapedia Stat for more information for how stats are calculated (modeled after Let's Go Eevee/Pickachu with a higher emphasis on IVs)
        
        /// <summary>
        /// Gets a Blockemon's status assuming 0 av's and a neutral nature.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="level"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public int GetBlockemonStats(StatType type, int level, int iv)
        {
            return GetBlockemonStats(type, level, iv, 0, 1);
        }

        /// <summary>
        /// Gets a Blockemon's status using a neutral nature.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="level"></param>
        /// <param name="iv"></param>
        /// <param name="av"></param>
        /// <returns></returns>
        public int GetBlockemonStats(StatType type, int level, int iv, int av)
        {
            return GetBlockemonStats(type, level, iv, av, 1);
        }

        /// <summary>
        /// Gets a Blockemon's stats using all multipliers.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="level"></param>
        /// <param name="iv"></param>
        /// <param name="av"></param>
        /// <param name="natureMultiplier"></param>
        /// <returns></returns>
        public int GetBlockemonStats(StatType type, int level, int iv, int av, float natureMultiplier)
        {
            int baseStat = 0;
            switch(type)
            {
                case StatType.Health:
                    baseStat = baseHealth;
                    break;
                case StatType.Attack:
                    baseStat = baseAttack;
                    break;
                case StatType.SpecialAttack:
                    baseStat = baseSpecialAttack;
                    break;
                case StatType.Defense:
                    baseStat = baseDefense;
                    break;
                case StatType.SpecialDefense:
                    baseStat = baseSpecialDefense;
                    break;
                case StatType.Speed:
                    baseStat = baseSpeed;
                    break;
            }

            int basis = (level * (baseStat + iv) / 50);
            int avAdd = Mathf.FloorToInt(av / 4);
            int ivAdd = Mathf.FloorToInt(level * iv / 50);
            // health is calculated differently than the other stats
            if(type == StatType.Health)
            {
                return basis + level + 10 + avAdd + ivAdd;
            }
            return Mathf.CeilToInt((basis + 5) * natureMultiplier + avAdd + ivAdd);
        }

        /// <summary>
        /// Amount of experienced required for reaching a certain level.
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public int ExperienceRequiredForLevel(int level)
        {
            if(level <= 1)
            {
                return 0;
            }
            int _multiplier = experienceMultiplier < 1 ? 1 : experienceMultiplier;
            return level * level * level * _multiplier;
        }

        /// <summary>
        /// Experience gained from battling a pokemon. level == level of wild pokemon
        /// </summary>
        /// <param name="level">level of wild pokemon</param>
        /// <returns></returns>
        public int ExperienceGained(int level)
        {
            return Mathf.CeilToInt((level * baseExperienceYield) / 7);
        }
    }
}

