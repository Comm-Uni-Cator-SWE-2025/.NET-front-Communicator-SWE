/*
 * -----------------------------------------------------------------------------
 *  File: Icon.cs
 *  Owner: Geetheswar V
 *  Roll Number : 142201025
 *  Module : UX
 *
 * -----------------------------------------------------------------------------
 */
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Communicator.Icons;

/// <summary>
/// Custom icon control using Tabler Icons font
/// Usage: 
///   Outline: &lt;icons:Icon IconName="arrow-right" IconSize="24" /&gt;
///   Filled:  &lt;icons:Icon IconName="arrow-right-filled" IconSize="24" /&gt;
/// </summary>
public class Icon : Control
{
    static Icon()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Icon), new FrameworkPropertyMetadata(typeof(Icon)));

        // Set default font family
        FontFamilyProperty.OverrideMetadata(typeof(Icon),
            new FrameworkPropertyMetadata(new FontFamily("/Communicator.Icons;component/Assets/Fonts/#tabler-icons")));
    }

    /// <summary>
    /// Icon name (e.g., "arrow-right" for outline, "arrow-right-filled" for filled)
    /// </summary>
    public static readonly DependencyProperty IconNameProperty =
        DependencyProperty.Register(
            nameof(IconName),
            typeof(string),
            typeof(Icon),
            new PropertyMetadata(string.Empty, OnIconNameChanged));

    /// <summary>
    /// Icon size in pixels (default: 24)
    /// </summary>
    public static readonly DependencyProperty IconSizeProperty =
        DependencyProperty.Register(
            nameof(IconSize),
            typeof(double),
            typeof(Icon),
            new PropertyMetadata(24.0));

    /// <summary>
    /// The Unicode character code for the icon (internal, computed from Name)
    /// </summary>
    public static readonly DependencyProperty GlyphProperty =
        DependencyProperty.Register(
            nameof(Glyph),
            typeof(string),
            typeof(Icon),
            new PropertyMetadata(string.Empty));

    public string IconName
    {
        get => (string)GetValue(IconNameProperty);
        set => SetValue(IconNameProperty, value);
    }

    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    public string Glyph
    {
        get => (string)GetValue(GlyphProperty);
        private set => SetValue(GlyphProperty, value);
    }

    private static void OnIconNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Icon icon)
        {
            icon.UpdateGlyph();
        }
    }

    private void UpdateGlyph()
    {
        if (string.IsNullOrWhiteSpace(IconName))
        {
            Glyph = string.Empty;
            return;
        }

        // Get the Unicode value for the icon name
        string? unicode = IconCodes.GetUnicode(IconName);
        Glyph = unicode ?? string.Empty;
    }
}

