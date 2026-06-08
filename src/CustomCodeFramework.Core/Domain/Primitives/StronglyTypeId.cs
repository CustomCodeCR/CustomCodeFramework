namespace CustomCodeFramework.Core.Domain.Primitives;

public abstract record StronglyTypedId<TValue>(TValue Value)
    where TValue : notnull
{
    public override string ToString()
    {
        return Value.ToString() ?? string.Empty;
    }
}
