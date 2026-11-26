using LaTerzaCampana.Events;
using LaTerzaCampana.Core;

int checks = 0;
void Assert(bool condition, string description)
{
    if (!condition) throw new Exception("TEST FALLITO: " + description);
    checks++;
}
foreach (var hero in Enum.GetValues<HeroId>())
{
    foreach (var level in Enum.GetValues<Difficulty>())
    {
        var g = new GameRules(hero, level);
        Assert(g.GetSkills().Contains("Parla"), "Parla per ogni protagonista");
        Assert(g.State.Hero == hero, "Protagonista selezionato");
        Assert(g.State.Difficulty == level, "Difficoltà selezionata");
        Assert(!g.CanOpenCentralStationDoor(), "Portone chiuso senza chiave");
        Assert(g.SearchStationBin().ItemId == "chiave_stazione", "Chiave trovata nel bidone preesistente");
        Assert(g.CanOpenCentralStationDoor(), "Portone aperto con chiave");
        Assert(g.SearchStationBin().AlreadyOpened, "Niente chiave duplicata");
        g.HelpPassenger(true);
        g.ShowTicket(true);
        g.AnswerVoice(false);
        var loaded = GameRules.FromJson(g.ToJson());
        Assert(loaded.State.Hero == hero && loaded.State.Difficulty == level, "Roundtrip JSON");
        Assert(loaded.HasItem("chiave_stazione"), "Salvataggio chiave");
        Assert(loaded.State.StoryFlags.Contains("ticket_shown"), "Flag storia persistenti");
        Assert(loaded.GetEquipmentStats().Defense >= 3, "Bonus difesa equipaggiamento");
    }
}
var sample = new GameRules(HeroId.Gladia, Difficulty.DeSade);
var easy = new GameRules(HeroId.Gladia, Difficulty.Facile);
sample.AdvanceMinutes(20); easy.AdvanceMinutes(20);
Assert(sample.State.Hunger < easy.State.Hunger, "Fame più rapida De Sade");
Assert(sample.EnemyAttack(10) > easy.EnemyAttack(10), "Nemici più forti De Sade");
foreach (var result in Enumerable.Range(0, 100))
{
    var g = new GameRules(HeroId.Gladia, Difficulty.Sopravvivenza);
    var first = g.SearchContainer("barrel-a", new Random(result));
    Assert(!first.AlreadyOpened, "Primo loot");
    Assert(g.SearchContainer("barrel-a", new Random(result)).AlreadyOpened, "Barile una sola volta");
}
Assert(sample.Talk("enemy").Length > 5, "Parla in combattimento");
foreach(int choice in new[]{1,2,3})
{
    var g=new GameRules(HeroId.Gladia,Difficulty.Sopravvivenza);
    var logic=new PentesileaAndStillnessLogic(g.State.StoryFlags);
    logic.EnterMap("casa_olmo");
    for(int i=0;i<11;i++)Assert(logic.OnTileChanged()==null,"Nessun evento prima del passo 12");
    Assert(logic.OnTileChanged()==EventSignal.StartPentesileaIntro,"Evento al passo 12");
    Assert(logic.Advance(120,true).Count==0,"Nessuna morte durante intro");
    logic.FinishPentesileaIntro(choice);
    var saved=GameRules.FromJson(g.ToJson());
    Assert(saved.State.StoryFlags.Contains("pentesilea_met"),"Incontro persistente");
    Assert(saved.State.StoryFlags.Contains(choice==1?"pentesilea_trust_high":choice==2?"pentesilea_trust_neutral":"pentesilea_trust_low"),"Fiducia persistente");
    logic.EnterMap("strada");logic.EnterMap("casa_olmo");
    for(int i=0;i<15;i++)Assert(logic.OnTileChanged()==null,"Nessuna ripetizione");
    Assert(logic.Advance(29,true).Count==0,"Prima dei 30 secondi");
    Assert(logic.Advance(1,true).SequenceEqual(new[]{EventSignal.StillnessWarning90}),"Avviso 30");
    Assert(logic.Advance(300,false).Count==0&&logic.StationarySeconds==30,"Inventario sospende senza azzerare");
    Assert(logic.Advance(15,true).SequenceEqual(new[]{EventSignal.StillnessWarning110}),"Avviso 45");
    Assert(logic.Advance(15,true).SequenceEqual(new[]{EventSignal.StillnessDeath120}),"Morte 60");
    Assert(logic.Advance(10,true).Count==0,"Morte unica");
    logic.ResetAfterLoadingGame();Assert(logic.StationarySeconds==0&&!logic.DeathTriggered,"Caricamento azzera");
    Assert(logic.Advance(120,true).Count==3,"Delta lungo produce i tre segnali");
    logic.OnTileChanged();Assert(logic.StationarySeconds==0,"Movimento azzera");
    logic.Advance(30,true);logic.EnterMap("strada");Assert(logic.StationarySeconds==0,"Cambio mappa azzera");
}
var retry=new PentesileaAndStillnessLogic(new HashSet<string>());
retry.EnterMap("casa_olmo");for(int i=0;i<11;i++)retry.OnTileChanged();
retry.EnterMap("strada");retry.EnterMap("casa_olmo");Assert(retry.StepsInsideHouse==0,"Uscita anticipata azzera i passi");
foreach(int mask in Enumerable.Range(0,8))
{
    var g=new GameRules(HeroId.Gladia,Difficulty.Sopravvivenza);
    for(int ch=0;ch<3;ch++)
    {
        bool first=(mask&(1<<ch))!=0;
        Assert(GladiaOrigin.Apply(g,ch,first),"Scelta incipit");
        Assert(!GladiaOrigin.Apply(g,ch,!first),"Ricompensa non duplicabile");
    }
    var loaded=GameRules.FromJson(g.ToJson());
    Assert(loaded.GetSkills().SequenceEqual(g.GetSkills()),"Abilità persistenti");
    Assert(loaded.State.Inventory.OrderBy(x=>x.Key).SequenceEqual(g.State.Inventory.OrderBy(x=>x.Key)),"Oggetti persistenti");
    Assert(loaded.GetSkills()[1]==((mask&2)!=0?"Mani benedette":"Pronto soccorso"),"Cura corretta");
    Assert(loaded.GetSkills()[2]==((mask&2)!=0?"Taglio netto":"Sigillo del focolare"),"Slot fisico corretto");
    Assert(loaded.GetSkills()[3]==((mask&4)!=0?"Sussurro di Braka":"Litania di contenimento"),"Rituale corretto");
    Assert(loaded.State.Equipped.ContainsKey(GearSlot.Accessorio)==((mask&4)==0),"Medaglione coerente");
    loaded.State.Body=1;loaded.State.Infection=30;loaded.State.Bleeding=true;var h=GladiaOrigin.Heal(loaded,false);
    Assert(loaded.State.Body==1+h&&!loaded.State.Bleeding,"Cura effettiva");
    Assert(loaded.State.Infection==((mask&2)!=0?15:30),"Infezione coerente");
}
Assert(!GladiaOrigin.Apply(new GameRules(HeroId.Ambrosh,Difficulty.Facile),0,true),"Incipit esclusivo di Gladia");
foreach(var h in Enum.GetValues<HeroId>()){
 var g=new GameRules(h,Difficulty.Sopravvivenza);var b=CombatStats100.Base(h);var e=CombatStats100.Effective(g);
 Assert(e.Attack>b.Attack,"Weapon bonus");Assert(e.Defense>b.Defense,"Armor bonus");Assert(CombatStats100.Effective(GameRules.FromJson(g.ToJson()))==e,"Stats persist");
 g.LoseLimb84("gambaS");Assert(CombatStats100.Effective(g).Agility<e.Agility,"Leg injury affects agility");g.LoseLimb84("braccioS");g.LoseLimb84("braccioD");Assert(CombatStats100.Effective(g).Attack<e.Attack,"Arm injury affects attack");}
Assert(CombatStats100.FleeChance(18,12,Difficulty.Sopravvivenza)>CombatStats100.FleeChance(13,12,Difficulty.Sopravvivenza),"Agility affects flee");
Assert(CombatStats100.FleeChance(14,12,Difficulty.DeSade)<CombatStats100.FleeChance(14,12,Difficulty.Facile),"Difficulty affects flee");
Assert(CombatStats100.SkillDamage(24,20,8)>CombatStats100.SkillDamage(24,11,8),"Arcane attack scales");
Assert(CombatStats100.IncomingDamage(21,19,0)<CombatStats100.IncomingDamage(21,10,0),"Defense scales");
Console.WriteLine($"OK — {checks} verifiche del nucleo C# superate.");
