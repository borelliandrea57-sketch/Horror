using Godot;
using System;
using System.Linq;
public partial class GameRoot
{
    private Vector2 streetNpcVisual;
    private int streetNpcFacing;
    private float streetNpcClock;
    private bool streetNpcMoving;
    private void MigrateStreetCollisions()
    {
        if(!game.State.StoryFlags.Add("geometry_v57"))return;
        foreach(var key in game.State.CollisionEdits.Keys.Where(k=>k.StartsWith("strada:")||k.StartsWith("strada_bassa:")||k.StartsWith("vicolo:")||k.StartsWith("casa_olmo:")).ToArray())game.State.CollisionEdits.Remove(key);
        if(game.State.StoryFlags.Contains("pentesilea_met"))foreach(string id in new[]{"olmo_undead_1","olmo_undead_2"})game.State.DefeatedEnemies.Add("casa_olmo:"+id);
    }
    private void ResetStreetPatrol()
    {
        if(!maps.TryGetValue("strada",out var street))return;
        var npc=street.Npcs.FirstOrDefault(n=>n.Id=="corrupted_wanderer");
        if(npc==null)return;
        npc.X=20;npc.Y=14;streetNpcVisual=TileCenter(20,14);streetNpcFacing=0;streetNpcClock=.5f;streetNpcMoving=false;
    }
    private void UpdateStreetPatrol(float dt)
    {
        if(game.State.Map!="strada"||HideCorruptedNpc("corrupted_wanderer"))return;
        var n=CurrentMap().Npcs.FirstOrDefault(n=>n.Id=="corrupted_wanderer");if(n==null)return;
        var dest=TileCenter(n.X,n.Y);streetNpcVisual=streetNpcVisual.MoveToward(dest,dt*T*2.6f);
        streetNpcMoving=streetNpcVisual.DistanceTo(dest)>1f;
        streetNpcClock-=dt;if(streetNpcClock>0||streetNpcMoving)return;
        streetNpcClock=.45f+(float)rng.NextDouble()*.75f;
        if(Math.Abs(n.X-game.State.TileX)+Math.Abs(n.Y-game.State.TileY)<=1)return;
        var choices=new[]{Vector2I.Up,Vector2I.Down,Vector2I.Left,Vector2I.Right}.Where(d=>{
            int x=n.X+d.X,y=n.Y+d.Y;
            return y>=5&&y<=21&&x>=12&&x<=22&&!Blocked("strada",x,y)&&!(x==game.State.TileX&&y==game.State.TileY)&&!enemyPositions.Values.Contains(new Vector2I(x,y));
        }).ToArray();
        if(choices.Length==0)return;
        var step=choices[rng.Next(choices.Length)];n.X+=step.X;n.Y+=step.Y;streetNpcFacing=step==Vector2I.Down?0:step==Vector2I.Left?1:step==Vector2I.Right?2:3;streetNpcMoving=true;
    }
    private bool IsDormantOlmoZombie(string id)=>game.State.Map=="casa_olmo"&&(id=="olmo_undead_1"||id=="olmo_undead_2")&&!game.State.StoryFlags.Contains("pentesilea_met");
    private bool DormantOlmoZombieAt(int x,int y)=>enemyPositions.Any(e=>IsDormantOlmoZombie(e.Key)&&e.Value==new Vector2I(x,y));
}
