using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Neo4j.Driver;

namespace Codeblaze.SemanticKernel.Plugins.Neo4j;

public static class Neo4jPluginBuilderExtension
{
    public static void AddNeo4jCypherGenPlugin(this Kernel kernel, string url, string username, string password)
    {
        var chat = kernel.GetRequiredService<IChatCompletionService>();
        
        var plugin = new Neo4jCypherGenPlugin(chat, url, username, password);

        kernel.ImportPluginFromObject(plugin, nameof(Neo4jCypherGenPlugin));
    }
    
    public static void AddNeo4jCypherGenPlugin(this Kernel kernel, IDriver driver)
    {
        var chat = kernel.GetRequiredService<IChatCompletionService>();
        
        var plugin = new Neo4jCypherGenPlugin(chat, driver);

        kernel.ImportPluginFromObject(plugin, nameof(Neo4jCypherGenPlugin));
    }
}