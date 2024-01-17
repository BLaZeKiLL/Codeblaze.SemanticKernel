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

    public async IAsyncEnumerable<string> GetCollectionsAsync(CancellationToken cancellationToken = new CancellationToken())
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

    // update embeddings
    public Task<string> UpsertAsync(string collectionName, MemoryRecord record,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<string> UpsertBatchAsync(string collectionName, IEnumerable<MemoryRecord> records,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task<MemoryRecord?> GetAsync(string collectionName, string key, bool withEmbedding = false,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<MemoryRecord> GetBatchAsync(string collectionName, IEnumerable<string> keys,
        bool withEmbeddings = false,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task RemoveAsync(string collectionName, string key,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task RemoveBatchAsync(string collectionName, IEnumerable<string> keys,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
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
            
            yield return (new MemoryRecord(new MemoryRecordMetadata(true, id, "", "", "neo4j", ""), embedding, id), score);
        }
    }

    public async Task<(MemoryRecord, double)?> GetNearestMatchAsync(string collectionName, ReadOnlyMemory<float> embedding,
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