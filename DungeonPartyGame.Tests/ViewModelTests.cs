using DungeonPartyGame.Core.Models;
using DungeonPartyGame.Core.Services;
using DungeonPartyGame.UI.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Moq;
using Xunit;

namespace DungeonPartyGame.Tests;

public class ViewModelTests
{
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly Mock<ILogger<PartyViewModel>> _partyLoggerMock;
    private readonly Mock<ILogger<MainViewModel>> _mainLoggerMock;
    private readonly DiceService _diceService;
    private readonly GearService _gearService;
    private readonly Mock<ISkillSelector> _skillSelectorMock;

    public ViewModelTests()
    {
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _partyLoggerMock = new Mock<ILogger<PartyViewModel>>();
        _mainLoggerMock = new Mock<ILogger<MainViewModel>>();
        _diceService = new DiceService();
        _gearService = new GearService();
        _skillSelectorMock = new Mock<ISkillSelector>();

        _loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_partyLoggerMock.Object);
        _mainLoggerMock = new Mock<ILogger<MainViewModel>>();

        _loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
                         .Returns(_partyLoggerMock.Object);
        _loggerFactoryMock.Setup(x => x.CreateLogger(nameof(PartyViewModel)))
                         .Returns(_partyLoggerMock.Object);
        _loggerFactoryMock.Setup(x => x.CreateLogger(nameof(MainViewModel)))
                         .Returns(_mainLoggerMock.Object);
    }

    [Fact]
    public void PartyViewModel_InitializesWithPartyMembers()
    {
        // Arrange
        var gameSession = new GameSession();
        var party = new Party();
        var character = new Character("Test Fighter", CharacterRole.Fighter, new Stats(15, 12, 14, 100));
        party.Add(character);
        gameSession.AddParty(party);

        // Act
        var viewModel = new PartyViewModel(gameSession, _loggerFactoryMock.Object);

        // Assert
        Assert.Single(viewModel.PartyMembers);
        Assert.Equal("Test Fighter", viewModel.PartyMembers[0].Name);
        Assert.Null(viewModel.SelectedCharacter);
    }

    [Fact]
    public void PartyViewModel_SelectCharacterCommand_SetsSelectedCharacter()
    {
        // Arrange
        var gameSession = new GameSession();
        var party = new Party();
        var character = new Character("Test Fighter", CharacterRole.Fighter, new Stats(15, 12, 14, 100));
        party.Add(character);
        gameSession.AddParty(party);

        var viewModel = new PartyViewModel(gameSession, _loggerFactoryMock.Object);

        // Act
        viewModel.SelectCharacterCommand.Execute(character);

        // Assert
        Assert.Equal(character, viewModel.SelectedCharacter);
    }

    [Fact]
    public void PartyViewModel_NavigateToSkillsCommand_ExecutesWithoutError()
    {
        // Arrange
        var gameSession = new GameSession();
        var party = new Party();
        var character = new Character("Test Fighter", CharacterRole.Fighter, new Stats(15, 12, 14, 100));
        party.Add(character);
        gameSession.AddParty(party);

        var viewModel = new PartyViewModel(gameSession, _loggerFactoryMock.Object);
        viewModel.SelectedCharacter = character;

        // Act & Assert - Should not throw
        var exception = Record.Exception(() => viewModel.NavigateToSkillsCommand.Execute(null));
        Assert.Null(exception);
    }

    [Fact]
    public void PartyViewModel_NavigateBackCommand_ExecutesWithoutError()
    {
        // Arrange
        var gameSession = new GameSession();
        var party = new Party();
        gameSession.AddParty(party);

        var viewModel = new PartyViewModel(gameSession, _loggerFactoryMock.Object);

        // Act & Assert - Should not throw
        var exception = Record.Exception(() => viewModel.NavigateBackCommand.Execute(null));
        Assert.Null(exception);
    }
}