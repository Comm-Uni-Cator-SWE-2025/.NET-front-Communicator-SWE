# UX.Core - Shared Infrastructure Guide

## Overview

UX.Core is a shared class library that provides common UI/UX infrastructure for all feature modules in the Meet application. It ensures consistency across the application and eliminates code duplication.

## What's Included

### 1. MVVM Base Classes
- **ObservableObject**: Base class for all ViewModels implementing INotifyPropertyChanged
- **RelayCommand**: Command implementation for button bindings and actions
- **INavigationScope**: Interface for ViewModels that manage their own navigation

### 2. Services
- **IToastService / ToastService**: Show success/error/warning/info notifications
- **IThemeService / ThemeService**: Dynamic theme switching (Light/Dark)
- **INavigationService**: Navigation abstraction (interface only)

### 3. Models
- **ToastMessage**: Model for toast notifications
- **AppTheme**: Theme enumeration (Light, Dark)

### 4. Behaviors
- **PasswordBoxBehavior**: Attached behavior for binding PasswordBox.Password property in MVVM

### 5. UI Resources
- **Themes**: LightTheme.xaml, DarkTheme.xaml (color palettes)
- **Styles**: ControlStyles.xaml (Button, TextBox, PasswordBox, Toggle styles)
- **Converters**: BooleanToVisibilityConverter for XAML bindings

## Getting Started

### 1. Add Reference to Your Project

```xml
<!-- YourModule.csproj -->
<ItemGroup>
  <ProjectReference Include="..\UX.Core\UX.Core.csproj" />
</ItemGroup>
```

Or via command line:
```powershell
dotnet add reference ..\UX.Core\UX.Core.csproj
```

### 2. Import Namespaces

```csharp
using UX.Core;                  // ObservableObject, RelayCommand
using UX.Core.Models;           // ToastMessage, AppTheme
using UX.Core.Services;         // IToastService, IThemeService
using UX.Core.Converters;       // Converters (if needed in code-behind)
```

## Creating ViewModels

### Basic ViewModel

```csharp
using UX.Core;
using UX.Core.Services;
using System.Windows.Input;

namespace YourModule.ViewModels
{
    public class WhiteboardViewModel : ObservableObject
    {
        private readonly IToastService _toastService;
        private readonly IThemeService _themeService;
        private string _title = string.Empty;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public ICommand SaveCommand { get; }

        // Constructor injection - GUI will provide services
        public WhiteboardViewModel(IToastService toastService, IThemeService themeService)
        {
            _toastService = toastService;
            _themeService = themeService;
            
            SaveCommand = new RelayCommand(Save, CanSave);
        }

        private void Save(object? parameter)
        {
            // Your save logic here
            _toastService.ShowSuccess("Whiteboard saved successfully!");
        }

        private bool CanSave(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(Title);
        }
    }
}
```

### Using Property Change Notifications

```csharp
private string _name = string.Empty;
private int _count;
private bool _isEnabled;

public string Name
{
    get => _name;
    set => SetProperty(ref _name, value);  // Automatically raises PropertyChanged
}

public int Count
{
    get => _count;
    set
    {
        if (SetProperty(ref _count, value))
        {
            // SetProperty returns true if value changed
            // Do additional work here if needed
            OnPropertyChanged(nameof(DisplayText));  // Notify related properties
        }
    }
}

public bool IsEnabled
{
    get => _isEnabled;
    set => SetProperty(ref _isEnabled, value);
}

public string DisplayText => $"{Name} ({Count})";  // Computed property
```

## Using Services

### Toast Notifications

```csharp
// Success notification
_toastService.ShowSuccess("Operation completed successfully!");

// Error notification
_toastService.ShowError("Failed to save file");

// Warning notification
_toastService.ShowWarning("This action cannot be undone");

// Info notification
_toastService.ShowInfo("New version available", duration: 5000);
```

### Theme Service

```csharp
// Get current theme
var currentTheme = _themeService.CurrentTheme;

// Set theme
_themeService.SetTheme(AppTheme.Dark);

// Listen to theme changes
_themeService.ThemeChanged += OnThemeChanged;

private void OnThemeChanged(object? sender, AppTheme newTheme)
{
    // React to theme change if needed
}
```

## Using UI Resources in XAML

### Import Theme and Styles

```xaml
<UserControl x:Class="YourModule.Views.WhiteboardView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <UserControl.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!-- Import UX.Core themes -->
                <ResourceDictionary Source="/UX.Core;component/Themes/LightTheme.xaml"/>
                
                <!-- Import UX.Core styles -->
                <ResourceDictionary Source="/UX.Core;component/Styles/ControlStyles.xaml"/>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </UserControl.Resources>
    
    <!-- Your content here -->
</UserControl>
```

### Using Theme Colors

```xaml
<!-- Use dynamic resources for theme-aware colors -->
<Border Background="{DynamicResource CardBackgroundBrush}"
        BorderBrush="{DynamicResource BorderBrush}"
        BorderThickness="1"
        CornerRadius="8">
    
    <TextBlock Text="Hello"
               Foreground="{DynamicResource TextPrimaryBrush}"/>
</Border>
```

### Available Theme Resources

**Colors:**
- PrimaryBrush, PrimaryHoverBrush, PrimaryPressedBrush
- SuccessBrush, ErrorBrush, WarningBrush
- AppBackgroundBrush, SurfaceBrush, CardBackgroundBrush
- TextPrimaryBrush, TextSecondaryBrush, TextTertiaryBrush
- BorderBrush, BorderHoverBrush, BorderFocusBrush
- And more...

