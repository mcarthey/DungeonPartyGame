using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using DungeonPartyGame.Core.Services;
using DungeonPartyGame.Core.Models;

namespace DungeonPartyGame.Tests;

public class EventServiceTests
{
    private readonly Mock<ILogger<EventService>> _mockLogger;
    private readonly Mock<ILogger<CurrencyService>> _mockCurrencyLogger;
    private readonly CurrencyService _currencyService;
    private readonly EventService _eventService;

    public EventServiceTests()
    {
        _mockLogger = new Mock<ILogger<EventService>>();
        _mockCurrencyLogger = new Mock<ILogger<CurrencyService>>();
        _currencyService = new CurrencyService(_mockCurrencyLogger.Object);
        _eventService = new EventService(_mockLogger.Object, _currencyService);
    }

    [Fact]
    public void GetActiveEvents_ReturnsOnlyActiveEvents()
    {
        // Act
        var activeEvents = _eventService.GetActiveEvents();

        // Assert
        Assert.NotEmpty(activeEvents);
        Assert.All(activeEvents, evt => Assert.True(evt.IsActive()));
    }

    [Fact]
    public void GetEventsByType_ReturnsCorrectType()
    {
        // Act
        var dailyQuests = _eventService.GetEventsByType(EventType.DailyQuest);

        // Assert
        Assert.NotEmpty(dailyQuests);
        Assert.All(dailyQuests, evt => Assert.Equal(EventType.DailyQuest, evt.Type));
    }

    [Fact]
    public void GetEventById_ReturnsCorrectEvent()
    {
        // Act
        var evt = _eventService.GetEventById("daily_combat");

        // Assert
        Assert.NotNull(evt);
        Assert.Equal("Daily Combat Challenge", evt.Name);
    }

    [Fact]
    public void UpdateObjectiveProgress_IncreasesProgress()
    {
        // Arrange
        var evt = _eventService.GetEventById("daily_combat");
        var objective = evt!.Objectives.First();
        var initialProgress = objective.CurrentAmount;

        // Act
        _eventService.UpdateObjectiveProgress("daily_combat", objective.Id, 1);

        // Assert
        Assert.Equal(initialProgress + 1, objective.CurrentAmount);
    }

    [Fact]
    public void UpdateObjectiveProgress_MarkEventAsCompleted_WhenAllObjectivesMet()
    {
        // Arrange
        var evt = _eventService.GetEventById("daily_combat");
        var objective = evt!.Objectives.First();

        // Act - Complete the objective
        _eventService.UpdateObjectiveProgress("daily_combat", objective.Id, objective.TargetAmount);

        // Assert
        Assert.True(evt.IsCompleted());
        Assert.Equal(EventStatus.Completed, evt.Status);
    }

    [Fact]
    public void UpdateObjectiveProgress_FiresEventCompletedEvent()
    {
        // Arrange
        var eventFired = false;
        GameEvent? completedEvent = null;

        _eventService.EventCompleted += (evt) =>
        {
            eventFired = true;
            completedEvent = evt;
        };

        var evt = _eventService.GetEventById("daily_progress");
        var objective = evt!.Objectives.First();

        // Act - Complete the objective
        _eventService.UpdateObjectiveProgress("daily_progress", objective.Id, objective.TargetAmount);

        // Assert
        Assert.True(eventFired);
        Assert.NotNull(completedEvent);
        Assert.Equal("daily_progress", completedEvent.Id);
    }

    [Fact]
    public void CanClaimRewards_ReturnsTrueWhenCompleted()
    {
        // Arrange
        var evt = _eventService.GetEventById("daily_progress");
        var objective = evt!.Objectives.First();
        _eventService.UpdateObjectiveProgress("daily_progress", objective.Id, objective.TargetAmount);

        // Act
        var canClaim = _eventService.CanClaimRewards("daily_progress");

        // Assert
        Assert.True(canClaim);
    }

    [Fact]
    public void CanClaimRewards_ReturnsFalseWhenNotCompleted()
    {
        // Act
        var canClaim = _eventService.CanClaimRewards("daily_progress");

        // Assert
        Assert.False(canClaim);
    }

