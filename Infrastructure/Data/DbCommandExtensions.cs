using System.Data;

namespace Turismo.Infrastructure.Data;

public static class DbCommandExtensions
{
    public static void AddParameter(this IDbCommand command, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = $"$p{command.Parameters.Count}";
        parameter.Value = value ?? DBNull.Value;

        if (IsSqliteCommand(command))
        {
            command.CommandText = ReplaceFirst(command.CommandText, "?", parameter.ParameterName);
        }

        command.Parameters.Add(parameter);
    }

    public static string GetIdentityQuery(this IDbConnection connection)
    {
        return IsSqliteConnection(connection)
            ? "SELECT last_insert_rowid()"
            : "SELECT @@IDENTITY";
    }

    public static bool IsSqliteConnection(this IDbConnection connection)
    {
        return connection.GetType().FullName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static bool IsSqliteCommand(IDbCommand command)
    {
        return command.GetType().FullName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static string ReplaceFirst(string text, string search, string replacement)
    {
        var index = text.IndexOf(search, StringComparison.Ordinal);
        return index < 0
            ? text
            : string.Concat(text.AsSpan(0, index), replacement, text.AsSpan(index + search.Length));
    }
}
