using System;
using System.Linq;
using System.Collections.Generic;
using Godot;
using LaTerzaCampana.Core;
public partial class GameRoot {
    private record Gait87(float Speed,float Chase,float Cadence,float Bob,float Sway,float Tilt,float Compression,float Memory,float Pause);
    private Gait87 Profile87(WorldEnemy e)=> (e.Kind=="zombie"?Z74(e.Id).Key:e.Kind) switch {
        "heavy"=>new(1.85f,1.30f,4.6f,3.4f,3.0f,.075f,.075f,7f,.18f),
        "cyst"=>new(2.65f,1.55f,7.2f,2.3f,2.1f,.10f,.045f,5.5f,.12f),
        "thin"=>new(4.25f,1.65f,13.5f,5.2f,1.5f,.17f,.06f,5f,.02f),
        "eye"=>new(3.1f,1.45f,8.6f,2.1f,3.2f,.13f,.035f,4.6f,.13f),
        "hunch"=>new(2.8f,1.50f,9.4f,7.4f,2.5f,.21f,.12f,6f,.16f),
        "flayed"=>new(3.75f,1.65f,15f,3.8f,3.3f,.15f,.07f,7.5f,.03f),
        "falciante"=>new(3.4f,1.65f,10.5f,3.6f,1.4f,.15f,.06f,8f,.04f),
        "ossuary"=>new(4.4f,1.65f,11.3f,6.4f,2.7f,.15f,.1f,9f,.04f),
        "stitch"=>new(3.1f,1.6f,9.2f,4.6f,2.3f,.20f,.09f,6.4f,.10f),
        "sexton"=>new(2.8f,1.5f,6.8f,2.7f,1.5f,.10f,.04f,7.2f,.13f),
        "bell"=>new(1.7f,1.2f,3.8f,1.7f,1.2f,.05f,.07f,9f,.22f),
        _=>new(3.2f,1.45f,9f,2.8f,2.2f,.08f,.045f,5.8f,.10f)};
    private void DrawEnemyGait87(WorldEnemy e,Vector2 at,int facing,bool moving,bool chasing,float width,float height){
        var tex=Art(EnemyAtlas59(e));if(tex==null||tex.GetWidth()<640||tex.GetHeight()<376)return;
        var g=Profile87(e);float cadence=g.Cadence*(chasing?1.36f:1f);float phase=tick*cadence+e.X*.17f+e.Y*.13f;
        float wave=Mathf.Sin(phase),step=Mathf.Abs(Mathf.Sin(phase)),amount=moving?1f:chasing?.38f:.12f;
        string kind=e.Kind=="zombie"?Z74(e.Id).Key:e.Kind;
        if(kind=="heavy")step=Mathf.Pow(step,3);else if(kind=="hunch")step=Mathf.Pow(step,.55f);else if(kind=="flayed")wave+=.24f*Mathf.Sin(phase*3.2f);else if(kind=="cyst")wave+=.35f*Mathf.Sin(phase*2.7f);
        int frame=moving?((int)(phase*1.27f)%8+8)%8:0;
        float sy=at.Y-camera.Y+15,sx=at.X-camera.X;if(sx< -110||sx>W+110||sy< -110||sy>H+160)return;
        float bob=g.Bob*step*amount*(chasing?1.25f:1f);float tilt=g.Tilt*wave*amount;
        if(chasing&&(facing==1||facing==2))tilt+=(facing==1?-.075f:.075f);
        DrawEllipse(sx,sy,width*(.23f-bob*.004f),5f,new Color(.04f,.03f,.025f,.38f));
        DrawSetTransform(new Vector2(sx+g.Sway*wave*amount,sy-bob),tilt,new Vector2(1+g.Compression*wave*amount,1-g.Compression*wave*amount));
        DrawTextureRectRegion(tex,new Rect2(-width/2,-height,width,height),new Rect2(frame*80,facing*94,80,94));DrawSetTransform(Vector2.Zero);
    }
    private void Check87(){int n=0;void C(bool ok,string msg){if(!ok)throw new Exception(msg);n++;}
        game.StartNew(HeroId.Gladia,Difficulty.Sopravvivenza);ResetEventRules();screen=Screen.World;
        foreach(var (id,x,y) in new[]{("strada_bassa",15,4),("casa_olmo",14,16),("casa_olmo_servizio",10,12),("casa_medico",3,18),("casa_sarto",14,10),("casa_cantina",4,10),("rifugio_basso",9,11),("magazzino_basso",9,11),("officina_bassa",9,11),("archivio_basso",9,11)}){
            EnterMap(id,x,y);C(Art("map87_"+id)!=null,"map "+id);var start=new Vector2I(game.State.TileX,game.State.TileY);C(!Blocked(id,start.X,start.Y),"spawn "+id);
            var seen=new HashSet<Vector2I>{start};var q=new Queue<Vector2I>(seen);while(q.Count>0){var v=q.Dequeue();foreach(var step in NeighborSteps){var next=v+step;if(!Blocked(id,next.X,next.Y)&&seen.Add(next))q.Enqueue(next);}}
            foreach(var d in CurrentMap().Doors)C(seen.Any(v=>Math.Abs(v.X-d.X)+Math.Abs(v.Y-d.Y)<=1),"reachable door "+id+":"+d.Id);
            foreach(var p in CurrentMap().Props.Where(p=>p.Id.StartsWith("decor87_"))){C(Art(p.ArtFile)!=null,"art "+p.Id);C(seen.Any(v=>Math.Abs(v.X-p.X)+Math.Abs(v.Y-p.Y)<=2),"reachable prop "+p.Id);}
            foreach(var e in CurrentMap().Enemies)C(seen.Contains(new(e.X,e.Y)),"enemy "+e.Id);
        }
        var profiles=Enumerable.Range(1,6).Select(i=>Profile87(new WorldEnemy{Kind="zombie",Id="test"+i})).Concat(new[]{"falciante","ossuary","stitch","sexton","bell","corrupted"}.Select(k=>Profile87(new WorldEnemy{Kind=k,Id=k}))).ToArray();C(profiles.Distinct().Count()==12,"distinct profiles");C(profiles.All(p=>p.Chase>1&&p.Cadence>0),"chase acceleration");GD.Print($"GAIT87_OK {n}");GetTree().Quit();}
}
