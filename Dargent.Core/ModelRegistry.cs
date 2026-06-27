namespace Dargent.Core;

public class ModelRegistry(string agentDirectory)
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private Model[] LoadModels()
    {
        var modelsFilePath = Path.Combine(agentDirectory, "models.json");
        if (!File.Exists(modelsFilePath))
            return [];

        var json = File.ReadAllText(modelsFilePath);
        return JsonSerializer.Deserialize<ModelList>(json, _jsonSerializerOptions)?.Models ?? [];
    }
    
    public Model GetDefaultModel()
    {
        return LoadModels().First();
    }
}

public class ModelList
{
    public required Model[] Models { get; set; }
}

public class Model
{
    public required string Name { get; set; }
    public required string ApiKey { get; set; }
    public required string GatewayUrl { get; set; }
}