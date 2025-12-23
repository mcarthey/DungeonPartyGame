using DungeonPartyGame.Core.Models;
using Xunit;

namespace DungeonPartyGame;

public class PartyTests
{
    [Fact]
    public void Add_AddsCharacterToParty()
    {
        // Arrange
        var party = new Party();
        var character = CreateTestCharacter("Test");

        // Act
        party.Add(character);

        // Assert
        Assert.Single(party.Members);
        Assert.Equal(character, party.Members[0]);
    }

    [Fact]
    public void Add_ThrowsException_WhenPartyIsFull()
    {
        // Arrange
        var party = new Party();
        for (int i = 0; i < 5; i++)
        {
            party.Add(CreateTestCharacter($"Char{i}"));
        }

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => party.Add(CreateTestCharacter("Extra")));
        Assert.Equal("Party is full", exception.Message);
    }

    [Fact]
    public void Members_ReturnsReadOnlyList()
    {
        // Arrange
        var party = new Party();
        var character = CreateTestCharacter("Test");
        party.Add(character);

        // Act & Assert
        Assert.IsAssignableFrom<IReadOnlyList<Character>>(party.Members);
    }

    private static Character CreateTestCharacter(string name)
    {
        var stats = new Stats(10, 10, 10, 100);
        var equipment = new Equipment(new Weapon("Sword", 5, 10));
        var skills = new List<Skill> { new Skill("Attack", "Basic attack", 1.0, 0) };
        return new Character(name, stats, equipment, skills);
    }
}