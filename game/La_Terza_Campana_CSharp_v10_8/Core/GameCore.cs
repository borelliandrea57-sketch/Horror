using System.Text.Json;
using System.Text.Json.Serialization;

namespace LaTerzaCampana.Core;

public enum HeroId { Gladia, Ambrosh, Roderick, Anatema }
public enum Difficulty { Facile, Sopravvivenza, DeSade }
public enum GearSlot { Arma, Corpo, Testa, Accessorio }

public sealed record HeroDefinition(string Name, int StartingBody, int StartingMind, int AttackBonus,
    string Description, IReadOnlyList<string> Skills);
public sealed record DifficultyModifiers(double HungerRate, double EnemyAttack, double EnemyDefense);
public sealed record GearDefinition(string Id, string DisplayName, GearSlot Slot, int Attack, int Defense, int MindProtection);
public sealed record LootResult(string ItemId, string DisplayName, int Quantity, bool AlreadyOpened);

public static class Catalog
{
    public static readonly IReadOnlyDictionary<HeroId, HeroDefinition> Heroes = new Dictionary<HeroId, HeroDefinition>
    {
        [HeroId.Gladia] = new("Gladia", 95, 95, 0, "L’angelo del focolare. Guarisce i bisognosi, ma ode Braka, il dio della devastazione.",
            ["Parla", "Pronto soccorso", "Taglio netto", "Litania pallida"]),
        [HeroId.Ambrosh] = new("Ambrosh", 86, 100, 0, "Il consacrato a Nuwdanìe, dea della procreazione. Combatte contro una benedizione che non conosce misura.",
            ["Parla", "Analisi anatomica", "Fendente metodico", "Eco del vuoto"]),
        [HeroId.Roderick] = new("Roderick", 100, 82, 3, "Il reduce, legato a Nyarlatoth, dio della follia. Vede l’impossibile e richiama creature.",
            ["Parla", "Visione irrazionale", "Richiamo del velo", "Patto della follia"]),
        [HeroId.Anatema] = new("Anatema", 96, 90, 0, "Un eretico senza padrone. I segni sul suo corpo non sono opera sua.", ["Parla", "Volontà ferrea", "Fendente eretico", "Rottura del patto"])
    };

    public static readonly IReadOnlyDictionary<Difficulty, DifficultyModifiers> Difficulties =
        new Dictionary<Difficulty, DifficultyModifiers>
        {
            [Difficulty.Facile] = new(0.70, 0.85, 0.85),
            [Difficulty.Sopravvivenza] = new(1.00, 1.00, 1.00),
            [Difficulty.DeSade] = new(1.65, 1.35, 1.25)
        };

    public static readonly IReadOnlyDictionary<string, string> Items = new Dictionary<string, string>
    {
        ["pane"] = "Pane duro", ["mela"] = "Mela avvizzita", ["conserva"] = "Conserva di carne",
        ["bende"] = "Fasciatura sporca", ["erbe"] = "Erbe medicinali",
        ["infuso"] = "Infuso amaro", ["chiave_stazione"] = "Chiave della Stazione Centrale", ["chiave_bassa"] = "Chiave dell’officina", ["chiave_cisterne"] = "Chiave dell’archivio", ["grimaldello"] = "Grimaldello consumabile", ["lettera_scarlatta"] = "Lettera Scarlatta"
    };

    public static readonly IReadOnlyDictionary<string, GearDefinition> Equipment = new Dictionary<string, GearDefinition>
    {
        ["coltello"] = new("coltello", "Coltello corto", GearSlot.Arma, 5, 0, 0),
        ["bastone"] = new("bastone", "Bastone ferrato", GearSlot.Arma, 8, 1, 0),
        ["martello"] = new("martello", "Martello da reduce", GearSlot.Arma, 11, 0, 0),
        ["giacca"] = new("giacca", "Giacca da viaggio", GearSlot.Corpo, 0, 3, 0),
        ["corazza"] = new("corazza", "Corazza imbottita", GearSlot.Corpo, 0, 8, 0),
        ["cappuccio"] = new("cappuccio", "Cappuccio di tela", GearSlot.Testa, 0, 2, 0),
        ["elmo"] = new("elmo", "Elmo del capotreno", GearSlot.Testa, 0, 6, 0),
        ["medaglione"] = new("medaglione", "Medaglione scheggiato", GearSlot.Accessorio, 0, 1, 4)
    };
}

public sealed record FallenEnemy(string Map,string Id,string Kind,int X,int Y,string Loot,int Quantity);

public sealed record EnemyLocation86(int X,int Y,int Facing);

