using System.Text;
using Confluent.Kafka;

namespace AutoSalonPet.Services;

public static class KafkaHeaders
{
    public const string RetryCount = "retry-count";
    public const string Error = "error";

    public static int GetRetryCount(Headers? headers)
    {
        var header = headers?.FirstOrDefault(h => h.Key == RetryCount);

        if (header is null)
        {
            return 0;
        }

        return int.TryParse(Encoding.UTF8.GetString(header.GetValueBytes()), out var value) ? value : 0;
    }

    public static Headers CreateHeaders(int retryCount, string? error = null)
    {
        var headers = new Headers();
        headers.Add(RetryCount, Encoding.UTF8.GetBytes(retryCount.ToString()));

        if (!string.IsNullOrEmpty(error))
        {
            headers.Add(Error, Encoding.UTF8.GetBytes(error));
        }

        return headers;
    }
}
