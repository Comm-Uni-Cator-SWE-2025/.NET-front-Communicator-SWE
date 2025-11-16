// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanvasApp.DataModel;

/// <summary>
/// Defines the type of action performed on the canvas for state management.
/// </summary>
public enum CanvasActionType
{
    /// <summary>
    /// Represents the initial empty state.
    /// </summary>
    Initial,

    /// <summary>
    /// Represents the creation of a new shape.
    /// </summary>
    Create,
    /// <summary>
    /// Represents the deletion of an existing shape.
    /// </summary>
    Delete,

    /// <summary>
    /// Represents the modification of a shape's properties (color, thickness, etc.).
    /// </summary>
    Modify
}
