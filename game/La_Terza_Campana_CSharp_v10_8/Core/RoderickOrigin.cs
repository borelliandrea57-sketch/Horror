namespace LaTerzaCampana.Core;
public static class RoderickOrigin
{
    public static bool Apply(GameRules g,int chapter,bool first)
    {
        var s=g.State;if(s.Hero!=HeroId.Roderick||chapter<0||chapter>2||!s.StoryFlags.Add($"roderick_origin_choice_{chapter}"))return false;
        switch(chapter){
            case 0: if(first){s.StoryFlags.Add("rod_deep_vision");s.Mind=Math.Max(1,s.Mind-7);g.AddItem("infuso");}else{g.AddItem("bende",2);s.StoryFlags.Add("rod_grounded");s.Mind=Math.Min(100,s.Mind+6);}break;
            case 1: if(first){s.StoryFlags.Add("rod_binding");g.AddItem("conserva");}else{s.StoryFlags.Add("rod_summon");g.AddItem("infuso");}break;
            case 2: if(first){s.StoryFlags.Add("rod_mark");s.Mind=Math.Max(1,s.Mind-10);}else{s.StoryFlags.Add("rod_anchor");s.OwnedEquipment.Add("medaglione");s.Equipped[GearSlot.Accessorio]="medaglione";}break;
        }return true;
    }
    public static bool CanBind(GameRules g,string kind,IReadOnlyDictionary<string,int> parts)
    {
        int limit=(kind=="zombie"?22:kind=="stitch"?36:0)+(g.State.StoryFlags.Contains("rod_binding")?8:0);
        return g.State.Hero==HeroId.Roderick && g.State.FamiliarKind=="" && (kind is "zombie" or "stitch") && parts.GetValueOrDefault("torace")>0 && parts.GetValueOrDefault("torace")<=limit && (parts.GetValueOrDefault("gambaS")==0||parts.GetValueOrDefault("gambaD")==0);
    }
}
