using UnityEngine;

[System.Serializable]
public struct Currency
{
    public string id;
    public Sprite icon;
    public int startAmount;

    public string ToJson()
    {
        return "{\"id\":\"" + id + "\"," +
            "\"startAmount\":" + startAmount + "}";
    }
}
