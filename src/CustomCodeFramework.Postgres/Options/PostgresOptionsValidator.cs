using Microsoft.Extensions.Options;

namespace CustomCodeFramework.Postgres.Options;

public sealed class PostgresOptionsValidator : IValidateOptions<PostgresOptions>
{
    public ValidateOptionsResult Validate(string? name, PostgresOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            return ValidateOptionsResult.Fail("Postgres connection string is required.");
        }

        if (options.CommandTimeoutSeconds <= 0)
        {
            return ValidateOptionsResult.Fail(
                "Postgres command timeout must be greater than zero."
            );
        }

        return ValidateOptionsResult.Success;
    }
}
