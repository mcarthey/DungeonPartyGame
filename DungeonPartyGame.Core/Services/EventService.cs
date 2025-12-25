using Microsoft.Extensions.Logging;

namespace DungeonPartyGame.Core.Services;
using DungeonPartyGame.Core.Models;

public class EventService
{
    private readonly ILogger<EventService> _logger;
    private readonly CurrencyService _currencyService;
    private readonly List<GameEvent> _events = new();

    public event Action<GameEvent>? EventStarted;
    public event Action<GameEvent>? EventCompleted;
    public event Action<GameEvent>? EventExpired;

    public EventService(ILogger<EventService> logger, CurrencyService currencyService)
    {
        _logger = logger;
        _currencyService = currencyService;
        InitializeEvents();
    }

    private void InitializeEvents()
    {
        _events.Clear();
        var now = DateTime.Now;

        // Daily Quest - Combat focused
        var dailyCombat = new GameEvent
        {
            Id = "daily_combat",
            Name = "Daily Combat Challenge",
            Description = "Win 5 combats today",
            Type = EventType.DailyQuest,
            Status = EventStatus.Active,
            StartTime = now.Date,
            EndTime = now.Date.AddDays(1).AddSeconds(-1),
            Objectives = new List<EventObjective>
            {
                new EventObjective
                {
                    Description = "Win 5 combats",
                    TargetAmount = 5,
                    CurrentAmount = 0
                }
            },
            Rewards = new List<Currency>
            {
                new Currency(CurrencyType.Gold, 500),
                new Currency(CurrencyType.EventTokens, 10)
            },
            IconEmoji = "⚔️",
            ThemeColor = "#E74C3C"
        };
        _events.Add(dailyCombat);

        // Daily Quest - Level up
        var dailyProgress = new GameEvent
        {
            Id = "daily_progress",
            Name = "Character Development",
            Description = "Level up any character or unlock a skill",
            Type = EventType.DailyQuest,
            Status = EventStatus.Active,
            StartTime = now.Date,
            EndTime = now.Date.AddDays(1).AddSeconds(-1),
            Objectives = new List<EventObjective>
            {
                new EventObjective
                {
                    Description = "Gain 1000 XP",
                    TargetAmount = 1000,
                    CurrentAmount = 0
                }
            },
            Rewards = new List<Currency>
            {
                new Currency(CurrencyType.Gold, 300)
            },
            IconEmoji = "📈",
            ThemeColor = "#3498DB"
        };
        _events.Add(dailyProgress);

        // Weekly Challenge
        var weeklyChallenge = new GameEvent
        {
            Id = "weekly_gear_master",
            Name = "Gear Master",
            Description = "Upgrade gear 10 times this week",
            Type = EventType.WeeklyChallenge,
            Status = EventStatus.Active,
            StartTime = GetStartOfWeek(now),
            EndTime = GetStartOfWeek(now).AddDays(7).AddSeconds(-1),
            Objectives = new List<EventObjective>
            {
                new EventObjective
                {
                    Description = "Upgrade gear 10 times",
                    TargetAmount = 10,
                    CurrentAmount = 0
                }
            },
            Rewards = new List<Currency>
            {
                new Currency(CurrencyType.Gold, 2000),
                new Currency(CurrencyType.Gems, 25),
                new Currency(CurrencyType.EventTokens, 50)
            },
            ExperienceReward = 500,
            IconEmoji = "🏆",
            ThemeColor = "#F39C12"
        };
        _events.Add(weeklyChallenge);

        // Limited Time Event - Holiday Special
        var holidayEvent = new GameEvent
        {
            Id = "winter_festival",
            Name = "🎄 Winter Festival",
            Description = "Celebrate the season with special rewards!",
            Type = EventType.Holiday,
            Status = EventStatus.Active,
            StartTime = now,
            EndTime = now.AddDays(7),
            Objectives = new List<EventObjective>
            {
                new EventObjective
                {
                    Description = "Win 20 combats",
                    TargetAmount = 20,
                    CurrentAmount = 0
                },
                new EventObjective
                {
                    Description = "Collect 10000 Gold",
                    TargetAmount = 10000,
                    CurrentAmount = 0
                },
                new EventObjective
                {
                    Description = "Unlock 5 skills",
                    TargetAmount = 5,
                    CurrentAmount = 0
                }
            },
            Rewards = new List<Currency>
            {
                new Currency(CurrencyType.Gold, 5000),
                new Currency(CurrencyType.Gems, 100),
                new Currency(CurrencyType.EventTokens, 200)
            },
            IconEmoji = "🎁",
            ThemeColor = "#16A085"
        };
        _events.Add(holidayEvent);

        _logger.LogInformation($"Initialized {_events.Count} events");
    }

