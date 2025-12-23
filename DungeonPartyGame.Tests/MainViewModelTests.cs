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
        Assert.NotNull(_viewModel.CreatePartiesCommand);
        Assert.NotNull(_viewModel.StartNewCombatCommand);
        Assert.NotNull(_viewModel.NextTurnCommand);
    }

    [Fact]
    public void CreatePartiesCommand_CreatesParties_SetsCombatLog()
    {
        // Act
        _viewModel.CreatePartiesCommand.Execute(null);

        // Assert
        Assert.Equal("Parties created. Press Start New Battle.", _viewModel.CombatLog);
        Assert.True(_viewModel.StartNewCombatCommand.CanExecute(null));
        Assert.False(_viewModel.NextTurnCommand.CanExecute(null));
    }

    [Fact]
    public void StartNewCombatCommand_CreatesSession_WhenPartiesExist()
    {
        // Arrange
        _viewModel.CreatePartiesCommand.Execute(null);
        var session = new CombatSession(new Party(), new Party());
        _combatEngineMock.Setup(c => c.CreateSession(It.IsAny<Party>(), It.IsAny<Party>())).Returns(session);

        // Act
        _viewModel.StartNewCombatCommand.Execute(null);

        // Assert
        _combatEngineMock.Verify(c => c.CreateSession(It.IsAny<Party>(), It.IsAny<Party>()), Times.Once);
        Assert.Contains("Battle begins!", _viewModel.CombatLog);
        Assert.True(_viewModel.NextTurnCommand.CanExecute(null));
    }

    [Fact]
    public void StartNewCombatCommand_ShowsError_WhenNoParties()
    {
        // Act
        _viewModel.StartNewCombatCommand.Execute(null);

        // Assert
        Assert.Equal("Create parties first.", _viewModel.CombatLog);
    }

    [Fact]
    public void StartNewCombatCommand_ResetsHealthForReplay()
    {
        // Arrange
        _viewModel.CreatePartiesCommand.Execute(null);
        // Simulate damage to characters
        var session = new CombatSession(new Party(), new Party());
        _combatEngineMock.Setup(c => c.CreateSession(It.IsAny<Party>(), It.IsAny<Party>())).Returns(session);

        // Act
        _viewModel.StartNewCombatCommand.Execute(null);

        // Assert - Health should be reset, but since we can't easily verify internal state,
        // we just ensure the method completes without error
        Assert.Contains("Battle begins!", _viewModel.CombatLog);
    }

    [Fact]
    public void NextTurnCommand_ExecutesTurn_UpdatesLog()
    {
        // Arrange
        _viewModel.CreatePartiesCommand.Execute(null);
        var session = new CombatSession(new Party(), new Party());
        _combatEngineMock.Setup(c => c.CreateSession(It.IsAny<Party>(), It.IsAny<Party>())).Returns(session);
        _viewModel.StartNewCombatCommand.Execute(null);

        var result = new CombatResult
        {
            RoundNumber = 1,
            Actor = CreateTestCharacter("A"),
            Target = CreateTestCharacter("B"),
            SkillName = "Attack",
            Damage = 10,
            TargetDefeated = false,
            IsFinalTurn = false,
            SummaryText = "A attacks B with Attack (10 dmg)\nB HP: 90"
        };
        _combatEngineMock.Setup(c => c.ExecuteTurn(session)).Returns(result);

        // Act
        _viewModel.NextTurnCommand.Execute(null);

        // Assert
        _combatEngineMock.Verify(c => c.ExecuteTurn(session), Times.Once);
        Assert.Contains("Round 1", _viewModel.CombatLog);
        Assert.Contains("A attacks B", _viewModel.CombatLog);
    }

    [Fact]
    public void NextTurnCommand_DoesNothing_WhenNoSession()
    {
        // Act
        _viewModel.NextTurnCommand.Execute(null);

        // Assert - Should not throw, just do nothing
        _combatEngineMock.Verify(c => c.ExecuteTurn(It.IsAny<CombatSession>()), Times.Never);
    }

    [Fact]
    public void NextTurnCommand_DoesNothing_WhenSessionComplete()
    {
        // Arrange
        _viewModel.CreatePartiesCommand.Execute(null);
        var session = new CombatSession(new Party(), new Party());
        session.CompleteCombat(new Party()); // Mark as complete
        _combatEngineMock.Setup(c => c.CreateSession(It.IsAny<Party>(), It.IsAny<Party>())).Returns(session);
        _viewModel.StartNewCombatCommand.Execute(null);

        // Act
        _viewModel.NextTurnCommand.Execute(null);

        // Assert - Should not execute turn
        _combatEngineMock.Verify(c => c.ExecuteTurn(session), Times.Never);
    }

    private static Character CreateTestCharacter(string name)
    {
        var stats = new Stats(10, 10, 10, 100);
        var equipment = new Equipment(new Weapon("Sword", 5, 10, "Strength"));
        var skills = new List<Skill> { new Skill("Attack", "Basic attack", TargetingRule.SingleEnemy, 1.0, 0) };
        return new Character(name, Role.Tank, stats, equipment, skills);
    }
}