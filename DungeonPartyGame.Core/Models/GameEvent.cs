namespace DungeonPartyGame.Core.Models;

public enum EventType
{
    DailyQuest,     // Complete daily objectives
    WeeklyChallenge, // Harder weekly objectives
    LimitedTime,    // Special timed events
    Holiday,        // Seasonal/holiday events
    Community       // Server-wide events
}

public enum EventStatus
{
    Upcoming,
    Active,
    Completed,
    Expired,
    Claimed
}

public class GameEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public EventType Type { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Upcoming;

    // Timing
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    // Objectives
    public List<EventObjective> Objectives { get; set; } = new();

    // Rewards
    public List<Currency> Rewards { get; set; } = new();
    public List<string> GearRewards { get; set; } = new();
    public int? ExperienceReward { get; set; }

    // Visual
    public string IconEmoji { get; set; } = "🎯";
    public string ThemeColor { get; set; } = "#4CAF50";

    public bool IsActive()
    {
        var now = DateTime.Now;
        return now >= StartTime && now <= EndTime && Status == EventStatus.Active;
    }

    public TimeSpan? GetTimeRemaining()
    {
        if (!IsActive())
            return null;

        var remaining = EndTime - DateTime.Now;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    public TimeSpan? GetTimeUntilStart()
    {
        if (DateTime.Now >= StartTime)
            return null;

        return StartTime - DateTime.Now;
    }

    public float GetCompletionPercentage()
    {
        if (Objectives.Count == 0)
            return 0;

        var completed = Objectives.Count(o => o.IsCompleted);
        return (float)completed / Objectives.Count * 100;
    }

    public bool IsCompleted()
    {
        return Objectives.All(o => o.IsCompleted);
    }
}

public class EventObjective
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Description { get; set; } = string.Empty;
    public int TargetAmount { get; set; }
    public int CurrentAmount { get; set; }
    public bool IsCompleted => CurrentAmount >= TargetAmount;

    public void AddProgress(int amount)
    {
        CurrentAmount = Math.Min(CurrentAmount + amount, TargetAmount);
    }

    public string GetProgressDisplay()
    {
        return $"{CurrentAmount}/{TargetAmount}";
    }
}

public class DailyReward
{
    public int Day { get; set; }
    public List<Currency> Rewards { get; set; } = new();
    public bool IsClaimed { get; set; }
    public DateTime? ClaimedAt { get; set; }

    public string GetRewardDisplay()
    {
        if (Rewards.Count == 0)
            return "No rewards";

        return string.Join(", ", Rewards.Select(r => $"{GetCurrencySymbol(r.Type)}{r.Amount}"));
    }

    private static string GetCurrencySymbol(CurrencyType type)
    {
        return type switch
        {
            CurrencyType.Gold => "💰",
            CurrencyType.Gems => "💎",
            CurrencyType.EventTokens => "🎫",
            CurrencyType.BattlePoints => "⭐",
            _ => ""
        };
    }
}

public class LoginStreak
{
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public List<DailyReward> RewardSchedule { get; set; } = new();

    public bool CanClaimToday()
    {
        if (!LastLoginDate.HasValue)
            return true;

        return LastLoginDate.Value.Date < DateTime.Now.Date;
    }

    public void UpdateStreak()
    {
        var now = DateTime.Now;

        if (!LastLoginDate.HasValue)
        {
            // First login ever
            CurrentStreak = 1;
            LongestStreak = 1;
        }
        else if (LastLoginDate.Value.Date == now.Date.AddDays(-1))
        {
            // Consecutive day
            CurrentStreak++;
            LongestStreak = Math.Max(LongestStreak, CurrentStreak);
        }
        else if (LastLoginDate.Value.Date < now.Date.AddDays(-1))
        {
            // Streak broken
            CurrentStreak = 1;
        }
        // Same day login doesn't change streak

        LastLoginDate = now;
    }

    public DailyReward? GetTodaysReward()
    {
        if (RewardSchedule.Count == 0 || CurrentStreak <= 0)
            return null;

        // Cycle through reward schedule (1-indexed streak to 0-indexed array)
        var dayIndex = (CurrentStreak - 1) % RewardSchedule.Count;
        return RewardSchedule[dayIndex];
    }
}
