using System;
using System.Windows.Media;

namespace TimelapsGit
{
    public class AgeGradient
    {
        private readonly ITimeSpanContainer _timeSpanContainer;

        public AgeGradient(ITimeSpanContainer timeSpanContainer)
        {
            _timeSpanContainer = timeSpanContainer;
        }

        public Color Lookup(DateTime dateTime)
        {
            double relativeAge = _timeSpanContainer.RelativeAgeOf(dateTime);
            return new Color { A = 255, R = (byte)(relativeAge * 255), G = (byte)(relativeAge * 255), B = 255 };
        }
    }
}