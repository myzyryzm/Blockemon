using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuffType
{
    Buff,
    Nerf,
}

public abstract class BaseSkillBuff
{
    public Sprite icon;
    public BuffType type;
    public CharacterEffectData buffEffects;
    [Header("Apply chance, 1 = 100%")]
    [Range(0f, 1f)]
    public float applyChance;
    [Range(0f, 1f)]
    public float applyChanceIncreaseEachLevel;
    [Header("Attributes")]
    public CalculatedAttributes attributes;
    public CalculatedAttributes attributesIncreaseEachLevel;
    [Header("Heals")]
    [Tooltip("This will multiply with pAtk to calculate heal amount, You can set this value to be negative to make it as poison")]
    public float pAtkHealRate = 0;
    public float pAtkHealRateIncreaseEachLevel = 0;
#if !NO_MAGIC_STATS
    [Tooltip("This will multiply with mAtk to calculate heal amount, You can set this value to be negative to make it as poison")]
    public float mAtkHealRate = 0;
    public float mAtkHealRateIncreaseEachLevel = 0;
#endif
    [Header("Extra")]
    [Tooltip("Amount of buffs that will be cleared randomly on target")]
    [Range(0, 100)]
    public int clearBuffs = 0;
    [Tooltip("Amount of nerfs that will be cleared randomly on target")]
    [Range(0, 100)]
    public int clearNerfs = 0;
    [Tooltip("If this is True, chance to stun will be equals to apply chance")]
    public bool isStun;

    public bool RandomToApply(int level = 1)
    {
        return Random.value <= GetApplyChance(level);
    }

    public float GetApplyChance(int level = 1)
    {
        return applyChance + (applyChanceIncreaseEachLevel * level);
    }

    public CalculatedAttributes GetAttributes(int level = 1)
    {
        return attributes + (attributesIncreaseEachLevel * level);
    }

    public float GetPAtkHealRate(int level = 1)
    {
        return pAtkHealRate + (pAtkHealRateIncreaseEachLevel * level);
    }

#if !NO_MAGIC_STATS
    public float GetMAtkHealRate(int level = 1)
    {
        return mAtkHealRate + (mAtkHealRateIncreaseEachLevel * level);
    }
#endif
}

public abstract class BaseSkill : BaseGameData
{
    public Sprite icon;
    public bool isPassive;
    public BaseSkillCastAnimationData castAnimation;

    public abstract List<BaseSkillBuff> GetBuffs();
}
