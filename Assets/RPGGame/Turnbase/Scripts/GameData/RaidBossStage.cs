using System.Collections.Generic;
using UnityEngine;

public class RaidBossStage : BaseRaidBossStage
{
    [Header("Environment")]
    public EnvironmentData environment;
    [Header("Battle")]
    public CharacterItem character;
    public int level;
}
