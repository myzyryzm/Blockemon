using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseGameData : ScriptableObject
{
    public string title;
    [Multiline]
    public string description;

    public string tag;

    public virtual string Id { get { return name; } }
    protected virtual void OnValidate() { }
}
