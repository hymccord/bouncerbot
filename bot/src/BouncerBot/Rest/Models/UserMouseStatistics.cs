namespace BouncerBot.Rest.Models;

public class UserMouseStatistics
{
    public MouseCatchRecord[] Mice { get; set; } = null!;
}

public class MouseCatchRecord
{
    public int MouseId { get; set; }
    public int NumCatches { get; set; }
}

public class UserItemCategoryCompletion
{
    public UserItemCategory[] Categories { get; set; } = [];
}

public class UserItemCategory
{
    public string Name { get; set; } = null!;
    public bool IsComplete { get; set; }
}
