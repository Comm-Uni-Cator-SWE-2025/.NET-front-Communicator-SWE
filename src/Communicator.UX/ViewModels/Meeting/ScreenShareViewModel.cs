using System;
using System.Threading.Tasks;
using System.Drawing; // For Bitmap
using System.Drawing.Imaging; // For BitmapData
using System.IO;
using System.Windows.Media.Imaging; // This is the C# 'BufferedImage'
using Communicator.ScreenShare;     // Your new project
using Controller;
using GUI.Services;
using UX.Core;


namespace GUI.ViewModels.Meeting;

/// <summary>
/// Describes the screen share tab, holding the current user for access control or display.
/// </summary>
public class ScreenShareViewModel : ObservableObject, INavigationScope
{
    // --- Private Fields ---
    private readonly AbstractRPC? _rpc;

    // Change the declaration of _currentFrame to be nullable
    private BitmapSource? _currentFrame;

    // Update the property type to match (optional, but recommended for consistency)
    public BitmapSource? CurrentFrame
    {
        get => _currentFrame;
        set => SetProperty(ref _currentFrame, value);
    }

    // --- Constructor ---
    // It ONLY asks for the 'IAbstractRPC' service.
    public ScreenShareViewModel(AbstractRPC? rpc)
    {
        _rpc = rpc;

        // This is the C# translation of your 'initComponents()'
        SubscribeToFrames();
    }

    // --- Core Logic (from Java ScreenNVideoModel.java) ---
    private void SubscribeToFrames()
    // Change this line to allow for null return value from ConvertBitmapToBitmapSource
    {
        // This replaces your 'onImageReceived' Consumer.
        Action<UIImage> onImageReceived = (uiImage) => {
            // Convert the System.Drawing.Bitmap from your UIImage
            // into a System.Windows.Media.Imaging.BitmapSource for WPF.
            BitmapSource? wpfFrame = ConvertBitmapToBitmapSource(uiImage.Image);

            // And update the assignment to CurrentFrame to handle possible null
            App.Current.Dispatcher.Invoke(() => {
                if (wpfFrame != null)
                {
                    CurrentFrame = wpfFrame;
                }
            });
        };

        // Subscribe to the network
        //_rpc.Subscribe(Utils.UPDATE_UI, (args) => {
        //    try
        //    {
        //        // 1. Deserialize (from your RImage.cs)
        //        RImage rImage = RImage.Deserialize(args);
        //        if (rImage == null || rImage.Image == null)
        //        {
        //            return new byte[] { 0 };
        //        }

        //        // 2. Transform (Java's BufferedImage -> C#'s Bitmap)
        //        using Bitmap bitmap = CreateBitmapFromPixelArray(rImage.Image);

        //        // 3. Wrap (from your UIImage.cs)
        //        UIImage uiImage = new UIImage(bitmap, rImage.Ip, 1);

        //        // 4. Call the local handler to update the UI
        //        onImageReceived?.Invoke(uiImage);

        //        // 5. Send Acknowledgment
        //        byte[] res = { uiImage.IsSuccess };
        //        return res;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"[ScreenShareVM] Error processing frame: {ex.Message}");
        //        return new byte[] { 0 }; // Failure
        //    }
        //});
    }

    // --- Helper Methods to convert pixel data ---

    private static Bitmap CreateBitmapFromPixelArray(int[][] image)
    {
        int height = image.Length;
        int width = image[0].Length;

        // Java's BufferedImage -> C#'s Bitmap
        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

        // Fast way to set pixels
        var rect = new Rectangle(0, 0, width, height);
        BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
        IntPtr ptr = bmpData.Scan0;

        for (int i = 0; i < height; i++)
        {
            // Copy one row of pixel data at a time
            System.Runtime.InteropServices.Marshal.Copy(image[i], 0, ptr + (i * bmpData.Stride), width);
        }
        bitmap.UnlockBits(bmpData);
        return bitmap;
    }

    // --- Helper method to convert for WPF ---
    private static BitmapImage? ConvertBitmapToBitmapSource(System.Drawing.Bitmap bitmap)
    {
        if (bitmap == null)
        {
            return null;
        }

        // This is the standard, efficient way to convert
        // System.Drawing.Bitmap (which your UIImage has)
        // to System.Windows.Media.Imaging.BitmapSource (which WPF needs)
        using var memory = new MemoryStream();
        bitmap.Save(memory, ImageFormat.Bmp);
        memory.Position = 0;

        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = memory;
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.EndInit();
        bitmapImage.Freeze(); // Makes it thread-safe

        return bitmapImage;
    }

    // --- INavigationScope Implementation (as you provided) ---
    // Your Java logic has no back/forward, so we return false.

    public bool CanNavigateBack => false;
    public bool CanNavigateForward => false;
    public void NavigateBack() { }
    public void NavigateForward() { }
    public event EventHandler? NavigationStateChanged;
}