    private DateTime GetStartOfWeek(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * diff).Date;
    }

    public List<GameEvent> GetActiveEvents()
    {
        return _events.Where(e => e.IsActive()).ToList();
    }

    public List<GameEvent> GetEventsByType(EventType type)
    {
        return _events.Where(e => e.Type == type && e.IsActive()).ToList();
    }

    public GameEvent? GetEventById(string id)
    {
        return _events.FirstOrDefault(e => e.Id == id);
    }

    public void UpdateObjectiveProgress(string eventId, string objectiveId, int progress)
    {
        var gameEvent = GetEventById(eventId);
        if (gameEvent == null)
            return;

        var objective = gameEvent.Objectives.FirstOrDefault(o => o.Id == objectiveId);
        if (objective == null)
            return;

        objective.AddProgress(progress);
        _logger.LogInformation($"Event '{gameEvent.Name}' objective progress: {objective.GetProgressDisplay()}");

        if (gameEvent.IsCompleted() && gameEvent.Status != EventStatus.Completed)
        {
            gameEvent.Status = EventStatus.Completed;
            _logger.LogInformation($"Event '{gameEvent.Name}' completed!");
            EventCompleted?.Invoke(gameEvent);
        }
    }

    public bool CanClaimRewards(string eventId)
    {
        var gameEvent = GetEventById(eventId);
        return gameEvent != null && gameEvent.IsCompleted() && gameEvent.Status == EventStatus.Completed;
    }

    public bool TryClaimRewards(string eventId)
    {
        var gameEvent = GetEventById(eventId);
        if (gameEvent == null || !CanClaimRewards(eventId))
        {
            _logger.LogWarning($"Cannot claim rewards for event: {eventId}");
            return false;
        }

        // Grant rewards
        if (gameEvent.Rewards.Count > 0)
        {
            _currencyService.AddCurrency(gameEvent.Rewards, $"Event reward: {gameEvent.Name}");
        }

        if (gameEvent.ExperienceReward.HasValue)
        {
            _logger.LogInformation($"Granted {gameEvent.ExperienceReward} XP from event: {gameEvent.Name}");
        }

        gameEvent.Status = EventStatus.Claimed;
        _logger.LogInformation($"Claimed rewards for event: {gameEvent.Name}");

        return true;
    }

    public void CheckExpiredEvents()
    {
        var now = DateTime.Now;
        foreach (var gameEvent in _events.Where(e => e.Status == EventStatus.Active || e.Status == EventStatus.Completed))
        {
            if (now > gameEvent.EndTime)
            {
                gameEvent.Status = EventStatus.Expired;
                _logger.LogInformation($"Event expired: {gameEvent.Name}");
                EventExpired?.Invoke(gameEvent);
            }
        }
    }

    public List<GameEvent> GetAllEvents()
    {
        return _events.ToList();
    }
}

public class DailyRewardService
{
    private readonly ILogger<DailyRewardService> _logger;
    private readonly CurrencyService _currencyService;
    private LoginStreak _loginStreak;

    public event Action<DailyReward>? RewardClaimed;

    public DailyRewardService(ILogger<DailyRewardService> logger, CurrencyService currencyService)
    {
        _logger = logger;
        _currencyService = currencyService;
        _loginStreak = InitializeLoginStreak();
    }

    private LoginStreak InitializeLoginStreak()
    {
        var streak = new LoginStreak();

        // Define 7-day reward cycle
        streak.RewardSchedule = new List<DailyReward>
        {
            new DailyReward
            {
                Day = 1,
                Rewards = new List<Currency> { new Currency(CurrencyType.Gold, 100) }
            },
            new DailyReward
            {
                Day = 2,
                Rewards = new List<Currency> { new Currency(CurrencyType.Gold, 200) }
            },
            new DailyReward
            {
                Day = 3,
                Rewards = new List<Currency> { new Currency(CurrencyType.Gold, 300), new Currency(CurrencyType.Gems, 5) }
            },
            new DailyReward
            {
                Day = 4,
                Rewards = new List<Currency> { new Currency(CurrencyType.Gold, 400) }
            },
            new DailyReward
            {
                Day = 5,
                Rewards = new List<Currency> { new Currency(CurrencyType.Gold, 500), new Currency(CurrencyType.Gems, 10) }
            },
            new DailyReward
            {
                Day = 6,
                Rewards = new List<Currency> { new Currency(CurrencyType.Gold, 750) }
            },
            new DailyReward
            {
                Day = 7,
                Rewards = new List<Currency> { new Currency(CurrencyType.Gold, 1000), new Currency(CurrencyType.Gems, 25) }
            }
        };

        return streak;
    }

    public LoginStreak GetLoginStreak()
    {
        return _loginStreak;
    }

    public bool CanClaimDailyReward()
    {
        return _loginStreak.CanClaimToday();
    }

    public DailyReward? GetTodaysReward()
    {
        return _loginStreak.GetTodaysReward();
    }

    public bool TryClaimDailyReward()
    {
        if (!CanClaimDailyReward())
        {
            _logger.LogWarning("Cannot claim daily reward - already claimed today");
            return false;
        }

        _loginStreak.UpdateStreak();

        var reward = _loginStreak.GetTodaysReward();
        if (reward == null)
        {
            _logger.LogWarning("No reward configured for today");
            return false;
        }

        // Grant rewards
        _currencyService.AddCurrency(reward.Rewards, "Daily Login Reward");

        reward.IsClaimed = true;
        reward.ClaimedAt = DateTime.Now;

        _logger.LogInformation($"Claimed daily reward for day {_loginStreak.CurrentStreak}. Rewards: {reward.GetRewardDisplay()}");
        RewardClaimed?.Invoke(reward);

        return true;
    }

    public List<DailyReward> GetRewardSchedule()
    {
        return _loginStreak.RewardSchedule;
    }
}
