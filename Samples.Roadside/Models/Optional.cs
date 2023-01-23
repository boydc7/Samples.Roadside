namespace Samples.Roadside.Models;

public class Optional<T>
    where T : class
{
    private readonly T _instance;
   
    public Optional(T instance)
    {
        _instance = instance;
    }

    public static Optional<T> Empty() => new(null);

    public T Get()
        => _instance ?? throw new InvalidOperationException("Optional is empty");

    public bool IsPresent()
        => _instance != null;
}
