using System;
using Godot;
using LaTerzaCampana.Core;

public partial class GameRoot
{
    private float fountainClock61;
    private bool threeMothers73;
    private string threeMothersText73="";
    private const float StatuePanDuration73=7.0f;
    private static float StatuePan73(float time){float u=Mathf.Clamp((time-.55f)/(StatuePanDuration73-.55f),0,1);return u*u*(3-2*u);}
    private void OpenThreeMothers73(WorldProp p){OpenFountain61();threeMothers73=true;threeMothersText73=p.Note.Length>0?p.Note:"Tre madri di pietra stringono la croce sopra una vasca scura.";}
    private void AdvanceFountain73(bool cancel=false){if(threeMothers73&&!cancel&&fountainClock61<StatuePanDuration73){fountainClock61=StatuePanDuration73;QueueRedraw();return;}CloseFountain61();}
    private void DrawThreeMothers73(){
        Rect(0,0,W,H,new Color(.018f,.014f,.012f));
        var tex=Art("three_mothers73");
        float pan=StatuePan73(fountainClock61);
        if(tex!=null){float sw=tex.GetWidth(),sh=tex.GetHeight(),viewH=sw*520f/W;
            float y=Mathf.Lerp(sh-viewH,0,pan);
            DrawTextureRectRegion(tex,new Rect2(0,74,W,520),new Rect2(0,y,sw,viewH));}
        if(game.State.Mind<30)Rect(0,74,W,520,new Color(.28f,.035f,.04f,.09f+.035f*Mathf.Sin(tick*2)));
        Rect(0,0,W,74,new Color(.015f,.012f,.011f,.97f));
        Inscribe("DÙSTERBEDRAGNA / STAZIONE CENTRALE",40,30,18,InkGold);Inscribe("Fontana delle Tre Madri",40,61,28,Parchment);
        Rect(0,594,W,126,new Color(.015f,.012f,.011f,.98f));SootRule(42,602,1195,true);
        if(fountainClock61>=StatuePanDuration73){InscribedParagraph(threeMothersText73,52,632,1168,21,26,2,Parchment);Inscribe("E / INVIO — TORNA ALLA PIAZZA",850,704,17,InkGold);}
        else {string line=pan<.36f?"L’acqua scura lambisce la pietra consumata…":pan<.72f?"Tre figure stringono una croce avvolta da corde.":"Lo sguardo sale. La croce sembra non avere fine.";Inscribe(line,52,644,22,Parchment);Inscribe("E / INVIO — SALTA L’INQUADRATURA",850,704,17,InkGold);}
        if(fountainClock61<.45f)Rect(0,0,W,H,new Color(0,0,0,1-fountainClock61/.45f));
    }
    private void CheckStatue73(){int count=0;void C(bool ok,string m){if(!ok)throw new Exception(m);count++;}
        game.StartNew(HeroId.Gladia,Difficulty.Sopravvivenza);ResetEventRules();screen=Screen.World;EnterMap("station",10,14);var pos=new Vector2I(game.State.TileX,game.State.TileY);var f=CurrentMap().Props.Find(p=>p.Id=="station_fountain")!;InteractProp(f);C(screen==Screen.Fountain&&threeMothers73,"station opens cinematic");C(Art("three_mothers73")!=null,"new art loaded");C(StatuePan73(0)==0&&StatuePan73(7)==1&&StatuePan73(3)<StatuePan73(5),"pan rising");AdvanceFountain73();C(screen==Screen.Fountain&&fountainClock61==7,"skip pan");AdvanceFountain73();C(screen==Screen.World&&pos==new Vector2I(game.State.TileX,game.State.TileY),"return unchanged");
        var master=maps.Values.SelectMany(m=>m.Npcs).First(n=>n.Id=="stationmaster");game.State.StoryFlags.Add("ticket_shown");InteractNpc(master);C(speech.Contains("mai visto")&&speech.Contains("permesso"),"ticket dialogue");game.State.StoryFlags.Remove("ticket_shown");InteractNpc(master);C(speech.Contains("mai visto")&&choices.Count==2,"no ticket dialogue");choices[0].Do();C(game.State.StoryFlags.Contains("late_ticket")&&speech.Contains("mai visto"),"late ticket");screen=Screen.World;EnterMap("piazza",20,23);InteractProp(CurrentMap().Props.Find(p=>p.Id=="well")!);C(screen==Screen.Fountain&&!threeMothers73,"other fountain preserved");GD.Print($"STATUE73_OK {count}");GetTree().Quit();}

