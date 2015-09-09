using System;
using TestHelpers;
using FluentAssertions;
using MVVMMessagingService;

namespace ServiceTests
{
    public class MessagingServceTests
    {
        private MessagingService _messagingService = new MessagingService();

        [UnitTest]
        public void WhenSubscribingToServiceWithNullRecipientThenShouldThrowArgumentNullException()
        {
            Action act = () => _messagingService.Subscribe(null, "TOKEN", () => { });
            act.ShouldThrow<ArgumentNullException>();
        }
        [UnitTest]
        public void WhenSubscribedToMessageWithTokenThenShouldRecieveNotificationWithMessageWhenSentUsingToken()
        {
            var result = "";

            _messagingService.Subscribe(this, "TOKEN", () => result = "RESULT");

            result.Should().BeNullOrEmpty();

            _messagingService.Publish(token: "TOKEN");

            result.Should().Be("RESULT");
        }
        [UnitTest]
        public void WhenSubscribedToMessageWithTokenThenShouldNotRecieveNotificationWhenNonMatchingTokenPublished()
        {
            var result = "";

            _messagingService.Subscribe(this, "TOKEN", () => result = "RESULT");

            result.Should().BeNullOrEmpty();

            _messagingService.Publish(token: "DIFFERENTTOKEN");

            result.Should().NotBe("RESULT");
            result.Should().BeNullOrEmpty();
        }
        [UnitTest]
        public void WhenSubscribedToMessageThenShouldRecieveNotificationWithMessageParameter()
        {
            var result = "";

            _messagingService.Subscribe<string>(this, (parameter) => result = parameter);

            result.Should().BeNullOrEmpty();

            _messagingService.Publish(message: "Message");

            result.Should().Be("Message");
        }
        [UnitTest]
        public void WhenMultipleSubscriptionsToNotificationThenEachShouldRecieveNotification()
        {
            var result = "";
            var result2 = "";

            _messagingService.Subscribe<string>(this, (parameter) => result = parameter);
            _messagingService.Subscribe<string>(this, (parameter) => result2 = parameter);

            result.Should().BeNullOrEmpty();
            result2.Should().BeNullOrEmpty();

            _messagingService.Publish(message: "Message");

            result.Should().Be("Message");
            result2.Should().Be("Message");
        }
        [UnitTest]
        public void WhenMultipleSubscriptionsToNotificationAndMessageSentToTargetThenOnlyTargetShouldRecieveNotification()
        {
            var result = "";

            _messagingService.Subscribe<string>(this, (parameter) => result = parameter);
            var testObject = new TestRecipientObject(_messagingService);

            result.Should().BeNullOrEmpty();
            testObject.Message.Should().BeNullOrEmpty();

            _messagingService.Publish<string, TestRecipientObject>(message: "Message");

            result.Should().BeNullOrEmpty();
            testObject.Message.Should().Be("Message");
        }
        [UnitTest]
        public void WhenUnsubscribingFromAllNotificationThenShouldNoLongerRecieveNotifications()
        {
            var testObject = new TestRecipientObject(_messagingService);

            testObject.Message.Should().BeNullOrEmpty();

            _messagingService.Publish(message: "Message");

            testObject.Message.Should().Be("Message");

            testObject.Message = string.Empty;

            _messagingService.Unsubscribe(testObject);

            _messagingService.Publish(message: "Message");

            testObject.Message.Should().BeNullOrEmpty();
        }
        [UnitTest]
        public void WhenUnsubscribingFromSpecificNotificationThenShouldNoLongerRecieveSpecificMessageButStillRecieveOthers()
        {
            var testObject = new TestRecipientObject(_messagingService);

            testObject.Message.Should().BeNullOrEmpty();
            testObject.IntMessage.Should().Be(0);

            _messagingService.Publish(message: "Message");
            _messagingService.Publish(message: 99);

            testObject.Message.Should().Be("Message");
            testObject.IntMessage.Should().Be(99);

            _messagingService.Unsubscribe<int>(testObject);

            _messagingService.Publish(message: "MessageTwo");
            _messagingService.Publish(message: 123);

            testObject.Message.Should().Be("MessageTwo");
            testObject.IntMessage.Should().Be(99);
        }
    }
}
