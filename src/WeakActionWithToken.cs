using System;
using System.Reflection;

namespace MVVMMessagingService
{
    internal class WeakActionWithToken
    {
        private Action _action;

        protected WeakReference Reference { get; set; }
        protected WeakReference ActionReference { get; set; }
        protected MethodInfo Method { get; set; }
        public object Target
        {
            get
            {
                return Reference != null
                    ? Reference.Target
                    : null;
            }
        }
        public object ActionTarget
        {
            get
            {
                return ActionReference != null
                    ? ActionReference.Target
                    : null;
            }
        }
        public virtual bool IsAlive
        {
            get
            {
                if (_action == null && Reference == null)
                    return false;

                if (_action != null && Reference == null)
                    return true;

                return Reference.IsAlive;
            }
        }

        public void Execute()
        {
            if (_action != null)
            {
                _action();
                return;
            }

            if (!IsAlive)
                return;

            if (Method != null && ActionReference != null && ActionTarget != null)
                Method.Invoke(ActionTarget, null);
        }
        public void MarkForDeletion()
        {
            Reference = null;
            ActionReference = null;
            Method = null;
            _action = null;
        }

        public WeakActionWithToken(Action action) : this(action == null ? null : action.Target, action) { }
        public WeakActionWithToken(object target, Action action)
        {
            if (action.Method.IsStatic)
            {
                _action = action;

                if (target != null)
                {
                    Reference = new WeakReference(target);
                }

                return;
            }

            Method = action.Method;
            ActionReference = new WeakReference(action.Target);
            Reference = new WeakReference(target);
        }
        public WeakActionWithToken() { }
    }
    internal class WeakActionWithToken<T> : WeakActionWithToken, IExecuteWithObject
    {
        private Action<T> _action;

        public override bool IsAlive
        {
            get
            {
                if (_action == null && Reference == null)
                    return false;

                if (_action != null && Reference == null)
                    return true;

                return Reference.IsAlive;
            }
        }

        public new void Execute()
        {
            Execute(default(T));
        }
        public void Execute(T parameter)
        {
            if (_action != null)
            {
                _action(parameter);
                return;
            }

            if (!IsAlive)
                return;

            if (Method != null && ActionReference != null && ActionTarget != null)
                Method.Invoke(ActionTarget, new object[] { parameter });
        }
        public void Execute(object parameter)
        {
            Execute((T)parameter);
        }

        public WeakActionWithToken(Action<T> action) : this(action == null ? null : action.Target, action) { }
        public WeakActionWithToken(object target, Action<T> action)
        {
            if (action.Method.IsStatic)
            {
                _action = action;

                if (target != null)
                    Reference = new WeakReference(target);

                return;
            }

            Method = action.Method;
            ActionReference = new WeakReference(action.Target);
            Reference = new WeakReference(target);
        }
    }
}
