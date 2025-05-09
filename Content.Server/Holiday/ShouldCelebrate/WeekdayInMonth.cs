// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Globalization;
using JetBrains.Annotations;

namespace Content.Server.Holiday.ShouldCelebrate
{
    /// <summary>
    ///     For a holiday that happens the first instance of a weekday on a month.
    /// </summary>
    [UsedImplicitly]
    public sealed partial class WeekdayInMonth : DefaultHolidayShouldCelebrate
    {
        [DataField("weekday")] private DayOfWeek _weekday = DayOfWeek.Monday;

        [DataField("occurrence")] private uint _occurrence = 1;

        public override bool ShouldCelebrate(DateTime date, HolidayPrototype holiday)
        {
            // Not the needed month.
            if (date.Month != (int) holiday.BeginMonth)
                return false;

            // Occurrence NEEDS to be between 1 and 4.
            _occurrence = Math.Max(1, Math.Min(_occurrence, 4));

            var calendar = new GregorianCalendar();

            var d = new DateTime(date.Year, date.Month, 1, calendar);
            for (var i = 1; i <= 7; i++)
            {
                if (d.DayOfWeek != _weekday)
                {
                    d = d.AddDays(1);
                    continue;
                }

                d = d.AddDays(7 * (_occurrence-1));

                return date.Day == d.Day;
            }

            return false;
        }
    }
}