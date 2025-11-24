/*
 * -----------------------------------------------------------------------------
 *  File: UllImageTests.cs
 *  Owner: Devansh Manoj Kesan
 *  Roll Number :142201017
 *  Module : ScreenShare
 *
 * -----------------------------------------------------------------------------
 */

/*
 * Test cases for the UIImage wrapper.
 * I documented the basic behaviors (constructor, setters, equality)
 * so that anyone reading the file understands why the assertions exist.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using Communicator.ScreenShare;

namespace Communicator.ScreenShare.Tests;

public sealed class UIImageTests : IDisposable
{
    private readonly List<Bitmap> _createdBitmaps = new();

    private Bitmap CreateBitmap()
    {
        var bitmap = new Bitmap(2, 2);
        _createdBitmaps.Add(bitmap);
        return bitmap;
    }

    [Fact]
    public void ConstructorSetIsSuccessAndToStringCoverCoreBehavior()
    {
        // simple smoke test: make sure constructor works and toString shows the info we expect
        var bitmap = CreateBitmap();
        var image = new UIImage(bitmap, "192.168.0.1", 0);

        Assert.Same(bitmap, image.Image);
        Assert.Equal("192.168.0.1", image.Ip);
        Assert.Equal(0, image.IsSuccess);

        image.SetIsSuccess(true);
        Assert.Equal(1, image.IsSuccess);

        image.SetIsSuccess(false);
        Assert.Contains("192.168.0.1", image.ToString());
        Assert.Contains("0", image.ToString());
    }

    [Fact]
    public void EqualsAndGetHashCodeHandleSameAndDifferentValues()
    {
        // compares same/different objects so equals + hash behave nicely
        var bitmap = CreateBitmap();
        var first = new UIImage(bitmap, "same", 1);
        var secondSame = new UIImage(bitmap, "same", 1);
        var differentBitmap = CreateBitmap();
        var thirdDifferent = new UIImage(differentBitmap, "other", 0);

        Assert.True(first.Equals(first));
        Assert.True(first.Equals(secondSame));
        Assert.False(first.Equals(thirdDifferent));
        Assert.False(first.Equals("not a UIImage"));
        Assert.False(first.Equals(null!));

        Assert.Equal(first.GetHashCode(), secondSame.GetHashCode());
        Assert.NotEqual(first.GetHashCode(), thirdDifferent.GetHashCode());
    }

    [Fact]
    public void IsSuccessCanBeToggledMultipleTimes()
    {
        // quick flip test to show SetIsSuccess can toggle back and forth safely
        var bitmap = CreateBitmap();
        var image = new UIImage(bitmap, "toggle", 1);

        image.SetIsSuccess(false);
        Assert.Equal(0, image.IsSuccess);

        image.SetIsSuccess(true);
        Assert.Equal(1, image.IsSuccess);
    }

    public void Dispose()
    {
        foreach (var bitmap in _createdBitmaps)
        {
            bitmap.Dispose();
        }
        _createdBitmaps.Clear();
    }
}


