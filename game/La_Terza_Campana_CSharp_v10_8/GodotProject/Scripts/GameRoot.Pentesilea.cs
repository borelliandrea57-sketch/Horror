using System;
using System.Linq;
using System.Collections.Generic;
using Godot;
using LaTerzaCampana.Core;
using LaTerzaCampana.Events;

public partial class GameRoot
{
    private enum ExtraScene { None, Pentesilea, Stillness }
    private ExtraScene eventScene;
    private PentesileaAndStillnessLogic? eventRules;
    private bool movedThisFrame, pentesileaDialogue, stillnessDeath;
    private AudioStreamPlayer? eventAudio;
    private float eventTime;
    private int eventSoundIndex, pentesileaLineIndex, pentesileaChoice;
    private Vector2 pentesileaArrival, pentesileaStop;
    private readonly List<Vector2I> sceneCorpses=new();
    private readonly List<(string Speaker,string Text,string Portrait)> pentesileaLines=new();
    private const string PentesileaPortrait="portrait_pentesilea";
    private static readonly (float Time,string Sound)[] IntroSounds={
        (0,"event_shots"),(2,"event_zombies"),(3,"event_burst"),(4.5f,"event_burst"),
        (6,"event_burst"),(8,"event_shots"),(9,"event_brass"),(10,"event_drag")};

