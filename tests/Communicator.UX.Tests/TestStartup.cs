/*
 * Ensures environment variables are loaded from the .env file at solution root
 * before any tests run. Uses module initializer (C# 9+).
 */
using System.Runtime.CompilerServices;
using Communicator.Controller;

namespace Communicator.UX.Tests;

internal static class TestStartup
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        EnvLoader.Load();
    }
}