    [Fact]
    public void TryClaimRewards_WithCompletedEvent_GrantsRewards()
    {
        // Arrange
        var evt = _eventService.GetEventById("daily_progress");
        var objective = evt!.Objectives.First();
        _eventService.UpdateObjectiveProgress("daily_progress", objective.Id, objective.TargetAmount);

        var initialGold = _currencyService.GetBalance(CurrencyType.Gold);

        // Act
        var success = _eventService.TryClaimRewards("daily_progress");

        // Assert
        Assert.True(success);
        Assert.Equal(initialGold + 300, _currencyService.GetBalance(CurrencyType.Gold));
        Assert.Equal(EventStatus.Claimed, evt.Status);
    }

    [Fact]
    public void TryClaimRewards_WithIncompleteEvent_Fails()
    {
        // Arrange
        var initialGold = _currencyService.GetBalance(CurrencyType.Gold);

        // Act
        var success = _eventService.TryClaimRewards("daily_progress");

        // Assert
        Assert.False(success);
        Assert.Equal(initialGold, _currencyService.GetBalance(CurrencyType.Gold));
    }

    [Fact]
    public void EventObjective_IsCompleted_WhenTargetMet()
    {
        // Arrange
        var objective = new EventObjective
        {
            TargetAmount = 10,
            CurrentAmount = 10
        };

        // Assert
        Assert.True(objective.IsCompleted);
    }

    [Fact]
    public void EventObjective_AddProgress_IncreasesCurrentAmount()
    {
        // Arrange
        var objective = new EventObjective
        {
            TargetAmount = 10,
            CurrentAmount = 5
        };

        // Act
        objective.AddProgress(3);

        // Assert
        Assert.Equal(8, objective.CurrentAmount);
    }

    [Fact]
    public void EventObjective_AddProgress_CapsAtTarget()
    {
        // Arrange
        var objective = new EventObjective
        {
            TargetAmount = 10,
            CurrentAmount = 8
        };

        // Act
        objective.AddProgress(5); // Would go to 13, but should cap at 10

        // Assert
        Assert.Equal(10, objective.CurrentAmount);
        Assert.True(objective.IsCompleted);
    }

    [Fact]
    public void EventObjective_GetProgressDisplay_FormatsCorrectly()
    {
        // Arrange
        var objective = new EventObjective
        {
            TargetAmount = 10,
            CurrentAmount = 7
        };

        // Act
        var display = objective.GetProgressDisplay();

        // Assert
        Assert.Equal("7/10", display);
    }

    [Fact]
    public void GameEvent_GetCompletionPercentage_CalculatesCorrectly()
    {
        // Arrange
        var evt = new GameEvent
        {
            Objectives = new List<EventObjective>
            {
                new EventObjective { TargetAmount = 10, CurrentAmount = 10 }, // Complete
                new EventObjective { TargetAmount = 10, CurrentAmount = 5 },  // Incomplete
                new EventObjective { TargetAmount = 10, CurrentAmount = 0 },  // Not started
                new EventObjective { TargetAmount = 10, CurrentAmount = 10 }  // Complete
            }
        };

        // Act
        var percentage = evt.GetCompletionPercentage();

        // Assert
        Assert.Equal(50f, percentage); // 2 out of 4 = 50%
    }

    [Fact]
    public void GameEvent_IsActive_ReturnsTrueWhenInTimeRange()
    {
        // Arrange
        var evt = new GameEvent
        {
            StartTime = DateTime.Now.AddHours(-1),
            EndTime = DateTime.Now.AddHours(1),
            Status = EventStatus.Active
        };

        // Act
        var isActive = evt.IsActive();

        // Assert
        Assert.True(isActive);
    }

    [Fact]
    public void GameEvent_IsActive_ReturnsFalseWhenExpired()
    {
        // Arrange
        var evt = new GameEvent
        {
            StartTime = DateTime.Now.AddHours(-2),
            EndTime = DateTime.Now.AddHours(-1),
            Status = EventStatus.Active
        };

        // Act
        var isActive = evt.IsActive();

        // Assert
        Assert.False(isActive);
    }

    [Fact]
    public void GameEvent_GetTimeRemaining_ReturnsCorrectValue()
    {
        // Arrange
        var evt = new GameEvent
        {
            StartTime = DateTime.Now.AddHours(-1),
            EndTime = DateTime.Now.AddHours(2),
            Status = EventStatus.Active
        };

        // Act
        var timeRemaining = evt.GetTimeRemaining();

        // Assert
        Assert.NotNull(timeRemaining);
        Assert.True(timeRemaining.Value.TotalHours > 1);
        Assert.True(timeRemaining.Value.TotalHours < 3);
    }

