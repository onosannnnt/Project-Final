[System.Serializable]
public class ActiveBuff
{
    public Buff Data { get; private set; }
    public int CurrentDuration { get; set; }
    public int CurrentStack { get; set; }
    public bool isInitialized { get; set; }
    public bool wasReappliedThisTurn { get; set; }
    public System.Collections.Generic.Dictionary<string, object> CustomState = new();

    public ActiveBuff(Buff data)
    {
        this.Data = data;
        this.CurrentDuration = data.Duration;
        this.CurrentStack = 1;
        this.isInitialized = true;
        this.wasReappliedThisTurn = false;
    }
}
