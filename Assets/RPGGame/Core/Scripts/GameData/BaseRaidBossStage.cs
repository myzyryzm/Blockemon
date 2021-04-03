using UnityEngine;

[System.Serializable]
public struct RaidBossReward
{
    public int rankMin;
    public int rankMax;
    public CurrencyAmount[] rewardCustomCurrencies;
    public int rewardSoftCurrency;
    public int rewardHardCurrency;
    public ItemAmount[] rewardItems;
}

public abstract class BaseRaidBossStage : BaseGameData
{
    public StageType stageType;
    public Sprite icon;
    public int recommendBattlePoint;
    [Header("Rewards")]
    public RaidBossReward[] rewards;
    [Header("Stamina")]
    public int requireStamina;
    [Tooltip("If this is not empty it will use stamina which its ID is this value")]
    public string requireCustomStamina;
    [Header("Event")]
    public StageAvailability[] availabilities;
    public bool hasAvailableDate;
    [Range(1, 9999)]
    public int startYear = 1;
    public Month startMonth = Month.January;
    [Range(1, 31)]
    public int startDay = 1;
    public int durationDays;
}
