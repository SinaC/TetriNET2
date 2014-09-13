﻿using System;
using System.Collections.Generic;
using System.Threading;
using TetriNET2.Common.ActionQueue;

namespace TetriNET2.Tests.Server.Mocking
{
    public class ActionQueueMock : IActionQueue
    {
        private readonly List<Action> _actions = new List<Action>();

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
            _actions.Add(action);
        }

        public void Clear()
        {
            _actions.Clear();
        }
    }
}
