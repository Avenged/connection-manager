using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vanara.PInvoke;
using Windows.Foundation.Collections;

namespace ConnectionManagerFullTrust
{
    public class ProgressHandler : IDisposable
    {
        private ManualResetEvent operationsCompletedEvent;
        private PipeStream connection;

        private class OperationWithProgress
        {
            public int Progress { get; set; }
            public bool Canceled { get; set; }
        }

        private Shell32.ITaskbarList4 taskbar;
        private ConcurrentDictionary<string, OperationWithProgress> operations;

        public System.Windows.Forms.IWin32Window OwnerWindow { get; set; }

        public ProgressHandler(PipeStream conn)
        {
            taskbar = Win32API.CreateTaskbarObject();
            operations = new ConcurrentDictionary<string, OperationWithProgress>();
            operationsCompletedEvent = new ManualResetEvent(true);
            connection = conn;
        }

        public int Progress
        {
            get
            {
                var ongoing = operations.ToList().Where(x => !x.Value.Canceled);
                return ongoing.Any() ? (int)ongoing.Average(x => x.Value.Progress) : 0;
            }
        }

        public void AddOperation(string uid)
        {
            operations.TryAdd(uid, new OperationWithProgress());
            UpdateTaskbarProgress();
            operationsCompletedEvent.Reset();
        }

        public void RemoveOperation(string uid)
        {
            operations.TryRemove(uid, out _);
            UpdateTaskbarProgress();
            if (!operations.Any())
            {
                operationsCompletedEvent.Set();
            }
        }

        public async void UpdateOperation(string uid, int progress)
        {
            if (operations.TryGetValue(uid, out var op))
            {
                op.Progress = progress;
                await Win32API.SendMessageAsync(connection, new ValueSet() {
                        { "Progress", progress },
                        { "OperationID", uid }
                    });
                UpdateTaskbarProgress();
            }
        }

        public bool CheckCanceled(string uid)
        {
            if (operations.TryGetValue(uid, out var op))
            {
                return op.Canceled;
            }
            return true;
        }

        public void TryCancel(string uid)
        {
            if (operations.TryGetValue(uid, out var op))
            {
                op.Canceled = true;
                UpdateTaskbarProgress();
            }
        }

        private void UpdateTaskbarProgress()
        {
            if (OwnerWindow == null || taskbar == null)
            {
                return;
            }
            if (operations.Any())
            {
                taskbar.SetProgressValue(OwnerWindow.Handle, (ulong)Progress, 100);
            }
            else
            {
                taskbar.SetProgressState(OwnerWindow.Handle, Shell32.TBPFLAG.TBPF_NOPROGRESS);
            }
        }

        public void WaitForCompletion()
        {
            operationsCompletedEvent.WaitOne();
        }

        public void Dispose()
        {
            operationsCompletedEvent?.Dispose();
        }
    }
}
