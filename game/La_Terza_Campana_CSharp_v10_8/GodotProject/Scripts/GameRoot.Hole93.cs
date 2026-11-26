using System;
using System.Linq;
using Godot;
using LaTerzaCampana.Core;
public partial class GameRoot
{
    private bool holeScene93;
    private float holeClock93;
    private void InspectHole93(string name){
        if(game.State.StoryFlags.Contains("found81:key"))Say(name,"Le larve continuano a muoversi tra le radici. La chiave non c'è più.");
        else Say(name,"Fra le larve e i vermi si intravede una chiave. Per raggiungerla dovrai infilare la mano tra le radici.","",new Choice("Infila la mano",()=>{if(game.State.StoryFlags.Add("found81:key"))game.AddItem("chiave_bassa");Say(name,"Qualcosa si contorce sotto le dita. Riesci a estrarre la chiave. **x1 Chiave dell’officina**.");holeScene93=true;holeClock93=2f;}),new Choice("Lascia stare",CloseDialogue));
        holeScene93=true;holeClock93=0f;
    }
    private void DrawHole93(){
        // Draw in a clipped scene area above the dialogue: full illustration scaled to fit.
        Image("hole93_close",new Rect2(0,0,W,720));
        if(!game.State.StoryFlags.Contains("found81:key"))Image("hole93_key",new Rect2(565,364,143,86));
        for(int i=0;i<25;i++){
            float phase=i*2.37f;float bx=640+Mathf.Sin(phase*1.73f)*(85+i%5*18);float by=400+Mathf.Cos(phase*.83f)*(19+i%4*9);float f=tick*(1.0f+i%4*.23f)+phase;
            bx+=Mathf.Sin(f*.6f)*13;by+=Mathf.Cos(f*.83f)*4;float angle=phase+Mathf.Sin(f*.4f)*.48f;var along=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle)*.55f);var cross=new Vector2(-along.Y,along.X);
            int segments=i%5==0?12:7;float step=i%5==0?2.7f:2.1f;
            for(int j=0;j<segments;j++){float q=j/(float)(segments-1);float size=1.3f+Mathf.Sin(q*Mathf.Pi)*(i%5==0?1.9f:2.7f);var p=new Vector2(bx,by)+along*(j*step)+cross*(Mathf.Sin(f-j*.55f)*2.7f);DrawEllipse(p.X+1,p.Y+2,size+1,size*.72f+1,new Color(.055f,.03f,.02f,.67f));DrawEllipse(p.X,p.Y,size,size*.72f,new Color(.49f+.08f*Mathf.Sin(q*4),.39f,.24f));DrawEllipse(p.X-.7f,p.Y-.8f,size*.72f,size*.43f,new Color(.74f,.64f,.43f,.86f));if(j==0)DrawCircle(p,1.3f,new Color(.19f,.12f,.08f));}
        }
        Rect(0,0,W,H,new Color(.07f,.035f,.018f,.10f));
    }
    private void ReadOlmoDocs93(bool inventory=false){Say("Documenti dell’Olmo","17 novembre. I registri delle consegne non coincidono più con i nomi dei residenti. Ho nascosto un attrezzo nel doppiofondo della scrivania. Non bussate alla porta che respira. Se udite tre colpi dal muro, non rispondete.");if(inventory)returnScreen=Screen.Inventory;}
    private void CheckHole93(){int n=0;void C(bool ok,string label){if(!ok)throw new Exception(label);n++;}
        game.StartNew(HeroId.Gladia,Difficulty.Sopravvivenza);ResetEventRules();screen=Screen.World;EnterMap("casa_olmo",10,4);var hole=CurrentMap().Props.First(p=>p.Id=="olmo_hole82");C(Art("hole93_close")!=null,"closeup art");C(Art("hole93_key")!=null,"key art");InteractProp(hole);C(holeScene93&&holeClock93==0&&choices.Count==2,"fade and options");choices[1].Do();C(!holeScene93&&!game.HasItem("chiave_bassa"),"decline");InteractProp(hole);choices[0].Do();C(holeScene93&&game.HasItem("chiave_bassa"),"key collected");CloseDialogue();InteractProp(hole);C(choices.Count==0&&game.State.Inventory["chiave_bassa"]==1,"no duplicate");CloseDialogue();EnterMap("casa_olmo_servizio",10,11);var desk=CurrentMap().Props.First(p=>p.Id=="olmo_desk92");InteractProp(desk);C(choices.Count==3,"desk choices");choices[0].Do();C(game.State.StoryFlags.Contains("olmo_docs92"),"documents");InteractProp(desk);choices[0].Do();C(game.HasItem("grimaldello"),"lockpick loot");InteractProp(desk);C(choices.Count==0&&game.State.Inventory["grimaldello"]==1,"desk once");CloseDialogue();EnterMap("strada_bassa",15,4);var door=CurrentMap().Doors.First(d=>d.Id=="locked_basso_2");InteractDoor(door);C(choices.Any(c=>c.Title.Contains("grimaldello")),"pick option");choices.First(c=>c.Title.Contains("grimaldello")).Do();C(!game.HasItem("grimaldello")&&game.State.StoryFlags.Contains("opened81:strada_bassa:locked_basso_2"),"consumed opened");CloseDialogue();InteractDoor(door);C(game.State.Map=="archivio_basso","permanent unlocked");game=GameRules.FromJson(game.ToJson());C(game.State.StoryFlags.Contains("olmo_docs92")&&game.State.StoryFlags.Contains("found81:key"),"saved");GD.Print($"HOLE93_OK {n}");GetTree().Quit();}
}
