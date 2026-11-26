namespace LaTerzaCampana.Core;

public static class GladiaOrigin
{
    public static bool Apply(GameRules game,int chapter,bool first)
    {
        var s=game.State;
        if(s.Hero!=HeroId.Gladia || chapter<0 || chapter>2 || !s.StoryFlags.Add($"gladia_origin_choice_{chapter}")) return false;
        switch(chapter)
        {
            case 0:
                if(first){s.StoryFlags.Add("gladia_mercy");s.Inventory["pane"]=Math.Max(0,s.Inventory.GetValueOrDefault("pane")-1);game.AddItem("erbe");s.Mind=Math.Min(100,s.Mind+4);}
                else{s.StoryFlags.Add("gladia_pragmatic");game.AddItem("pane");s.Body=Math.Min(100,s.Body+4);}
                break;
            case 1:
                if(first){s.StoryFlags.Add("gladia_blessed_hands");game.AddItem("bende");s.Body=Math.Max(1,s.Body-4);}
                else{s.StoryFlags.Add("gladia_hearth_seal");game.AddItem("infuso");}
                break;
            case 2:
                if(first){s.StoryFlags.Add("gladia_braka_mark");s.Mind=Math.Max(0,s.Mind-10);s.Body=Math.Min(100,s.Body+3);}
                else{s.StoryFlags.Add("gladia_containment_litany");s.Mind=Math.Min(100,s.Mind+6);s.OwnedEquipment.Add("medaglione");s.Equipped[GearSlot.Accessorio]="medaglione";}
                break;
        }
        return true;
    }
    public static int Heal(GameRules g,bool combat)
    {
        var s=g.State;
        if(s.Hero!=HeroId.Gladia)return 0;
        int amount=s.StoryFlags.Contains("gladia_blessed_hands")?(combat?44:40):s.StoryFlags.Contains("gladia_mercy")?(combat?36:33):(combat?32:28);
        s.Body=Math.Min(100,s.Body+amount);s.Bleeding=false;
        if(s.StoryFlags.Contains("gladia_blessed_hands"))s.Infection=Math.Max(0,s.Infection-15);
        if(s.StoryFlags.Contains("gladia_mercy"))s.Mind=Math.Min(100,s.Mind+4);
        return amount;
    }
}
