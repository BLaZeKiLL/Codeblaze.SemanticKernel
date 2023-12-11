using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Neo4j.Driver;

namespace Codeblaze.SemanticKernel.Plugins.Neo4j;

public class NeoResult
{
    public bool Success { get; set; }
    public string? Cypher { get; set; }
    public List<IRecord>? Result { get; set; }
}

public class Neo4jPlugin
{
    private readonly IDriver _driver;
    private readonly IChatCompletionService _chat;
    
    public string Schema { get; }

    private const string NODE_PROPS_QUERY = """
                                            CALL apoc.meta.data()
                                            YIELD label, other, elementType, type, property
                                            WHERE NOT type = "RELATIONSHIP" AND elementType = "node"
                                            WITH label AS nodeLabels, collect(property) AS properties
                                            RETURN {labels: nodeLabels, properties: properties} AS output
                                            """;

    private const string REL_PROPS_QUERY = """
                                           CALL apoc.meta.data()
                                           YIELD label, other, elementType, type, property
                                           WHERE NOT type = "RELATIONSHIP" AND elementType = "relationship"
                                           WITH label AS nodeLabels, collect(property) AS properties
                                           RETURN {type: nodeLabels, properties: properties} AS output
                                           """;

    private const string REL_QUERY = """
                                     CALL apoc.meta.data()
                                     YIELD label, other, elementType, type, property
                                     WHERE type = "RELATIONSHIP" AND elementType = "node"
                                     RETURN {source: label, relationship: property, target: other} AS output
                                     """;
    
    public Neo4jPlugin(Kernel kernel, string url, string username, string password)
    {
        _driver = GraphDatabase.Driver(url, AuthTokens.Basic(username, password));
        _chat = kernel.GetRequiredService<IChatCompletionService>();
        
        Schema = GetSchema().GetAwaiter().GetResult();
    }
    
    private async Task<string> GetSchema()
    {
        
        var node_props = JsonSerializer.Serialize((await Query(NODE_PROPS_QUERY)).Select(x => x.Values["output"]).ToList());
        var rel_props = JsonSerializer.Serialize((await Query(REL_PROPS_QUERY)).Select(x => x.Values["output"]).ToList());
        var rels = JsonSerializer.Serialize((await Query(REL_QUERY)).Select(x => x.Values["output"]).ToList());
        
        return $"""
                 This is the schema representation of the Neo4j database.
                 Node properties are the following:
                 {node_props}
                 Relationship properties are the following:
                 {rel_props}
                 Relationship point from source to target nodes
                 {rels}
                 Make sure to respect relationship types and directions
                 """;
    }

    public async Task<string> GenerateCypher(string prompt)
    {
        var system = $"""
                       Task: Generate Cypher queries to query a Neo4j graph database based on the provided schema definition.
                       Instructions:
                       Use only the provided relationship types and properties.
                       Do not use any other relationship types or properties that are not provided.
                       If you cannot generate a Cypher statement based on the provided schema, explain the reason to the user.
                       Schema:
                       {Schema}
                       
                       Note: Do not include any explanations or apologies in your responses.
                       """;

        var history = new ChatHistory
        {
            new(AuthorRole.System, system),
            new(AuthorRole.User, prompt)
        };

        var result = await _chat.GetChatMessageContentsAsync(history);
        
        return result[0].Content;
    }

    public async Task<NeoResult> Run(string prompt)
    {
        var cypher = await GenerateCypher(prompt);

        try
        {
            var result = await Query(cypher);

            return new NeoResult
            {
                Success = true,
                Cypher = cypher,
                Result = result
            };
        }
        catch
        {
            return new NeoResult { Success = false, Cypher = cypher };
        }
    }
    
    private async Task<List<IRecord>> Query(string query)
    {
        await using var session = _driver.AsyncSession(o => o.WithDatabase("neo4j"));
        
        return await session.ExecuteReadAsync(async transaction =>
        {
            var cursor = await transaction.RunAsync(query);

            return await cursor.ToListAsync();
        });
    }
    
    
}