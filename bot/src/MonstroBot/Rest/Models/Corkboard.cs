namespace MonstroBot.Rest.Models;
public class Corkboard
{
    public List<CorkboardMessage> CorkboardMessages { get; set; } = [];
}

public class CorkboardMessage
{
    public string SnUserId { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime CreateDate { get; set; }
}
