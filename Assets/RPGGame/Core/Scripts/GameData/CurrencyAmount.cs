[System.Serializable]
public struct CurrencyAmount
{
    public string Id { get; set; }
    public int Amount { get; set; }

    public string ToJson()
    {
        return "{" +
            "\"id\":\"" + Id + "\"," +
            "\"amount\":" + Amount + "}";
    }
}
