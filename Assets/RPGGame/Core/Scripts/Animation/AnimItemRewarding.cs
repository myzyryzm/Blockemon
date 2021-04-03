using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimItemRewarding : MonoBehaviour
{
    public UIItem uiRewardItem;

    public virtual void Play(PlayerItem item)
    {
        uiRewardItem.SetData(item);
    }
}
