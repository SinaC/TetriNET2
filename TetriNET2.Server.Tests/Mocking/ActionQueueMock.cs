using System;
using System.Collections.Generic;
using System.Threading;
using TetriNET2.Common.ActionQueue;

namespace TetriNET2.Server.Tests.Mocking
{
    public class ActionQueueMock : IActionQueue
    {
        private readonly Queue<Action> _actions = new Queue<Action>();

        public int ActionCount { get { return _actions.Count; } }

        public void Start(CancellationTokenSource cancellationTokenSource)
        {
            // NOP
        }

        public void Wait(int milliseconds)
        {
            // NOP
        }

        public void Enqueue(Action action)
        {
            _actions.Enqueue(action);
        }

        public void Clear()
        {
            _actions.Clear();
        }

        public void DequeueAndExecuteFirstAction()
        {
            Action action = _actions.Dequeue();
            action();
        }
    }
}
