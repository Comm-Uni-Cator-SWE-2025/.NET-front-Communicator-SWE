/*
 * -----------------------------------------------------------------------------
 *  File: GoogleIcon.cs
 *  Owner: Pramodh Sai
 *  Roll Number : 112201029
 *  Module : UX
 *
 * -----------------------------------------------------------------------------
 */
using System.Windows;
using System.Windows.Controls;

namespace Communicator.Icons;

/// <summary>
/// Google logo with official brand colors
/// Usage: &lt;icons:GoogleIcon Width="20" Height="20" /&gt;
/// </summary>
public class GoogleIcon : Control
{
    static GoogleIcon()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(GoogleIcon),
            new FrameworkPropertyMetadata(typeof(GoogleIcon)));
    }

    /// <summary>
    /// Width and Height are already inherited from FrameworkElement
    /// The template will use these values for sizing
    /// </summary>
}

