using DungeonPartyGame.Core.Models;
using DungeonPartyGame.Core.Services;
using Moq;
using Xunit;

namespace DungeonPartyGame;

public class CombatEventsTests
{
    [Fact]
    public void CombatEngine_FiresEvents_WhenCombatOccurs()
    {
        // Arrange
        var mockEventHandler = new Mock<ICombatEventHandler>();
        var diceService = new DiceService();
        var gearService = new GearService();
        var skillSelector = new Mock<ISkillSelector>();
        skillSelector.Setup(s => s.SelectSkill(It.IsAny<Character>(), It.IsAny<CombatSession>())).Returns((Skill)null);

        var combatEngine = new CombatEngine(diceService, gearService, skillSelector.Object, mockEventHandler.Object);

        var fighter = new Character("Fighter", CharacterRole.Fighter, new Stats(10, 10, 10, 100));
        var rogue = new Character("Rogue", CharacterRole.Rogue, new Stats(10, 10, 10, 10)); // Low HP to ensure defeat

        var partyA = new Party();
        partyA.Add(fighter);
        var partyB = new Party();
        partyB.Add(rogue);

        // Act
        var session = combatEngine.CreateSession(partyA, partyB);
        var result = combatEngine.ExecuteRound(session);

        // Assert
        mockEventHandler.Verify(h => h.OnCombatStarted(partyA, partyB), Times.Once);
        mockEventHandler.Verify(h => h.OnTurnStarted(fighter), Times.Once);
        mockEventHandler.Verify(h => h.OnDamageDealt(fighter, rogue, It.IsAny<int>(), false), Times.Once);
        mockEventHandler.Verify(h => h.OnTurnEnded(fighter), Times.Once);
        mockEventHandler.Verify(h => h.OnCharacterDefeated(rogue), Times.Once);
        mockEventHandler.Verify(h => h.OnCombatEnded(partyA, partyB), Times.Once);
    }
}