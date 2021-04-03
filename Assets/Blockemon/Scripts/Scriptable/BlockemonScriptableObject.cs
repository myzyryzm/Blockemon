using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blockemon
{
    [CreateAssetMenu(fileName = "Blockemon Scriptable", menuName = "ScriptableObjects/BlockemonScriptableObject", order = 1)]
    public class BlockemonScriptableObject : ScriptableObject
    {
        public int maxIV = 15;
        public int minIV = 0;
        public int maxAV = 256;

        public List<BlockemonBaseData> blockemonBaseDatas = new List<BlockemonBaseData>();
        public List<BlockemonNature> natures = new List<BlockemonNature>();
    }
}
