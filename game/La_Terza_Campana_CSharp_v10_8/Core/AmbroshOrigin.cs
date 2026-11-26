namespace LaTerzaCampana.Core;
public static class AmbroshOrigin
{
    public static bool Apply(GameRules g,int chapter,bool first)
    {
        var s=g.State;if(s.Hero!=HeroId.Ambrosh||chapter<0||chapter>2||!s.StoryFlags.Add($"ambrosh_origin_choice_{chapter}"))return false;
        switch(chapter){
            case 0: if(first){s.StoryFlags.Add("ambrosh_devout");g.AddItem("infuso");}else{s.StoryFlags.Add("ambrosh_repressed");s.Body=Math.Min(100,s.Body+8);g.AddItem("pane");}break;
            case 1: if(first){s.StoryFlags.Add("ambrosh_ritual_control");g.AddItem("bende");}else{s.StoryFlags.Add("ambrosh_physical_control");g.AddItem("infuso",2);g.AddItem("erbe");}break;
            case 2: if(first){s.StoryFlags.Add("ambrosh_nuwdanie_mark");s.Mind=Math.Max(1,s.Mind-12);s.Body=Math.Min(100,s.Body+4);}else{s.StoryFlags.Add("ambrosh_closed_gate");s.OwnedEquipment.Add("medaglione");s.Equipped[GearSlot.Accessorio]="medaglione";}break;
        }return true;
    }
}
