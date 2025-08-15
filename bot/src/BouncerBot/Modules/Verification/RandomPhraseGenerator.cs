using BouncerBot;

using CrypticWizard.RandomWordGenerator;

namespace BouncerBot.Modules.Verification;

public interface IRandomPhraseGenerator
{
    /// <summary>
    /// Generates a random phrase.
    /// </summary>
    /// <returns>A random phrase as a string.</returns>
    string Generate(int numAdjectives = 2, int numNouns = 2);
}

public class RandomPhraseGenerator : IRandomPhraseGenerator
{
    private static readonly WordGenerator s_wordGenerator = new();

    public string Generate(int numAdjectives = 2, int numNouns = 2)
    {

        var adjectives = s_wordGenerator.GetWords(WordGenerator.PartOfSpeech.adj, numAdjectives);
        var nouns = s_wordGenerator.GetWords(WordGenerator.PartOfSpeech.noun, numNouns);
        var words = string.Join(' ', adjectives.Concat(nouns).Select(Capitalize));

        static string Capitalize(string word) => $"{char.ToUpper(word[0])}{word[1..]}";

        return words;
    }
}
