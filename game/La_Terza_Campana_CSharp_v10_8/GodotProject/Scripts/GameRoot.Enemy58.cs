using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class GameRoot
{
    private sealed class EnemyBrain
    {
        public Vector2I Home, LastSeen, Patrol;
        public Vector2 Visual;
        public float Memory, Pause, Grace;
        public bool HasPatrol;
        public EnemyBrain(Vector2I p,Vector2 visual){Home=LastSeen=Patrol=p;Visual=visual;}
    }
    private readonly Dictionary<string,EnemyBrain> enemyBrains=new();
    private float recentStepNoise;
    private static readonly Vector2I[] NeighborSteps={Vector2I.Down,Vector2I.Left,Vector2I.Right,Vector2I.Up};
    private static int Manhattan(Vector2I a,Vector2I b)=>Math.Abs(a.X-b.X)+Math.Abs(a.Y-b.Y);
    private bool NpcBlocksEnemy(Vector2I p)=>CurrentMap().Npcs.Any(n=>n.X==p.X&&n.Y==p.Y&&!HidePentesileaNpc(n.Id)&&!HideCorruptedNpc(n.Id)&&!(n.Id=="woman"&&game.State.CompanionJoined));
    private bool EnemyTileFree(string id,Vector2I p)=>!Blocked(game.State.Map,p.X,p.Y)&&!NpcBlocksEnemy(p)&&!enemyPositions.Any(e=>e.Key!=id&&e.Value==p);
    // Sample the ray in tile coordinates; diagonal corners are occluded if either adjoining tile is solid.
    private bool EnemyCanSee(Vector2I a,Vector2I b)
    {
        Vector2 start=new(a.X+.5f,a.Y+.5f),end=new(b.X+.5f,b.Y+.5f);
        int steps=Math.Max(1,Mathf.CeilToInt(start.DistanceTo(end)*6));Vector2I prev=a;
        for(int i=1;i<=steps;i++)
        {
            Vector2 v=start.Lerp(end,i/(float)steps);var p=new Vector2I(Mathf.FloorToInt(v.X),Mathf.FloorToInt(v.Y));
            if(p==prev)continue;
            if(p.X!=prev.X&&p.Y!=prev.Y&&(Blocked(game.State.Map,p.X,prev.Y)||Blocked(game.State.Map,prev.X,p.Y)))return false;
            if(Blocked(game.State.Map,p.X,p.Y))return false;
            prev=p;
        }
        return true;
    }
    private Vector2I? EnemyNextStep(string id,Vector2I from,Vector2I goal,int maxSteps=1800)
    {
        if(from==goal)return null;
        var q=new Queue<Vector2I>();q.Enqueue(from);
        var prev=new Dictionary<Vector2I,Vector2I>{{from,from}};
        var depth=new Dictionary<Vector2I,int>{{from,0}};
        while(q.Count>0&&prev.Count<1800)
        {
            var at=q.Dequeue();if(depth[at]>=maxSteps)continue;
            foreach(var d in NeighborSteps)
            {
                var next=at+d;if(prev.ContainsKey(next)||!EnemyTileFree(id,next))continue;
                prev[next]=at;depth[next]=depth[at]+1;
                if(next==goal){while(prev[next]!=from)next=prev[next];return next;}
                q.Enqueue(next);
            }
        }
        return null;
    }
    private void UpdateEnemyVisuals(float dt)
    {
        recentStepNoise=movedThisFrame?.5f:Math.Max(0,recentStepNoise-dt);
        foreach(var entry in enemyPositions)
        {
            if(!enemyBrains.TryGetValue(entry.Key,out var b))enemyBrains[entry.Key]=b=new EnemyBrain(entry.Value,TileCenter(entry.Value.X,entry.Value.Y));
            var e=CurrentMap().Enemies.FirstOrDefault(e=>e.Id==entry.Key);float speed=e==null?3f:Profile87(e).Speed*(b.Memory>0?Profile87(e).Chase:1f);if(game.State.Map=="binari")speed*=1.2f;b.Visual=b.Visual.MoveToward(TileCenter(entry.Value.X,entry.Value.Y),dt*T*speed);
            b.Memory=Math.Max(0,b.Memory-dt);b.Pause=Math.Max(0,b.Pause-dt);b.Grace=Math.Max(0,b.Grace-dt);
        }
    }
    private void MoveEnemies58()
    {
        var heroPos=new Vector2I(game.State.TileX,game.State.TileY);
        foreach(var entry in enemyPositions.ToArray())
        {
            string id=entry.Key;Vector2I p=entry.Value;
            var enemy=CurrentMap().Enemies.FirstOrDefault(e=>e.Id==id);if(enemy==null||IsDormantOlmoZombie(id))continue;
            // Keep the mandatory introduction uninterrupted. The two remaining zombies wake up after the dialogue.
            if(game.State.Map=="casa_olmo"&&!game.State.StoryFlags.Contains("pentesilea_met"))continue;
            if(!enemyBrains.TryGetValue(id,out var b))enemyBrains[id]=b=new EnemyBrain(p,TileCenter(p.X,p.Y));
            if(game.State.EnemyParts.TryGetValue(game.State.Map+":"+id,out var wounds)){int legs=(wounds.GetValueOrDefault("gambaS",1)>0?1:0)+(wounds.GetValueOrDefault("gambaD",1)>0?1:0);if(legs==0&&Manhattan(p,heroPos)>1)continue;if(legs==1&&rng.NextDouble()<.55)continue;}
            if(b.Grace>0||b.Visual.DistanceTo(TileCenter(p.X,p.Y))>3)continue;
            int distance=Manhattan(p,heroPos),face=enemyFacing.GetValueOrDefault(id);
            var forward=NeighborSteps[Math.Clamp(face,0,3)];var offset=heroPos-p;
            bool inCone=distance<=2||offset.X*forward.X+offset.Y*forward.Y>=0;
            bool seen=distance<=(enemy.Kind=="ossuary"?10:7)&&inCone&&EnemyCanSee(p,heroPos);
            bool heard=recentStepNoise>0&&distance<=3&&EnemyNextStep(id,p,heroPos,4)!=null;
            if(seen||heard)
            {
                b.LastSeen=heroPos;b.Memory=Profile87(enemy).Memory;b.HasPatrol=false;b.Pause=0;
                if(alertedEnemies.Add(id))PlaySound(enemy.Kind=="ossuary"?"monster_cry":"monster_move",-7);
            }
            if(distance<=1&&EnemyCanSee(p,heroPos)){StartBattle(enemy);return;}
            Vector2I target;
            if(b.Memory>0){target=b.LastSeen;if(p==target&&!seen&&!heard){b.Memory=0;b.Pause=.6f;continue;}}
            else
            {
                alertedEnemies.Remove(id);if(b.Pause>0||enemy.Kind=="bell")continue;
                if(!b.HasPatrol||p==b.Patrol)
                {
                    var candidates=new List<Vector2I>();
                    for(int x=-3;x<=3;x++)for(int y=-3;y<=3;y++)
                    {
                        var v=b.Home+new Vector2I(x,y);if(v==p||v==heroPos||Manhattan(v,b.Home)>4||!EnemyTileFree(id,v))continue;candidates.Add(v);
                    }
                    if(candidates.Count==0){b.Pause=1;continue;}
                    var reachable=candidates.OrderBy(_=>rng.Next()).Where(v=>EnemyNextStep(id,p,v,8)!=null).Take(1).ToArray();
                    if(reachable.Length==0){b.Pause=1;continue;}
                    b.Patrol=reachable[0];b.HasPatrol=true;b.Pause=.45f+(float)rng.NextDouble()*.85f;continue;
                }
                target=b.Patrol;
            }
            var next=EnemyNextStep(id,p,target);
            if(!next.HasValue){if(b.Memory<=0)b.HasPatrol=false;b.Pause=.35f;continue;}
            if(next.Value==heroPos){if(b.Memory>0)StartBattle(enemy);if(screen!=Screen.World)return;continue;}
            var d=next.Value-p;enemyFacing[id]=d==Vector2I.Down?0:d==Vector2I.Left?1:d==Vector2I.Right?2:3;
            enemyPositions[id]=next.Value;
            if(b.Memory>0)b.Pause=Profile87(enemy).Pause*(.7f+(float)rng.NextDouble()*.6f);
            if(b.Memory<=0)b.Pause=(game.State.Map is "casa_medico" or "strada_bassa" ? .12f : .25f)+(float)rng.NextDouble()*(game.State.Map is "casa_medico" or "strada_bassa" ? .18f : .35f);
        }
    }
    private void RetreatFromEnemy(string id)
    {
        if(enemyBrains.TryGetValue(id,out var b)){b.Grace=2.8f;b.Memory=0;b.HasPatrol=false;b.Pause=1f;}
        var heroPos=new Vector2I(game.State.TileX,game.State.TileY);
        if(!enemyPositions.TryGetValue(id,out var enemy))return;
        var safe=NeighborSteps.Select(d=>heroPos+d).Where(p=>EnemyTileFree("",p)&&Manhattan(p,enemy)>Manhattan(heroPos,enemy)).OrderByDescending(p=>Manhattan(p,enemy)).ToArray();
        if(safe.Length>0){game.State.TileX=safe[0].X;game.State.TileY=safe[0].Y;visibleHero=TileCenter(safe[0].X,safe[0].Y);}
    }
    private string EnemyAtlas58(WorldEnemy e)=>e.Kind=="zombie"&&e.Id.EndsWith("4")?"anim_zombie_alt":EnemyAtlas(e.Kind);
    private void CheckEnemy58()
    {
        int count=0;
        void Check(bool condition,string message){if(!condition)throw new InvalidOperationException(message);count++;}
        game.StartNew(LaTerzaCampana.Core.HeroId.Gladia,LaTerzaCampana.Core.Difficulty.Sopravvivenza);
        ResetEventRules();screen=Screen.World;EnterMap("casa_olmo",14,16);
        game.State.StoryFlags.Add("pentesilea_met");game.State.DefeatedEnemies.Add("casa_olmo:olmo_undead_1");game.State.DefeatedEnemies.Add("casa_olmo:olmo_undead_2");RebuildEnemies();
        Check(!EnemyCanSee(new(10,12),new(13,12)),"wall must occlude sight");
        Check(EnemyCanSee(new(14,13),new(14,16)),"clear corridor sight");
        Check(EnemyNextStep("test",new(10,12),new(13,12)) is not null,"detour through open doorway");
        Check(EnemyNextStep("test",new(10,12),new(13,12),4) is null,"short hearing path blocked");
        enemyPositions.Clear();enemyBrains.Clear();enemyFacing.Clear();
        string id="olmo_undead_3";var start=new Vector2I(14,12);enemyPositions[id]=start;enemyFacing[id]=0;
        enemyBrains[id]=new EnemyBrain(start,TileCenter(start.X,start.Y));
        MoveEnemies58();Check(enemyPositions[id]==new Vector2I(14,13),"enemy pursues visible player");
        Check(enemyBrains[id].Memory>0,"last seen memory");
        UpdateEnemyVisuals(.1f);Check(enemyBrains[id].Visual!=TileCenter(14,13)&&enemyBrains[id].Visual!=TileCenter(14,12),"movement interpolation");
        enemyBrains[id].Visual=TileCenter(14,13);RetreatFromEnemy(id);Check(enemyBrains[id].Grace>0,"flee grace");
        Check(!Blocked(game.State.Map,game.State.TileX,game.State.TileY),"flee landing not blocked");
        var before=enemyPositions[id];MoveEnemies58();Check(enemyPositions[id]==before,"no attack during grace");
        game.State.TileX=25;game.State.TileY=19;
        enemyBrains[id].Grace=0;enemyBrains[id].Memory=0;enemyBrains[id].Pause=0;enemyBrains[id].HasPatrol=false;
        MoveEnemies58();Check(enemyBrains[id].HasPatrol,"enemy patrols when no player sensed");
        enemyBrains[id].Pause=0;var patrolStart=enemyPositions[id];MoveEnemies58();
        Check(enemyPositions[id]!=patrolStart&&!Blocked("casa_olmo",enemyPositions[id].X,enemyPositions[id].Y),"patrol takes walkable step");
        game.State.StoryFlags.Remove("pentesilea_met");game.State.DefeatedEnemies.Clear();ResetEventRules();EnterMap("casa_olmo",14,16);
        for(int i=0;i<12;i++)eventRules!.OnTileChanged();StartPentesileaCutscene();
        Check(sceneCorpses.Count==2,"two bodies in scene");
        Check(sceneCorpses.All(p=>!Blocked("casa_olmo",p.X,p.Y)&&p!=new Vector2I(game.State.TileX,game.State.TileY)),"bodies placed on clear floor");
        Check(!enemyPositions.ContainsKey("olmo_undead_1")&&!enemyPositions.ContainsKey("olmo_undead_2"),"no duplicate active zombies");
        GD.Print($"AI58_CHECKS_PASSED: {count}");GetTree().Quit();
    }
}
