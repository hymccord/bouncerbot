namespace MonstroBot.Rest.Models;

public class UserMouseStatistics
{
    public MouseCatchRecord[] Mice { get; set; } = null!;
}

public class MouseCatchRecord
{
    public int MouseId { get; set; }
    public int NumCatches { get; set; }
}

public class UserProfileItems
{
    public UserProfileItemCategory[] Categories { get; set; } = [];
}

public class UserProfileItemCategory
{
    public string Name { get; set; } = null!;
    public bool IsComplete { get; set; }
}
