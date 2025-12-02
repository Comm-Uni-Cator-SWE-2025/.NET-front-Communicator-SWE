/*
 * -----------------------------------------------------------------------------
 *  File: EnvLoader.cs
 *  Owner: Geetheswar V
 *  Module : Controller
 *  Description: Loads environment variables from a .env file at solution root.
 * -----------------------------------------------------------------------------
 */
using System;
using System.IO;

namespace Communicator.Controller;

/// <summary>
/// Utility class to load environment variables from a .env file.
/// The file is searched upward from the current directory until it is found.
/// </summary>
public static class EnvLoader
{
    private static bool s_loaded;
    private static readonly object s_lock = new();

    /// <summary>
    /// Loads environment variables from the first .env file found
    /// by searching from <paramref name="startDirectory"/> upward.
    /// If no .env is found, does nothing (no error).
    /// </summary>
    /// <param name="startDirectory">Directory to start searching from. Defaults to current directory.</param>
    public static void Load(string? startDirectory = null)
    {
        lock (s_lock)
        {
            if (s_loaded)
            {
                return;
            }

            string? envFilePath = FindEnvFile(startDirectory ?? Directory.GetCurrentDirectory());
            if (envFilePath != null)
            {
                LoadFromFile(envFilePath);
            }

            s_loaded = true;
        }
    }

    /// <summary>
    /// Searches upward from <paramref name="startDir"/> to find .env file.
    /// </summary>
    private static string? FindEnvFile(string startDir)
    {
        string? dir = startDir;
        while (!string.IsNullOrEmpty(dir))
        {
            string candidate = Path.Combine(dir, ".env");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            dir = Directory.GetParent(dir)?.FullName;
        }

        return null;
    }

    /// <summary>
    /// Parses the .env file and sets environment variables.
    /// </summary>
    private static void LoadFromFile(string filePath)
    {
        foreach (string line in File.ReadAllLines(filePath))
        {
            string trimmed = line.Trim();
            // Skip empty lines and comments
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
            {
                continue;
            }

            int idx = trimmed.IndexOf('=', StringComparison.Ordinal);
            if (idx <= 0)
            {
                continue;
            }

            string key = trimmed.Substring(0, idx).Trim();
            string value = trimmed.Substring(idx + 1).Trim();

            // Remove surrounding quotes if present
            if ((value.StartsWith('"') && value.EndsWith('"')) ||
                (value.StartsWith('\'') && value.EndsWith('\'')))
            {
                value = value.Substring(1, value.Length - 2);
            }

            // Only set if not already present (don't override existing)
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}
