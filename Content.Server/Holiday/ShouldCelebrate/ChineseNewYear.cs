// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using System.Globalization;
using Content.Server.Holiday.Interfaces;

namespace Content.Server.Holiday.ShouldCelebrate
{
    public sealed partial class ChineseNewYear : IHolidayShouldCelebrate
    {
        public bool ShouldCelebrate(DateTime date, HolidayPrototype holiday)
        {
            var chinese = new ChineseLunisolarCalendar();

            var chineseNewYear = chinese.ToDateTime(date.Year, 1, 1, 0, 0, 0, 0);

            return date.Day == chineseNewYear.Day && date.Month == chineseNewYear.Month;
        }
    }
}
