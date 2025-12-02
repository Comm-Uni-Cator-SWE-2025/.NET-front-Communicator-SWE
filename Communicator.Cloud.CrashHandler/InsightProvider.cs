// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communicator.Cloud.CrashHandler;

public class InsightProvider
{
    private HttpClient _client;

    private bool _connectionFlag;

    private string _deploymentModel = "gemini-2.5-flash";

    public InsightProvider()
    {
        try
        {

        }
        catch
        {
            _connectionFlag = false;
        }
    }

    public string GetInsights(string exceptionPrompt)
    {
        return "Error: unable to connect to model...";
    }
}
