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

    [Fact]
    public void AliveMembers_ReturnsOnlyLivingCharacters()
    {
        // Arrange
        var party = new Party();
        var aliveChar = CreateTestCharacter("Alive");
        var deadChar = CreateTestCharacter("Dead");
        deadChar.ApplyDamage(1000); // Kill the character
        party.Add(aliveChar);
        party.Add(deadChar);

        // Act
        var aliveMembers = party.AliveMembers;

        // Assert
        Assert.Single(aliveMembers);
        Assert.Equal(aliveChar, aliveMembers[0]);
    }

    [Fact]
    public void IsDefeated_ReturnsFalse_WhenPartyHasAliveMembers()
    {
        // Arrange
        var party = new Party();
        var character = CreateTestCharacter("Alive");
        party.Add(character);

        // Act & Assert
        Assert.False(party.IsDefeated);
    }

    [Fact]
    public void IsDefeated_ReturnsTrue_WhenPartyHasNoAliveMembers()
    {
        // Arrange
        var party = new Party();
        var character = CreateTestCharacter("Dead");
        character.ApplyDamage(1000); // Kill the character
        party.Add(character);

        // Act & Assert
        Assert.True(party.IsDefeated);
    }

    [Fact]
    public void IsDefeated_ReturnsTrue_WhenPartyIsEmpty()
    {
        // Arrange
        var party = new Party();

        // Act & Assert
        Assert.True(party.IsDefeated);
    }

    private static Character CreateTestCharacter(string name)
    {
        var stats = new Stats(10, 10, 10, 100);
        var equipment = new Equipment(new Weapon("Sword", 5, 10, "Strength"));
        var skills = new List<Skill> { new Skill("Attack", "Basic attack", TargetingRule.SingleEnemy, 1.0, 0) };
        return new Character(name, Role.Tank, stats, equipment, skills);
    }
}