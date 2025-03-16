using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using System.Linq;
using System.Collections.Generic;

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class CandleLevelsIndicator : Indicator
    {
        [Parameter("Candle Look Back", DefaultValue = 1, MinValue = 1)]
        public int CandleLookBack { get; set; }

        [Parameter("Custom Levels (comma separated)", DefaultValue = "0,25,50,75,100")]
        public string CustomLevels { get; set; }

        [Parameter("Show Level Text", DefaultValue = true)]
        public bool ShowLevelText { get; set; }

        [Parameter("Line Thickness", DefaultValue = 1, MinValue = 1, MaxValue = 5)]
        public int LineThickness { get; set; }

        [Parameter("Line Style", DefaultValue = LineStyle.DotsVeryRare)]
        public LineStyle LineStyleParam { get; set; }

        // Warna untuk setiap level
        [Parameter("Level 1 Color", DefaultValue = "Red")]
        public Color Level1Color { get; set; }

        [Parameter("Level 2 Color", DefaultValue = "Green")]
        public Color Level2Color { get; set; }

        [Parameter("Level 3 Color", DefaultValue = "Yellow")]
        public Color Level3Color { get; set; }

        [Parameter("Level 4 Color", DefaultValue = "Blue")]
        public Color Level4Color { get; set; }

        [Parameter("Level 5 Color", DefaultValue = "Magenta")]
        public Color Level5Color { get; set; }

        private double[] levels;
        private Color[] colors;
        private List<ChartTrendLine> trendLines = new List<ChartTrendLine>();
        private List<ChartText> textLabels = new List<ChartText>();
        private int lastProcessedIndex = -1;

        protected override void Initialize()
        {
            levels = ParseLevels(CustomLevels);
            colors = new Color[] { Level1Color, Level2Color, Level3Color, Level4Color, Level5Color };
        }

        public override void Calculate(int index)
        {
            if (index < CandleLookBack || index == lastProcessedIndex)
                return;

            lastProcessedIndex = index;
            int targetIndex = index - CandleLookBack;
            var candle = Bars[targetIndex];

            double high = candle.High;
            double low = candle.Low;
            double range = high - low;

            ClearOldObjects();

            for (int i = 0; i < levels.Length; i++)
            {
                double price = low + range * levels[i];
                int percentage = (int)(levels[i] * 100);
                DrawLevel(targetIndex, price, colors[i % colors.Length], i, percentage);
            }
        }

        private void DrawLevel(int index, double price, Color color, int levelIndex, int percentage)
        {
            var startTime = Bars[index].OpenTime;
            var endTime = Bars[index + 1].OpenTime;

            var line = Chart.DrawTrendLine($"Level_{levelIndex}", startTime, price, endTime, price, color, LineThickness, LineStyleParam);
            trendLines.Add(line);

            if (ShowLevelText)
            {
                string text = $"{percentage}%";
                var textLabel = Chart.DrawText($"LevelText_{levelIndex}", text, endTime, price, color);
                textLabels.Add(textLabel);
            }
        }

        private void ClearOldObjects()
        {
            foreach (var line in trendLines)
                Chart.RemoveObject(line.Name);
            trendLines.Clear();

            foreach (var text in textLabels)
                Chart.RemoveObject(text.Name);
            textLabels.Clear();
        }

        private double[] ParseLevels(string input)
        {
            return input.Split(',')
                        .Select(s => double.TryParse(s.Trim(), out double val) ? val / 100.0 : (double?)null)
                        .Where(val => val.HasValue)
                        .Select(val => val.Value)
                        .OrderBy(val => val)
                        .ToArray();
        }
    }
}