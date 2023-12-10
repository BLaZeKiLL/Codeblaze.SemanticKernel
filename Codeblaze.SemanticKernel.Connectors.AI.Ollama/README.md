# Ollama Connector
<p align="center">
    <a href="https://www.nuget.org/packages/Codeblaze.SemanticKernel.Connectors.AI.Ollama">
        <img alt="Nuget" src="https://img.shields.io/nuget/v/Codeblaze.SemanticKernel.Connectors.AI.Ollama?label=ollama">
    </a>
    <a href="https://github.com/BLaZeKiLL/Codeblaze.SemanticKernel/actions/workflows/build.yml">
        <img alt="GitHub Workflow Status (with event)" src="https://img.shields.io/github/actions/workflow/status/BLaZeKiLL/Codeblaze.SemanticKernel/build.yml">
    </a>
    <a href="https://github.com/BLaZeKiLL/Codeblaze.SemanticKernel/blob/main/LICENSE.md">
        <img alt="GitHub" src="https://img.shields.io/github/license/BLaZeKiLL/Codeblaze.SemanticKernel">
    </a>
    <a href="https://www.youtube.com/c/CodeBlazeX">
        <img alt="YouTube Channel Subscribers" src="https://img.shields.io/youtube/channel/subscribers/UC_qfPIYfXOvg0SDAc8Z68WA?label=CodeBlaze&style=social">
    </a>
</p>

Supports
- text generation
- chat completion
- embedding generation

> :warning: **Embedding generation**: Is experimental in the semantic kernel.

### Quick Start
- Text Generation

    Configure the kernel
    ```cs
    var builder = new KernelBuilder();

    // provide the HTTP client used to interact with Ollama API
    builder.Services.AddTransient<HttpClient>();

    builder.AddOllamaTextGeneration(
        config["Ollama:Model"], // Ollama model Id
        config["Ollama:BaseUrlGeneration"] // Ollama endpoint
    );

    var kernel = builder.Build();
    ```

    Usage
    ```cs
    const string prompt = """
    Bot: How can I help you?
    User: {{$input}}
                        
    ---------------------------------------------
                        
    The intent of the user in 5 words or less:
    """;
    
    var result = await kernel.InvokePromptAsync(prompt, new KernelArguments
    {
        {"input", input}
    });

    System.Console.WriteLine(result.GetValue<string>());
    ```

- Chat Completion

    Configure the kernel
    ```cs
    var builder = new KernelBuilder();

    // provide the HTTP client used to interact with Ollama API
    builder.Services.AddTransient<HttpClient>();

    builder.AddOllamaChatCompletion(
        config["Ollama:Model"], // Ollama model Id
        config["Ollama:BaseUrlGeneration"] // Ollama endpoint
    );

    var kernel = builder.Build();
    ```

    Usage
    ```cs
    var chat = _Kernel.GetRequiredService<IChatCompletionService>();

    var history = new ChatHistory();
    
    // add messages to current chat history as required
    history.AddSystemMessage("...");
    history.AddAssistantMessage("...");
    history.AddUserMessage(input);

    // result is a list of all chat messages 
    // including the output of current prompt
    var result = await chat.GetChatMessageContentsAsync(history);

    // Print the last message
    System.Console.WriteLine(result[^1].Content);
    ```

- Embedding Generation (Experimental)

    Configure the kernel
    ```cs
    var builder = new KernelBuilder();

    // provide the HTTP client used to interact with Ollama API
    builder.Services.AddTransient<HttpClient>();

    builder.AddOllamaTextEmbeddingGeneration(
        config["Ollama:Model"], // Ollama model Id
        config["Ollama:BaseUrlGeneration"] // Ollama endpoint
    );

    // Configure memory backend (e.g Azure Cognitive Search)

    var kernel = builder.Build();
    ```

    Usage
    ```cs
    var memory = _Kernel.GetRequiredService<ISemanticTextMemory>();

    // This will internally call Ollama embedding service to generate embeddings
    memory.SaveReferenceAsync(
        collection: "collection",
        externalSourceName: "ext-collection",
        externalId: id, // reference id (database entity id)
        description: input,
        text: input
    );
    ```