using Microsoft.Azure.Cosmos;

namespace Incepted.Db.Repos;

internal static class RepoUtils
{
    public static async Task<(IEnumerable<T> values, double ruTotal)> GetResultsAsync<T>(this Container container, string query)
    {
        using FeedIterator<T> feed = container.GetItemQueryIterator<T>(query);

        double ruTotal = 0;
        var result = new List<T>();
        while (feed.HasMoreResults)
        {
            FeedResponse<T> feedResponse = await feed.ReadNextAsync();
            result.AddRange(feedResponse.Resource);
            ruTotal += feedResponse.RequestCharge;
        }

        return (result, ruTotal);
    }
}
