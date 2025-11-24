# UX.Icons

A custom WPF icon library using **Tabler Icons** (5,800+ MIT-licensed icons).

## Features

✅ **5,800+ Icons** - Comprehensive icon set with outline and filled variants  
✅ **MIT Licensed** - Free for commercial use, redistributable  
✅ **Font-based** - Lightweight, scalable, colorizable  
✅ **Easy to Use** - Simple XAML syntax  
✅ **Windows 10/11** - No external dependencies  
✅ **Interactive Showcase** - Browse all icons with search and click-to-copy

## Installation

### 1. Add Project Reference

In your WPF application's `.csproj` file:

```xml
<ItemGroup>
  <ProjectReference Include="..\UX.Icons\UX.Icons.csproj" />
</ItemGroup>
```

### 2. Add Namespace in XAML

In your Window or UserControl:

```xml
<Window x:Class="YourApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:icons="clr-namespace:UX.Icons;assembly=UX.Icons">
    
    <!-- Your content here -->
    
</Window>
```

## Usage

### Basic Icon

```xml
<!-- Simple outline icon -->
<icons:Icon IconName="message-circle" IconSize="24" />

<!-- Colored icon -->
<icons:Icon IconName="video" IconSize="32" Foreground="Blue" />

<!-- Filled variant (use -filled suffix) -->
<icons:Icon IconName="heart-filled" IconSize="20" Foreground="Red" />

<!-- Large icon -->
<icons:Icon IconName="settings" IconSize="48" Foreground="#333" />
```

### In Buttons

```xml
<Button Padding="10,5" Background="#2196F3" BorderBrush="Transparent">
    <StackPanel Orientation="Horizontal">
        <icons:Icon IconName="phone" IconSize="16" Foreground="White" />
        <TextBlock Text="Call" Margin="8,0,0,0" Foreground="White" />
    </StackPanel>
</Button>

<Button>
    <StackPanel Orientation="Horizontal">
        <icons:Icon IconName="logout" IconSize="18" />
        <TextBlock Text="Sign Out" Margin="5,0,0,0" />
    </StackPanel>
</Button>
```

### In Lists/Menus

```xml
<ListView>
    <ListViewItem>
        <StackPanel Orientation="Horizontal">
            <icons:Icon IconName="home" IconSize="20" Foreground="#666" />
            <TextBlock Text="Home" Margin="10,0,0,0" />
        </StackPanel>
    </ListViewItem>
    <ListViewItem>
        <StackPanel Orientation="Horizontal">
            <icons:Icon IconName="user" IconSize="20" Foreground="#666" />
            <TextBlock Text="Profile" Margin="10,0,0,0" />
        </StackPanel>
    </ListViewItem>
</ListView>
```

### With Data Binding

```xml
<icons:Icon 
    IconName="{Binding CurrentIconName}" 
    IconSize="{Binding IconSize}" 
    Foreground="{Binding IconColor}" />
```

**Example ViewModel:**
```csharp
public class MyViewModel
{
    public string CurrentIconName { get; set; } = "video";
    public double IconSize { get; set; } = 24;
    public Brush IconColor { get; set; } = Brushes.Blue;
}
```

### In Styles and Templates

```xml
<Style x:Key="IconButtonStyle" TargetType="Button">
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="Button">
                <Border Background="{TemplateBinding Background}" 
                        CornerRadius="4" 
                        Padding="10">
                    <icons:Icon IconName="{TemplateBinding Tag}" 
                               IconSize="20" 
                               Foreground="{TemplateBinding Foreground}" />
                </Border>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>

<!-- Usage -->
<Button Style="{StaticResource IconButtonStyle}" 
        Tag="settings" 
        Foreground="White" 
        Background="#2196F3" />
```

## Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `IconName` | `string` | `""` | Icon name (e.g., "arrow-right" or "heart-filled") |
| `IconSize` | `double` | `24` | Icon size in pixels |
| `Foreground` | `Brush` | `Black` | Icon color (inherited from parent if not set) |
| `HorizontalAlignment` | `HorizontalAlignment` | `Left` | Horizontal alignment |
| `VerticalAlignment` | `VerticalAlignment` | `Top` | Vertical alignment |

## Icon Naming Convention

- **Outline icons**: Use the base name (e.g., `"video"`, `"heart"`, `"user"`)
- **Filled icons**: Append `-filled` suffix (e.g., `"video-filled"`, `"heart-filled"`, `"user-filled"`)

## Browsing Available Icons

Run the **Icon Showcase Window** to browse all 5,800+ icons:

```csharp
var showcaseWindow = new UX.Icons.IconShowcaseWindow();
showcaseWindow.ShowDialog();
```

Or run the project directly:
```bash
dotnet run --project UX.Icons\UX.Icons.csproj
```

Features:
- **Click to Copy** - Click any icon to copy its XAML code
- **Search** - Filter icons by name
- **Alphabetical Grouping** - Icons organized A-Z

**Browse All**: Visit [tabler.io/icons](https://tabler.io/icons) or run the showcase window

## Advanced Usage

### Dynamic Icon Loading

```csharp
// In code-behind
var icon = new UX.Icons.Icon
{
    IconName = "video",
    IconSize = 24,
    Foreground = Brushes.Blue
};

myContainer.Children.Add(icon);
```

### Checking Icon Existence

```csharp
if (UX.Icons.IconCodes.Exists("my-icon"))
{
    // Icon exists, safe to use
}

// Get all available icon names
var allIcons = UX.Icons.IconCodes.GetAllIconNames();
```

### Custom Styling

```xml
<icons:Icon IconName="settings" IconSize="32">
    <icons:Icon.RenderTransform>
        <RotateTransform Angle="45" CenterX="16" CenterY="16" />
    </icons:Icon.RenderTransform>
</icons:Icon>
```

## Updating Icons

To update to the latest Tabler Icons version:

1. Download new `tabler-icons.ttf` and `tabler-icons.css` from [Tabler Icons](https://github.com/tabler/tabler-icons)
2. Replace `Assets/Fonts/tabler-icons.ttf`
3. Update CSS path in `GenerateIconCodes.ps1`
4. Run: `.\GenerateIconCodes.ps1`
5. Rebuild project

## Credits

**Icons**: [Tabler Icons](https://github.com/tabler/tabler-icons) by Paweł Kuna  
**License**: MIT License  
**Version**: Tabler Icons v3.35.0  
**Total Icons**: 5,800+ (outline + filled variants)

## License

This module uses Tabler Icons under the MIT License. Free for personal and commercial use.
