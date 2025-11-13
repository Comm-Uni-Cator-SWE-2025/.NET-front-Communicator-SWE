// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Drawing;

namespace Communicator.ScreenShare;
public sealed class UIImage : IEquatable<UIImage>
{
    /// <summary>
    /// Gets the image.
    /// (Java's BufferedImage is System.Drawing.Bitmap in .NET)
    /// </summary>
    public Bitmap Image { get; init; }

    /// <summary>
    /// Gets the IP address associated with the image.
    /// </summary>
    public string Ip { get; init; }

    /// <summary>
    /// Gets a byte indicating success (1 for true, 0 for false).
    /// </summary>
    public byte IsSuccess { get; private set; }

    public UIImage(Bitmap image, string ip, byte isSuccess)
    {
        Image = image;
        Ip = ip;
        IsSuccess = isSuccess;
    }

    /// <summary>
    /// Sets the success status using a boolean value.
    /// </summary>
    /// <param name="val">True for success, false otherwise.</param>
    public void SetIsSuccess(bool val)
    {
        IsSuccess = (byte)(val ? 1 : 0);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current instance.
    /// </summary>
    public override bool Equals(object obj)
    {
        return Equals(obj as UIImage);
    }

    /// <summary>
    /// Determines whether the specified UIImage is equal to the current instance.
    /// </summary>
    public bool Equals(UIImage other)
    {
        if (other is null)
        {
            return false;
        }
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        // This replicates the behavior of Java's Objects.equals,
        // which performs reference equality on 'image'.
        return Equals(Image, other.Image) &&
               Ip == other.Ip &&
               IsSuccess == other.IsSuccess;
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        // C#'s modern equivalent of Java's Objects.hash()
        return HashCode.Combine(Image, Ip, IsSuccess);
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string representation.</returns>
    public override string ToString()
    {
        // Replicating the Java record-style toString() format
        return $"UIImage[Image={Image}, Ip={Ip}, IsSuccess={IsSuccess}]";
    }
}
