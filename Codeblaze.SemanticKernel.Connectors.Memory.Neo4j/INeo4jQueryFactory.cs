using Microsoft.SemanticKernel;

namespace Codeblaze.SemanticKernel.Connectors.Memory.Neo4j;

public interface INeo4jQueryFactory
{
    (string, object) ListIndexQuery();
    (string, object) CreateIndexQuery(string collectionName);
    (string, object) DropIndexQuery(string collectionName);
    (string, object) GetNearestMatchQuery(string collectionName, ReadOnlyMemory<float> embedding, double minRelevanceScore, int limit = 1);
}

/// <summary>
/// Neo4j 5.15 or above is supported by this query factory, vector functionality was introduced in an earlier version of neo4j
/// If you are using an old version you may need to implement some of the queries below using old cypher, refer neo4j docs
/// </summary>
/// <param name="name">Index name</param>
/// <param name="node">Node on which index is created</param>
/// <param name="property">Property of the node on which index is created</param>
/// <param name="dimensions">Vector index dimensions</param>
public class Neo4jVectorIndexQueryFactory(string name, string node, string property, int dimensions)
    : INeo4jQueryFactory
{
    public virtual (string, object) ListIndexQuery()
    {
        return ("SHOW VECTOR INDEXES YIELD name", null);
    }

    public virtual (string, object) CreateIndexQuery(string collectionName)
    {
        if (name != collectionName) throw new KernelException($"Kernel passed {collectionName} index but query factory is configured with {name} index");
        
        return ("""
                CREATE VECTOR INDEX `$name`
                FOR (n: $node) ON (n.$property)
                OPTIONS {indexConfig: {
                 `vector.dimensions`: $dimensions,
                 `vector.similarity_function`: 'cosine'
                }
                """, new
        {
            name, node, property, dimensions
        });
    }

    public virtual (string, object) DropIndexQuery(string collectionName)
    {
        if (name != collectionName) throw new KernelException($"Kernel passed {collectionName} index but query factory is configured with {name} index");

        return ("DROP INDEX $name", new { name });
    }

    public (string, object) GetNearestMatchQuery(string collectionName, ReadOnlyMemory<float> embedding, double minRelevanceScore, int limit = 1)
    {
        if (name != collectionName) throw new KernelException($"Kernel passed {collectionName} index but query factory is configured with {name} index");
        
        return ("""
                CALL db.index.vector.queryNodes($name, $limit, $embedding)
                YIELD node, score
                WHERE score >= $minRelevanceScore
                RETURN node.id AS id, score
                """, new
        {
            name,
            limit,
            embedding = embedding.ToArray(),
            minRelevanceScore
        });
    }
}