    [Fact]
    public void MultiObjectiveEvent_RequiresAllObjectivesForCompletion()
    {
        // Arrange
        var evt = _eventService.GetEventById("winter_festival");
        Assert.NotNull(evt);
        Assert.Equal(3, evt.Objectives.Count);

        // Act - Complete only 2 out of 3 objectives
        _eventService.UpdateObjectiveProgress("winter_festival", evt.Objectives[0].Id, evt.Objectives[0].TargetAmount);
        _eventService.UpdateObjectiveProgress("winter_festival", evt.Objectives[1].Id, evt.Objectives[1].TargetAmount);

        // Assert
        Assert.False(evt.IsCompleted());
        Assert.NotEqual(EventStatus.Completed, evt.Status);
    }

    [Fact]
    public void WeeklyEvent_HasCorrectTimeframe()
    {
        // Act
        var evt = _eventService.GetEventById("weekly_gear_master");

        // Assert
        Assert.NotNull(evt);
        Assert.Equal(EventType.WeeklyChallenge, evt.Type);
        var duration = evt.EndTime - evt.StartTime;
        Assert.True(duration.TotalDays >= 6 && duration.TotalDays <= 8); // Approximately 7 days
    }

    [Fact]
    public void AllEvents_HaveValidRewards()
    {
        // Act
        var events = _eventService.GetAllEvents();

        // Assert
        Assert.All(events, evt =>
        {
            var hasRewards = evt.Rewards.Count > 0 ||
                           evt.ExperienceReward.HasValue ||
                           evt.GearRewards.Count > 0;

            Assert.True(hasRewards, $"Event {evt.Name} has no rewards configured");
        });
    }

    [Fact]
    public void AllEvents_HaveValidObjectives()
    {
        // Act
        var events = _eventService.GetAllEvents();

        // Assert
        Assert.All(events, evt =>
        {
            Assert.NotEmpty(evt.Objectives);
            Assert.All(evt.Objectives, obj =>
            {
                Assert.True(obj.TargetAmount > 0, $"Event {evt.Name} has objective with invalid target");
            });
        });
    }
}

public class DailyRewardServiceTests
{
    private readonly Mock<ILogger<DailyRewardService>> _mockLogger;
    private readonly Mock<ILogger<CurrencyService>> _mockCurrencyLogger;
    private readonly CurrencyService _currencyService;
    private readonly DailyRewardService _dailyRewardService;

    public DailyRewardServiceTests()
    {
        _mockLogger = new Mock<ILogger<DailyRewardService>>();
        _mockCurrencyLogger = new Mock<ILogger<CurrencyService>>();
        _currencyService = new CurrencyService(_mockCurrencyLogger.Object);
        _dailyRewardService = new DailyRewardService(_mockLogger.Object, _currencyService);
    }

    [Fact]
    public void GetLoginStreak_ReturnsInitializedStreak()
    {
        // Act
        var streak = _dailyRewardService.GetLoginStreak();

        // Assert
        Assert.NotNull(streak);
        Assert.Equal(7, streak.RewardSchedule.Count);
    }

    [Fact]
    public void CanClaimDailyReward_ReturnsTrueOnFirstLogin()
    {
        // Act
        var canClaim = _dailyRewardService.CanClaimDailyReward();

        // Assert
        Assert.True(canClaim);
    }

    [Fact]
    public void TryClaimDailyReward_OnFirstLogin_Succeeds()
    {
        // Arrange
        var initialGold = _currencyService.GetBalance(CurrencyType.Gold);

        // Act
        var success = _dailyRewardService.TryClaimDailyReward();

        // Assert
        Assert.True(success);
        Assert.Equal(1, _dailyRewardService.GetLoginStreak().CurrentStreak);
        Assert.Equal(initialGold + 100, _currencyService.GetBalance(CurrencyType.Gold)); // Day 1 reward
    }

    [Fact]
    public void TryClaimDailyReward_AlreadyClaimedToday_Fails()
    {
        // Arrange
        _dailyRewardService.TryClaimDailyReward();
        var goldAfterFirstClaim = _currencyService.GetBalance(CurrencyType.Gold);

        // Act
        var success = _dailyRewardService.TryClaimDailyReward();

        // Assert
        Assert.False(success);
        Assert.Equal(goldAfterFirstClaim, _currencyService.GetBalance(CurrencyType.Gold)); // No additional reward
    }

