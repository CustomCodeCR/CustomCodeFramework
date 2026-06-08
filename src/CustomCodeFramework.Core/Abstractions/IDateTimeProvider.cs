namespace CustomCodeFramework.Core.Abstractions;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }

    DateOnly Today { get; }
}
