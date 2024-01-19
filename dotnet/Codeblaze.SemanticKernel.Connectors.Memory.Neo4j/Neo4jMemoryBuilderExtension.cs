using Microsoft.SemanticKernel.Memory;

namespace Codeblaze.SemanticKernel.Connectors.Memory.Neo4j;

#pragma warning disable SKEXP0003
public static class Neo4jMemoryBuilderExtension
{
    public static MemoryBuilder WithNeo4jMemoryStore(this MemoryBuilder builder, string url, string username, string password, INeo4jQueryFactory queryFactory)
    {
        builder.WithMemoryStore(_ => new Neo4jMemoryStore(url, username, password, queryFactory));
        
        return builder;
    }
}
#pragma warning enable SKEXP0003