using System.ComponentModel;
using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Neo4j.Driver;

namespace Codeblaze.SemanticKernel.Plugins.Neo4j;

/// <summary>
/// Cypher gen query execution result
/// </summary>
public class Neo4jResult
{
    public bool Success { get; set; }
    public string? Cypher { get; set; }
    public List<IRecord>? Result { get; set; }
}

/// <summary>
/// Cypher code gen plugin
/// </summary>
public class Neo4jCypherGenPlugin
{
    private readonly IDriver _driver;
    private readonly IChatCompletionService _chat;
    
    /// <summary>
    /// Text bases representation of schema for the current database
    /// </summary>
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
    
    /// <summary>
    /// Creates a neo4j cypher gen plugin instance
    /// </summary>
    /// <param name="chat">Chat service from semantic kernel to be used as generation backend</param>
    /// <param name="url">Neo4j url, used by the neo4j driver</param>
    /// <param name="username">Neo4j database username, used by the neo4j driver</param>
    /// <param name="password">Neo4j database password, used by the neo4j driver</param>
    public Neo4jCypherGenPlugin(IChatCompletionService chat, string url, string username, string password)
    {
        _driver = GraphDatabase.Driver(url, AuthTokens.Basic(username, password));
        _chat = chat;
        
        Schema = GetSchema().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Creates a neo4j cypher gen plugin instance
    /// </summary>
    /// <param name="chat">Chat service from semantic kernel to be used as generation backend</param>
    /// <param name="driver">Neo4j driver to be used for executing cypher</param>
    public Neo4jCypherGenPlugin(IChatCompletionService chat, IDriver driver)
    {
        _driver = driver;
        _chat = chat;
        
        Schema = GetSchema().GetAwaiter().GetResult();
    }
    
    /// <summary>
    /// SK Function to generate cypher, execute it and return the result
    /// </summary>
    /// <param name="prompt">prompt against which cypher is to be generated</param>
    /// <returns>Result containing, cypher and cypher execution result</returns>
    [KernelFunction, Description("Generates cypher code based on prompt and queries the database")]
    public async Task<Neo4jResult> Query(string prompt)
    {
        var cypher = await GenerateCypher(prompt);

        try
        {
            var result = await NeoQuery(cypher);

            return new Neo4jResult
            {
                Success = true,
                Cypher = cypher,
                Result = result
            };
        }
        catch
        {
            return new Neo4jResult { Success = false, Cypher = cypher };
        }
    }
    
    /// <summary>
    /// SK Function to generate cypher based on the prompt
    /// </summary>
    /// <param name="prompt">prompt against which cypher is to be generated</param>
    /// <returns>Generated cypher</returns>
    [KernelFunction, Description("Generates cypher code based on prompt")]
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
    
    private async Task<string> GetSchema()
    {
        var nodeProps = JsonSerializer.Serialize((await NeoQuery(NODE_PROPS_QUERY)).Select(x => x.Values["output"]).ToList());
        var relationProps = JsonSerializer.Serialize((await NeoQuery(REL_PROPS_QUERY)).Select(x => x.Values["output"]).ToList());
        var relations = JsonSerializer.Serialize((await NeoQuery(REL_QUERY)).Select(x => x.Values["output"]).ToList());
        
        return $"""
                 This is the schema representation of the Neo4j database.
                 Node properties are the following:
                 {nodeProps}
                 Relationship properties are the following:
                 {relationProps}
                 Relationship point from source to target nodes
                 {relations}
                 Make sure to respect relationship types and directions
                 """;
    }
    
    private async Task<List<IRecord>> NeoQuery(string query)
    {
        await using var session = _driver.AsyncSession(o => o.WithDatabase("neo4j"));
        
        return await session.ExecuteReadAsync(async transaction =>
        {
            var cursor = await transaction.RunAsync(query);

            return await cursor.ToListAsync();
        });
    }
}