    private void InstallPentesileaNpc()
    {
        if(!maps.TryGetValue("casa_olmo",out var house))return;
        if(!house.Npcs.Any(n=>n.Id=="pentesilea"))
            house.Npcs.Add(new WorldNpc{Id="pentesilea",Name="Pentesilea",Portrait=PentesileaPortrait,X=13,Y=5});
        if(!house.Doors.Any(d=>d.Id=="olmo_service"))
            house.Doors.Add(new WorldDoor{Id="olmo_service",Label="Passaggio di servizio",X=10,Y=3,To="casa_olmo_servizio",Spawn=new double[]{10,11}});
        if(!maps.ContainsKey("casa_olmo_servizio"))
        {
            var room=new WorldMap{Name="Casa dell’Olmo — stanza di servizio",W=19,H=16,Style="chapel"};
            room.Blocks.AddRange(new[]{new double[]{0,0,19,3},new double[]{0,0,3,16},new double[]{16,0,3,16},new double[]{0,15,19,1}});
            room.Doors.Add(new WorldDoor{Id="return_olmo",Label="Torna nella Casa dell’Olmo",X=10,Y=13,To="casa_olmo",Spawn=new double[]{14,3}});
            room.Props.Add(new WorldProp{Kind="crate",Id="olmo_service_crate",Name="Cassa di servizio",X=7,Y=6,ArtFile="art_02_baule_tavole",Scale=.75,Solid=new double[]{0,0,1,1},Note="La polvere odora di ferro. Qualcuno ha spostato di recente questa cassa."});
            maps.Add("casa_olmo_servizio",room);
        }
    }
    private bool HidePentesileaDoor(string id)=>id=="olmo_service"&&!game.State.StoryFlags.Contains("pentesilea_bandage_given");
    private bool HidePentesileaNpc(string id)=>id=="pentesilea"&&
        (!game.State.StoryFlags.Contains(PentesileaAndStillnessLogic.PentesileaMetFlag)||pentesileaDialogue);
    private void ResetEventRules()
    {
        eventRules=new PentesileaAndStillnessLogic(game.State.StoryFlags);
        eventRules.EnterMap(game.State.Map);eventRules.ResetAfterLoadingGame();
        eventScene=ExtraScene.None;eventTime=0;pentesileaDialogue=false;stillnessDeath=false;
        pentesileaLines.Clear();sceneCorpses.Clear();pentesileaLineIndex=0;pentesileaChoice=0;eventAudio?.Stop();
    }
    private void AdvanceStillness(double delta)
    {
        if(eventRules==null)return;
        var signals=eventRules.Advance(delta,true);
        if(signals.Contains(EventSignal.StillnessDeath120)){StartStillnessDeath();return;}
        if(signals.Contains(EventSignal.StillnessWarning110))PlaySound("event_drone",-4);
        else if(signals.Contains(EventSignal.StillnessWarning90))PlaySound("event_heartbeat",-3);
    }
    private void StartPentesileaCutscene()
    {
        if(eventRules?.IntroPending!=true||screen!=Screen.World)return;
        eventScene=ExtraScene.Pentesilea;eventTime=0;eventSoundIndex=0;walking=false;
        visibleHero=TileCenter(game.State.TileX,game.State.TileY);
        // Find a free nearby staging tile. The scene never introduces combatants or collisions.
        var s=game.State;
        var offsets=new[]{new Vector2I(0,-2),new Vector2I(0,2),new Vector2I(2,0),new Vector2I(-2,0),new Vector2I(0,-1),new Vector2I(0,1),new Vector2I(1,0),new Vector2I(-1,0)};
        pentesileaStop=TileCenter(15,4);
        foreach(string id in new[]{"olmo_undead_1","olmo_undead_2"})enemyPositions.Remove(id);
        foreach(var o in offsets)
        {
            int x=s.TileX+o.X,y=s.TileY+o.Y;
            if(!Blocked(s.Map,x,y)&&!enemyPositions.Values.Contains(new Vector2I(x,y))&&EnemyCanSee(new Vector2I(s.TileX,s.TileY),new Vector2I(x,y))){pentesileaStop=TileCenter(x,y);break;}
        }
        // Short approach from a nearby tile, avoiding a path across the furniture.
        pentesileaArrival=pentesileaStop;
        int px=(int)(pentesileaStop.X/T),py=(int)(pentesileaStop.Y/T);
        foreach(var d in new[]{new Vector2I(0,-1),new Vector2I(0,1),new Vector2I(-1,0),new Vector2I(1,0)})
            if(!Blocked(s.Map,px+d.X,py+d.Y)&&!enemyPositions.Values.Contains(new Vector2I(px+d.X,py+d.Y))&&(px+d.X!=s.TileX||py+d.Y!=s.TileY)){pentesileaArrival=TileCenter(px+d.X,py+d.Y);break;}
        PlaceSceneCorpses();
        screen=Screen.Cutscene;messageTimer=0;sanityPlayer?.Stop();monsterPlayer?.Stop();
        PlaySound(IntroSounds[0].Sound);eventSoundIndex=1;
    }
    private void StartStillnessDeath()
    {
        eventScene=ExtraScene.Stillness;eventTime=0;walking=false;stillnessDeath=true;
        screen=Screen.Cutscene;messageTimer=0;sanityPlayer?.Stop();monsterPlayer?.Stop();
        PlaySound("event_drone",-2);
    }
    private void UpdateExtraScene(float dt)
    {
        eventTime+=dt;
        if(eventScene==ExtraScene.Pentesilea)
        {
            while(eventSoundIndex<IntroSounds.Length&&eventTime>=IntroSounds[eventSoundIndex].Time)
                PlaySound(IntroSounds[eventSoundIndex++].Sound);
            if(eventTime>=12f){eventScene=ExtraScene.None;BeginPentesileaDialogue();}
        }
        else if(eventScene==ExtraScene.Stillness&&eventTime>=4f)
        {
            PlaySound("event_bell");eventScene=ExtraScene.None;
            game.State.Body=0;Die("Il mondo non dimentica chi smette di attraversarlo.");
        }
    }
    private void DrawPentesilea(Vector2 at,int direction,int frame)
    {
        var tex=Art("anim_pentesilea74");if(tex==null)return;
        float x=at.X-camera.X,y=at.Y-camera.Y;
        PenteShadow74(new Vector2(x,y),74);float breath=frame==0?Mathf.Sin(tick*2.5f)*1.2f:0;DrawTextureRectRegion(tex,new Rect2(x-36,y-86+breath,72,96-breath),new Rect2(frame%4*128,direction*128,128,128));
    }
    private void AddPentesileaSceneSprites(List<(float depth,Action draw)> sprites)
    {
        if((eventScene==ExtraScene.Pentesilea&&eventTime>=3)||pentesileaDialogue)
        {
            float progress=pentesileaDialogue?1:Mathf.Clamp((eventTime-7.7f)/2.3f,0,1);
            Vector2 at=pentesileaArrival.Lerp(pentesileaStop,progress);
            Vector2 travel=pentesileaStop-pentesileaArrival;
            Vector2 look=visibleHero-pentesileaStop;
            Vector2 facing=progress>=1?look:travel;
            int direction=Math.Abs(facing.Y)>=Math.Abs(facing.X)?(facing.Y>=0?0:3):(facing.X>=0?2:1);
            int frame=progress>=1?0:(int)(eventTime*7)%4;
            if(!pentesileaDialogue&&eventTime<7.7f){int fireFrame=eventTime<3.3f?2:((int)((eventTime-3)*8)%4)+4;var target=sceneCorpses.Count>0?TileCenter(sceneCorpses[eventTime<4.8f?0:Math.Min(1,sceneCorpses.Count-1)].X,sceneCorpses[eventTime<4.8f?0:Math.Min(1,sceneCorpses.Count-1)].Y):visibleHero;bool flip=target.X<at.X;sprites.Add((at.Y/T,()=>DrawPentesileaFire59(at,fireFrame,flip)));}
            else sprites.Add((at.Y/T,()=>DrawPentesilea(at,direction,frame)));
        }
        if(game.State.Map!="casa_olmo")return;
        bool inScene=eventScene==ExtraScene.Pentesilea||pentesileaDialogue;
        bool met=game.State.StoryFlags.Contains("pentesilea_met");
        if(!inScene)return;
        var bodies=inScene?sceneCorpses:SavedOlmoCorpses();
        for(int i=0;i<bodies.Count;i++)
        {
            int j=i;Vector2 at=TileCenter(bodies[i].X,bodies[i].Y);
            // Bodies are on the floor, underneath standing actors at the same depth.
            sprites.Add((at.Y/T-.10f,()=>DrawSceneUndead(at,j,inScene&&eventScene==ExtraScene.Pentesilea?Mathf.Clamp((eventTime-(3+j*1.8f))/.65f,0,1):1)));
        }
    }
    private void PlaceSceneCorpses()
    {
        sceneCorpses.Clear();var player=new Vector2I(game.State.TileX,game.State.TileY);
        var stop=new Vector2I((int)(pentesileaStop.X/T),(int)(pentesileaStop.Y/T));
        var arrival=new Vector2I((int)(pentesileaArrival.X/T),(int)(pentesileaArrival.Y/T));
        var options=new List<Vector2I>();
        for(int x=-3;x<=3;x++)for(int y=-3;y<=3;y++)
        {
            var p=stop+new Vector2I(x,y);if(p==player||p==stop||p==arrival||Blocked(game.State.Map,p.X,p.Y)||enemyPositions.Values.Contains(p)||!EnemyCanSee(stop,p))continue;
            options.Add(p);
        }
        foreach(var p in options.OrderByDescending(p=>CorpseClearance(p)).ThenByDescending(p=>Math.Min(2,Manhattan(p,stop))).ThenByDescending(p=>Manhattan(p,player)))
        {if(sceneCorpses.Any(q=>Manhattan(p,q)<2))continue;sceneCorpses.Add(p);if(sceneCorpses.Count==2)break;}
    }
    private int CorpseClearance(Vector2I p)
    {
        int horizontal=(!Blocked("casa_olmo",p.X-1,p.Y)?1:0)+(!Blocked("casa_olmo",p.X+1,p.Y)?1:0);
        int vertical=(!Blocked("casa_olmo",p.X,p.Y-1)?1:0)+(!Blocked("casa_olmo",p.X,p.Y+1)?1:0);
        return horizontal==2?4:vertical==2?3:Math.Max(horizontal,vertical);
    }
    private List<Vector2I> SavedOlmoCorpses()
    {
        var result=new List<Vector2I>();
        foreach(var flag in game.State.StoryFlags.Where(f=>f.StartsWith("olmo_body:")).OrderBy(f=>f))
        {var parts=flag.Split(':');if(parts.Length==4&&int.TryParse(parts[2],out int x)&&int.TryParse(parts[3],out int y)&&!Blocked("casa_olmo",x,y))result.Add(new Vector2I(x,y));}
        if(result.Count==0)foreach(var e in CurrentMap().Enemies.Where(e=>e.Id=="olmo_undead_1"||e.Id=="olmo_undead_2"))result.Add(new Vector2I(e.X,e.Y));
        return result;
    }
    private void DrawSceneUndead(Vector2 world,int index,float fall)
    {
        string suffix="z74_"+Z74("olmo_undead_"+(index+1)).Key;Vector2 at=world-camera;
        if(fall>=.99f)
        {
            var corpse=Art("corpse_"+suffix);if(corpse==null)return;
            float w=corpse.GetWidth()*.95f,h=corpse.GetHeight()*.95f;
            int cx=(int)(world.X/T),cy=(int)(world.Y/T);
            bool sideClear=!Blocked("casa_olmo",cx-1,cy)&&!Blocked("casa_olmo",cx+1,cy);
            bool vertical=!sideClear&&!Blocked("casa_olmo",cx,cy-1)&&!Blocked("casa_olmo",cx,cy+1);
            if(!sideClear&&!vertical){w=Math.Min(w,T*.83f);h*=.85f;}
            DrawEllipse(at.X,at.Y+9,vertical?h*.45f:w*.38f,vertical?w*.32f:h*.29f,new Color(.06f,.035f,.025f,.35f));
            if(vertical){DrawSetTransform(at+new Vector2(0,6),Mathf.Pi/2,new Vector2(.88f,1));DrawTextureRect(corpse,new Rect2(-w/2,-h/2,w,h),false);DrawSetTransform(Vector2.Zero);}
            else DrawTextureRect(corpse,new Rect2(at.X-w/2,at.Y-h/2+6,w,h),false);return;
        }
        var tex=Art("anim_"+suffix);if(tex==null)return;
        float angle=(index%2==0?-1:1)*fall*Mathf.Pi/2;
        DrawSetTransform(at+new Vector2(0,15),angle,new Vector2(1,1-fall*.12f));
        DrawTextureRectRegion(tex,new Rect2(-44,-89,88,89),new Rect2(0,(index%2==0?1:2)*94,80,94));
        DrawSetTransform(Vector2.Zero);
    }
    private void DrawExtraScene()
    {
        DrawWorld();
        if(eventScene==ExtraScene.Pentesilea)
        {
            float t=eventTime;
            // Slow, localized muzzle glow; no full-screen flashing or forced camera shake.
            Vector2 door=pentesileaArrival-camera;
            float glow=t>=3&&t<7.3f?Mathf.Max(0,1-((t-3)%1.5f)*2):0;
            if(glow>0&&sceneCorpses.Count>0){var at=pentesileaArrival-camera;var aim=TileCenter(sceneCorpses[t<4.8f?0:Math.Min(1,sceneCorpses.Count-1)].X,sceneCorpses[t<4.8f?0:Math.Min(1,sceneCorpses.Count-1)].Y)-camera;var muzzle=at+new Vector2(aim.X<at.X?-40:40,-41);DrawLine(muzzle,aim+new Vector2(0,-22),new Color(1,.72f,.32f,glow*.4f),1.4f);DrawCircle(muzzle,6+glow*8,new Color(1,.56f,.19f,glow*.65f));for(int j=0;j<4;j++)DrawEllipse(at.X,at.Y+9,15+j*12,5+j*4,new Color(.9f,.53f,.22f,glow*.045f));}
            for(int i=5;i>=1;i--)DrawCircle(door,18*i,new Color(.94f,.54f,.23f,glow*.025f));
            if(t>=3&&t<12)
                for(int i=0;i<20;i++)
                {
                    float drift=(t*13+i*13)%100;
                    var smoke=Art("smoke59");float size=35+i%5*5;if(smoke!=null){var at=door+new Vector2(Mathf.Sin(i*4.2f+t*.8f)*drift,-drift*.65f);DrawTextureRect(smoke,new Rect2(at-new Vector2(size/2,size/2),new Vector2(size,size)),false,new Color(.65f,.59f,.49f,.12f*(1-drift/125)));}
                }
            Rect(0,0,W,64,new Color(.02f,.015f,.015f,.94f));Rect(0,618,W,102,new Color(.02f,.015f,.015f,.95f));
            Text("CASA DELL’OLMO  ·  UN'ALTRA SOPRAVVISSUTA",32,40,20,Gold);
            string line=t<2?"Tre spari lontani. Poi un silenzio troppo breve.":t<3?"Un urlo rauco. Qualcosa cade dietro la parete.":t<8?"Raffiche. Le sagome si accasciano una dopo l'altra.":t<10?"Un ultimo sparo. I bossoli rotolano sulle assi.":"Il fumo si dirada. La ragazza abbassa l’arma e ti punta gli occhi addosso.";
            Paragraph(line,35,662,1180,23,31,Cream,2);
        }
        else
        {
            float t=eventTime;Vector2 at=visibleHero-camera;
            Rect(0,0,W,H,new Color(0,0,0,Mathf.Clamp(t*.16f,0,.78f)));
            DrawWaitingGod(at,Mathf.Clamp(t/2.5f,0,1),Mathf.Lerp(.36f,1f,Mathf.Clamp(t/2.1f,0,1)));
            if(t>2.8f)Rect(0,0,W,H,new Color(0,0,0,Mathf.Clamp((t-2.8f)/1.2f,0,1)));
        }
    }
    private void DrawWaitingGod(Vector2 at,float descent,float opacity)
    {
        var tex=Art("waiting94_"+((int)(tick*10)%12))??Art("waiting94");if(tex==null)return;
        float approach=Mathf.SmoothStep(0,1,Mathf.Clamp(descent,0,1));
        // Approach along the cobbles rather than hanging as a screen-sized portrait.
        float pulse=1+.018f*Mathf.Sin(tick*3.1f);float w=(240+approach*280)*pulse,h=w*tex.GetHeight()/tex.GetWidth();
        Vector2 feet=at+new Vector2(Mathf.Sin(tick*1.4f)*5,Mathf.Lerp(-240f,30f,approach));
        Vector2 core=feet-new Vector2(0,h*.48f);
        var shadow=Art("waiting94_shadow");if(shadow!=null)DrawTextureRect(shadow,new Rect2(feet.X-w*.48f,feet.Y-w*.22f,w*.96f,w*.25f),false,new Color(.75f,.68f,.59f,opacity*.85f));
        // Dark ambient illumination and a soft outline, matching the painted stone.
        DrawSetTransform(core,Mathf.Sin(tick*1.7f)*.012f,new Vector2(1,1+.014f*Mathf.Sin(tick*3)));
        DrawTextureRect(tex,new Rect2(-w/2,-h/2,w,h),false,new Color(.84f,.80f,.75f,opacity));DrawSetTransform(Vector2.Zero);
        for(int i=0;i<16;i++){float phase=(tick*.12f+i*.137f)%1f;float x=feet.X+Mathf.Sin(i*7.31f+tick*.55f)*w*.36f;float y=feet.Y-18-phase*48;DrawCircle(new Vector2(x,y),.7f+i%2,new Color(.52f,.40f,.28f,opacity*.14f*(1-phase)));}
    }
    private void DrawStillnessWarning()
    {
        if(eventRules==null)return;
        double seconds=eventRules.StationarySeconds;
        if(seconds>=30)
        {
            float strength=seconds>=45?.35f:.12f;
            for(int i=0;i<7;i++)
            {
                float a=strength*(1-i/7f)*.18f;
                Frame(i*9,i*9,W-i*18,H-i*18,new Color(.01f,0,.02f,a),18);
            }
            if(seconds<33)DrawWaitingGod(visibleHero-camera+new Vector2(32,-45),.75f,.25f);
            if(seconds>=45){DrawWaitingGod(visibleHero-camera+new Vector2(0,-35),.65f,.22f+.13f*(float)((seconds-45)/15));}

        }
    }
    private void AddPentesileaLine(string name,string line,string portrait="")=>pentesileaLines.Add((name,line,portrait));
    private void PLine(string text)=>AddPentesileaLine("Pentesilea",text,PentesileaPortrait);
    private void HLine(string text)=>AddPentesileaLine(Catalog.Heroes[game.State.Hero].Name,text,"portrait_"+HeroKey());
    private void BeginPentesileaDialogue()
    {
        pentesileaDialogue=true;pentesileaChoice=0;pentesileaLineIndex=0;pentesileaLines.Clear();
        AddPentesileaLine("????","Un altro?",PentesileaPortrait);
        AddPentesileaLine("","Sollevi istintivamente le mani. L'arma è più alta della ragazza che la regge. Fra i denti, quando sorride, luccica qualcosa di metallico.");
        HLine("Sono vivo! Non sparare!");AddPentesileaLine("????","Lo dicono tutti. Poi cominciano a mordere.",PentesileaPortrait);
        AddPentesileaLine("","Fa un passo avanti. Una cascata di capelli castani le striscia sulle assi sconnesse. Con la punta dello stivale gira il corpo di uno degli assalitori.");
        AddPentesileaLine("????","Niente occhi lucidi. Respiri. Ti tremano le mani. Va bene: per ora ti credo.",PentesileaPortrait);
        HLine("Chi sei?");PLine("Pentesilea. Non chiedermi come ho fatto a entrare, perché sto ancora cercando di capirlo.");
        switch(game.State.Hero)
        {
            case HeroId.Gladia:HLine("Sei ferita? Posso controllare.");PLine("Non è tutto sangue mio. E quello che lo è può aspettare.");break;
            case HeroId.Ambrosh:HLine("Queste creature mostrano una trasformazione impossibile.");PLine("L'impossibile non regge molto quando gli spari.");break;
            default:HLine("Quella mitragliatrice pesa più di te.");PLine("Eppure sono ancora io a portarla.");break;
        }
        HLine("Che cosa erano quelle cose?");PLine("Persone, qualche ora fa. Adesso sono la ragione per cui questo bestione pesa meno quando lo porto carico.");
        HLine("Hai... un proiettile tra i denti?");PLine("Un ricordo. Non è stato un appuntamento piacevole.");
        AddPentesileaLine("","Da qualche parte nelle fondamenta risuona un colpo lento, come una campana soffocata.");
        PLine("Ascoltami bene. Le strade si muovono. Le porte mentono. E qualunque cosa tu faccia... non restare fermo a fissare il vuoto troppo a lungo.");
        HLine("Perché?");PLine("Perché qui c'è qualcosa che nota chi aspetta.");
        NextPentesileaLine();
    }
    private void NextPentesileaLine()
    {
        if(pentesileaLineIndex<pentesileaLines.Count)
        {
            var line=pentesileaLines[pentesileaLineIndex++];Say(line.Speaker,line.Text,line.Portrait);return;
        }
        if(pentesileaChoice==0)
        {
            Say("Pentesilea","Adesso dimmi una cosa. Perché sei entrato proprio in questa casa?",PentesileaPortrait,
                new Choice("Volevo aiutare qualcuno",()=>ChoosePentesilea(1)),
                new Choice("Cercavo provviste",()=>ChoosePentesilea(2)),
                new Choice("Non mi fido di te",()=>ChoosePentesilea(3)));return;
        }
        eventRules!.FinishPentesileaIntro(pentesileaChoice);pentesileaDialogue=false;
        for(int i=0;i<sceneCorpses.Count;i++){game.State.StoryFlags.Add($"olmo_body:{i}:{sceneCorpses[i].X}:{sceneCorpses[i].Y}");RecordCorpse59("olmo_undead_"+(i+1),"zombie",sceneCorpses[i]);}
        foreach(string id in new[]{"olmo_undead_1","olmo_undead_2"})game.State.DefeatedEnemies.Add("casa_olmo:"+id);
        var npc=CurrentMap().Npcs.FirstOrDefault(n=>n.Id=="pentesilea");
        if(npc!=null){npc.X=(int)(pentesileaStop.X/T);npc.Y=(int)(pentesileaStop.Y/T);}
        pentesileaLines.Clear();CloseDialogue();
    }
    private void ChoosePentesilea(int choice)
    {
        pentesileaChoice=choice;pentesileaLines.Clear();pentesileaLineIndex=0;
        HLine(choice==1?"Ho sentito gli spari. Volevo aiutare qualcuno.":choice==2?"Cercavo provviste. Non sapevo ci fosse qualcuno.":"Abbassa quell'arma. Non mi fido di te.");
        PLine(choice==1?"Aiutare? Con tutta questa gente che pensa solo a nascondersi? ...Sei una rarità. Cerca di non fartela passare.":choice==2?"Almeno non racconti favole. Qui perfino gli avanzi hanno imparato a mentire.":"E io non mi fido delle persone che mi danno ordini mentre ho ancora il dito sul grilletto.");
        if(choice==3)AddPentesileaLine("","Abbassa il mitra di un palmo, ma continua a seguire ogni movimento del protagonista.");
        PLine("Non ti seguirò per adesso. Ho una porta da chiudere e troppi morti dalla parte sbagliata. Se mi rivedi, annunciati prima di entrare.");
        AddPentesileaLine("","Sistema il cinturone, poggia il mitra sulla spalla e sparisce verso la parte interna dell'abitazione.");
        NextPentesileaLine();
    }
    private void TalkPentesilea()
    {
        var flags=game.State.StoryFlags;
        bool high=flags.Contains("pentesilea_trust_high"),low=flags.Contains("pentesilea_trust_low");
        string greeting=high?"Sei ancora vivo. Bene.":low?"Ah. Sei ancora tu. Non avvicinarti troppo.":"Che cosa vuoi sapere?";
        Say("Pentesilea",greeting,PentesileaPortrait,
            new Choice("Parla",()=>{
                if(high&&flags.Add("pentesilea_bandage_given")){game.AddItem("bende");Say("Pentesilea","Tieni questa benda. Ho aperto il passaggio di servizio, sulla parete in fondo alla casa. Vai pure.",PentesileaPortrait);}
                else Say("Pentesilea",low?"Non ho tempo per le chiacchiere.":"Non intendo seguirti. Ho ancora qualcosa da sistemare qui.",PentesileaPortrait);}),
            new Choice("Chiedi dei non-morti",()=>Say("Pentesilea","Erano persone. Quando smettono di respirare, non significa che abbiano smesso di muoversi.",PentesileaPortrait)),
            new Choice("Chiedi delle porte",()=>Say("Pentesilea","Non fidarti delle porte segnate. E non restare immobile troppo a lungo.",PentesileaPortrait)),
            new Choice("Congedati",CloseDialogue));
    }
}
