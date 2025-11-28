/*
 * -----------------------------------------------------------------------------
 *  File: NetworkMessageType.cs
 *  Owner: Sriram Nangunoori
 *  Roll Number : 112201019
 *  Module : Canvas
 *
 * -----------------------------------------------------------------------------
 */
namespace Communicator.Canvas;

/// <summary>
/// Defines the type of network message being sent.
/// </summary>
public enum NetworkMessageType
{
    /// <summary>
    /// A standard action (Create, Modify, Delete, Resurrect).
    /// </summary>
    NORMAL = 0,

    /// <summary>
    /// A message indicating a remote Undo operation.
    /// </summary>
    UNDO = 1,

    /// <summary>
    /// A message indicating a remote Redo operation.
    /// </summary>
    REDO = 2,

    /// <summary>
    /// A message indicating a full state restore (reset).
    /// </summary>
    RESTORE = 3,

    /// <summary>
    /// A message requesting the current shape dictionary.
    /// </summary>
    REQUEST_SHAPES = 4
}
