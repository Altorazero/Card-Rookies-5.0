public interface IValueSpec<out T>
{
    T Resolve(ExecutionContext context);
}