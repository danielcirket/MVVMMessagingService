using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MVVMMessagingService
{
    public class MessagingService : IMessagingService
    {
        private ConcurrentDictionary<Type, List<ActionAndToken>> _messageTypeWithRecipients = new ConcurrentDictionary<Type, List<ActionAndToken>>();
        private ConcurrentDictionary<object, List<ActionAndToken>> _noMessageRecipients = new ConcurrentDictionary<object, List<ActionAndToken>>();

        public void Subscribe(object recipient, object token, Action action)
        {
            if (recipient == null)
                throw new ArgumentNullException(nameof(recipient));

            var item = new ActionAndToken
            {
                Action = new WeakActionWithToken(action),
                Token = token
            };

            List<ActionAndToken> list = new List<ActionAndToken>();

            if (!_noMessageRecipients.ContainsKey(token))
                _noMessageRecipients.TryAdd(token, list);
            else
                list = _noMessageRecipients[token];

            list.Add(item);
        }
        public void Subscribe<TMessage>(object recipient, Action<TMessage> action)
        {
            Subscribe(recipient, null, action);
        }
        public void Subscribe<TMessage>(object recipient, object token, Action<TMessage> action)
        {
            var messageType = typeof(TMessage);

            var item = new ActionAndToken
            {
                Action = new WeakActionWithToken<TMessage>(action),
                Token = token
            };

            List<ActionAndToken> list = new List<ActionAndToken>();

            if (!_messageTypeWithRecipients.ContainsKey(messageType))
                _messageTypeWithRecipients.TryAdd(messageType, list);
            else
                list = _messageTypeWithRecipients[messageType];

            list.Add(item);
        }

        public void Publish(object token)
        {
            SendWithNoMessage(null, token);
        }
        public void Publish<TTarget>(object token)
        {
            var type = typeof(TTarget);
            SendWithNoMessage(type, token);
        }
        public void Publish<TMessage>(TMessage message)
        {
            Publish(message, null);
        }
        public void Publish<TMessage>(TMessage message, object token)
        {
            PublishToTargetOrType(message, null, token);
        }
        public void Publish<TMessage, TTarget>(TMessage message)
        {
            Publish<TMessage, TTarget>(message, null);
        }
        public void Publish<TMessage, TTarget>(TMessage message, object token)
        {
            PublishToTargetOrType(message, typeof(TTarget), token);
        }

        public void Unsubscribe(object recipient)
        {
            UnregisterFromList(null, recipient, null, _noMessageRecipients);
            UnregisterFromList(null, recipient, null, _messageTypeWithRecipients);
        }
        public void Unsubscribe<TMessage>(object recipient)
        {
            Unsubscribe<TMessage>(recipient, null);
        }
        public void Unsubscribe<TMessage>(object recipient, object token)
        {
            UnregisterFromList(typeof(TMessage), recipient, token, _messageTypeWithRecipients);

            // TODO: Do some cleanup after objects have been marked to be deleted.
        }

        private void SendWithNoMessage<T>(Type targetType, T token)
        {
            List<ActionAndToken> list = null;
            if (_noMessageRecipients.ContainsKey(token))
                list = _noMessageRecipients[token];

            if (list != null)
                PublishToListWithNoMessage(list, targetType, token);
        }
        private void PublishToTargetOrType<TMessage>(TMessage message, Type targetType, object token)
        {
            var messageType = typeof(TMessage);
            List<ActionAndToken> list = null;

            if (_messageTypeWithRecipients.ContainsKey(messageType))
                list = _messageTypeWithRecipients[messageType].Take(_messageTypeWithRecipients[messageType].Count).ToList();

            if (list != null)
                PublishToList(message, list, targetType, token);
        }
        private void PublishToListWithNoMessage(IEnumerable<ActionAndToken> weakActionsAndTokens, Type targetType, object token)
        {
            if (weakActionsAndTokens == null)
                return;

            var list = weakActionsAndTokens.ToList();
            var clonedList = list.Take(list.Count).ToList();

            foreach (var item in clonedList)
            {
                var executeAction = item.Action;

                if (item.Action != null && item.Action.IsAlive && item.Action.Target != null &&
                    (targetType == null || item.Action.Target.GetType() == targetType) &&
                        (item.Token != null && item.Token.Equals(token)))
                {
                    if (executeAction != null)
                        executeAction.Execute();
                }
            }
        }
        private void PublishToList<TMessage>(TMessage message, IEnumerable<ActionAndToken> weakActionsAndTokens, Type targetType, object token)
        {
            if (weakActionsAndTokens == null)
                return;

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
        private void UnregisterFromList<T>(Type messageType, object recipient, object token, ConcurrentDictionary<T, List<ActionAndToken>> list)
        {
            if (recipient == null || list.Count < 1)
                return;

            // TODO(Dan): If there a more efficient way of checking the key here,
            //            can't check in the first statement because of generic use?
            foreach (var key in list.Keys)
                foreach (var item in list[key])
                    if (recipient.Equals(item.Action.Target) && (messageType == null || key.Equals(messageType)) && (token == null || item.Token.Equals(token)))
                        item.Action.MarkForDeletion();
        }
    }
}
