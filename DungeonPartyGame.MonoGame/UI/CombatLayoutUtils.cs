using System;
using System.Collections.Generic;

namespace DungeonPartyGame.MonoGame.UI
{
    public static class CombatLayoutUtils
    {
        public static int ComputeHealthWidth(int barWidth, int current, int max)
        {
            if (max <= 0) return 0;
            float pct = (float)current / max;
            return Math.Max(0, (int)(barWidth * pct));
        }

        public static List<string> WrapTextSimple(string text, int maxChars)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(text)) return result;
            var words = text.Split(' ');
            var current = "";
            foreach (var w in words)
            {
                if (current.Length + w.Length + 1 <= maxChars)
                {
                    current = string.IsNullOrEmpty(current) ? w : current + " " + w;
                }
                else
                {
                    if (!string.IsNullOrEmpty(current)) result.Add(current);
                    if (w.Length <= maxChars)
                        current = w;
                    else
                    {
                        // Break long word
                        for (int i = 0; i < w.Length; i += maxChars)
                        {
                            int len = Math.Min(maxChars, w.Length - i);
                            result.Add(w.Substring(i, len));
                        }
                        current = "";
                    }
                }
            }
            if (!string.IsNullOrEmpty(current)) result.Add(current);
            return result;
        }

        public static int ComputeThumbHeight(int containerHeight, int totalLines, int visibleLines)
        {
            if (totalLines <= 0) return containerHeight;
            float frac = (float)visibleLines / Math.Max(1, totalLines);
            return Math.Max(12, (int)(containerHeight * frac));
        }

        public static int ComputeThumbY(int containerTop, int containerHeight, int totalLines, int visibleLines, int scrollFromBottom)
        {
            int maxScroll = Math.Max(0, totalLines - visibleLines);
            if (maxScroll <= 0) return containerTop;
            int effective = Math.Min(maxScroll, scrollFromBottom);
            int space = containerHeight - ComputeThumbHeight(containerHeight, totalLines, visibleLines) - 16;
            float ratio = (float)effective / maxScroll;
            return containerTop + 8 + (int)(space * ratio);
        }
    }
}
