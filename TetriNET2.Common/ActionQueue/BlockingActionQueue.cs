using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using TetriNET2.Common.Logger;

namespace TetriNET2.Common.ActionQueue
{
    public sealed class BlockingActionQueue : IActionQueue, IDisposable
    {
        private readonly BlockingCollection<Action> _gameActionBlockingCollection = new BlockingCollection<Action>(new ConcurrentQueue<Action>());

        private CancellationTokenSource _cancellationTokenSource;
        private Task _gameActionTask;

        public int ActionCount { get { return _gameActionBlockingCollection.Count; } }

        public void Start(CancellationTokenSource cancellationTokenSource)
        {
            _cancellationTokenSource = cancellationTokenSource;
            _gameActionTask = Task.Factory.StartNew(GameActionsTask, _cancellationTokenSource.Token);
        }

        public void Wait(int milliseconds)
        {
            _gameActionTask.Wait(milliseconds);
        }

        public void Enqueue(Action action)
        {
            _gameActionBlockingCollection.Add(action);
        }

        public void Clear()
        {
            while (_gameActionBlockingCollection.Count > 0)
            {
                Action item;
                _gameActionBlockingCollection.TryTake(out item);
            }
        }

        private void GameActionsTask()
        {
            Log.Default.WriteLine(LogLevels.Info, "GameActionsTask: Started");

            try
            {
                while (true)
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        Log.Default.WriteLine(LogLevels.Info, "GameActionsTask: Stop event raised");
                        break;
                    }
                    try
                    {
                        Action action;
                        bool taken = _gameActionBlockingCollection.TryTake(out action, 10, _cancellationTokenSource.Token);
                        if (taken)
                        {
                            try
                            {
                                Log.Default.WriteLine(LogLevels.Debug, "GameActionsTask: Dequeue, item in queue {0}", _gameActionBlockingCollection.Count);
                                action();
                            }
                            catch (Exception ex)
                            {
                                Log.Default.WriteLine(LogLevels.Error, "GameActionsTask: Exception:{0}", ex);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Log.Default.WriteLine(LogLevels.Info, "GameActionsTask: TryTake cancelled");
                        break;
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                Log.Default.WriteLine(LogLevels.Error, "GameActionsTask: Cancelled exception. Exception: {0}", ex);
            }

            Log.Default.WriteLine(LogLevels.Info, "GameActionsTask: Stopped");
        }

        #region IDisposable

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _gameActionBlockingCollection.CompleteAdding();
                _gameActionBlockingCollection.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            //GC.SuppressFinalize(this);
        }

        #endregion
    }
}
