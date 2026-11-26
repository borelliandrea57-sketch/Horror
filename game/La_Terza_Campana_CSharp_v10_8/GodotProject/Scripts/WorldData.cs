using System;
using System.Collections.Generic;
using System.Text.Json;

public sealed class WorldMap
{
    public string Name { get; set; } = "";
    public int W { get; set; }
    public int H { get; set; }
    public string Style { get; set; } = "";
    public List<double[]> Blocks { get; set; } = new();
    public List<List<double[]>> Polygons { get; set; } = new();
    public List<double[]> SceneryBlocks { get; set; } = new();
    public List<WorldDoor> Doors { get; set; } = new();
    public List<WorldNpc> Npcs { get; set; } = new();
    public List<WorldProp> Props { get; set; } = new();
    public List<WorldEnemy> Enemies { get; set; } = new();
}
public sealed class WorldDoor
{
    public string Id { get; set; } = "";
    public int X { get; set; }
    public int Y { get; set; }
    public string Label { get; set; } = "";
    public string To { get; set; } = "";
    public double[] Spawn { get; set; } = new double[]{10,14};
    public double[]? InteractArea { get; set; }
    public int Hp { get; set; }
    public int Armor { get; set; }
}
public sealed class WorldNpc
{
    public string Id { get; set; } = "";
    public int X { get; set; }
    public int Y { get; set; }
    public string Name { get; set; } = "";
    public string Portrait { get; set; } = "";
}
public sealed class WorldProp
{
    public string Id { get; set; } = "";
    public double X { get; set; }
    public double Y { get; set; }
    public string Name { get; set; } = "";
    public string Note { get; set; } = "";
    public string Kind { get; set; } = "";
    public string Art { get; set; } = "";
    public string ArtFile { get; set; } = "";
    public double Scale { get; set; } = 1.0;
    public bool PaintOnly { get; set; }
    public double[]? Solid { get; set; }
    public double[]? Footprint { get; set; }
    public double[]? InteractArea { get; set; }
}
public sealed class WorldEnemy
{
    public string Id { get; set; } = "";
    public string Kind { get; set; } = "stitch";
    public int X { get; set; }
    public int Y { get; set; }
}

public static class WorldCatalog
{
    public static Dictionary<string,WorldMap> Read(string json) =>
        JsonSerializer.Deserialize<Dictionary<string,WorldMap>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
        ?? throw new InvalidOperationException("Archivio mappe assente o danneggiato.");
}
