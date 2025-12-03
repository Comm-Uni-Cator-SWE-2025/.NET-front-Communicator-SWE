using System;
using System.IO;
using System.Linq;
using System.Text;
using Communicator.App.ViewModels.Meeting;
using Communicator.Chat;
using Communicator.Controller.RPC;
using Communicator.UX.Core.Services;
using Communicator.Controller.Meeting;   // <-- real UserProfile
using Moq;
using Xunit;

namespace Communicator.App.Tests.Unit.ViewModels.Meeting
{
    public class ChatViewModelTests : IDisposable
    {
        private readonly Mock<IToastService> _mockToastService;
        private readonly Mock<IRPC> _mockRpc;
        private ChatViewModel? _viewModel;

        public ChatViewModelTests()
        {
            _mockToastService = new Mock<IToastService>();
            _mockRpc = new Mock<IRPC>();
        }

        public void Dispose()
        {
            _viewModel = null;
        }

        private ChatViewModel CreateViewModel(bool provideRpc = true)
        {
            var user = new UserProfile
            {
                Email = "test@example.com",
                DisplayName = "Test User",
                Role = ParticipantRole.STUDENT
            };

            // Pass null for rpcEventService to avoid requiring that type in tests
            _viewModel = new ChatViewModel(user, _mockToastService.Object, provideRpc ? _mockRpc.Object : null, null);
            return _viewModel;
        }

        [Fact]
        public void Constructor_WithValidParameters_InitializesSuccessfully()
        {
            var vm = CreateViewModel();

            Assert.NotNull(vm);
            Assert.NotNull(vm.Messages);
            Assert.Empty(vm.Messages);
            Assert.Equal(string.Empty, vm.MessageInput);
            Assert.False(vm.IsReplying);
            Assert.False(vm.HasAttachment);
        }

        [Fact]
        public void MessageInput_SetValue_UpdatesProperty()
        {
            var vm = CreateViewModel();
            vm.MessageInput = "hello";
            Assert.Equal("hello", vm.MessageInput);
        }

        [Fact]
        public void SendTextMessage_AddsMessageAndClearsInput()
        {
            var vm = CreateViewModel();
            vm.MessageInput = "Test message";

            vm.SendMessageCommand.Execute(null);

            Assert.Single(vm.Messages);
            Assert.Equal("Test message", vm.Messages[0].Content);
            Assert.True(vm.Messages[0].IsSentByMe);
            Assert.Equal(string.Empty, vm.MessageInput);
        }

        [Fact]
        public void CanSendMessage_ReturnsFalseWhenEmpty_TrueWhenHasText()
        {
            var vm = CreateViewModel();
            Assert.False(vm.SendMessageCommand.CanExecute(null));

            vm.MessageInput = "x";
            Assert.True(vm.SendMessageCommand.CanExecute(null));
        }

        [Fact]
        public void StartReply_SetsReplyQuoteText()
        {
            var vm = CreateViewModel();
            var message = new ChatMessage("m1", "Other", "Original", string.Empty, 0, Array.Empty<byte>(), "10:00", false, string.Empty);
            vm.Messages.Add(message);

            vm.StartReply(message);

            Assert.True(vm.IsReplying);
            Assert.Contains("Replying", vm.ReplyQuoteText, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public void CancelReply_ClearsReplyState()
        {
            var vm = CreateViewModel();
            var message = new ChatMessage("m1", "Other", "Original", string.Empty, 0, Array.Empty<byte>(), "10:00", false, string.Empty);
            vm.Messages.Add(message);
            vm.StartReply(message);

            vm.CancelReply();

            Assert.False(vm.IsReplying);
            Assert.Equal(string.Empty, vm.ReplyQuoteText);
        }

        [Fact]
        public void AttachFileCommand_SetsAttachment_WhenFileSelected()
        {
            var vm = CreateViewModel();
            var temp = new FileInfo(Path.GetTempFileName());
            try
            {
                File.WriteAllText(temp.FullName, "data");
                vm.RequestFileSelection += (_, e) => e.SelectedFile = temp;

                vm.AttachFileCommand.Execute(null);

                Assert.True(vm.HasAttachment);
                Assert.Equal(temp.Name, vm.AttachmentText);
            }
            finally
            {
                try { temp.Delete(); } catch { }
            }
        }

        [Fact]
        public void AttachFileCommand_DoesNotSet_WhenFileMissingOrTooLarge_ShowsError()
        {
            var vm = CreateViewModel();

            // Non-existent file
            var missing = new FileInfo("nonexistent_file_123.tmp");
            vm.RequestFileSelection += (_, e) => e.SelectedFile = missing;
            vm.AttachFileCommand.Execute(null);
            Assert.False(vm.HasAttachment);

            // Too large file: create temp and set length > 50MB
            var large = new FileInfo(Path.GetTempFileName());
            try
            {
                using (var fs = new FileStream(large.FullName, FileMode.Create)) { fs.SetLength(51L * 1024 * 1024); }

                vm.RequestFileSelection += (_, e) => e.SelectedFile = large;
                vm.AttachFileCommand.Execute(null);

                // Avoid expression tree issue by matching via predicate
                _mockToastService.Verify(x => x.ShowError(It.Is<string>(s => s.Contains("File is too large")), It.IsAny<int>()), Times.AtLeastOnce);
            }
            finally
            {
                try { large.Delete(); } catch { }
            }
        }

        [Fact]
        public void DeleteMessage_RemovesOwnMessage_AndCallsRpc()
        {
            var vm = CreateViewModel();
            var own = new ChatMessage("m1", "You", "Hey", string.Empty, 0, Array.Empty<byte>(), "10:00", true, string.Empty);
            vm.Messages.Add(own);

            _mockRpc.Setup(r => r.Call(It.IsAny<string>(), It.IsAny<byte[]>()))
                .ReturnsAsync(Array.Empty<byte>());

            vm.DeleteMessageCommand.Execute(own);

            Assert.Empty(vm.Messages);
            _mockRpc.Verify(r => r.Call("chat:delete-message", It.IsAny<byte[]>()), Times.Once);
        }

    }
}
