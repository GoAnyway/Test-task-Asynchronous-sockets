using System;
using System.Collections.Generic;

namespace TestTask.EventArguments
{
    /// <summary>
    ///     Arguments of the event that is called when all data has been received from the server.
    /// </summary>
    public class AllDataReceivedEventArgs : EventArgs
    {
        public AllDataReceivedEventArgs(IEnumerable<int> values)
        {
            Values = values;
        }

        public IEnumerable<int> Values { get; }
    }
}