public sealed class SaveData
{
    public HeroId Hero { get; set; } = HeroId.Gladia;
    public Difficulty Difficulty { get; set; } = Difficulty.Sopravvivenza;
    public int Body { get; set; } = 95;
    public int Mind { get; set; } = 95;
    public double Hunger { get; set; } = 100;
    public string Map { get; set; } = "station";
    public int TileX { get; set; } = 10;
    public int TileY { get; set; } = 14;
    public Dictionary<string, int> Inventory { get; set; } = new();
    public Dictionary<GearSlot, string> Equipped { get; set; } = new();
    public HashSet<string> StoryFlags { get; set; } = new();
    public HashSet<string> SearchedContainers { get; set; } = new();
    public Dictionary<string,FallenEnemy> Corpses { get; set; } = new();
    public Dictionary<string,EnemyLocation86> EnemyLocations86 { get; set; } = new();
    public Dictionary<string,Dictionary<string,int>> EnemyParts { get; set; } = new();
    public HashSet<string> DefeatedEnemies { get; set; } = new();
    public Dictionary<string,int> DoorDamage { get; set; } = new();
    public HashSet<string> OwnedEquipment { get; set; } = new();
    public HashSet<string> Injuries { get; set; } = new();
    public HashSet<string> LostLimbs84 { get; set; } = new();
    public Dictionary<string,bool> CollisionEdits { get; set; } = new();
    public string FamiliarKind { get; set; } = "";
    public bool CompanionJoined { get; set; } = false;
    public bool Bleeding { get; set; } = false;
    public int Infection { get; set; } = 0;
    public int Prayers83 { get; set; } = 0;
}

/// <summary>Godot-independent rules engine, testable without the editor or graphical runtime.</summary>
public sealed class GameRules
{
    public SaveData State { get; private set; }

    public GameRules(HeroId hero, Difficulty difficulty)
    {
        State = new SaveData();
        StartNew(hero, difficulty);
    }

    public void StartNew(HeroId hero, Difficulty difficulty)
    {
        var h = Catalog.Heroes[hero];
        State = new SaveData { Hero = hero, Difficulty = difficulty, Body = h.StartingBody, Mind = h.StartingMind };
        AddItem("pane", 2);
        AddItem("bende", hero == HeroId.Gladia ? 3 : 2);
        if (hero == HeroId.Gladia) AddItem("erbe", 1);
        if (hero == HeroId.Ambrosh) AddItem("infuso", 2);
        State.Equipped[GearSlot.Arma] = hero switch
        {
            HeroId.Ambrosh => "bastone", HeroId.Roderick => "martello", _ => "coltello"
        };
        State.Equipped[GearSlot.Corpo] = "giacca";
        foreach (var id in State.Equipped.Values) State.OwnedEquipment.Add(id);
    }

    public static GameRules FromJson(string json)
    {
        var saved = JsonSerializer.Deserialize<SaveData>(json, SerializerOptions)
                    ?? throw new InvalidDataException("Salvataggio non valido.");
        var rules = new GameRules(saved.Hero, saved.Difficulty) { State = saved };
        foreach(var limb in saved.LostLimbs84) saved.Injuries.Add(limb);
        if(rules.NoArms84) saved.Equipped.Remove(GearSlot.Arma);
        return rules;
    }

