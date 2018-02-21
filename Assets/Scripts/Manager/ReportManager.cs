using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class ReportManager
{
    public static event Action ReportCollected = delegate {};

    /// <summary>
    /// Key is stage name.
    /// Value is bitwise or-ed collected report numbers.
    /// </summary>
    private static Dictionary<string, int> collectedReports = new Dictionary<string, int>();

    public static bool IsCollected(string stageName, int reportNumber)
    {
        try
        {
            return (collectedReports[stageName] & (1 << reportNumber)) == 1;
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }

    public static void MarkCollected(string stageName, int reportNumber)
    {
        try
        {
            collectedReports[stageName] |= (1 << reportNumber);
        }
        catch (KeyNotFoundException)
        {
            collectedReports[stageName] = (1 << reportNumber);
        }

        ReportCollected();
    }
}
