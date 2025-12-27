using DungeonPartyGame.MonoGame.UI;
using Xunit;
using System.Linq;

namespace DungeonPartyGame.Tests
{
    public class CombatScreenLayoutTests
    {
        [Fact]
        public void ComputeHealthWidth_ReturnsProportionalWidth()
        {
            int barWidth = 60;
            int current = 15;
            int max = 30;

            int w = CombatLayoutUtils.ComputeHealthWidth(barWidth, current, max);

            Assert.Equal(30, w); // 50% of 60
        }

        [Fact]
        public void WrapTextSimple_SplitsLongWordsAndRespectsMaxChars()
        {
            var text = "This is a long message that should be wrapped into multiple lines to improve readability in the combat log.";
            var lines = CombatLayoutUtils.WrapTextSimple(text, 20);

            Assert.True(lines.Count > 1);
            Assert.All(lines, l => Assert.True(l.Length <= 20));

            // Ensure no content lost
            var recombined = string.Join(" ", lines.Select(l => l.Trim()));
            Assert.Contains("combat log", recombined);
        }

        [Fact]
        public void WrapTextSimple_BreaksLongSingleWords()
        {
            var word = "AAAAAAAAAAAAAAAAAAAAAAAAAA"; // 26 chars
            var lines = CombatLayoutUtils.WrapTextSimple(word, 10);
            Assert.True(lines.Count >= 3);
            Assert.All(lines, l => Assert.True(l.Length <= 10));
        }
    }
}
