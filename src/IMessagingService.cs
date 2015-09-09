using System;

namespace MVVMMessagingService
{
    public interface IMessagingService
    {
        void Subscribe(object recipient, object token, Action callback);
        void Subscribe<TMessage>(object recipient, Action<TMessage> callback);
        void Subscribe<TMessage>(object recipient, object token, Action<TMessage> callback);

        void Publish(object token);
        void Publish<TTarget>(object token);
        void Publish<TMessage>(TMessage message);
        void Publish<TMessage>(TMessage message, object token);
        void Publish<TMessage, TTarget>(TMessage message);
        void Publish<TMessage, TTarget>(TMessage message, object token);

        void Unsubscribe(object recipient);
        void Unsubscribe<TMessage>(object recipient);
        void Unsubscribe<TMessage>(object recipient, object token);
    }
}
