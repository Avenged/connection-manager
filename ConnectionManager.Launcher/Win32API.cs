using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vanara.PInvoke;
using Windows.Foundation.Collections;

namespace ConnectionManagerFullTrust
{
    internal class Win32API
    {

        public static async Task SendMessageAsync(PipeStream pipe, ValueSet valueSet, string requestID = null)
        {
            await IgnoreExceptions(async () =>
            {
                var message = new Dictionary<string, object>(valueSet);
                message.Add("RequestID", requestID);
                var serialized = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                await pipe.WriteAsync(serialized, 0, serialized.Length);
            });
        }

        public static Shell32.ITaskbarList4 CreateTaskbarObject()
        {
            var taskbar2 = new Shell32.ITaskbarList2();
            taskbar2.HrInit();
            return taskbar2 as Shell32.ITaskbarList4;
        }

        public static async Task<bool> IgnoreExceptions(Func<Task> action)
        {
            try
            {
                await action();
                return true;
            }
            catch (Exception ex)
            {
                //logger?.Info(ex, ex.Message);
                return false;
            }
        }
    }
}
