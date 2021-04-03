using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blockemon
{

    /// <summary>
    /// Contains the information regarding a Blockemon natures. A nature with a positiveStat of StatType.Health or a negativeStat of StatType.Health will be treated as a neutral nature.
    /// </summary>
    public struct BlockemonNature
    {
        public Nature nature;
        public StatType positiveStat;
        public StatType negativeStat;
    }
}
