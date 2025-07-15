namespace BouncerBot.Rest.Models;

public class UserMouseStatistics
{
    public MouseCatchRecord[] Mice { get; set; } = null!;
}

public class MouseCatchRecord
{
    public uint MouseId { get; set; }
    public uint NumCatches { get; set; }
}

public class UserItemCategoryCompletion
{
    public UserItemCategory[] Categories { get; set; } = [];
}

public class UserItemCategory
{
    public string Name { get; set; } = null!;
    public bool? IsComplete { get; set; }
}
