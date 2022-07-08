using System.Diagnostics;

namespace Retro_ML.Utils;
/// <summary>
/// Class used to store and read debug information. Useful for testing plugins.
/// </summary>
public static class DebugInfo
{
    private const string DEFAULT_CATEGORY = "None";

    private struct DebugInfoEntry
    {
        public string Name;
        public string Value;
        public string Category;
        public int Priority;
    }

    private static bool isDebug = false;
    private static readonly Mutex mutex = new();
    private static readonly List<DebugInfoEntry> infos = new();

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
    /// Adds new information to preview. A lower <paramref name="priority"/> means that the value will show up first.
    /// </summary>
    [Conditional("DEBUG")]
    public static void AddInfo(string name, string value, string category = DEFAULT_CATEGORY, int priority = 0)
    {
        _ = mutex.WaitOne();
        _ = infos.RemoveAll(e => e.Name == name);
        infos.Add(new DebugInfoEntry() { Name = name, Value = value, Category = category, Priority = priority });
        mutex.ReleaseMutex();
    }

    /// <summary>
    /// Returns the formatted string for all the logged info so far. Filters by <paramref name="category"/> if not null
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    public static string GetFormattedInfo(params string[] categories)
    {
        _ = mutex.WaitOne();
        string res = string.Empty;

        var entries = infos.ToList();
        int keyMaxLength = infos.Count == 0 ? 0 : infos.Max((i) => i.Name.Length);

        foreach (var entry in infos.Where(i => categories.Length == 0 || categories.Contains(i.Category)).OrderBy(i => i.Priority))
        {
            res += $"{entry.Name.PadLeft(keyMaxLength, ' ')} = {entry.Value}\n";
        }

        mutex.ReleaseMutex();
        return res;
    }

    /// <summary>
    /// Returns the current categories for all the stored entries
    /// </summary>
    public static string[] GetCategories()
    {
        HashSet<string> categories = new() { DEFAULT_CATEGORY };

        categories.UnionWith(infos.Select(i => i.Category));

        return categories.ToArray();
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
