using Microsoft.Extensions.DependencyInjection;

namespace Codeblaze.SemanticKernel.Plugins.Neo4j;

// Extend from IKernelBuilderPlugins
public static class Neo4jPluginBuilderExtension
{
    public static void AddNeo4jPlugin(this IServiceCollection services, string url, string username, string password)
    {
        
    }
}