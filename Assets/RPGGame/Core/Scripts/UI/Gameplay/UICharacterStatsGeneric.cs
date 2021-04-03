using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIFollowWorldObject))]
public class UICharacterStatsGeneric : UIBase
{
    public Text textTitle;
    public Image imageIcon;
    public GameObject characterStatsRoot;
    public Text textHpPerMaxHp;
    public Text textHpPercent;
    public Image imageHpGage;
    public UILevel uiLevel;
    public UICharacterBuff[] uiBuffs;
    public BaseCharacterEntity character;
    public bool notFollowCharacter;

    private UIFollowWorldObject tempObjectFollower;
    public UIFollowWorldObject TempObjectFollower
    {
        get
        {
            if (tempObjectFollower == null)
                tempObjectFollower = GetComponent<UIFollowWorldObject>();
            return tempObjectFollower;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        if (notFollowCharacter)
        {
            TempObjectFollower.enabled = false;
            TempObjectFollower.TempPositionFollower.enabled = false;
        }
        if (!characterStatsRoot)
            characterStatsRoot = root;
    }

    protected virtual void Update()
    {
        if (character == null)
            return;

        if (!notFollowCharacter)
            TempObjectFollower.targetObject = character.uiContainer;

        var itemData = character.Item.ItemData;
        var rate = (float)character.Hp / (float)character.MaxHp;

        if (textHpPerMaxHp != null)
            textHpPerMaxHp.text = character.Hp.ToString("N0") + "/" + character.MaxHp.ToString("N0");

        if (textHpPercent != null)
            textHpPercent.text = (rate * 100).ToString("N2") + "%";

        if (imageHpGage != null)
            imageHpGage.fillAmount = rate;

        if (textTitle != null)
            textTitle.text = itemData.title;

        if (imageIcon != null)
            imageIcon.sprite = itemData.icon;

        if (uiLevel != null)
        {
            uiLevel.level = character.Item.Level;
            uiLevel.maxLevel = character.Item.MaxLevel;
            uiLevel.currentExp = character.Item.CurrentExp;
            uiLevel.requiredExp = character.Item.RequiredExp;
        }

        var i = 0;
        var buffKeys = character.Buffs.Keys;
        foreach (var buffKey in buffKeys)
        {
            if (i >= uiBuffs.Length)
                break;
            var ui = uiBuffs[i];
            ui.buff = character.Buffs[buffKey];
            ui.Show();
            ++i;
        }
        for (; i < uiBuffs.Length; ++i)
        {
            var ui = uiBuffs[i];
            ui.Hide();
        }
    }

    public void ShowCharacterStats()
    {
        characterStatsRoot.SetActive(true);
    }

    public void HideCharacterStats()
    {
        characterStatsRoot.SetActive(false);
    }
}
