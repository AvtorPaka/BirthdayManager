namespace BirthdayNotificationsBot.Bll.Utils;

public static class DateOnlyExtensions
{
    public static int CountDaysFromBegining(this DateOnly date1)
    {
        HashSet<int> shortMonths = new HashSet<int>(new int[] {2, 4, 6, 9, 11});
        HashSet<int> longMonths = new HashSet<int>(new int[] {1, 3, 5, 7, 8, 10, 12});

        int countDaysDate1 = 0;
        for (int i = 1; i < date1.Month + 1; ++i)
        {
            if (i == 2)
            {
                countDaysDate1 += 28;
                continue;
            }
            if (shortMonths.Contains(i)) {countDaysDate1 += 30;}
            else if (longMonths.Contains(i)) {countDaysDate1 += 31;}
        }
        countDaysDate1 += date1.Day;

        return countDaysDate1;
    }

    public static int DifferenceInDays(this DateOnly date1, DateOnly date2)
    {   
        int diff = date1.CountDaysFromBegining() - date2.CountDaysFromBegining();
        return diff >= 0 ? diff : 365 + diff;
    }

    public static string FormatForString(this DateOnly dateOnly)
    {
        return $"{dateOnly.Day}.{(dateOnly.Month < 10 ? ('0' + dateOnly.Month.ToString()) : dateOnly.Month.ToString())}.{dateOnly.Year}";
    }
}
