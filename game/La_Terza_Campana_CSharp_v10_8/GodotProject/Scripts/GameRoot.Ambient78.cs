using System;
using System.Collections.Generic;
using Godot;
using LaTerzaCampana.Core;
public partial class GameRoot
{
    private readonly List<Vector2> deadSwallows78=new();
    private readonly List<Vector2> fallingSwallows78=new();
    private float swallowClock78=9f, swallowDropDelay78=-1f;
    private void ResetStreetOmens78(){deadSwallows78.Clear();fallingSwallows78.Clear();swallowClock78=6f+(float)rng.NextDouble()*5f;swallowDropDelay78=-1f;}
    private double PlayerHitChance78(Battle b,string part)
    {
        double chance=part switch{"torace"=>.88,"testa"=>.60,"braccioS"=>.73,"braccioD"=>.73,"gambaS"=>.68,"gambaD"=>.68,_=>.74};
        if(game.State.Injuries.Contains("braccioD")||game.State.Injuries.Contains("braccioS"))chance-=.10;
        if(b.Parts["gambaS"]==0&&b.Parts["gambaD"]==0)chance+=.12;
        else if(b.Parts["gambaS"]==0||b.Parts["gambaD"]==0)chance+=.06;
        chance+=(CombatStats100.Effective(game).Agility-EnemyAgility100(b))*.008;
        if(game.State.Mind<28)chance-=.06;
        return Math.Clamp(chance,.28,.95);
    }
    private double EnemyHitChance78(Battle b,int livingArms)
    {
        double chance=b.Kind switch{"bell"=>.82,"ossuary"=>.77,"sexton"=>.72,"corrupted"=>.74,"zombie"=>.69,_=>.71};
        if(livingArms<=1)chance-=.11;
        if(b.Parts["gambaS"]==0&&b.Parts["gambaD"]==0)chance-=.18;
        else if(b.Parts["gambaS"]==0||b.Parts["gambaD"]==0)chance-=.08;
        chance+=(EnemyAgility100(b)-CombatStats100.Effective(game).Agility)*.008;
        return Math.Clamp(chance,.18,.92);
    }
    private bool RollHit78(double chance,out bool graze)
    {
        double r=rng.NextDouble();
        graze=false;
        if(r<chance)return true;
        if(r<Math.Min(.98,chance+.12)){graze=true;return true;}
        return false;
    }
    private void UpdateStreetOmens78(float dt)
    {
        if(screen!=Screen.World)return;
        if(game.State.Map!="strada"){fallingSwallows78.Clear();swallowDropDelay78=-1f;return;}
        swallowClock78-=dt;
        if(swallowClock78<=0 && swallowDropDelay78<0 && fallingSwallows78.Count==0)
        {
            swallowClock78=12f+(float)rng.NextDouble()*7f;
            swallowDropDelay78=.85f;
            if(deadSwallows78.Count<3)PlaySound("swallow78",-10);
        }
        if(swallowDropDelay78>=0)
        {
            swallowDropDelay78-=dt;
            if(swallowDropDelay78<=0)
            {
                for(int i=0;i<2+rng.Next(2);i++)
                {
                    float x=Mathf.Clamp(visibleHero.X+(-4+rng.Next(9))*T*0.65f, T*2, CurrentMap().W*T-T*2);
                    float y=Math.Max(camera.Y-20, T*2);
                    fallingSwallows78.Add(new Vector2(x,y));
                }
                Notify("Le rondini si interrompono di colpo. Qualcosa cade sul selciato.",2.4f);
                PlaySound("bird_drop78",-8);
                swallowDropDelay78=-1f;
            }
        }
        for(int i=fallingSwallows78.Count-1;i>=0;i--)
        {
            var p=fallingSwallows78[i];
            p.Y+=dt*(230f+25f*i);
            p.X+=Mathf.Sin(tick*7f+i)*10f*dt;
            fallingSwallows78[i]=p;
            if(p.Y>visibleHero.Y+T*3.8f||p.Y>(CurrentMap().H-2)*T)
            {
                fallingSwallows78.RemoveAt(i);
                deadSwallows78.Add(new Vector2(p.X,Mathf.Min(p.Y,(CurrentMap().H-2)*T)));
                while(deadSwallows78.Count>6)deadSwallows78.RemoveAt(0);
            }
        }
    }
    private void DrawStreetOmens78()
    {
        if(screen!=Screen.World||game.State.Map!="strada")return;
        var dead=Art("swallow78_dead"); var fly=Art("swallow78");
        foreach(var p in deadSwallows78)
        {
            var q=p-camera;
            if(dead!=null)DrawTextureRect(dead,new Rect2(q.X-11,q.Y-6,22,12),false,new Color(.92f,.92f,.92f,.95f));
            else {DrawLine(q+new Vector2(-5,0),q+new Vector2(5,0),new Color(.12f,.10f,.10f),2); DrawLine(q+new Vector2(0,0),q+new Vector2(4,3),new Color(.12f,.10f,.10f),2);}
        }
        foreach(var p in fallingSwallows78)
        {
            var q=p-camera;
            if(fly!=null)DrawTextureRect(fly,new Rect2(q.X-8,q.Y-5,16,10),false,new Color(.95f,.95f,.95f,.95f));
            else {DrawLine(q+new Vector2(-5,-2),q+new Vector2(0,0),new Color(.16f,.14f,.14f),2); DrawLine(q+new Vector2(5,-2),q+new Vector2(0,0),new Color(.16f,.14f,.14f),2);}
        }
    }
}