    private static string? NpcModel61(string id)=>id switch {"keeper"=>"keeper","woman"=>"ada","survivor"=>"neri","oldtraveller"=>"oldtraveller",_=>null};
    private void DrawNpc61(string model,Vector2 pos,int facing,int frame,float w=64,float h=100,bool world=false,bool walk=false)
    {
        var tex=Art("npc61_"+model+(walk?"_walk":"_idle"));if(tex==null)return;
        var p=world?pos-camera:pos;
        DrawEllipse(p.X,p.Y+12,w*.20f,4,new Color(.03f,.025f,.02f,.38f));
        DrawTextureRectRegion(tex,new Rect2(p.X-w/2,p.Y-h+16,w,h),new Rect2(frame%8*80,facing*112,80,112));
    }
    private void DrawWorldNpc61(string model,Vector2 pos)
    {
        var toward=visibleHero-pos;int facing=toward.Length()>T*5?0:Mathf.Abs(toward.X)>Mathf.Abs(toward.Y)?(toward.X>0?2:1):(toward.Y>0?0:3);
        DrawNpc61(model,pos,facing,(int)(tick*3.2f+pos.X/23)%8,68,100,true);
    }
    private void DrawPatient61(Vector2 npc)
    {
        var tex=Art("npc61_patient");if(tex==null)return;
        DrawTextureRectRegion(tex,new Rect2(npc.X-29,npc.Y-88,58,82),new Rect2((int)(tick*3)%8*96,0,96,120));
    }
    private string FountainText61()=> (game.State.Hero,game.State.Mind<30) switch {
        (HeroId.Gladia,true)=>"Le figure di pietra mi fissano. Braka sussurra: «Anche la misericordia si scioglie nell’acqua.» Per un istante il riflesso ha il mio volto, ma non il mio sorriso.",
        (HeroId.Ambrosh,true)=>"I cerchi sulla superficie si allargano contro il moto dell’acqua. Conto le figure. Una, due… una quarta mi osserva dal riflesso.",
        (HeroId.Roderick,true)=>"Una campana suona sotto l’acqua. Le figure della statua voltano la testa, una dopo l’altra. O forse sono soltanto io a vedere qualcosa che non esiste.",
        (HeroId.Gladia,false)=>"La pietra delle tre figure è consumata dalle mani di chi pregava qui. L’acqua è stagnante e scura. Mi chiedo quante persone siano venute a chiedere sollievo, e quante siano tornate a casa senza.",
        (HeroId.Ambrosh,false)=>"Le incisioni sulla colonna appartengono a epoche differenti. Qualcuno ha legato il monumento con del filo, come per impedire alla pietra di aprirsi. La vasca odora di ferro.",
        _=>"Il vento attraversa la piazza, ma nella vasca l’acqua quasi non si muove. Le figure sono legate alla colonna. Non è un monumento alla salvezza: sembra un avvertimento."};
    private void OpenFountain61(){fountainClock61=0;threeMothers73=false;walking=false;screen=Screen.Fountain;messageTimer=0;PlaySound(game.State.Mind<30?"mind_whisper":"ritual_hum",-13);QueueRedraw();}
    private void CloseFountain61(){screen=Screen.World;walking=false;sanityPlayer?.Stop();soundPlayer?.Stop();QueueRedraw();}
    private void DrawFountain61()
    {
        if(threeMothers73){DrawThreeMothers73();return;}
        float t=Mathf.Min(fountainClock61,4),zoom=1f+.025f*t;
        Image("fountain61",new Rect2(-W*(zoom-1)*.48f,74-420*(zoom-1)*.42f,W*zoom,420*zoom));
        bool disturbed=game.State.Mind<30;
        if(disturbed){Rect(0,0,W,H,new Color(.25f,.025f,.03f,.13f+.06f*Mathf.Sin(tick*3)));for(int i=0;i<3;i++){float a=(tick*.32f+i/3f)%1;DrawArc(new Vector2(603,310),15+44*a,0,Mathf.Tau,38,new Color(.7f,.26f,.17f,(1-a)*.15f),1.2f);}}
        Rect(0,0,W,74,new Color(.015f,.012f,.011f,.91f));Inscribe("DÙSTERBEDRAGNA / PIAZZA PRINCIPALE",40,30,18,InkGold);Inscribe("La Fontana dei Senzavoce",40,61,28,Parchment);
        Rect(0,484,W,236,new Color(.015f,.012f,.011f,.94f));SootRule(42,502,1195,true);
        Inscribe(Catalog.Heroes[game.State.Hero].Name,50,542,25,disturbed?Clot:InkGold);InscribedParagraph(FountainText61(),52,579,1168,23,32,3,Parchment);
        Inscribe("E / INVIO / ESC  —  TORNA ALLA PIAZZA",750,688,19,InkGold);
        if(fountainClock61<.45f)Rect(0,0,W,H,new Color(0,0,0,1-fountainClock61/.45f));
    }
    private void CheckDetail61()
    {
        int count=0;void Check(bool v,string what){if(!v)throw new Exception(what);count++;}
        foreach(var h in Enum.GetValues<HeroId>()){game.StartNew(h,Difficulty.Sopravvivenza);game.State.Mind=90;string normal=FountainText61();game.State.Mind=15;Check(normal!=FountainText61(),"mental variant");}
        game.StartNew(HeroId.Gladia,Difficulty.Sopravvivenza);ResetEventRules();screen=Screen.World;EnterMap("piazza",20,23);var p=CurrentMap().Props.Find(p=>p.Id=="well");Check(p!=null,"fountain exists");var pos=new Vector2I(game.State.TileX,game.State.TileY);InteractProp(p!);Check(screen==Screen.Fountain,"fountain event");CloseFountain61();Check(screen==Screen.World&&pos==new Vector2I(game.State.TileX,game.State.TileY),"returns without moving");
        foreach(var n in new[]{"girl","mother","keeper","neri","ada","oldtraveller"}){var a=Art("npc61_"+n+"_idle");Check(a!=null&&a.GetWidth()==640&&a.GetHeight()==448,"npc idle atlas "+n);Check(Art("npc61_"+n+"_walk")!=null,"npc walk atlas "+n);}
        Check(Art("npc61_patient")!=null,"patient");Check(Art("fountain61")!=null,"fountain art");GD.Print($"DETAIL61_OK {count}");GetTree().Quit();
    }
}
