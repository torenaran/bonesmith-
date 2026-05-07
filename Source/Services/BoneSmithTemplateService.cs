using System.Text.Json;
using BoneSmith.Models;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace BoneSmith.Services;

public sealed class BoneSmithTemplateService
{
    private readonly IDalamudPluginInterface pluginInterface;
    private readonly IPluginLog log;

    private readonly JsonSerializerOptions jsonOptions = new()
    {
        WriteIndented = true,
    };

    public List<BoneSmithTemplate> Templates { get; private set; } = [];

    private string TemplateDirectory =>
        Path.Combine(pluginInterface.ConfigDirectory.FullName, "BoneSmithTemplates");

    public BoneSmithTemplateService(IDalamudPluginInterface pluginInterface, IPluginLog log)
    {
        this.pluginInterface = pluginInterface;
        this.log = log;

        Directory.CreateDirectory(TemplateDirectory);
        LoadAll();
    }

    public void LoadAll()
    {
        Templates.Clear();
        Directory.CreateDirectory(TemplateDirectory);

        foreach (var file in Directory.GetFiles(TemplateDirectory, "*.json"))
        {
            try
            {
                var json = File.ReadAllText(file);
                var template = JsonSerializer.Deserialize<BoneSmithTemplate>(json, jsonOptions);

                if (template is not null)
                    Templates.Add(template);
            }
            catch (Exception ex)
            {
                log.Warning(ex, "Failed to load BoneSmith template {File}", file);
            }
        }

        Templates = Templates
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public BoneSmithTemplate CreateNew(string name)
    {
        var template = new BoneSmithTemplate
        {
            Name = string.IsNullOrWhiteSpace(name) ? "New BoneSmith Template" : name.Trim(),
        };

        Templates.Add(template);
        Save(template);

        return template;
    }

    public void Save(BoneSmithTemplate template)
    {
        Directory.CreateDirectory(TemplateDirectory);

        template.UpdatedAt = DateTimeOffset.Now;

        var path = GetPath(template);
        var json = JsonSerializer.Serialize(template, jsonOptions);

        File.WriteAllText(path, json);
        LoadAll();
    }

    public void Delete(BoneSmithTemplate template)
    {
        var path = GetPath(template);

        if (File.Exists(path))
            File.Delete(path);

        LoadAll();
    }

    private string GetPath(BoneSmithTemplate template)
    {
        return Path.Combine(TemplateDirectory, $"{template.UniqueId}.json");
    }
}