using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimelapsGit
{
    public interface ITimeSpanContainer
    {
        DateTime Start { get; }
        DateTime Stop { get; }
    }

    public static class TimeSpanContainer
    {
        public static double RelativeAgeOf(this ITimeSpanContainer timeSpanContainer, DateTime dateTime)
        {
            var age = dateTime.Subtract(timeSpanContainer.Start);
            var maxAge = timeSpanContainer.Stop.Subtract(timeSpanContainer.Start);
            double relativeAge = age.TotalHours / maxAge.TotalHours;
            return relativeAge;
        }
    }
}