**Effects:**
- CardShadow
- ElevatedShadow

### Using Button Styles

```xaml
<!-- Primary button -->
<Button Style="{StaticResource PrimaryButtonStyle}"
        Content="Save"
        Command="{Binding SaveCommand}"/>

<!-- Secondary button -->
<Button Style="{StaticResource SecondaryButtonStyle}"
        Content="Cancel"
        Command="{Binding CancelCommand}"/>
```

### Using TextBox and PasswordBox

```xaml
<!-- Automatically styled (default style applied) -->
<TextBox Text="{Binding Username}"/>

<PasswordBox x:Name="PasswordBox"/>

<!-- Or explicitly use the style -->
<TextBox Style="{StaticResource TextBoxStyle}"
         Text="{Binding Email}"/>
```

### Using Toggle Switch

```xaml
<CheckBox Style="{StaticResource ToggleSwitchStyle}"
          IsChecked="{Binding IsEnabled}"/>
```

### Using Converters

```xaml
<UserControl.Resources>
    <converters:BooleanToVisibilityConverter x:Key="BoolToVis"/>
</UserControl.Resources>

<Button Visibility="{Binding IsLoggedIn, Converter={StaticResource BoolToVis}}"
        Content="Logout"/>

<!-- Invert visibility by passing parameter -->
<TextBlock Visibility="{Binding IsLoggedIn, 
                                Converter={StaticResource BoolToVis},
                                ConverterParameter=Invert}"
           Text="Please log in"/>
```

## Dependency Injection Pattern

### How GUI Provides Services

GUI creates service instances and injects them into your ViewModels:

```csharp
// In GUI/App.xaml.cs
public static IToastService ToastService { get; private set; } = new ToastService();
public static IThemeService ThemeService { get; private set; } = new ThemeService();

// GUI creates your ViewModel with services
var whiteboardVM = new WhiteboardViewModel(
    App.ToastService,
    App.ThemeService
);
```

### Your ViewModel Constructor

```csharp
public WhiteboardViewModel(
    IToastService toastService,
    IThemeService themeService,
    UserProfile user)
{
    _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));
    _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
    _user = user ?? throw new ArgumentNullException(nameof(user));
    
    InitializeCommands();
}
```

## Testing Your ViewModels

### Creating Mock Services

```csharp
public class MockToastService : IToastService
{
    public List<string> Messages { get; } = new();
    public event Action<ToastMessage>? ToastRequested;

    public void ShowSuccess(string message, int duration = 3000)
    {
        Messages.Add($"SUCCESS: {message}");
    }

    public void ShowError(string message, int duration = 3000)
    {
        Messages.Add($"ERROR: {message}");
    }

    // Implement other methods...
}
```

### Unit Test Example

```csharp
[Fact]
public void SaveCommand_ShowsSuccessToast_WhenSaveSucceeds()
{
    // Arrange
    var mockToast = new MockToastService();
    var mockTheme = new MockThemeService();
    var viewModel = new WhiteboardViewModel(mockToast, mockTheme);
    
    viewModel.Title = "Test Whiteboard";
    
    // Act
    viewModel.SaveCommand.Execute(null);
    
    // Assert
    Assert.Contains("SUCCESS", mockToast.Messages[0]);
}
```

## Best Practices

### 1. Always Use Dependency Injection
```csharp
// GOOD - Services injected via constructor
public MyViewModel(IToastService toastService)
{
    _toastService = toastService;
}

// BAD - Don't access App.ToastService directly in your module
public MyViewModel()
{
    App.ToastService.ShowSuccess("Bad practice!");  // Don't do this!
}
```

### 2. Use DynamicResource for Themes
```xaml
<!-- GOOD - Responds to theme changes -->
<Border Background="{DynamicResource CardBackgroundBrush}"/>

<!-- BAD - Won't update when theme changes -->
<Border Background="{StaticResource CardBackgroundBrush}"/>
```

### 3. Implement IDisposable if Needed
```csharp
public class WhiteboardViewModel : ObservableObject, IDisposable
{
    public WhiteboardViewModel(IToastService toastService)
    {
        _toastService = toastService;
        _toastService.ToastRequested += OnToastRequested;
    }

    public void Dispose()
    {
        _toastService.ToastRequested -= OnToastRequested;
    }
}
```

### 4. Null Checks in Constructors
```csharp
public WhiteboardViewModel(IToastService toastService, UserProfile user)
{
    _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));
    _user = user ?? throw new ArgumentNullException(nameof(user));
}
```

## Common Patterns

### Command with Parameter
```csharp
public ICommand DeleteCommand { get; }

public MyViewModel(IToastService toastService)
{
    DeleteCommand = new RelayCommand(Delete, CanDelete);
}

private void Delete(object? parameter)
{
    if (parameter is string itemId)
    {
        // Delete item
        _toastService.ShowSuccess($"Deleted item {itemId}");
    }
}

private bool CanDelete(object? parameter)
{
    return parameter is string id && !string.IsNullOrEmpty(id);
}
```

### Async Operations
```csharp
public ICommand SaveCommand { get; }

public MyViewModel(IToastService toastService)
{
    SaveCommand = new RelayCommand(async _ => await SaveAsync());
}

private async Task SaveAsync()
{
    try
    {
        await Task.Run(() => {
            // Long running operation
        });
        
        _toastService.ShowSuccess("Saved successfully!");
    }
    catch (Exception ex)
    {
        _toastService.ShowError($"Save failed: {ex.Message}");
    }
}
```
