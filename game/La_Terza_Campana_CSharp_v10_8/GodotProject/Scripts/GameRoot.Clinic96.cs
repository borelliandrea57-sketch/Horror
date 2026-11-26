using System;
using System.Linq;
using System.Collections.Generic;
using Godot;
using LaTerzaCampana.Core;
public partial class GameRoot
{
    private bool clinicPortrait96,clinicAttack96;
    private float clinicClock96;
    private bool clinicImpact97;
    private const float ClinicImpactTime97=1.52f, ClinicEndTime97=8.4f;
    private const string ClinicDeath97="I fiori continuano a tremare. Tu, invece, non ti muovi più.";
    private Vector2 clinicOrigin98,clinicEscape98;
    private int clinicBeat98;
    private static float Beat98(float t,float start,float end)=>Ease95((t-start)/(end-start));
    private static float Crawl98(float t)=>.52f*Beat98(t,3.45f,4.38f)+.48f*Beat98(t,4.95f,5.92f);
    private void PlanClinic98(int x,int y){
        clinicOrigin98=TileCenter(x,y);clinicEscape98=clinicOrigin98;clinicBeat98=0;
        var from=new Vector2I(x,y);var away=(clinicOrigin98-new Vector2(852,337)).Normalized();
        var options=NeighborSteps.Select(d=>from+d).Where(v=>!Blocked("casa_medico",v.X,v.Y)&&!ClinicRoom96(v.X,v.Y)&&!enemyPositions.Values.Contains(v)).OrderByDescending(v=>(TileCenter(v.X,v.Y)-clinicOrigin98).Dot(away)+(v.Y==y?65:0)).ToArray();
        if(options.Length>0){var next=options[0];clinicEscape98=clinicOrigin98.Lerp(TileCenter(next.X,next.Y),.83f);}
    }
    private void UpdateClinicFatal97(float dt){
        if(!clinicAttack96||screen!=Screen.Dialogue)return;
        if(previewMode)return;
        clinicClock96+=dt;
        if(clinicBeat98<1&&clinicClock96>=.80f){clinicBeat98=1;PlaySound("event_heartbeat",-7);}
        if(!clinicImpact97&&clinicClock96>=ClinicImpactTime97){
            clinicImpact97=true;game.State.Body=0;game.State.Bleeding=true;
            game.State.Mind=Math.Max(0,game.State.Mind-12);PlaySound("monster_attack",-2);shakeTime=.35f;
        }
        if(clinicBeat98<2&&clinicClock96>=2.65f){clinicBeat98=2;PlaySound("door_hit",-12);}
        if(clinicBeat98<3&&clinicClock96>=3.45f){clinicBeat98=3;PlaySound("event_drag",-8);}
        if(clinicBeat98<4&&clinicClock96>=4.95f){clinicBeat98=4;PlaySound("event_drag",-12);}
        if(clinicBeat98<5&&clinicClock96>=6.22f){clinicBeat98=5;PlaySound("event_heartbeat",-20);}
        if(clinicClock96>=ClinicEndTime97){clinicAttack96=false;choices.Clear();Die(ClinicDeath97);}
    }
    private static bool ClinicRoom96(int x,int y)=>x>=13&&x<=23&&y>=1&&y<=12;
    private void MigrateClinic96(){if(!game.State.StoryFlags.Add("geometry_v96"))return;foreach(var k in game.State.CollisionEdits.Keys.Where(k=>k.StartsWith("casa_medico:")).ToArray())game.State.CollisionEdits.Remove(k);}
    private bool ClinicKeyAvailable99()=>!game.State.StoryFlags.Contains("clinic99_key_taken");
    private void DrawClinicMap96(){
        if(game.State.StoryFlags.Contains("clinic96_flowers_removed"))Image("clinic96_cleared",new Rect2(new Vector2(830,316)-camera,new Vector2(45,40)));
        if(ClinicKeyAvailable99())Image("clinic99_key",new Rect2(new Vector2(886,421)-camera,new Vector2(24,18)));
    }
    private void TakeClinicKey99(){
        if(game.State.StoryFlags.Add("clinic99_key_taken"))game.AddItem("chiave_cisterne");
        PlaySound("door_open",-17);
        Say("Donna dei fiori","Le dita sono gelide. Liberi la piccola chiave dalla sua mano. **x1 Chiave dell’archivio**.");
    }
    private void InspectPatient96(){
        var f=game.State.StoryFlags;f.Add("clinic96_entered");
        if(f.Contains("clinic96_attacked")){Say("Donna dei fiori","La donna è tornata immobile. Non hai il coraggio di avvicinarti di nuovo.");return;}
        if(f.Contains("clinic96_flowers_removed")){
            if(ClinicKeyAvailable99())Say("Donna dei fiori","Le labbra sono immobili. Nella mano stringe ancora una piccola chiave di ottone.","",new Choice("Prendi la chiave",TakeClinicKey99),new Choice("Allontanati",CloseDialogue));
            else Say("Donna dei fiori","Le labbra sono immobili. Il profumo dolciastro si è attenuato. Non risponde.");
            return;
        }
        var opts=new List<Choice>{new("Rimuovi i fiori",()=>{f.Add("clinic96_flowers_removed");PlaySound("mind_scrape",-14);if(ClinicKeyAvailable99())Say("Donna dei fiori","Gli steli oppongono una resistenza innaturale. La donna espira lentamente. Nella mano stringe ancora una chiave.","",new Choice("Prendi la chiave",TakeClinicKey99),new Choice("Allontanati",CloseDialogue));else Say("Donna dei fiori","Gli steli oppongono una resistenza innaturale. Quando li estrai, la donna espira lentamente. La stanza torna silenziosa.");}),new("Lasciala stare",CloseDialogue)};
        if(ClinicKeyAvailable99())opts.Add(new Choice("Prendi la chiave",TakeClinicKey99));
        Say("????","La donna giace sulla lettiga, gli occhi aperti e immobili. Un mazzo di fiori spunta tra le labbra. I petali tremano, ma non c’è vento."+(ClinicKeyAvailable99()?" Una chiave spunta dalle sue dita.":""),"",opts.ToArray());clinicPortrait96=true;clinicClock96=0;
    }
    private void DrawClinicPortrait96(){Image("clinic96_patient",new Rect2(0,-30,W,853));Rect(0,0,W,H,new Color(.055f,.035f,.025f,.13f));}
    private bool ClinicStep96(int x,int y){if(game.State.Map!="casa_medico")return false;var f=game.State.StoryFlags;if(ClinicRoom96(x,y)){f.Add("clinic96_entered");return false;}if(!f.Contains("clinic96_entered")||f.Contains("clinic96_flowers_removed")||!f.Add("clinic96_attacked"))return false;
        visibleHero=TileCenter(x,y);PlanClinic98(x,y);walking=false;PlaySound("mind_scrape",-13);
        Say("????","Un fruscio alle tue spalle.");clinicAttack96=true;clinicImpact97=false;clinicClock96=0;return true;
    }
    private string ClinicLastWords98()=>game.State.Hero switch{
        HeroId.Gladia=>"Non ancora… c'è ancora qualcuno da aiutare.",
        HeroId.Ambrosh=>"No… questa volta volevo scegliere io.",
        _=>"Non è un altro ricordo. Non questa volta."};
    private void DrawClinicAttack96(){
        float t=clinicClock96;float fall=Beat98(t,2.05f,3.15f),kneel=Beat98(t,1.75f,2.58f),crawl=Crawl98(t);
        var mouth=new Vector2(852,337);var move=clinicEscape98-clinicOrigin98;var away=move.LengthSquared()>1?move.Normalized():(clinicOrigin98-mouth).Normalized();
        if(away.LengthSquared()<.01f)away=Vector2.Down;
        var pos=clinicOrigin98+move*crawl;float zoom=1.34f+.68f*Beat98(t,.1f,1.44f)+.55f*Beat98(t,2.2f,3.55f);
        var focus=mouth.Lerp(clinicOrigin98,.62f+.38f*Beat98(t,1.7f,3.45f)).Lerp(pos,.75f*fall);
        var mapSize=new Vector2(CurrentMap().W*T,CurrentMap().H*T);var half=new Vector2(W,H)/(2*zoom);
        focus=new Vector2(Mathf.Clamp(focus.X,half.X,mapSize.X-half.X),Mathf.Clamp(focus.Y,half.Y,mapSize.Y-half.Y));
        var shake=new Vector2(Mathf.Sin(t*83),Mathf.Cos(t*73))*7*Mathf.Max(0,1-Math.Abs(t-ClinicImpactTime97)/.18f);
        var offset=new Vector2(W/2,H/2)-focus*zoom+shake;
        Vector2 S(Vector2 p)=>p*zoom+offset;
        Image("map96_casa_medico",new Rect2(offset,mapSize*zoom));
        var feet=S(pos)+new Vector2(0,15*zoom);
        float pull=0;
        if(t>=3.45f&&t<4.38f)pull=Mathf.Sin(Mathf.Pi*(t-3.45f)/.93f);
        if(t>=4.95f&&t<5.92f)pull=Mathf.Sin(Mathf.Pi*(t-4.95f)/.97f);
        float facingAngle=Mathf.Atan2(away.X,-away.Y);
        if(Math.Abs(away.X)<.01f)facingAngle=clinicOrigin98.X<852?-1.25f:1.25f;
        var flatAxis=new Vector2(Mathf.Sin(facingAngle),-Mathf.Cos(facingAngle));var shadowCenter=feet+new Vector2(0,-8*zoom*fall);
        DrawSetTransform(shadowCenter,Mathf.Atan2(flatAxis.Y,flatAxis.X));
        DrawEllipse(0,0,(12+17*fall)*zoom,(5+2*fall)*zoom,new Color(.025f,.018f,.014f,.49f));DrawSetTransform(Vector2.Zero);
        var atlas=Art("anim_"+InjuryKey());
        if(atlas!=null){
            // Continuous strip geometry: adjoining strips share both edge vertices.
            bool right=away.X>=0;float side=right?1:-1;int row=facingAngle>0?2:1;
            if(t<1.75f)row=dir;
            int frame=t<ClinicImpactTime97?0:fall<.95f?2:pull>.15f?1+(int)(pull*5):0;
            float h=88*zoom*(1-.12f*kneel)*(1-.29f*fall),w=50*zoom*(1-.15f*fall);
            float recoil=Beat98(t,1.52f,1.64f)*(1-Beat98(t,1.65f,1.96f));
            float face=fall*facingAngle;var forward=new Vector2(Mathf.Sin(face),-Mathf.Cos(face));
            Vector2 baseAt=feet-flatAxis*h*.47f*fall+new Vector2(0,-6*zoom*kneel);
            const int strips=16;
            Vector2 Axis(float u){float contraction=(.23f*kneel*(1-fall)+.13f*pull*fall)*Mathf.Sin(u*Mathf.Pi);var normal=new Vector2(-forward.Y,forward.X);return forward*(u*h*(1-.12f*pull*fall))+normal*(contraction*h*side)+new Vector2(0,recoil*3*zoom);}
            var lo=new Vector2[strips+1];var hi=new Vector2[strips+1];
            for(int i=0;i<=strips;i++){
                float u=i/(float)strips;var c=baseAt+Axis(u);var tangent=(Axis(Math.Min(1,u+.015f))-Axis(Math.Max(0,u-.015f))).Normalized();var across=new Vector2(-tangent.Y,tangent.X);
                float width=w*(1+.045f*pull*Mathf.Sin(u*6));lo[i]=c-across*width/2;hi[i]=c+across*width/2;
            }
            for(int i=0;i<strips;i++){
                float u0=i/(float)strips,u1=(i+1)/(float)strips;
                Vector2[] vertices={lo[i+1],hi[i+1],hi[i],lo[i]};
                float y0=(row*90+90*(1-u1))/atlas.GetHeight(),y1=(row*90+90*(1-u0))/atlas.GetHeight();float x0=frame*44f/atlas.GetWidth(),x1=(frame*44f+44)/atlas.GetWidth();
                Vector2[] uv={new(x0,y0),new(x1,y0),new(x1,y1),new(x0,y1)};
                DrawPolygon(vertices,new Color[]{new(.91f,.87f,.82f)},uv,atlas);
            }
        }
        var start=S(mouth);var target=S(clinicOrigin98+new Vector2(0,-33));
        var direction=(target-start).Normalized();if(direction.LengthSquared()<.01f)direction=Vector2.Down;
        var delta=target+direction*27*zoom-start;var normal=new Vector2(-direction.Y,direction.X);
        float q=Beat98(t,1.24f,ClinicImpactTime97)*(1-Beat98(t,1.83f,2.36f));var prev=start;
        if(q>.001f)for(int i=1;i<=64;i++){
            float u=i/64f*q;float bend=Mathf.Sin(u*12+t*10)*11*zoom*Mathf.Sin(u*Mathf.Pi);
            bend*=Mathf.Clamp(Mathf.Abs(u-start.DistanceTo(target)/Math.Max(1,delta.Length()))*9,0,1);
            var at=start+delta*u+normal*bend;float thick=(2+11*Mathf.Pow(1-i/65f,.72f))*zoom;
            DrawLine(prev+new Vector2(2,4),at+new Vector2(2,4),new Color(.045f,.028f,.025f,.6f),thick+4,true);
            DrawLine(prev,at,new Color(.11f,.061f,.055f),thick+2,true);DrawLine(prev,at,new Color(.31f,.18f,.16f),thick,true);
            DrawLine(prev-normal*thick*.23f,at-normal*thick*.23f,new Color(.52f,.33f,.27f,.8f),Math.Max(1,thick*.18f),true);prev=at;
        }
        if(clinicImpact97){
            float age=t-ClinicImpactTime97;
            // Restrained impact particles, then a small mark on the clothing rather than graphic anatomy.
            if(t<2.2f){DrawEllipse(target.X,target.Y,3.8f*zoom,3*zoom,new Color(.29f,.04f,.035f,.85f));for(int i=0;i<8;i++){float a=i*2.39996f;float speed=9+(i%4)*7;var spot=target+new Vector2(Mathf.Cos(a)*speed*age,Mathf.Sin(a)*speed*age+age*age*10)*zoom;DrawCircle(spot,1.2f*zoom,new Color(.36f,.06f,.05f,Mathf.Clamp(1-age/.68f,0,1)));}}
            if(age<.19f)Rect(0,0,W,H,new Color(.24f,.04f,.025f,.19f*(1-age/.19f)));
        }
        Rect(0,0,W,H,new Color(.035f,.025f,.02f,.17f+.13f*Beat98(t,5.7f,7.4f)));
        var light=Art("light_mask59");if(light!=null){var c=S(pos).Lerp(start,.24f*(1-fall));float r=410+90*(1-fall);var dark=new Color(0,0,0,.85f);float l=c.X-r,rr=c.X+r,top=c.Y-r,bot=c.Y+r;if(l>0)Rect(0,0,l,H,dark);if(rr<W)Rect(rr,0,W-rr,H,dark);if(top>0)Rect(Math.Max(0,l),0,Math.Min(W,rr)-Math.Max(0,l),top,dark);if(bot<H)Rect(Math.Max(0,l),bot,Math.Min(W,rr)-Math.Max(0,l),H-bot,dark);DrawTextureRect(light,new Rect2(l,top,2*r,2*r),false,new Color(1,1,1,.85f));}
        float bars=64*Beat98(t,0,.55f);Rect(0,0,W,bars,Colors.Black);Rect(0,H-bars,W,bars,Colors.Black);
        string line=t<1.22f?"Un fruscio. Poi, il silenzio.":t<2.8f?"":t<3.55f?"Le ginocchia cedono.":t<6.25f?ClinicLastWords98():"Il profumo dei fiori non se ne va.";
        if(line.Length>0)Center(line,W/2,H-24,22,new Color(.84f,.77f,.67f,1));
        if(t>7.0f)Rect(0,0,W,H,new Color(0,0,0,Beat98(t,7.0f,ClinicEndTime97)));
    }
    private void CheckClinic96(){int n=0;void C(bool ok,string label){if(!ok)throw new Exception(label);n++;}game.StartNew(HeroId.Gladia,Difficulty.Sopravvivenza);ResetEventRules();screen=Screen.World;EnterMap("casa_medico",9,18);C(Art("map96_casa_medico")!=null,"new map");C(Art("clinic96_patient")!=null,"portrait");C(Art("clinic96_cleared")!=null,"cleared sprite");var seen=new HashSet<Vector2I>{new(9,18)};var queue=new Queue<Vector2I>(seen);while(queue.Count>0){var v=queue.Dequeue();foreach(var step in NeighborSteps){var next=v+step;if(!Blocked("casa_medico",next.X,next.Y)&&seen.Add(next))queue.Enqueue(next);}}foreach(var p in CurrentMap().Props)C(seen.Any(v=>Math.Abs(v.X-p.X)+Math.Abs(v.Y-p.Y)<=2),"reachable "+p.Id);foreach(var e in CurrentMap().Enemies)C(seen.Contains(new(e.X,e.Y)),"enemy "+e.Id);C(seen.Contains(new(17,10)),"patient access");C(seen.Contains(new(9,19)),"exit");int body=game.State.Body,mind=game.State.Mind;C(!ClinicStep96(17,10),"enter");InspectPatient96();C(clinicPortrait96&&choices.Count==3,"inspect");choices[1].Do();C(!ClinicStep96(17,10),"inside no attack");C(ClinicStep96(16,13)&&clinicAttack96,"leave attack");C(game.State.Body==body,"alive before impact");UpdateClinicFatal97(.3f);C(game.State.Body==body&&screen==Screen.Dialogue,"anticipation");UpdateClinicFatal97(1.3f);C(game.State.Body==0&&game.State.Mind==mind-12&&clinicImpact97,"instant death at impact");UpdateClinicFatal97(2.2f);C(screen==Screen.Dialogue&&clinicAttack96&&game.State.Body==0,"fall before game over");C(Crawl98(3.5f)<Crawl98(4.4f)&&Crawl98(4.4f)<Crawl98(6f),"two pulls");C(!Blocked("casa_medico",(int)(clinicEscape98.X/T),(int)(clinicEscape98.Y/T)),"crawl walkable");UpdateClinicFatal97(5f);C(screen==Screen.Dead&&!clinicAttack96,"automatic game over");C(!ClinicStep96(16,13)&&game.State.Body==0,"no repeat");C(GameRules.FromJson(game.ToJson()).State.StoryFlags.Contains("clinic96_attacked"),"attack persists");game.StartNew(HeroId.Roderick,Difficulty.Sopravvivenza);ResetEventRules();screen=Screen.World;EnterMap("casa_medico",17,10);InspectPatient96();choices[0].Do();CloseDialogue();body=game.State.Body;C(!ClinicStep96(16,13)&&game.State.Body==body,"removal prevents attack");C(GameRules.FromJson(game.ToJson()).State.StoryFlags.Contains("clinic96_flowers_removed"),"removal persists");foreach(var h in Enum.GetValues<HeroId>())foreach(var d in Enum.GetValues<Difficulty>()){
            game.StartNew(h,d);ResetEventRules();screen=Screen.World;EnterMap("casa_medico",17,10);game.State.Body=100;
            C(ClinicStep96(16,13),"ambush "+h+":"+d);UpdateClinicFatal97(1.6f);C(game.State.Body==0,"fatal "+h+":"+d);UpdateClinicFatal97(7f);C(screen==Screen.Dead,"death "+h+":"+d);
        }GD.Print($"CLINIC98_OK {n}");GetTree().Quit();}
}
