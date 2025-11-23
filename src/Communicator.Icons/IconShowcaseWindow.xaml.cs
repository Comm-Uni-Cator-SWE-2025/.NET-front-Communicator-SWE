/*
 * -----------------------------------------------------------------------------
 *  File: IconShowcaseWindow.xaml.cs
 *  Owner: Dhruvadeep
 *  Roll Number : 142201026
 *  Module : UX
 *
 * -----------------------------------------------------------------------------
 */
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Communicator.Icons;

public partial class IconShowcaseWindow : Window
{
    private ObservableCollection<IconGroup> _allIconGroups = new();
    private ObservableCollection<IconGroup> _filteredIconGroups = new();

    public IconShowcaseWindow()
    {
        InitializeComponent();
        LoadIcons();
        IconsItemsControl.ItemsSource = _filteredIconGroups;
    }

    private void LoadIcons()
    {
        var allIcons = IconCodes.GetAllIconNames().OrderBy(name => name).ToList();

        IEnumerable<IconGroup> groups = allIcons
            .GroupBy(name => char.ToUpper(name[0]))
            .OrderBy(g => g.Key)
            .Select(g => new IconGroup {
                Letter = g.Key.ToString(),
                Icons = new ObservableCollection<string>(g.ToList()),
                Count = g.Count()
            });

        _allIconGroups = new ObservableCollection<IconGroup>(groups);
        _filteredIconGroups = new ObservableCollection<IconGroup>(_allIconGroups);

        int totalCount = allIcons.Count;
        TotalCountText.Text = $"({totalCount:N0} icons)";
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = SearchBox.Text?.ToLower() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(searchText))
        {
            _filteredIconGroups.Clear();
            foreach (IconGroup group in _allIconGroups)
            {
                _filteredIconGroups.Add(group);
            }
        }
        else
        {
            _filteredIconGroups.Clear();

            foreach (IconGroup group in _allIconGroups)
            {
                var filteredIcons = group.Icons
                    .Where(icon => icon.Contains(searchText))
                    .ToList();

                if (filteredIcons.Any())
                {
                    _filteredIconGroups.Add(new IconGroup {
                        Letter = group.Letter,
                        Icons = new ObservableCollection<string>(filteredIcons),
                        Count = filteredIcons.Count
                    });
                }
            }
        }

        int visibleCount = _filteredIconGroups.Sum(g => g.Count);
        TotalCountText.Text = $"({visibleCount:N0} icons)";
    }

    private void IconCard_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is Border border && border.DataContext is string iconName)
        {
            string xamlCode = $"<icons:Icon IconName=\"{iconName}\" IconSize=\"24\" />";
            Clipboard.SetText(xamlCode);

            // Find the "Copied!" TextBlock in the border's content
            if (border.Child is Grid grid)
            {
                foreach (object? child in grid.Children)
                {
                    if (child is TextBlock tb && tb.Name == "CopiedText")
                    {
                        // Show the "Copied!" message
                        tb.Visibility = Visibility.Visible;

                        // Hide it after 1.5 seconds
                        var timer = new System.Windows.Threading.DispatcherTimer {
                            Interval = TimeSpan.FromSeconds(1.5)
                        };
                        timer.Tick += (s, args) => {
                            tb.Visibility = Visibility.Collapsed;
                            timer.Stop();
                        };
                        timer.Start();
                        break;
                    }
                }
            }
        }
    }
}

public class IconGroup : INotifyPropertyChanged
{
    private string _letter = string.Empty;
    private ObservableCollection<string> _icons = new();
    private int _count;

    public string Letter
    {
        get => _letter;
        set {
            _letter = value;
            OnPropertyChanged(nameof(Letter));
        }
    }

    public ObservableCollection<string> Icons
    {
        get => _icons;
        set {
            _icons = value;
            OnPropertyChanged(nameof(Icons));
        }
    }

    public int Count
    {
        get => _count;
        set {
            _count = value;
            OnPropertyChanged(nameof(Count));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

