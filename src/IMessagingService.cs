using System;

namespace MVVMMessagingService
{
    public interface IMessagingService
    {
        void Register<TMessage>(object recipient, Action<TMessage> callback);
        void Register<TMessage>(object recipient, object token, Action<TMessage> callback);

        void Send<TMessage>(TMessage message);
        void Send<TMessage>(TMessage message, object token);
        void Send<TMessage, TTarget>(TMessage message);
        void Send<TMessage, TTarget>(TMessage message, object token);

        void Unregister(object recipient);
        void Unregister<TMessage>(object recipient);
        void Unregister<TMessage>(object recipient, object token);
    }
}
