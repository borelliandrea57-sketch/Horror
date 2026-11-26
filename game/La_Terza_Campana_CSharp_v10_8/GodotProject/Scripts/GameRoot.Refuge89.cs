using System;
using System.Linq;
using System.Collections.Generic;
using Godot;
using LaTerzaCampana.Core;
public partial class GameRoot
{
    private const string ScarletText89="A chi troverà il rifugio: non aspettate la terza campana. Abbiamo contato sette persone, ma al mattino le coperte erano otto. Non aprite quando qualcuno bussa dal lato sbagliato del muro. Lasciate un posto vuoto alla tavola. — E.";
    private void ReadScarlet89(bool inventory=false){Say("Lettera Scarlatta",ScarletText89,"scarlet89");if(inventory)returnScreen=Screen.Inventory;}
    private bool InteractRefuge89(WorldProp p){
        if(p.Id=="decor87_rifugio_basso_2"){
            if(game.HasItem("lettera_scarlatta")){Say(p.Name,"Hai già controllato la fodera. Rimangono soltanto lenzuola fradice.");return true;}
            Say(p.Name,"Fra le lenzuola trovi una cucitura di filo rosso. Sotto la fodera si intravede una busta.","",new Choice("Scuci la fodera",()=>{if(game.State.StoryFlags.Add("refuge89_scarlet"))game.AddItem("lettera_scarlatta");ReadScarlet89();}),new Choice("Lascia stare",CloseDialogue));return true;
        }
        if(p.Kind!="loot89")return false;
        string item=p.Id switch{"refuge89_medicine"=>"bende","refuge89_supply"=>"conserva","refuge89_cabinet"=>"infuso",_=>"pane"};
        string key=game.State.Map+":"+p.Id;
        if(game.State.SearchedContainers.Add(key)){game.AddItem(item);Say(p.Name,p.Note+" **x1 "+Catalog.Items[item]+"**.");}
        else Say(p.Name,"Hai già frugato qui. Non rimane nulla di utile.");return true;
    }
    private void CheckRefuge89(){int n=0;void C(bool ok,string msg){if(!ok)throw new Exception(msg);n++;}
        game.StartNew(HeroId.Gladia,Difficulty.Sopravvivenza);ResetEventRules();screen=Screen.World;EnterMap("rifugio_basso",9,11);
        C(Art("map89_rifugio_basso")!=null,"new map art");C(Art("scarlet89")!=null,"letter art");C(Art("light_mask59")!=null,"light mask");
        var m=CurrentMap();var start=new Vector2I(game.State.TileX,game.State.TileY);var seen=new HashSet<Vector2I>{start};var q=new Queue<Vector2I>();q.Enqueue(start);
        while(q.Count>0){var at=q.Dequeue();foreach(var v in new[]{Vector2I.Up,Vector2I.Down,Vector2I.Left,Vector2I.Right}){var next=at+v;if(!Blocked("rifugio_basso",next.X,next.Y)&&seen.Add(next))q.Enqueue(next);}}
        C(seen.Count>=140,"walkable rooms");C(seen.Any(v=>Math.Abs(v.X-9)+Math.Abs(v.Y-12)<=1),"exit reachable");
        foreach(var p in m.Props){C(Art(p.ArtFile)!=null,"art "+p.Id);C(seen.Any(v=>Math.Abs(v.X-p.X)+Math.Abs(v.Y-p.Y)<=1.01),"access "+p.Id);}
        var wardrobe=m.Props.First(p=>p.Id=="decor87_rifugio_basso_2");InteractProp(wardrobe);C(choices.Count==2&&!game.HasItem("lettera_scarlatta"),"hidden letter choice");choices[1].Do();C(!game.HasItem("lettera_scarlatta"),"decline");InteractProp(wardrobe);choices[0].Do();C(game.State.Inventory["lettera_scarlatta"]==1&&speech.Contains("terza campana"),"letter recovered");InteractProp(wardrobe);C(choices.Count==0&&game.State.Inventory["lettera_scarlatta"]==1,"unique letter");
        foreach(var p in m.Props.Where(p=>p.Kind=="loot89")){InteractProp(p);var copy=game.ToJson();InteractProp(p);C(game.ToJson()==copy,"loot once "+p.Id);}
        game=GameRules.FromJson(game.ToJson());C(game.HasItem("lettera_scarlatta"),"persistent letter");screen=Screen.Inventory;invTab=3;InventoryKey(9,Key.Key9);C(screen==Screen.Dialogue&&returnScreen==Screen.Inventory,"read from diary");CloseDialogue();C(screen==Screen.Inventory,"return diary");GD.Print($"REFUGE89_OK {n}");GetTree().Quit();
    }
}
