namespace TestTask.Data.EventArguments
{
    public class DataReceivedEventArgs : CommonEventArgs
    {
        public DataReceivedEventArgs(int requestId, int data) : base(requestId)
        {
            Data = data;
        }

        public int Data { get; }
    }
}