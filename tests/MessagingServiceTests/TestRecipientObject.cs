using MVVMMessagingService;

namespace ServiceTests
{
    internal class TestRecipientObject
    {
        public string Message { get; set; }
        public int IntMessage { get; set; }

        private void Callback(string message)
        {
            Message = message;
        }
        private void IntCallback(int message)
        {
            IntMessage = message;
        }

        public TestRecipientObject(IMessagingService messagingService)
        {
            messagingService.Subscribe<string>(this, message => Callback(message));
            messagingService.Subscribe<int>(this, message => IntCallback(message));
        }
    }
}
