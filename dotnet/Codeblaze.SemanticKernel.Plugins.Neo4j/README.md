# Neo4j Cypher Codegen
This package provides semantic kernel plugin for Neo4j cyher codegen support, Enabling talking to knowledge graphs respecting it's schema

### Important
- Plugin has been tested with gpt-3.5-turbo, other models may require tweaks to the prompts
- Make sure your neo4j instance has the [apoc plugin](https://neo4j.com/docs/apoc/current/)
- On initialization, the plugin queries the database to generate a text representation of schema

### Quick Start
- Install from [nuget](https://www.nuget.org/packages/Codeblaze.SemanticKernel.Plugins.Neo4j)
    ```
    dotnet add package Codeblaze.SemanticKernel.Plugins.Neo4j
    ```
- Configure the kernel
    ```csharp
    var builder = Kernel.CreateBuilder();
    
    builder.AddOpenAIChatCompletion(
        config["OpenAI:Model"], 
        config["OpenAI:Key"]
    );
        
    var kernel = builder.Build();
        
    kernel.AddNeo4jCypherGenPlugin(
        config["Neo4j:Url"], 
        config["Neo4j:Username"], 
        config["Neo4j:Password"]
    );
    ```
- Query the database using prompts
    ```csharp
    var prompt = "Which movies came out in 1990?";
  
    var result = await _Kernel.InvokeAsync<Neo4jResult>(
        nameof(Neo4jCypherGenPlugin), "Query", 
        new KernelArguments
        {
            { "prompt", prompt }
        }
    );
  
    result.Cypher; // Generated cypher (string)
    result.Result; // Neo4j query execution result (List<IRecord>)
    ```

### Semantic Kernel Functions
- Query
  - **description**
    
    Generates cypher code based on the provided prompt, executes it against the configured neo4j instance
    returns both cypher and the prompt
  - **usage**
    ```csharp
      _Kernel.InvokeAsync<Neo4jResult>(
          nameof(Neo4jCypherGenPlugin), "Query", 
          new KernelArguments
          {
              { "prompt", prompt }
          }
      );
    ```

- GenerateCypher
  - **description**

    Generates cypher code based on the provided prompt
  - **usage**
    ```csharp
        _Kernel.InvokeAsync<string>(
            nameof(Neo4jCypherGenPlugin), "GenerateCypher", 
            new KernelArguments
            {
                { "prompt", prompt }
            }
        );
    ```