    public string ToJson() => JsonSerializer.Serialize(State, SerializerOptions);
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true, Converters = { new JsonStringEnumConverter() }
    };

    public void AddItem(string id, int count = 1)
    {
        if (count < 1 || !Catalog.Items.ContainsKey(id)) throw new ArgumentException("Oggetto non valido.");
        State.Inventory[id] = State.Inventory.GetValueOrDefault(id, 0) + count;
    }

    public bool HasItem(string id) => State.Inventory.GetValueOrDefault(id) > 0;
    public void ShowTicket(bool shown)
    {
        if (shown) { State.StoryFlags.Add("ticket_shown"); State.Mind = Math.Max(0, State.Mind - 5); }
        else { State.StoryFlags.Add("ticket_hidden"); State.Hunger = Math.Max(0, State.Hunger - 6); }
    }
    public void HelpPassenger(bool helped)
    {
        if (helped)
        {
            State.StoryFlags.Add("passenger_helped");
            State.Body = Math.Max(1, State.Body - 14);
            AddItem("bende");
        }
        else { State.StoryFlags.Add("passenger_ignored"); State.Mind = Math.Max(0, State.Mind - 11); }
    }
    public void AnswerVoice(bool answered)
    {
        if (answered) { State.StoryFlags.Add("voice_answered"); AddItem("infuso"); State.Mind = Math.Max(0, State.Mind - 13); }
        else { State.StoryFlags.Add("voice_ignored"); State.Hunger = Math.Max(0, State.Hunger - 8); }
    }

    /// <summary>La chiave si trova nel bidone preesistente, non in un nuovo cumulo di rifiuti.</summary>
    public LootResult SearchStationBin()
    {
        const string id = "station_existing_bin";
        if (!State.SearchedContainers.Add(id)) return new("", "Già rovistato", 0, true);
        AddItem("chiave_stazione");
        return new("chiave_stazione", Catalog.Items["chiave_stazione"], 1, false);
    }

    public bool CanOpenCentralStationDoor() => HasItem("chiave_stazione") || State.StoryFlags.Contains("opened93:station:stationhall");

    /// <summary>70% cibo, 9% bende, 7% erbe, 14% vuoto. One roll per container ID.</summary>
    public LootResult SearchContainer(string containerId, Random random, bool guaranteed=false)
    {
        if (string.IsNullOrWhiteSpace(containerId)) throw new ArgumentException("Identificatore richiesto.");
        if (!State.SearchedContainers.Add(containerId)) return new("", "Già rovistato", 0, true);
        int roll = random.Next(100);
        string id = roll switch
        {
            < 70 => new[] { "pane", "mela", "conserva" }[random.Next(3)],
            < 79 => "bende",
            < 86 => "erbe",
            _ => guaranteed ? "infuso" : ""
        };
        if (id.Length > 0) AddItem(id);
        return new(id, id.Length > 0 ? Catalog.Items[id] : "Niente", id.Length > 0 ? 1 : 0, false);
    }

    public bool NoArms84 => State.LostLimbs84.Contains("braccioD") && State.LostLimbs84.Contains("braccioS");
    public bool LoseLimb84(string limb)
    {
        if(limb is not ("braccioD" or "braccioS" or "gambaD" or "gambaS") || !State.LostLimbs84.Add(limb)) return false;
        State.Injuries.Add(limb); State.Bleeding=true;
        if(NoArms84) State.Equipped.Remove(GearSlot.Arma);
        return true;
    }
    public bool Equip(string gearId)
    {
        if (!Catalog.Equipment.TryGetValue(gearId, out var gear)) return false;
        if(gear.Slot==GearSlot.Arma && NoArms84) return false;
        State.Equipped[gear.Slot] = gearId;
        return true;
    }

    public (int Attack, int Defense, int MindProtection) GetEquipmentStats()
    {
        int a = 0, d = 0, m = 0;
        foreach (var id in State.Equipped.Values)
        {
            if (!Catalog.Equipment.TryGetValue(id, out var item)) continue;
            if(item.Slot==GearSlot.Arma && NoArms84) continue;
            a += item.Attack; d += item.Defense; m += item.MindProtection;
        }
        return (a, d, m);
    }

    public double EnemyAttack(double baseAttack) => baseAttack * Catalog.Difficulties[State.Difficulty].EnemyAttack;
    public double EnemyDefense(double baseDefense) => baseDefense * Catalog.Difficulties[State.Difficulty].EnemyDefense;
    public void AdvanceMinutes(double minutes, double baseHungerLossPerMinute = 0.08) =>
        State.Hunger = Math.Max(0, State.Hunger - Math.Max(0, minutes) * baseHungerLossPerMinute *
                                 Catalog.Difficulties[State.Difficulty].HungerRate);

    public IReadOnlyList<string> GetSkills()
    {
        var skills=Catalog.Heroes[State.Hero].Skills.ToList();
        if(State.Hero==HeroId.Ambrosh){
            if(State.StoryFlags.Contains("ambrosh_ritual_control"))skills[1]="Respiro di contenimento";
            if(State.StoryFlags.Contains("ambrosh_physical_control"))skills[1]="Disciplina del corpo";
            if(State.StoryFlags.Contains("ambrosh_devout"))skills[2]="Laccio votivo";
            if(State.StoryFlags.Contains("ambrosh_nuwdanie_mark"))skills[3]="Sovraccarico vitale";
            else if(State.StoryFlags.Contains("ambrosh_closed_gate"))skills[3]="Sigillo della soglia";
            return skills;
        }
        if(State.Hero!=HeroId.Gladia)return skills;
        if(State.StoryFlags.Contains("gladia_blessed_hands"))skills[1]="Mani benedette";
        if(State.StoryFlags.Contains("gladia_hearth_seal"))skills[2]="Sigillo del focolare";
        if(State.StoryFlags.Contains("gladia_braka_mark"))skills[3]="Sussurro di Braka";
        else if(State.StoryFlags.Contains("gladia_containment_litany"))skills[3]="Litania di contenimento";
        return skills;
    }
    public string Talk(string target)
    {
        if (string.IsNullOrWhiteSpace(target)) return "Non c'è nessuno con cui parlare.";
        return target switch
        {
            "stationmaster" when State.StoryFlags.Contains("ticket_shown") => "Il Capostazione riconosce il biglietto.",
            "stationmaster" => "Il Capostazione chiede di vedere un documento.",
            "companion" => "La tua compagna aspetta che tu dica qualcosa.",
            "enemy" => "La creatura reagisce alla tua voce, ma non risponde ancora.",
            _ => "Nessuna risposta."
        };
    }
}
