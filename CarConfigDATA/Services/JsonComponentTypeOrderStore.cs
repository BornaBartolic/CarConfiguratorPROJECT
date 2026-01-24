using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace CarConfigDATA.Services;

public class JsonComponentTypeOrderStore : IComponentTypeOrderStore
{
    private readonly IWebHostEnvironment _env;
    private readonly object _lock = new();

  
    public JsonComponentTypeOrderStore(IWebHostEnvironment env)
    {
        _env = env;
        // da se sazna gdje je root projekta 
    }

    private string FilePath =>
        Path.Combine(_env.ContentRootPath, "App_Data", "componentTypeOrder.json");

    public Task<List<int>> GetOrderAsync()
    {
        lock (_lock)
        {
            if (!File.Exists(FilePath))
                return Task.FromResult(new List<int>());

            var json = File.ReadAllText(FilePath);
            if (string.IsNullOrWhiteSpace(json))
                return Task.FromResult(new List<int>());

            try
            {
                return Task.FromResult(JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>());
            }
            catch
            {
                return Task.FromResult(new List<int>());
            }
        }
    }

    public Task SaveOrderAsync(List<int> orderedTypeIds)
    {
        lock (_lock)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);

            var json = JsonSerializer.Serialize(
                orderedTypeIds.Distinct().ToList(),
                new JsonSerializerOptions { WriteIndented = true }
            );

            File.WriteAllText(FilePath, json);
            return Task.CompletedTask;
        }
    }
}
