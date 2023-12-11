using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Codeblaze.SemanticKernel.Plugins.Neo4j;

public static class Neo4jPluginBuilderExtension
{
    public static void AddNeo4jPlugin(this IServiceCollection services, string url, string username, string password)
    {
    }
}