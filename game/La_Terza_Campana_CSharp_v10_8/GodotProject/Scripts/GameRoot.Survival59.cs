using System;
using System.Linq;
using System.Collections.Generic;
using Godot;
using LaTerzaCampana.Core;

public partial class GameRoot
{
    private void DrawPentesileaFire59(Vector2 at,int frame,bool flip)
    {var tex=Art("anim_pentesilea_fire5974");if(tex==null)return;var p=at-camera;PenteShadow74(p,87);float kick=frame>=4?Mathf.Sin(eventTime*21)*2.8f:0;DrawSetTransform(p+new Vector2(0,10),kick*.009f,new Vector2(flip?-1:1,1));DrawTextureRectRegion(tex,new Rect2(-49+kick,-94,98,98),new Rect2((frame%4)*160,(frame/4)*160,160,160));DrawSetTransform(Vector2.Zero);}
    private string EnemyAtlas59(WorldEnemy e)=>e.Kind=="falciante"?"anim_falciante84":e.Kind=="zombie"?"anim_z74_"+Z74(e.Id).Key:EnemyAtlas(e.Kind);
    private string ZombieBattleArt(string id)=>"battle_z74_"+Z74(id).Key;
    private void LoadBattleInjuries59(WorldEnemy e)
    {
        if(battle==null)return;
        battle.Armed=e.Kind is "falciante" or "sexton" or "corrupted" or "stitch";
        if(e.Kind=="zombie"){var z=Z74(e.Id);battle.Parts=new(){{"torace",z.Torso},{"testa",z.Head},{"braccioS",z.Arm},{"braccioD",z.Arm},{"gambaS",z.Leg},{"gambaD",z.Leg}};zombieHit74=zombieSwing74=-10;}
        if(game.State.EnemyParts.TryGetValue(game.State.Map+":"+e.Id,out var previous))
            foreach(var part in Parts)if(previous.TryGetValue(part,out int hp))battle.Parts[part]=hp;
        battle.Log=e.Kind=="falciante"?"La roncola può recidere un arto per sempre. Disarmalo o difenditi.":battle.Armed?"Il nemico impugna un'arma. Colpisci il braccio destro per disarmarlo.":"La creatura si avvicina. Scegli dove colpire.";
    }
    private void SaveBattleInjuries59(){if(battle!=null)game.State.EnemyParts[game.State.Map+":"+battle.Id]=new(battle.Parts);}
    private void DrawBattleHero59()=>DrawHeroRig95();
    private void DrawBattleAnatomy59()
    {
        if(battle==null)return;
        Panel(24,72,184,235,.8f);Text("ANATOMIA",40,99,17,Gold);
        for(int i=0;i<Parts.Length;i++){
            string p=Parts[i];int hp=battle.Parts[p];float y=127+i*27;
            if(battle.Phase==1&&i==target75)Rect(30,y-17,171,23,new Color(.48f,.32f,.14f,.48f));Text(PartName(p),37,y,14,hp==0?Red:battle.Phase==1&&i==target75?Gold:Cream);Text(hp==0?"—":hp.ToString(),164,y,14,hp==0?Red:Gold);
        }
        if(battle.Armed)Text(battle.Parts["braccioD"]==0?"DISARMATO":"ARMA: DX",40,289,15,battle.Parts["braccioD"]==0?Red:Gold);
        if(battle.Kind is "zombie" or "corrupted"){
            string k=battle.Kind=="corrupted"?"corrupted":ZombieBattleArt(battle.Id).Replace("battle_","");
            var tex=Art("combat_parts_"+k);if(tex!=null){
                // The anatomical left side is the viewer's right in the front view.
                int mask=(battle.Parts["braccioD"]==0?1:0)|(battle.Parts["braccioS"]==0?2:0)|(battle.Parts["gambaD"]==0?4:0)|(battle.Parts["gambaS"]==0?8:0);
                Panel(1050,240,185,218,.66f);Text("STATO FISICO",1063,265,15,Gold);
                var old=TextureFilter;TextureFilter=TextureFilterEnum.Nearest;DrawTextureRectRegion(tex,new Rect2(1062,269,160,188),new Rect2((mask%4)*80,(mask/4)*94,80,94));TextureFilter=old;
            }
        }
    }
    private void DrawDarkness59()
    {
        if(game.State.Map is not ("casa_olmo" or "binari" or "casa_olmo_servizio" or "casa_medico" or "casa_cantina" or "archivio_basso" or "rifugio_basso"))return;
        bool cinematic=eventScene==ExtraScene.Pentesilea||pentesileaDialogue;
        var center=cinematic?(visibleHero.Lerp(pentesileaDialogue?pentesileaStop:pentesileaArrival,.45f)-camera):visibleHero-camera;float radius=T*(cinematic?5.8f:game.State.Map=="casa_medico"?5.4f:game.State.Map=="rifugio_basso"?4.4f:4.0f);
        var tex=Art("light_mask59");if(tex==null)return;
        float opacity=cinematic?.91f:game.State.Map=="casa_medico"?.87f:game.State.Map=="rifugio_basso"?.89f:.93f;var dark=new Color(0,0,0,opacity);
        float l=center.X-radius,r=center.X+radius,t=center.Y-radius,b=center.Y+radius;
        if(l>0)Rect(0,0,l,H,dark);if(r<W)Rect(r,0,W-r,H,dark);
        if(t>0)Rect(Math.Max(0,l),0,Math.Min(W,r)-Math.Max(0,l),t,dark);
        if(b<H)Rect(Math.Max(0,l),b,Math.Min(W,r)-Math.Max(0,l),H-b,dark);
        DrawTextureRect(tex,new Rect2(l,t,r-l,b-t),false,new Color(1,1,1,opacity));
    }
    private void RecordCorpse59(string id,string kind,Vector2I? location=null)
    {
        string key=game.State.Map+":"+id;if(game.State.Corpses.ContainsKey(key))return;
        var e=CurrentMap().Enemies.FirstOrDefault(e=>e.Id==id);
        var at=location??enemyPositions.GetValueOrDefault(id,e==null?new Vector2I(game.State.TileX,game.State.TileY):new Vector2I(e.X,e.Y));
        if(Blocked(game.State.Map,at.X,at.Y)){var near=NeighborSteps.Select(d=>at+d).FirstOrDefault(v=>!Blocked(game.State.Map,v.X,v.Y));if(near!=Vector2I.Zero)at=near;}
        string[] loot=kind=="zombie"?Z74(id).Loot:new[]{"","pane","mela","bende","erbe","conserva","infuso"};
        string item=loot[rng.Next(loot.Length)];game.State.Corpses[key]=new FallenEnemy(game.State.Map,id,kind,at.X,at.Y,item,item==""?0:1);
    }
    private void Migrate59()
    {
        if(game.State.StoryFlags.Add("geometry_v59"))foreach(var key in game.State.CollisionEdits.Keys.Where(k=>k.StartsWith("casa_olmo:")).ToArray())game.State.CollisionEdits.Remove(key);
        if(game.State.Map=="casa_olmo"&&game.State.StoryFlags.Contains("pentesilea_met")){
            var old=SavedOlmoCorpses();for(int i=0;i<old.Count;i++)RecordCorpse59("olmo_undead_"+(i+1),"zombie",old[i]);
        }
        foreach(var e in CurrentMap().Enemies)if(game.State.DefeatedEnemies.Contains(game.State.Map+":"+e.Id)&&!game.State.StoryFlags.Contains("spared_"+e.Id))RecordCorpse59(e.Id,e.Kind,new(e.X,e.Y));
    }
    private void AddCorpses59(List<(float depth,Action draw)> sprites)
    {
        foreach(var c in game.State.Corpses.Values.Where(c=>c.Map==game.State.Map)){
            var at=TileCenter(c.X,c.Y)-camera;string suffix=c.Kind=="zombie"?"z74_"+Z74(c.Id).Key:c.Kind;
            var tex=Art("corpse_"+suffix);if(tex==null)continue;
            sprites.Add((c.Y+.35f,()=>{float w=Math.Min(T*1.65f,tex.GetWidth()),h=w*tex.GetHeight()/tex.GetWidth();bool vertical=Blocked(c.Map,c.X-1,c.Y)||Blocked(c.Map,c.X+1,c.Y);DrawEllipse(at.X,at.Y+5,vertical?13:28,vertical?24:10,new Color(.18f,.035f,.025f,.38f));DrawSetTransform(at+new Vector2(0,4),vertical?Mathf.Pi/2:0);DrawTextureRect(tex,new Rect2(-w/2,-h/2,w,h),false);DrawSetTransform(Vector2.Zero);}));
        }
    }
    private void AddCorpseTargets(List<Target> options,float x,float y,float tx,float ty)
    {
        foreach(var pair in game.State.Corpses.Where(c=>c.Value.Map==game.State.Map)){
            var c=pair.Value;var r=new double[]{c.X,c.Y,1,1};if(DistanceToRect(x,y,r)<=1.1f&&EnemyCanSee(new((int)x,(int)y),new(c.X,c.Y)))options.Add(new("corpse",pair.Key,c.Kind=="zombie"?"Cadavere — "+Z74(c.Id).Name.ToLowerInvariant():"Cadavere",pair.Key,DistanceToRect(tx,ty,r)+.05f));
        }
    }
    private void LootCorpse59(string key)
    {
        if(!game.State.Corpses.TryGetValue(key,out var c))return;
        if(!game.State.SearchedContainers.Add("corpse:"+key)){Notify("Hai già frugato questo corpo.");return;}
        if(c.Quantity<=0){Notify("Niente di utile addosso al cadavere.");return;}
        game.AddItem(c.Loot,c.Quantity);Notify($"**x{c.Quantity} {Catalog.Items[c.Loot]}**",4);
    }
    private void CheckSurvival59()
    {
        int checks=0;void Check(bool ok,string message){if(!ok)throw new InvalidOperationException(message);checks++;}
        game.StartNew(HeroId.Gladia,Difficulty.Sopravvivenza);ResetEventRules();screen=Screen.World;EnterMap("casa_olmo",14,16);
        var e=CurrentMap().Enemies.First(x=>x.Id=="olmo_undead_4");StartBattle(e);Check(battle!.Parts.Count==6,"six parts");
        battle.Parts["braccioD"]=0;battle.Parts["braccioS"]=0;game.State.Bleeding=false;int before=game.State.Body;EnemyTurn();Check(game.State.Body==before,"disabled arms stop damage");
        battle.Parts["gambaS"]=0;SaveBattleInjuries59();battle=null;StartBattle(e);Check(battle!.Parts["gambaS"]==0&&battle.Parts["braccioD"]==0,"persist wounds");
        var pos=enemyPositions[e.Id];WinBattle();string key="casa_olmo:"+e.Id;Check(game.State.Corpses.TryGetValue(key,out var c)&&c.X==pos.X&&c.Y==pos.Y,"body remains at enemy location");
        game.State.Corpses[key]=new FallenEnemy("casa_olmo",e.Id,e.Kind,pos.X,pos.Y,"bende",1);int amount=game.State.Inventory.GetValueOrDefault("bende");LootCorpse59(key);LootCorpse59(key);Check(game.State.Inventory.GetValueOrDefault("bende")==amount+1,"one-time loot");
        var saved=GameRules.FromJson(game.ToJson());Check(saved.State.Corpses.ContainsKey(key)&&saved.State.SearchedContainers.Contains("corpse:"+key),"corpse and loot saved");
        game.StartNew(HeroId.Gladia,Difficulty.Sopravvivenza);ResetEventRules();screen=Screen.World;EnterMap("casa_olmo",14,16);for(int i=0;i<12;i++)eventRules!.OnTileChanged();StartPentesileaCutscene();Check(sceneCorpses.Count==2,"scene bodies staged");
        for(int i=0;i<sceneCorpses.Count;i++)RecordCorpse59("olmo_undead_"+(i+1),"zombie",sceneCorpses[i]);Check(game.State.Corpses.Count==2,"scene loot record");
        GD.Print($"SURVIVAL59_CHECKS_PASSED: {checks}");GetTree().Quit();
    }
}
