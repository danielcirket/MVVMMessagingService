using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MVVMMessagingService
{
    public class MessagingService : IMessagingService
    {
        private ConcurrentDictionary<Type, List<ActionAndToken>> _messageTypeWithRecipients = new ConcurrentDictionary<Type, List<ActionAndToken>>();

        public void Register<TMessage>(object recipient, Action<TMessage> action)
        {
            Register(recipient, null, action);
        }
        public void Register<TMessage>(object recipient, object token, Action<TMessage> action)
        {
            var messageType = typeof(TMessage);

            var item = new ActionAndToken
            {
                Action = new WeakActionWithToken<TMessage>(action),
                Token = token
            };

            List<ActionAndToken> list;

            if (!_messageTypeWithRecipients.ContainsKey(messageType))
            {
                list = new List<ActionAndToken>();
                _messageTypeWithRecipients.TryAdd(messageType, list);
            }
            else
            {
                list = _messageTypeWithRecipients[messageType];
            }

            list.Add(item);
        }

        public void Send<TMessage>(TMessage message)
        {
            Send(message, null);
        }
        public void Send<TMessage>(TMessage message, object token)
        {
            SendToTargetOrType(message, null, token);
        }
        public void Send<TMessage, TTarget>(TMessage message)
        {
            Send<TMessage, TTarget>(message, null);
        }
        public void Send<TMessage, TTarget>(TMessage message, object token)
        {
            SendToTargetOrType(message, typeof(TTarget), token);
        }

        private void SendToTargetOrType<TMessage>(TMessage message, Type targetType, object token)
        {
            var messageType = typeof(TMessage);
            List<ActionAndToken> list = null;

            if (_messageTypeWithRecipients.ContainsKey(messageType))
            {
                list = _messageTypeWithRecipients[messageType].Take(_messageTypeWithRecipients[messageType].Count).ToList();
            }

            if (list != null)
            {
                SendToList(message, list, targetType, token);
            }
        }
        private void SendToList<TMessage>(TMessage message, IEnumerable<ActionAndToken> weakActionsAndTokens, Type targetType, object token)
        {
            if (weakActionsAndTokens != null)
            {
                var list = weakActionsAndTokens.ToList();
                var clonedList = list.Take(list.Count).ToList();

                foreach (var item in clonedList)
                {
                    var executeAction = item.Action as IExecuteWithObject;

                    if (item.Action != null && item.Action.IsAlive && item.Action.Target != null &&
                        (targetType == null || item.Action.Target.GetType() == targetType) &&
                         ((item.Token == null && token == null) || item.Token != null && item.Token.Equals(token)))
                    {
                        if (executeAction != null)
                            executeAction.Execute(message);
                    }
                }
            }
        }

        public void Unregister(object recipient)
        {
            UnregisterFromList(null, recipient, null, _messageTypeWithRecipients);
        }
        public void Unregister<TMessage>(object recipient)
        {
            Unregister<TMessage>(recipient, null);
        }
        public void Unregister<TMessage>(object recipient, object token)
        {
            UnregisterFromList(typeof(TMessage), recipient, token, _messageTypeWithRecipients);
            // TODO: Do some cleanup?
        }

        private void UnregisterFromList(Type messageType, object recipient, object token, ConcurrentDictionary<Type, List<ActionAndToken>> list)
        {
            if (recipient == null || list.Count < 1 || !list.ContainsKey(messageType))
                return;

            foreach (var type in list.Keys)
            {
                // TODO: Actually deal with token if passed in and only de-register from that particular token.
                foreach (var item in list[type])
                {
                    if (recipient == item.Action.Target)
                        item.Action.MarkForDeletion();
                }
            }
        }
    }
}
