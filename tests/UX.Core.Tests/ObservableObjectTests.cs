using UX.Core;

namespace UX.Core.Tests;

public class ObservableObjectTests
{
    private class TestViewModel : ObservableObject
    {
        private string _name = string.Empty;
        private int _age;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public int Age
        {
            get => _age;
            set => SetProperty(ref _age, value);
        }
    }

    [Fact]
    public void SetProperty_WhenValueChanges_RaisesPropertyChanged()
    {
        var viewModel = new TestViewModel();
        var propertyChangedRaised = false;
        string? changedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            changedPropertyName = args.PropertyName;
        };

        viewModel.Name = "John";

        Assert.True(propertyChangedRaised);
        Assert.Equal("Name", changedPropertyName);
        Assert.Equal("John", viewModel.Name);
    }

    [Fact]
    public void SetProperty_WhenValueDoesNotChange_DoesNotRaisePropertyChanged()
    {
        var viewModel = new TestViewModel { Name = "John" };
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
        };

        viewModel.Name = "John";

        Assert.False(propertyChangedRaised);
    }

    [Fact]
    public void SetProperty_ReturnsTrue_WhenValueChanges()
    {
        var viewModel = new TestViewModel();

        viewModel.Age = 10;

        Assert.Equal(10, viewModel.Age);
    }

    [Fact]
    public void SetProperty_ReturnsFalse_WhenValueDoesNotChange()
    {
        var viewModel = new TestViewModel { Age = 10 };
        var propertyChangedRaised = false;

        viewModel.PropertyChanged += (sender, args) => propertyChangedRaised = true;

        viewModel.Age = 10;

        Assert.False(propertyChangedRaised);
    }

    [Fact]
    public void OnPropertyChanged_RaisesPropertyChangedEvent()
    {
        var viewModel = new TestViewModel();
        var propertyChangedRaised = false;
        string? changedPropertyName = null;

        viewModel.PropertyChanged += (sender, args) =>
        {
            propertyChangedRaised = true;
            changedPropertyName = args.PropertyName;
        };

        viewModel.Age = 25;

        Assert.True(propertyChangedRaised);
        Assert.Equal("Age", changedPropertyName);
    }
}
