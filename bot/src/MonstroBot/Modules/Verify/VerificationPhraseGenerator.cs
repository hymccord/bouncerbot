using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

using CrypticWizard.RandomWordGenerator;

namespace MonstroBot.Modules.Verify;

public interface IVerificationPhraseGenerator
{
    /// <summary>
    /// Generates a random phrase.
    /// </summary>
    /// <returns>A random phrase as a string.</returns>
    string GeneratePhrase(int numAdjectives = 2, int numNouns = 2);
}

public class VerificationPhraseGenerator : IVerificationPhraseGenerator
{
    private static readonly WordGenerator s_wordGenerator = new ();

    public string GeneratePhrase(int numAdjectives = 2, int numNouns = 2)
    {
        const string BasePhrase = "MouseHunt Discord Profile Verification: {0}";

        List<string> adjectives = s_wordGenerator.GetWords(WordGenerator.PartOfSpeech.adj, numAdjectives);
        List<string> nouns = s_wordGenerator.GetWords(WordGenerator.PartOfSpeech.noun, numNouns);
        string words = string.Join(' ', adjectives.Concat(nouns).Select(Capitalize));

        static string Capitalize(string word) => $"{char.ToUpper(word[0])}{word[1..]}";

        return string.Format(BasePhrase, words);
    }
}
