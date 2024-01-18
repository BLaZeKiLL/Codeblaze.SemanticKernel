using Microsoft.SemanticKernel.Memory;
using Neo4j.Driver;

namespace Codeblaze.SemanticKernel.Connectors.Memory.Neo4j;

/// <summary>
/// Collection in Neo4j context refers to index
/// </summary>
#pragma warning disable SKEXP0003
public class Neo4jMemoryStore : IMemoryStore, IDisposable
{
    private readonly IDriver _driver;
    private readonly INeo4jQueryFactory _queryFactory;

    public Neo4jMemoryStore(string url, string username, string password, INeo4jQueryFactory queryFactory)
    {
        _driver = GraphDatabase.Driver(url, AuthTokens.Basic(username, password));
        _queryFactory = queryFactory;
    }

    public async Task CreateCollectionAsync(string collectionName,
        CancellationToken cancellationToken = new CancellationToken())
    {
        if (await DoesCollectionExistAsync(collectionName, cancellationToken)) return;

        var (query, props) = _queryFactory.CreateIndexQuery(collectionName);

        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync(query, props).ConfigureAwait(false);

        await cursor.ConsumeAsync().ConfigureAwait(false);
    }

    public async IAsyncEnumerable<string> GetCollectionsAsync(
        CancellationToken cancellationToken = new CancellationToken())
    {
        var indexes = await GetIndexesAsync().ConfigureAwait(false);

        foreach (var record in indexes)
        {
            yield return record.As<string>();
        }
    }

    public async Task<bool> DoesCollectionExistAsync(string collectionName,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var indexes = await GetIndexesAsync().ConfigureAwait(false);

        return indexes.Contains(collectionName);
    }

    public async Task DeleteCollectionAsync(string collectionName,
        CancellationToken cancellationToken = new CancellationToken())
    {
        if (await DoesCollectionExistAsync(collectionName, cancellationToken)) return;

        var (query, props) = _queryFactory.DropIndexQuery(collectionName);

        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync(query, props).ConfigureAwait(false);

        await cursor.ConsumeAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="collectionName"></param>
    /// <param name="record"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<string> UpsertAsync(string collectionName, MemoryRecord record,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var (query, props) = _queryFactory.UpsertQuery(collectionName, record.Key, record.Embedding);

        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync(query, props).ConfigureAwait(false);

        var result = await cursor.SingleAsync().ConfigureAwait(false);

        return result["id"].As<string>();
    }

    public async IAsyncEnumerable<string> UpsertBatchAsync(string collectionName, IEnumerable<MemoryRecord> records,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var (query, props) = _queryFactory.UpsertBatchQuery(
            collectionName, 
            records.Select(x => x.Key), 
            records.Select(x => x.Embedding)
        );

        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync(query, props).ConfigureAwait(false);

        var results = await cursor.ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        foreach (var result in results)
        {
            yield return result["id"].As<string>();
        }
    }

    public async Task<MemoryRecord?> GetAsync(string collectionName, string key, bool withEmbedding = false,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var (query, props) = _queryFactory.GetQuery(collectionName, key);

        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync(query, props).ConfigureAwait(false);

        var record = await cursor.SingleAsync().ConfigureAwait(false);

        var id = record["id"].As<string>();
        var embedding = withEmbedding
            ? new ReadOnlyMemory<float>(record[_queryFactory.IndexProperty].As<float[]>())
            : ReadOnlyMemory<float>.Empty;

        return new MemoryRecord(
            new MemoryRecordMetadata(
                true, 
                id,
                record[_queryFactory.TextProperty].As<string>(), 
                "", "neo4j", 
                ""
            ),
            embedding, 
            id
        );
    }

    public async IAsyncEnumerable<MemoryRecord> GetBatchAsync(string collectionName, IEnumerable<string> keys,
        bool withEmbeddings = false,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var (query, props) = _queryFactory.GetBatchQuery(collectionName, keys);

        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync(query, props).ConfigureAwait(false);

        var result = await cursor.ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        foreach (var record in result)
        {
            var id = record["id"].As<string>();
            var embedding = withEmbeddings
                ? new ReadOnlyMemory<float>(record[_queryFactory.IndexProperty].As<float[]>())
                : ReadOnlyMemory<float>.Empty;

            yield return new MemoryRecord(
                new MemoryRecordMetadata(
                    true, 
                    id,
                    record[_queryFactory.TextProperty].As<string>(), 
                    "", "neo4j", 
                    ""
                ),
                embedding, 
                id
            );
        }
    }

    public async Task RemoveAsync(string collectionName, string key,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var (query, props) = _queryFactory.RemoveQuery(collectionName, key);

        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync(query, props).ConfigureAwait(false);

        await cursor.ConsumeAsync();
    }

    public async Task RemoveBatchAsync(string collectionName, IEnumerable<string> keys,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var (query, props) = _queryFactory.RemoveBatchQuery(collectionName, keys);

        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync(query, props).ConfigureAwait(false);

        await cursor.ConsumeAsync();
    }

    public async IAsyncEnumerable<(MemoryRecord, double)> GetNearestMatchesAsync(string collectionName,
        ReadOnlyMemory<float> embedding, int limit,
        double minRelevanceScore = 0, bool withEmbeddings = false,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var (query, props) = _queryFactory.GetNearestMatchQuery(collectionName, embedding, minRelevanceScore, limit);

        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync(query, props).ConfigureAwait(false);

        // Should use Async Linq and stream
        var result = await cursor.ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        foreach (var record in result)
        {
            var score = record["score"].As<double>();
            var id = record["id"].As<string>();
            var text = record["text"].As<string>();

            yield return (
                new MemoryRecord(
                    new MemoryRecordMetadata(true, id, text, "", "neo4j", ""), embedding, id
                ),
                score);
        }
    }

    public async Task<(MemoryRecord, double)?> GetNearestMatchAsync(string collectionName,
        ReadOnlyMemory<float> embedding,
        double minRelevanceScore = 0,
        bool withEmbedding = false, CancellationToken cancellationToken = new CancellationToken())
    {
        var (query, props) = _queryFactory.GetNearestMatchQuery(collectionName, embedding, minRelevanceScore);

        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync(query, props).ConfigureAwait(false);

        var result = await cursor.SingleAsync().ConfigureAwait(false);

        var score = result["score"].As<double>();
        var id = result["id"].As<string>();

        return (new MemoryRecord(new MemoryRecordMetadata(true, id, "", "", "neo4j", ""), embedding, id), score);
    }

    public void Dispose()
    {
        _driver.Dispose();
    }

    private async Task<List<string>> GetIndexesAsync()
    {
        var (query, _) = _queryFactory.ListIndexQuery();

        await using var session = _driver.AsyncSession();

        var cursor = await session.RunAsync(query).ConfigureAwait(false);

        return await cursor.ToListAsync(x => x.As<string>()).ConfigureAwait(false);
    }
}
#pragma warning enable SKEXP0003