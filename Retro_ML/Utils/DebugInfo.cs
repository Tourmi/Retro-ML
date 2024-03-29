﻿using System.Diagnostics;

namespace Retro_ML.Utils;
/// <summary>
/// <br/> Thread-safe class used to store and read debug information. Useful for testing plugins.
/// <br/> Void functions are safe to use, since when not in debug mode, the compiler will remove said calls
/// <br/> Check <see cref="DebugInfo.IsDebug"/> before using non-void functions in this class. 
/// </summary>
public static class DebugInfo
{
    private const string DEFAULT_CATEGORY = "Default";

    private struct DebugInfoEntry
    {
        public string Name;
        public string Value;
        public string Category;
        public int Priority;
    }

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
    /// Returns the formatted string for all the logged info so far. Filters by <paramref name="categories"/> if not empty
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    public static string GetFormattedInfo(params string[] categories)
    {
        _ = mutex.WaitOne();
        string res = string.Empty;

        var entries = infos.OrderBy(i => i.Priority).ThenBy(i => i.Name).ToList();
        int keyMaxLength = entries.Count == 0 ? 0 : entries.Max((i) => i.Name.Length);

        foreach (var entry in entries.Where(i => categories.Length == 0 || categories.Contains(i.Category)))
        {
            res += GetFormattedEntry(entry, keyMaxLength);
        }

        mutex.ReleaseMutex();
        return res;
    }

    private static string GetFormattedEntry(DebugInfoEntry entry, int leftPad)
    {
        leftPad += 3;
        string left = $"{entry.Name} = ".PadLeft(leftPad, ' ');
        string right = entry.Value.Replace("\n", "\n" + new String(' ', leftPad));
        return left + right + "\n";
    }

    /// <summary>
    /// Returns the current categories for all the stored entries
    /// </summary>
    public static string[] GetCategories()
    {
        _ = mutex.WaitOne();
        HashSet<string> categories = new() { DEFAULT_CATEGORY };

        categories.UnionWith(infos.Select(i => i.Category));

        mutex.ReleaseMutex();

        return categories.OrderBy(c => c.ToLower()).ToArray();
    }

    /// <summary>
    /// Returns true if running in debug mode
    /// </summary>
    /// <returns></returns>
    public static bool IsDebug
    {
        get
        {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            var isDebug = false;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
            Debug.Assert(isDebug = true);
            return isDebug;
        }
    }
}
