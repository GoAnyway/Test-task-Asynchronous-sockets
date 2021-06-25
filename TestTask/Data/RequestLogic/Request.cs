namespace TestTask.Data.RequestLogic
{
    public class Request
    {
        public Request(int id, RequestState state)
        {
            Id = id;
            State = state;
        }

        public int Id { get; set; }
        public RequestState State { get; set; }
    }
}