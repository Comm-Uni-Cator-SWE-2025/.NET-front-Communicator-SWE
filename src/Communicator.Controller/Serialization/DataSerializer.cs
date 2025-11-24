// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text;
using System.Text.Json;

namespace Communicator.Controller.Serialization;

/// <summary>
/// Utility class for serializing and deserializing objects to/from byte arrays.
/// Uses JSON format with UTF-8 encoding to match Java's Jackson ObjectMapper implementation.
/// </summary>
public static class DataSerializer
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Serializes an object to a byte array using JSON format with UTF-8 encoding.
    /// </summary>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>A byte array containing the UTF-8 encoded JSON representation of the object.</returns>
    /// <exception cref="JsonException">Thrown when the object cannot be serialized to JSON.</exception>
    public static byte[] Serialize(object obj)
    {
        string json = JsonSerializer.Serialize(obj, s_jsonOptions);
        return Encoding.UTF8.GetBytes(json);
    }

    /// <summary>
    /// Deserializes a byte array to an object of the specified type using JSON format with UTF-8 encoding.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="data">The byte array containing UTF-8 encoded JSON data.</param>
    /// <returns>The deserialized object of type T.</returns>
    /// <exception cref="JsonException">Thrown when the JSON data cannot be deserialized to the specified type.</exception>
    public static T Deserialize<T>(byte[] data)
    {
        string json = Encoding.UTF8.GetString(data);
        T? result = JsonSerializer.Deserialize<T>(json, s_jsonOptions);

        return result ?? throw new JsonException($"Failed to deserialize data to type {typeof(T).Name}. Result was null.");
    }

    /// <summary>
    /// Deserializes a byte array to an object of the specified type using JSON format with UTF-8 encoding.
    /// Non-generic version for compatibility with runtime type resolution.
    /// </summary>
    /// <param name="data">The byte array containing UTF-8 encoded JSON data.</param>
    /// <param name="type">The type to deserialize to.</param>
    /// <returns>The deserialized object.</returns>
    /// <exception cref="JsonException">Thrown when the JSON data cannot be deserialized to the specified type.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the type parameter is null.</exception>
    public static object Deserialize(byte[] data, Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        string json = Encoding.UTF8.GetString(data);
        object? result = JsonSerializer.Deserialize(json, type, s_jsonOptions);

        return result ?? throw new JsonException($"Failed to deserialize data to type {type.Name}. Result was null.");
    }
}
