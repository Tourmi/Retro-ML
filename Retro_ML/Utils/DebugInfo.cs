using System.Diagnostics;

namespace Retro_ML.Utils;
/// <summary>
/// Class used to store and read debug information. Useful for testing plugins.
/// </summary>
public static class DebugInfo
{
    private static bool isDebug = false;
    private static readonly Mutex mutex = new();
    private static readonly Dictionary<string, string> infos = new();

    /// <summary>
    /// Deletes all stored information
    /// </summary>
    [Conditional("DEBUG")]
    public static void ClearInfo()
    {
        _ = mutex.WaitOne();
        infos.Clear();
        mutex.ReleaseMutex();
    }

    /// <summary>
    /// Adds new information to preview.
    /// </summary>
    [Conditional("DEBUG")]
    public static void AddInfo(string key, string value)
    {
        _ = mutex.WaitOne();
        infos[key] = value;
        mutex.ReleaseMutex();
    }

    /// <summary>
    /// Returns all information to preview
    /// </summary>
    public static IEnumerable<KeyValuePair<string, string>> GetInfo()
    {
        if (IsDebug)
        {
            _ = mutex.WaitOne();
            var res = infos.AsEnumerable();
            mutex.ReleaseMutex();
            return res;
        }
        else
        {
            return Enumerable.Empty<KeyValuePair<string, string>>();
        }
    }

    public static string GetFormattedInfo()
    {
        _ = mutex.WaitOne();
        string res = string.Empty;

        var entries = infos.ToList();
        int keyMaxLength = infos.Count == 0 ? 0 : infos.Max((i) => i.Key.Length);

        foreach (var entry in infos)
        {
            res += $"{entry.Key.PadLeft(keyMaxLength, ' ')} = {entry.Value}";
        }

        mutex.ReleaseMutex();
        return res;
    }

    /// <summary>
    /// Returns true if running in debug mode
    /// </summary>
    /// <returns></returns>
    public static bool IsDebug
    {
        get
        {
            isDebug = false;
            Debug.Assert(isDebug = true);
            return isDebug;
        }
    }
}
