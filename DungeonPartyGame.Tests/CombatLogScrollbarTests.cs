using DungeonPartyGame.MonoGame.UI;
using Xunit;

namespace DungeonPartyGame.Tests
{
    public class CombatLogScrollbarTests
    {
        [Fact]
        public void ComputeThumbHeight_IsProportional()
        {
            int container = 120;
            int total = 100;
            int visible = 12;

            int h = CombatLayoutUtils.ComputeThumbHeight(container, total, visible);
            Assert.True(h > 10);
            Assert.True(h < container);
        }

        [Fact]
        public void ComputeThumbY_BottomScroll_PlacesThumbNearBottom()
        {
            int top = 10;
            int container = 120;
            int total = 100;
            int visible = 10;
            int scroll = total - visible; // scrolled to far older

            int y = CombatLayoutUtils.ComputeThumbY(top, container, total, visible, scroll);
            // Thumb should be lower than top
            Assert.True(y > top + 5);
        }

        [Fact]
        public void ComputeThumbY_NoScroll_PlacesThumbAtTop()
        {
            int top = 10;
            int container = 120;
            int total = 8;
            int visible = 12;
            int scroll = 0;

            int y = CombatLayoutUtils.ComputeThumbY(top, container, total, visible, scroll);
            Assert.Equal(top, y);
        }
    }
}
