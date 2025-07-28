
namespace BouncerBot.Rest;

[Serializable]
internal class PuzzleException : Exception
{
    public PuzzleException()
    {
    }

    public PuzzleException(string? message) : base(message)
    {
    }
}
