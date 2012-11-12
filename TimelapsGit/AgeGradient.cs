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
            var age = dateTime.Subtract(_timeSpanContainer.Start);
            var maxAge = _timeSpanContainer.Stop.Subtract(_timeSpanContainer.Start);
            double relativeAge = age.TotalHours / maxAge.TotalHours;

            return new Color { A = 255, R = (byte)(155 - relativeAge * 100), G = (byte)(155 - relativeAge * 100), B = 255 };
        }
    }
}