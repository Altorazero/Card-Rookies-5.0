public interface ICardExecutionHost
{
    void RequestContinue(CardExecution execution);
    void RequestCancel(CardExecution execution);
}

public sealed class EventQueueCardExecutionHost : ICardExecutionHost
{
    private readonly EventQueue _dispatcher;
    public EventQueueCardExecutionHost(EventQueue dispatcher) => _dispatcher = dispatcher;

    public void RequestContinue(CardExecution execution)
    {
        execution.Tick();
        _dispatcher.ProcessQueue();
    }

    public void RequestCancel(CardExecution execution)
    {
        execution.Context.EventContext.Event.Status = EventStatus.Cancelled;
        _dispatcher.ProcessQueue();
    }
}