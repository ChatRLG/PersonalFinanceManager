using System.Globalization;
using System.IO;
using System.Text;
using PersonalFinanceManager.Application.Contracts.Transactions;

namespace PersonalFinanceManager.Desktop.Services;

public interface ICsvService
{
    void ExportTransactions(IEnumerable<TransactionDto> transactions, string filePath);
    List<CreateTransactionRequest> ImportTransactions(string filePath);
}

/// <summary>
/// Simple delimiter-based CSV import/export for transactions.
/// No external library dependency — keeps the feed constraint clean.
/// Columns: Id, Date, Type, Amount, Currency, Description, AccountId, CategoryName, Notes
/// </summary>
public class CsvService : ICsvService
{
    public void ExportTransactions(IEnumerable<TransactionDto> transactions, string filePath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id,Date,Type,Amount,Currency,Description,AccountId,AccountName,CategoryId,CategoryName,Notes");

        foreach (var t in transactions)
        {
            sb.Append(t.Id).Append(',')
              .Append(t.TransactionDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)).Append(',')
              .Append(t.Type).Append(',')
              .Append(t.Amount.ToString(CultureInfo.InvariantCulture)).Append(',')
              .Append(t.Currency).Append(',')
              .Append(EscapeCsv(t.Description)).Append(',')
              .Append(t.AccountId).Append(',')
              .Append(EscapeCsv(t.AccountName ?? string.Empty)).Append(',')
              .Append(t.CategoryId).Append(',')
              .Append(EscapeCsv(t.CategoryName ?? string.Empty)).Append(',')
              .AppendLine(EscapeCsv(t.Notes ?? string.Empty));
        }

        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
    }

    /// <summary>
    /// Imports transactions from a CSV file.
    /// Expected columns (header row required):
    ///   Date (yyyy-MM-dd), Type, Amount, Currency, Description, AccountId, CategoryId, Notes (optional)
    /// </summary>
    public List<CreateTransactionRequest> ImportTransactions(string filePath)
    {
        var lines = File.ReadAllLines(filePath, Encoding.UTF8);
        if (lines.Length < 2) return new();

        // Parse header to find column indices.
        var headers = lines[0].Split(',').Select(h => h.Trim().ToLowerInvariant()).ToArray();
        int Idx(string name) => Array.IndexOf(headers, name);

        int idxDate = Idx("date"), idxType = Idx("type"), idxAmount = Idx("amount"),
            idxCurrency = Idx("currency"), idxDesc = Idx("description"),
            idxAccount = Idx("accountid"), idxCategory = Idx("categoryid"), idxNotes = Idx("notes");

        var result = new List<CreateTransactionRequest>();
        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var cols = SplitCsv(line);
            if (cols.Length < 6) continue;

            T Get<T>(int idx, Func<string, T> parse, T fallback)
            {
                if (idx < 0 || idx >= cols.Length) return fallback;
                try { return parse(cols[idx].Trim()); } catch { return fallback; }
            }

            result.Add(new CreateTransactionRequest
            {
                TransactionDate = Get(idxDate, s => DateTime.Parse(s, CultureInfo.InvariantCulture), DateTime.Today),
                Type = Get(idxType, s => s, "Expense"),
                Amount = Get(idxAmount, s => decimal.Parse(s, CultureInfo.InvariantCulture), 0m),
                Currency = Get(idxCurrency, s => s, "USD"),
                Description = Get(idxDesc, s => s, "Imported"),
                AccountId = Get(idxAccount, s => Guid.Parse(s), Guid.Empty),
                CategoryId = Get(idxCategory, s => Guid.Parse(s), Guid.Empty),
                Notes = idxNotes >= 0 && idxNotes < cols.Length ? cols[idxNotes].Trim() : null
            });
        }

        return result;
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }

    private static string[] SplitCsv(string line)
    {
        // Minimal RFC 4180 splitter (handles quoted fields).
        var result = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (inQuotes)
            {
                if (c == '"' && i + 1 < line.Length && line[i + 1] == '"') { current.Append('"'); i++; }
                else if (c == '"') inQuotes = false;
                else current.Append(c);
            }
            else if (c == '"') inQuotes = true;
            else if (c == ',') { result.Add(current.ToString()); current.Clear(); }
            else current.Append(c);
        }
        result.Add(current.ToString());
        return result.ToArray();
    }
}
