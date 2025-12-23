using DungeonPartyGame.Core.Models;
using DungeonPartyGame.Core.Services;
using DungeonPartyGame.UI.ViewModels;
using Moq;
using Xunit;

namespace DungeonPartyGame;

public class MainViewModelTests
{
    private readonly Mock<CombatEngine> _combatEngineMock;
    private readonly Mock<DiceService> _diceServiceMock;
    private readonly MainViewModel _viewModel;

    public MainViewModelTests()
    {
        _diceServiceMock = new Mock<DiceService>(MockBehavior.Loose, (Random)null);
        _combatEngineMock = new Mock<CombatEngine>(MockBehavior.Loose, _diceServiceMock.Object);
        _viewModel = new MainViewModel(_combatEngineMock.Object, _diceServiceMock.Object);
    }

    [Fact]
    public void Constructor_InitializesCommands()
    {
        // Assert
        Assert.NotNull(_viewModel.CreateCharactersCommand);
        Assert.NotNull(_viewModel.StartNewCombatCommand);
        Assert.NotNull(_viewModel.NextRoundCommand);
    }

    [Fact]
    public void CreateCharactersCommand_CreatesCharacters_SetsCombatLog()
    {
        // Act
        _viewModel.CreateCharactersCommand.Execute(null);

        // Assert
        Assert.Equal("Characters created. Press Start New Combat.", _viewModel.CombatLog);
        Assert.True(_viewModel.StartNewCombatCommand.CanExecute(null));
        Assert.False(_viewModel.NextRoundCommand.CanExecute(null));
    }

    [Fact]
    public void StartNewCombatCommand_CreatesSession_WhenCharactersExist()
    {
        // Arrange
        _viewModel.CreateCharactersCommand.Execute(null);
        var session = new CombatSession(CreateTestCharacter("A"), CreateTestCharacter("B"));
        _combatEngineMock.Setup(c => c.CreateSession(It.IsAny<Character>(), It.IsAny<Character>())).Returns(session);

        // Act
        _viewModel.StartNewCombatCommand.Execute(null);

        // Assert
        _combatEngineMock.Verify(c => c.CreateSession(It.IsAny<Character>(), It.IsAny<Character>()), Times.Once);
        Assert.Contains("Combat begins!", _viewModel.CombatLog);
        Assert.True(_viewModel.NextRoundCommand.CanExecute(null));
    }

    [Fact]
    public void StartNewCombatCommand_ShowsError_WhenNoCharacters()
    {
        // Act
        _viewModel.StartNewCombatCommand.Execute(null);

        // Assert
        Assert.Equal("Create characters first.", _viewModel.CombatLog);
    }

    [Fact]
    public void NextRoundCommand_ExecutesRound_UpdatesLog()
    {
        // Arrange
        _viewModel.CreateCharactersCommand.Execute(null);
        var session = new CombatSession(CreateTestCharacter("A"), CreateTestCharacter("B"));
        _combatEngineMock.Setup(c => c.CreateSession(It.IsAny<Character>(), It.IsAny<Character>())).Returns(session);
        _viewModel.StartNewCombatCommand.Execute(null);

        var result = new CombatResult
        {
            Round = 1,
            Attacker = "A",
            Defender = "B",
            SkillUsed = "Attack",
            Damage = 10,
            DefenderHP = 90,
            IsDefeated = false,
            LogMessage = "Round 1\nA uses Attack\nDamage: 10\nB HP: 90",
            IsFinalRound = false,
            SummaryText = "A attacks B for 10 damage."
        };
        _combatEngineMock.Setup(c => c.ExecuteRound(session)).Returns(result);

        // Act
        _viewModel.NextRoundCommand.Execute(null);

        // Assert
        _combatEngineMock.Verify(c => c.ExecuteRound(session), Times.Once);
        Assert.Contains("Round 1", _viewModel.CombatLog);
        Assert.Contains("A uses Attack", _viewModel.CombatLog);
    }

    [Fact]
    public void NextRoundCommand_DoesNothing_WhenNoSession()
    {
        // Act
        _viewModel.NextRoundCommand.Execute(null);

        // Assert - Should not throw, just do nothing
        _combatEngineMock.Verify(c => c.ExecuteRound(It.IsAny<CombatSession>()), Times.Never);
    }

    private static Character CreateTestCharacter(string name)
    {
        var stats = new Stats(10, 10, 10, 100);
        var equipment = new Equipment(new Weapon("Sword", 5, 10));
        var skills = new List<Skill> { new Skill("Attack", "Basic attack", 1.0, 0) };
        return new Character(name, stats, equipment, skills);
    }
}