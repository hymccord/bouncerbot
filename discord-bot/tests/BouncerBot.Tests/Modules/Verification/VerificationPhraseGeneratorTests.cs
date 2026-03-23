using BouncerBot.Modules.Verification;

namespace BouncerBot.Tests.Modules.Verification;

[TestClass]
public class VerificationPhraseGeneratorTests
{
    private readonly VerificationPhraseGenerator _generator;

    public VerificationPhraseGeneratorTests()
    {
        _generator = new VerificationPhraseGenerator();
    }

    [TestMethod]
    public void Generate_DefaultParameters_ReturnsPhraseWithPreamble()
    {
        // Arrange
        // Act
        var result = _generator.Generate();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.StartsWith(VerificationPhraseGenerator.Preamble));
    }

    [TestMethod]
    public void Generate_DefaultParameters_ContainsCapitalizedWords()
    {
        // Arrange
        // Act
        var result = _generator.Generate();

        // Assert
        var wordsAfterPreamble = result.Substring(VerificationPhraseGenerator.Preamble.Length).Trim();
        var words = wordsAfterPreamble.Split(' ');
        Assert.IsTrue(words.Length >= 4, "Should have at least 4 words (2 adjectives + 2 nouns)");
        foreach (var word in words)
        {
            Assert.IsTrue(char.IsUpper(word[0]), $"Word '{word}' should start with uppercase letter");
        }
    }

    [TestMethod]
    public void Generate_WithCustomAdjectives_ReturnsCorrectNumberOfWords()
    {
        // Arrange
        var numAdjectives = 3;
        var numNouns = 2;

        // Act
        var result = _generator.Generate(numAdjectives, numNouns);

        // Assert
        var wordsAfterPreamble = result.Substring(VerificationPhraseGenerator.Preamble.Length).Trim();
        var words = wordsAfterPreamble.Split(' ');
        Assert.AreEqual(numAdjectives + numNouns, words.Length);
    }

    [TestMethod]
    public void Generate_WithCustomNouns_ReturnsCorrectNumberOfWords()
    {
        // Arrange
        var numAdjectives = 1;
        var numNouns = 4;

        // Act
        var result = _generator.Generate(numAdjectives, numNouns);

        // Assert
        var wordsAfterPreamble = result.Substring(VerificationPhraseGenerator.Preamble.Length).Trim();
        var words = wordsAfterPreamble.Split(' ');
        Assert.AreEqual(numAdjectives + numNouns, words.Length);
    }

    [TestMethod]
    public void Generate_WithSingleWord_ReturnsValidPhrase()
    {
        // Arrange
        // Act
        var result = _generator.Generate(1, 0);

        // Assert
        Assert.IsTrue(result.StartsWith(VerificationPhraseGenerator.Preamble));
        var wordsAfterPreamble = result.Substring(VerificationPhraseGenerator.Preamble.Length).Trim();
        var words = wordsAfterPreamble.Split(' ');
        Assert.AreEqual(1, words.Length);
    }

    [TestMethod]
    public void Generate_WithZeroAdjectives_ReturnsOnlyNouns()
    {
        // Arrange
        var numNouns = 3;

        // Act
        var result = _generator.Generate(0, numNouns);

        // Assert
        var wordsAfterPreamble = result.Substring(VerificationPhraseGenerator.Preamble.Length).Trim();
        var words = wordsAfterPreamble.Split(' ');
        Assert.AreEqual(numNouns, words.Length);
    }

    [TestMethod]
    public void Generate_MultipleCalls_ReturnsDifferentPhrases()
    {
        // Arrange
        var results = new HashSet<string>();
        const int attempts = 10;

        // Act
        for (var i = 0; i < attempts; i++)
        {
            results.Add(_generator.Generate());
        }

        // Assert
        Assert.IsTrue(results.Count > 1, "Should generate different phrases on multiple calls");
    }

    [TestMethod]
    public void Generate_ReturnsNonEmptyString()
    {
        // Arrange
        // Act
        var result = _generator.Generate();

        // Assert
        Assert.IsFalse(string.IsNullOrWhiteSpace(result));
    }

    [TestMethod]
    public void Generate_PreambleFormat_IsCorrect()
    {
        // Arrange
        // Act
        var result = _generator.Generate();

        // Assert
        Assert.IsTrue(result.StartsWith("BouncerBot Verification:"));
    }

    [TestMethod]
    public void Generate_WithLargeParameters_HandlesCorrectly()
    {
        // Arrange
        var numAdjectives = 5;
        var numNouns = 5;

        // Act
        var result = _generator.Generate(numAdjectives, numNouns);

        // Assert
        var wordsAfterPreamble = result.Substring(VerificationPhraseGenerator.Preamble.Length).Trim();
        var words = wordsAfterPreamble.Split(' ');
        Assert.AreEqual(numAdjectives + numNouns, words.Length);
        Assert.IsTrue(result.StartsWith(VerificationPhraseGenerator.Preamble));
    }

    [TestMethod]
    public void Generate_AllWordsAreSeparatedBySpaces()
    {
        // Arrange
        // Act
        var result = _generator.Generate(3, 3);

        // Assert
        var wordsAfterPreamble = result.Substring(VerificationPhraseGenerator.Preamble.Length).Trim();
        Assert.IsFalse(wordsAfterPreamble.Contains("  "), "Should not contain double spaces");
    }
}
