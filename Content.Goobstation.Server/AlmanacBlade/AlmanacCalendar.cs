namespace Content.Goobstation.Server.AlmanacBlade;

public static class AlmanacCalendar
{
    public const double SynodicMonth = 29.530588853;

    private static readonly DateTime KnownNewMoon = new(2000, 1, 6, 18, 14, 0, DateTimeKind.Utc);

    private static readonly DateOnly[] Eclipses =
    {
        new(2026, 2, 17), new(2026, 3, 3), new(2026, 8, 12), new(2026, 8, 28),
        new(2027, 2, 6), new(2027, 2, 20), new(2027, 7, 18), new(2027, 8, 2), new(2027, 8, 17),
        new(2028, 1, 12), new(2028, 1, 26), new(2028, 7, 6), new(2028, 7, 22), new(2028, 12, 31),
        new(2029, 1, 14), new(2029, 6, 12), new(2029, 6, 26), new(2029, 7, 11), new(2029, 12, 5), new(2029, 12, 20),
        new(2030, 6, 1), new(2030, 6, 15), new(2030, 11, 25), new(2030, 12, 9),
    };

    public static double MoonAge(DateTime utc)
    {
        var days = (utc - KnownNewMoon).TotalDays % SynodicMonth;
        return days < 0 ? days + SynodicMonth : days;
    }

    public static double MoonIllumination(DateTime utc)
    {
        return (1 - Math.Cos(2 * Math.PI * MoonAge(utc) / SynodicMonth)) / 2;
    }

    public static bool IsFullMoon(DateTime utc)
    {
        return Math.Abs(MoonAge(utc) - SynodicMonth / 2) <= 1;
    }

    public static bool IsEclipse(DateOnly date)
    {
        return Array.IndexOf(Eclipses, date) >= 0;
    }

    public static bool IsLeapDay(DateOnly date)
    {
        return date.Month == 2 && date.Day == 29;
    }

    public static bool IsChristmas(DateOnly date)
    {
        return date.Month == 12 && date.Day == 25;
    }

    public static bool IsWednesdayBeforeChristmas(DateOnly date)
    {
        return date.DayOfWeek == DayOfWeek.Wednesday && date.Month == 12 && date.Day is >= 18 and <= 24;
    }

    public static bool IsMondayBeforeGarfield(DateOnly date)
    {
        return date.DayOfWeek == DayOfWeek.Monday;
    }

    public static bool IsAprilFoolsOver(DateTime local)
    {
        return local.Month == 4 && local.Day == 1 && local.Hour >= 12;
    }

    public static DateOnly Easter(int year)
    {
        var a = year % 19;
        var b = year / 100;
        var c = year % 100;
        var d = b / 4;
        var e = b % 4;
        var f = (b + 8) / 25;
        var g = (b - f + 1) / 3;
        var h = (19 * a + b - d - g + 15) % 30;
        var i = c / 4;
        var k = c % 4;
        var l = (32 + 2 * e + 2 * i - h - k) % 7;
        var m = (a + 11 * h + 22 * l) / 451;
        var month = (h + l - 7 * m + 114) / 31;
        var day = (h + l - 7 * m + 114) % 31 + 1;
        return new DateOnly(year, month, day);
    }

    public static bool IsSaturdayAfterEaster(DateOnly date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday && date > Easter(date.Year);
    }
}
