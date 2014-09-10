﻿using System;
using System.Threading;

namespace TetriNET2.Common.ActionQueue
{
    public interface IActionQueue
    {
        int ActionCount { get; }

        void Start(CancellationTokenSource cancellationTokenSource); // Cancel token to Stop
        void Wait(int milliseconds); // Wait until stopped or timeout elapsed

        void Enqueue(Action action);
        void Reset();
    }
}