    [Fact]
    public void TryClaimDailyReward_FiresRewardClaimedEvent()
    {
        // Arrange
        var eventFired = false;
        DailyReward? claimedReward = null;

        _dailyRewardService.RewardClaimed += (reward) =>
        {
            eventFired = true;
            claimedReward = reward;
        };

        // Act
        _dailyRewardService.TryClaimDailyReward();

        // Assert
        Assert.True(eventFired);
        Assert.NotNull(claimedReward);
        Assert.True(claimedReward.IsClaimed);
    }

    [Fact]
    public void GetRewardSchedule_Returns7DaySchedule()
    {
        // Act
        var schedule = _dailyRewardService.GetRewardSchedule();

        // Assert
        Assert.Equal(7, schedule.Count);
        Assert.All(schedule, reward => Assert.NotEmpty(reward.Rewards));
    }

    [Fact]
    public void RewardSchedule_IncreasingRewards()
    {
        // Act
        var schedule = _dailyRewardService.GetRewardSchedule();

        // Assert
        var day1Gold = schedule[0].Rewards.First(r => r.Type == CurrencyType.Gold).Amount;
        var day7Gold = schedule[6].Rewards.First(r => r.Type == CurrencyType.Gold).Amount;

        Assert.True(day7Gold > day1Gold, "Day 7 reward should be greater than Day 1");
        Assert.Equal(100, day1Gold);
        Assert.Equal(1000, day7Gold);
    }

    [Fact]
    public void RewardSchedule_Day3And5And7_IncludeGems()
    {
        // Act
        var schedule = _dailyRewardService.GetRewardSchedule();

        // Assert
        Assert.Contains(schedule[2].Rewards, r => r.Type == CurrencyType.Gems); // Day 3
        Assert.Contains(schedule[4].Rewards, r => r.Type == CurrencyType.Gems); // Day 5
        Assert.Contains(schedule[6].Rewards, r => r.Type == CurrencyType.Gems); // Day 7
    }

    [Fact]
    public void DailyReward_GetRewardDisplay_FormatsCorrectly()
    {
        // Arrange
        var reward = new DailyReward
        {
            Rewards = new List<Currency>
            {
                new Currency(CurrencyType.Gold, 500),
                new Currency(CurrencyType.Gems, 10)
            }
        };

        // Act
        var display = reward.GetRewardDisplay();

        // Assert
        Assert.Contains("💰", display);
        Assert.Contains("500", display);
        Assert.Contains("💎", display);
        Assert.Contains("10", display);
    }

    [Fact]
    public void LoginStreak_CanClaimToday_ReturnsTrueForNewPlayer()
    {
        // Arrange
        var streak = new LoginStreak();

        // Act
        var canClaim = streak.CanClaimToday();

        // Assert
        Assert.True(canClaim);
    }

    [Fact]
    public void LoginStreak_UpdateStreak_IncrementsOnFirstLogin()
    {
        // Arrange
        var streak = new LoginStreak
        {
            RewardSchedule = _dailyRewardService.GetRewardSchedule()
        };

        // Act
        streak.UpdateStreak();

        // Assert
        Assert.Equal(1, streak.CurrentStreak);
        Assert.Equal(1, streak.LongestStreak);
    }

    [Fact]
    public void LoginStreak_GetTodaysReward_ReturnsCyclicReward()
    {
        // Arrange
        var streak = new LoginStreak
        {
            CurrentStreak = 8, // Beyond 7 days, should cycle back
            RewardSchedule = _dailyRewardService.GetRewardSchedule()
        };

        // Act
        var reward = streak.GetTodaysReward();

        // Assert
        Assert.NotNull(reward);
        Assert.Equal(1, reward.Day); // Day 8 cycles to Day 1
    }

    [Fact]
    public void AllDailyRewards_HaveValidCurrencyAmounts()
    {
        // Act
        var schedule = _dailyRewardService.GetRewardSchedule();

        // Assert
        Assert.All(schedule, reward =>
        {
            Assert.All(reward.Rewards, currency =>
            {
                Assert.True(currency.Amount > 0, $"Day {reward.Day} has invalid currency amount");
            });
        });
    }
}
