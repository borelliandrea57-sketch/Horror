using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using LaTerzaCampana.Core;
public partial class GameRoot
{
    private void Unlock81(WorldDoor d,string key)
    {
        game.State.StoryFlags.Add("opened81:"+key);CloseDialogue();PlaySound("door_open");EnterMap(d.To,(int)d.Spawn[0],(int)d.Spawn[1]);
    }
    private void DrawOpenLocks87(){if(game.State.Map!="strada_bassa")return;foreach(var d in CurrentMap().Doors){if(!game.State.StoryFlags.Contains("opened81:strada_bassa:"+d.Id))continue;var t=Art("open90_"+d.Id);if(t!=null)DrawTextureRect(t,new Rect2(-camera,new Vector2(CurrentMap().W*T,CurrentMap().H*T)),false);}}
    private void Migrate90(){if(!game.State.StoryFlags.Add("geometry_v90"))return;foreach(var key in game.State.CollisionEdits.Keys.Where(k=>k.StartsWith("strada_bassa:")).ToArray())game.State.CollisionEdits.Remove(key);}
    private void Migrate81(){if(!game.State.StoryFlags.Add("geometry_v81"))return;foreach(var key in game.State.CollisionEdits.Keys.Where(k=>k.StartsWith("casa_medico:")||k.StartsWith("strada_bassa:")||k.StartsWith("casa_olmo_servizio:")).ToArray())game.State.CollisionEdits.Remove(key);}
    private void CheckMap81(){int n=0;void C(bool ok,string msg){if(!ok)throw new Exception(msg);n++;}
        game.StartNew(HeroId.Gladia,Difficulty.Sopravvivenza);ResetEventRules();screen=Screen.World;
        foreach(var (map,x,y) in new[]{("casa_medico",3,18),("strada_bassa",15,4),("rifugio_basso",9,11),("magazzino_basso",9,11),("officina_bassa",9,11),("archivio_basso",9,11)}){
            EnterMap(map,x,y);C(!Blocked(map,x,y),"spawn "+map);C(Art("map_"+map)!=null,"art "+map);
            var seen=new HashSet<Vector2I>{new(x,y)};var q=new Queue<Vector2I>(seen);while(q.Count>0){var v=q.Dequeue();foreach(var step in NeighborSteps){var next=v+step;if(!Blocked(map,next.X,next.Y)&&seen.Add(next))q.Enqueue(next);}}
            foreach(var d in CurrentMap().Doors)C(seen.Any(v=>Math.Abs(v.X-d.X)+Math.Abs(v.Y-d.Y)<=1),"door reachable "+d.Id);
            foreach(var pr in CurrentMap().Props)C(seen.Any(v=>Math.Abs(v.X-pr.X)+Math.Abs(v.Y-pr.Y)<=2),"prop reachable "+pr.Id);
            foreach(var e in CurrentMap().Enemies)C(seen.Contains(new(e.X,e.Y)),"enemy reachable "+e.Id);
        }
        EnterMap("strada",17,21);C(TrySouthStreetExit(Vector2I.Down)&&game.State.Map=="strada_bassa","south transition");
        var locked=CurrentMap().Doors.First(d=>d.Id=="locked_basso_1");InteractDoor(locked);C(screen==Screen.Dialogue&&choices.Count==1,"locked no force");CloseDialogue();int body=game.State.Body;AttackDoor(locked);C(game.State.Body==body,"cannot force padlock");game.AddItem("chiave_bassa");InteractDoor(locked);C(choices.Count==2,"key option");choices[0].Do();C(game.State.Map=="officina_bassa","key opens");
        EnterMap("strada_bassa",15,4);locked=CurrentMap().Doors.First(d=>d.Id=="locked_basso_2");InteractDoor(locked);C(choices.Count==1,"key not universal");CloseDialogue();game.State.StoryFlags.Add("street81_lockpick");InteractDoor(locked);C(choices.Count==2,"lockpick option");choices[0].Do();C(game.State.Map=="archivio_basso","skill opens");
        EnterMap("strada",17,12);var reinforced=new WorldDoor{Id="test81",Hp=999,Armor=3};body=game.State.Body;for(int i=0;i<3;i++){AttackDoor(reinforced);CloseDialogue();}C(game.State.Body==body-12,"2+4+6 door damage");C(PredictDoorSelfDamage81("strada:test81")==8,"next hit rises");C(GameRules.FromJson(game.ToJson()).State.StoryFlags.Contains("opened81:strada_bassa:locked_basso_2"),"lock persists");GD.Print($"MAP81_OK {n}");GetTree().Quit();
    }
}
