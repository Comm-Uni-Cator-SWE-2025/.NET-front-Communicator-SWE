/*
 * -----------------------------------------------------------------------------
 *  File: ScreenShareViewModelTests.cs
 *  Owner: Devansh Manoj Kesan
 *  Roll Number :142201017
 *  Module : ScreenShare
 *
 * -----------------------------------------------------------------------------
 */
using Communicator.App.ViewModels.Meeting;
using Communicator.Controller.Meeting;
using Communicator.ScreenShare.Tests.Helpers;

namespace Communicator.ScreenShare.Tests.ViewModels.Meeting;

public class ScreenShareViewModelTests
{
    [Fact]
    public void Constructor_PreservesReferencesAndInitialLayout()
    {
        // this one simply checks that the constructor keeps the user + participants we pass in
        var user = TestHelpers.CreateUserProfile("host@test.com", "Host", ParticipantRole.INSTRUCTOR);
        var participants = TestHelpers.CreateParticipants(2);

        var viewModel = new ScreenShareViewModel(user, participants);

        Assert.Equal("ScreenShare", viewModel.Title);
        Assert.Same(user, viewModel.CurrentUser);
        Assert.Same(participants, viewModel.Participants);
        Assert.True(viewModel.GridColumns > 0);
        Assert.True(viewModel.GridRows > 0);
    }

    [Fact]
    public void GridLayout_CoversAllThresholds()
    {
        // i loop through all participant counts so every branch of the layout math runs at least once
        var user = TestHelpers.CreateUserProfile("host@test.com", "Host");
        var expectations = new (int Count, int Columns, int Rows)[]
        {
            (0, 1, 1),
            (1, 1, 1),
            (2, 2, 1),
            (3, 2, 2),
            (5, 3, 2),
            (7, 3, 3),
            (10, 4, 3),
            (16, 4, 4),
            (20, 5, 4)
        };

        foreach (var (count, expectedColumns, expectedRows) in expectations)
        {
            var viewModel = new ScreenShareViewModel(user, TestHelpers.CreateParticipants(count));
            Assert.Equal(expectedColumns, viewModel.GridColumns);
            Assert.Equal(expectedRows, viewModel.GridRows);
        }
    }

    [Fact]
    public void ParticipantChanges_RecomputeGridLayout()
    {
        // here i add/remove/clear participants to make sure the grid reacts to collection changes
        var user = TestHelpers.CreateUserProfile("host@test.com", "Host");
        var participants = TestHelpers.CreateParticipants(1);
        var viewModel = new ScreenShareViewModel(user, participants);
        bool columnsChanged = false;
        bool rowsChanged = false;

        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(ScreenShareViewModel.GridColumns))
            {
                columnsChanged = true;
            }

            if (args.PropertyName == nameof(ScreenShareViewModel.GridRows))
            {
                rowsChanged = true;
            }
        };

        participants.Add(TestHelpers.CreateParticipant("two@test.com", "Two"));
        Assert.True(columnsChanged);
        Assert.False(rowsChanged);

        columnsChanged = false;
        participants.Add(TestHelpers.CreateParticipant("three@test.com", "Three"));
        Assert.True(rowsChanged);
        Assert.False(columnsChanged);

        participants.Clear();
        Assert.Equal(1, viewModel.GridColumns);
        Assert.Equal(1, viewModel.GridRows);
    }
}

