# Neo4j Memory Store
Supports
- Customizable queries
- Runtime properties
- Vector embedding Update, Get, Remove and Search
- TextMemoryPlugin support see [example 15](https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/KernelSyntaxExamples/Example15_TextMemoryPlugin.cs) of Semantic Kernel

> :warning: **Kernel Memory**: Is experimental in the semantic kernel.

### Quick Start
- Install from [nuget](https://www.nuget.org/packages/Codeblaze.SemanticKernel.Connectors.Memory.Neo4j)
    ```
    dotnet add package Codeblaze.SemanticKernel.Connectors.Memory.Neo4j
    ```
- Configure the memory store
  ```csharp
  var builder = new MemoryBuilder();

  builder.WithHttpClient(new HttpClient());
  builder.WithOllamaTextEmbeddingGeneration(config["Ollama:Model"], config["Ollama:BaseUrlEmbeddings"]);
  builder.WithNeo4jMemoryStore(config["Neo4j:Url"], config["Neo4j:Username"], config["Neo4j:Password"],
      new Neo4jVectorIndexQueryFactory(
          "<index-name>",
          "<node-name>",
          "<index-property-name>",
          "<text-property-name>",
          384 // Dimensions
      )
  );

  memory = builder.Build();
  ```
- Store embedding
  ```csharp
  memory.SaveReferenceAsync(
      collection: "<index-name>",
      externalSourceName: "<node-name>",
      externalId: "<node-id>",
      description: "not used",
      text: "<actual-text>"
  );
  ```
- Search embedding
  ```csharp
  var results = memory.SearchAsync("<index-name>", "<search-text>", limit: 3);
  ```
- Retrieve node
  ```csharp
  var results = await memory.GetAsync("<index-name>", "<node-id>");
  ```

### Query Factory
You can implement you own `INeo4jQueryFactory` or Extend `Neo4jVectorIndexQueryFactory` to modify
any of the queries (Default queries found in `Neo4jVectorQueries`).