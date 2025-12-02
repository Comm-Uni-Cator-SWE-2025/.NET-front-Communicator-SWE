using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Communicator.App.ViewModels.Meeting;
using Communicator.Controller.Meeting;
using Xunit;

namespace Communicator.App.Tests.Unit.ViewModels.Meeting;

public sealed class ParticipantsListViewModelTests
{
    private static UserProfile CreateTestUser(string email, string name)
    {
        return new UserProfile(email, name, ParticipantRole.STUDENT, null);
    }

    private static ParticipantViewModel CreateTestParticipant(string email, string name)
    {
        return new ParticipantViewModel(CreateTestUser(email, name));
    }

    [Fact]
    public void ConstructorInitializesWithEmptyCollection()
    {
        ObservableCollection<ParticipantViewModel> participants = new ObservableCollection<ParticipantViewModel>();

        ParticipantsListViewModel vm = new ParticipantsListViewModel(participants);

        Assert.Same(participants, vm.Participants);
        Assert.Equal(0, vm.ParticipantCount);
    }

    [Fact]
    public void ConstructorInitializesWithExistingParticipants()
    {
        ObservableCollection<ParticipantViewModel> participants = new ObservableCollection<ParticipantViewModel> {
            CreateTestParticipant("user1@test.com", "User 1"),
            CreateTestParticipant("user2@test.com", "User 2"),
            CreateTestParticipant("user3@test.com", "User 3")
        };

        ParticipantsListViewModel vm = new ParticipantsListViewModel(participants);

        Assert.Equal(3, vm.ParticipantCount);
    }

    [Fact]
    public void ParticipantCountUpdatesWhenParticipantAdded()
    {
        ObservableCollection<ParticipantViewModel> participants = new ObservableCollection<ParticipantViewModel>();
        ParticipantsListViewModel vm = new ParticipantsListViewModel(participants);

        Assert.Equal(0, vm.ParticipantCount);

        participants.Add(CreateTestParticipant("user@test.com", "User"));

        Assert.Equal(1, vm.ParticipantCount);
    }

    [Fact]
    public void ParticipantCountUpdatesWhenParticipantRemoved()
    {
        ParticipantViewModel participant = CreateTestParticipant("user@test.com", "User");
        ObservableCollection<ParticipantViewModel> participants = new ObservableCollection<ParticipantViewModel> {
            participant
        };
        ParticipantsListViewModel vm = new ParticipantsListViewModel(participants);

        Assert.Equal(1, vm.ParticipantCount);

        participants.Remove(participant);

        Assert.Equal(0, vm.ParticipantCount);
    }

    [Fact]
    public void ParticipantCountUpdatesWhenCollectionCleared()
    {
        ObservableCollection<ParticipantViewModel> participants = new ObservableCollection<ParticipantViewModel> {
            CreateTestParticipant("user1@test.com", "User 1"),
            CreateTestParticipant("user2@test.com", "User 2")
        };
        ParticipantsListViewModel vm = new ParticipantsListViewModel(participants);

        Assert.Equal(2, vm.ParticipantCount);

        participants.Clear();

        Assert.Equal(0, vm.ParticipantCount);
    }

    [Fact]
    public void ParticipantCountRaisesPropertyChanged()
    {
        ObservableCollection<ParticipantViewModel> participants = new ObservableCollection<ParticipantViewModel>();
        ParticipantsListViewModel vm = new ParticipantsListViewModel(participants);

        bool propertyChanged = false;
        vm.PropertyChanged += (s, e) => {
            if (e.PropertyName == nameof(vm.ParticipantCount))
            {
                propertyChanged = true;
            }
        };

        participants.Add(CreateTestParticipant("user@test.com", "User"));

        Assert.True(propertyChanged);
    }

    [Fact]
    public void ParticipantCountUpdatesOnMultipleAdditions()
    {
        ObservableCollection<ParticipantViewModel> participants = new ObservableCollection<ParticipantViewModel>();
        ParticipantsListViewModel vm = new ParticipantsListViewModel(participants);

        for (int i = 0; i < 5; i++)
        {
            participants.Add(CreateTestParticipant($"user{i}@test.com", $"User {i}"));
        }

        Assert.Equal(5, vm.ParticipantCount);
    }

    [Fact]
    public void ParticipantsCollectionIsSameInstanceAsProvided()
    {
        ObservableCollection<ParticipantViewModel> participants = new ObservableCollection<ParticipantViewModel>();
        ParticipantsListViewModel vm = new ParticipantsListViewModel(participants);

        Assert.Same(participants, vm.Participants);
    }

    [Fact]
    public void ParticipantCountUpdatesOnReplace()
    {
        ParticipantViewModel participant1 = CreateTestParticipant("user1@test.com", "User 1");
        ObservableCollection<ParticipantViewModel> participants = new ObservableCollection<ParticipantViewModel> {
            participant1
        };
        ParticipantsListViewModel vm = new ParticipantsListViewModel(participants);

        Assert.Equal(1, vm.ParticipantCount);

        // Replace the participant
        participants[0] = CreateTestParticipant("user2@test.com", "User 2");

        // Count should still be 1 after replace
        Assert.Equal(1, vm.ParticipantCount);
    }
}
