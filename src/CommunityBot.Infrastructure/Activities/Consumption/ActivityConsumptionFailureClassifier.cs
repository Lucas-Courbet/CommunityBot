using Npgsql;

namespace CommunityBot.Infrastructure.Activities.Consumption;

internal static class ActivityConsumptionFailureClassifier
{
    public static bool IsRetryable(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is TimeoutException)
                return true;

            if (current is PostgresException postgresException &&
                postgresException.SqlState is
                    PostgresErrorCodes.SerializationFailure or
                    PostgresErrorCodes.DeadlockDetected)
            {
                return true;
            }

            if (current is NpgsqlException { IsTransient: true })
                return true;
        }

        return false;
    }

    public static string Describe(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is PostgresException postgresException)
            {
                return
                    $"{exception.GetType().Name}: PostgreSQL " +
                    $"{postgresException.SqlState} - {postgresException.MessageText}";
            }
        }

        return $"{exception.GetType().Name}: {exception.Message}";
    }
}