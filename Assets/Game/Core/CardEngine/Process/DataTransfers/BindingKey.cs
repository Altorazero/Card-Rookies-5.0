public sealed class BindingKey<T>
{
    public string Name { get; }

    public BindingKey(string name)
    {
        Name = name;
    }

    public override string ToString()
    {
        return Name;
    }
}