using System;
using System.Linq;
using System.Collections.Generic;
using Godot;
using LaTerzaCampana.Core;

public partial class GameRoot
{
    // Each family has a distinct silhouette change, anticipation and follow-through.
    private record AttackProfile95(string Name,float Duration,float ArmL,float ArmR,float Head,float Lean,float Step,float Lift,float Squash,int Style);
    private static AttackProfile95 EnemyProfile95(string k)=>k switch{
        "z74_heavy"=>new("SCHIACCIAMENTO",1.34f,-1.65f,1.75f,-.12f,-.08f,54,5,.14f,0),
        "z74_cyst"=>new("SPASMO CISTICO",1.12f,-.62f,1.04f,.31f,.06f,28,3,.13f,1),
        "z74_thin"=>new("SCATTO AD ARTIGLIO",.82f,-.18f,1.48f,-.23f,-.19f,114,35,.02f,2),
        "z74_eye"=>new("MORSO OBLIQUO",1.03f,-.58f,.45f,-.40f,-.24f,67,12,.04f,3),
        "z74_hunch"=>new("BALZO DEL GOBBO",1.19f,-1.06f,1.18f,.18f,-.18f,78,77,.10f,4),
        "z74_flayed"=>new("DOPPIA ARTIGLIATA",1.15f,-1.33f,1.36f,-.20f,-.14f,83,15,.04f,5),
        "falciante84"=>new("FENDENTE DI RONCOLA",1.25f,-.14f,2.08f,-.08f,-.24f,64,9,.03f,6),
        "ossuary"=>new("FALCIATA D’OSSO",1.30f,-1.05f,1.40f,-.20f,-.16f,70,30,.07f,7),
        "bell"=>new("RINTOCCO",1.48f,-.68f,.68f,.12f,.03f,12,3,.19f,8),
        "stitch"=>new("PRESA DEL CUCITO",1.17f,-.95f,1.17f,-.08f,-.10f,63,4,.08f,9),
        "sexton"=>new("COLPO DALL’ALTO",1.33f,-.21f,1.96f,-.14f,-.12f,49,44,.07f,10),
        _=>new("FENDENTE DISPERATO",1.06f,-.45f,1.35f,-.18f,-.17f,58,10,.03f,11)};
    private static float Ease95(float x){x=Mathf.Clamp(x,0,1);return x*x*(3-2*x);}
    private static float K95(float t,float a,float b,float c,float d,float e){ // keys at 0, .27, .55, .73, 1
        if(t<.27f)return Mathf.Lerp(a,b,Ease95(t/.27f));
        if(t<.55f)return Mathf.Lerp(b,c,Ease95((t-.27f)/.28f));
        if(t<.73f)return Mathf.Lerp(c,d,Ease95((t-.55f)/.18f));
        return Mathf.Lerp(d,e,Ease95((t-.73f)/.27f));}
    private static Transform2D Tr95(Vector2 pos,float angle=0,float sx=1,float sy=1)=>new(angle,new Vector2(sx,sy),0,pos);
    private void DrawPiece95(Texture2D t,Rect2 r,Transform2D parent,Vector2 pivot,float angle,Vector2 offset,Color tint){
        DrawSetTransformMatrix(parent*Tr95(pivot+offset,angle)*Tr95(-pivot));DrawTextureRect(t,r,false,tint);}
    private Vector2 EnemyTarget95(float w,float h,int i,string k)=>i switch{0=>new(0,-h*.55f),1=>new(0,-h*.86f),_=>new Vector2((WoundAnchor84(k,i).X-.5f)*w,(WoundAnchor84(k,i).Y-1)*h)+new Vector2(i%2==0?15:-15,i<4?32:35)};
    private void DrawEnemyRig95(){
        if(battle==null)return;string k=BattleArt75();var tex=Art("limb75_"+k+"_0");if(tex==null)return;
        float scale=Math.Min(680f/tex.GetWidth(),398f/tex.GetHeight());float w=tex.GetWidth()*scale,h=tex.GetHeight()*scale;
        bool attacking=enemyAttack75>0;float t=EnemyProgress95;float u=HeroProgress95;var p=EnemyProfile95(k);
        bool disarmed=battle.Armed&&battle.Parts["braccioD"]<=0;
        float movement=attacking?K95(t,0,-.16f,1,.65f,0):0;
        float force=attacking?K95(t,0,-.75f,1,.46f,0):0;
        float lift=attacking?K95(t,0,.10f,1,.25f,0)*p.Lift:0;
        float hit=heroAttack75>0&&heroLanded95?K95(u,0,0,1,.38f,0):0;
        if(p.Style==8)movement*=.15f;
        if(p.Style==1)force*=.7f+.3f*Mathf.Sin(t*35);
        float x=835-p.Step*movement+hit*10,y=458-lift+Mathf.Sin(tick*2)*1.3f;
        float squash=attacking?K95(t,0,-.65f,1,.2f,0)*p.Squash:0;
        float lean=(attacking?p.Lean*force:0)+hit*.06f;
        var parent=Tr95(new(x,y),lean,1+squash*.7f,1-squash);
        DrawEllipse(x,459,w*.34f*(1-lift/250),10,new Color(.012f,.009f,.008f,.54f-lift/300));
        var r=new Rect2(-w/2,-h,w,h);Color tint=new(1,1-hit*.17f,1-hit*.17f);
        bool intact=Parts.All(part=>battle.Parts[part]>0);
        // Full illustration at rest; during animation use the existing anatomical layers.
        if(!attacking&&intact&&Art("intact82_"+k) is Texture2D full){DrawSetTransformMatrix(parent);DrawTextureRect(full,r,false,tint);}
        else{
            var pivots=new Vector2[6];var angles=new float[6];var offsets=new Vector2[6];
            foreach(int i in new[]{4,5,2,3,0,1}){
                Vector2 pivot=i==0?new(0,-h*.48f):i==1?new(0,-h*.77f):new((WoundAnchor84(k,i).X-.5f)*w,(WoundAnchor84(k,i).Y-1)*h);
                float angle=0;Vector2 off=Vector2.Zero;
                if(attacking){
                    if(i==2||i==3){float a=i==2?p.ArmL:p.ArmR;float phase=t;
                        if(p.Style==5&&i==2)phase=Mathf.Clamp(t-.12f,0,1);
                        if(p.Style==3)a*=.45f;if(disarmed)a*=.63f;
                        angle=K95(phase,0,a*.46f,-a*.45f,-a*.22f,0);
                        off=new Vector2(-3*movement,0);
                    }else if(i==1){angle=p.Head*force;off=new Vector2(-Math.Abs(p.Head)*28*force,Math.Abs(p.Head)*12*force);}
                    else if(i>=4){float stride=K95(t,0,.12f,1,-.18f,0);angle=(i==4?1:-1)*stride*(p.Style==4?.33f:.16f);off=new Vector2(0,lift*.17f);}
                }
                pivots[i]=pivot;angles[i]=angle;offsets[i]=off;
            }
            if(EnemyComposite95(k) is Texture2D composite)DrawEnemyMesh95(k,composite,r,parent,pivots,angles,offsets,tint);
        }
        if(battle.Phase==1&&!BattleBusy75&&battle.Parts[Parts[target75]]>0){var layer=Art("limb75_"+k+"_"+target75);if(layer!=null){DrawSetTransformMatrix(parent);DrawTextureRect(layer,r,false,new Color(1.35f,1.15f,.75f,.75f));}}
        DrawSetTransformMatrix(parent);DrawWoundDrops84(w,h);DrawSetTransform(Vector2.Zero);
        if(battle.Phase==1&&!BattleBusy75)Text("BERSAGLIO: "+PartName(Parts[target75]).ToUpperInvariant(),570,482,17,Gold);
    }
    private string HeroWeapon95()=>game.NoArms84?"":game.State.Equipped.GetValueOrDefault(GearSlot.Arma,"");
    private void DrawHeroRig95(){
        if(game.State.Hero==HeroId.Anatema){DrawAnatemaCombat101();return;}
        string key=InjuryKey();var full=Art("rig95_"+key+"_full");if(full==null)return;
        float h=288,w=h*full.GetWidth()/full.GetHeight();var r=new Rect2(-w/2,-h,w,h);
        bool attacking=heroAttack75>0,reacting=enemyAttack75>0;float t=HeroProgress95,v=EnemyProgress95;string weapon=HeroWeapon95();
        bool both=game.NoArms84;bool left=game.State.Injuries.Contains("braccioD")&&!game.State.Injuries.Contains("braccioS");float sign=left?-1:1;
        float thrust=attacking?K95(t,0,-.18f,1,.56f,0):0;
        float recoil=reacting?K95(v,0,0,enemyLanded82?1:.35f,.4f,0):0;
        float travel=weapon=="coltello"?91:weapon=="martello"?58:weapon=="bastone"?67:46;
        float x=333+travel*thrust-24*recoil,y=457+Mathf.Sin(tick*2)*1.2f;
        float lean=attacking?K95(t,0,.12f,-.17f,-.09f,0):enemyGuard95&&reacting?.13f*recoil:.20f*recoil;
        if(target75>=4&&attacking)y+=Math.Max(0,thrust)*9;if(both&&attacking){y-=Math.Max(0,thrust)*12;lean-=.14f*Math.Max(0,thrust);}
        var parent=Tr95(new(x,y),lean,1+.035f*recoil,1-.06f*recoil);
        DrawEllipse(x,459,46,10,new Color(.015f,.01f,.01f,.54f));
        if(!attacking&&!reacting){DrawSetTransformMatrix(parent);DrawTextureRect(full,r,false);DrawSetTransform(Vector2.Zero);return;}
        bool rod=game.State.Hero==HeroId.Roderick;Vector2 pivotL=new(-w*.22f,-h*(rod?.74f:.67f)),pivotR=new(w*.22f,-h*(rod?.74f:.67f));
        float swing=weapon switch{"coltello"=>K95(t,0,-.55f,1.35f,.78f,0),"bastone"=>K95(t,0,1.60f,.72f,-.28f,0),"martello"=>K95(t,0,2.48f,1.03f,.34f,0),_=>K95(t,0,-.40f,1.26f,.38f,0)};
        if(target75>=4)swing*=.72f;else if(target75==1)swing+=.20f*Math.Max(0,thrust);
        if(!attacking)swing=0;
        float aL=attacking?(left?-swing:-.35f*Math.Abs(thrust)):enemyGuard95&&reacting?-.95f*recoil:-.15f*recoil;
        float aR=attacking?(!left?swing:.32f*Math.Abs(thrust)):enemyGuard95&&reacting?.95f*recoil:.15f*recoil;
        // positive rotation in screen coordinates sends a downward right arm to the left; invert.
        aL=-aL;aR=-aR;
        var body=Art("rig95_"+key+"_body");if(body==null){DrawSetTransformMatrix(parent);DrawTextureRect(full,r,false);DrawSetTransform(Vector2.Zero);return;}
        DrawSetTransformMatrix(parent);DrawTextureRect(body,r,false);
        if(attacking&&!both&&weapon!=""&&Art("weapon95_"+weapon) is Texture2D blade){
            Vector2 pivot=left?pivotL:pivotR;float angle=left?aL:aR;
            float armLen=h*(rod?.31f:.24f);Vector2 hand=pivot+new Vector2(sign*w*.05f,armLen).Rotated(angle);
            float weaponAngle=weapon=="martello"?K95(t,-.18f,-2.0f,.30f,.9f,-.18f):weapon=="bastone"?K95(t,.05f,-1.25f,.42f,.85f,.05f):K95(t,.1f,-.55f,.06f,.7f,.1f);
            float length=weapon=="coltello"?64:weapon=="bastone"?108:101;
            DrawSetTransformMatrix(parent*Tr95(hand,weaponAngle));DrawTextureRect(blade,new Rect2(-length*.16f,-length*.17f,length,length*.34f),false);
        }
        foreach(var side in new[]{"l","r"}){var arm=Art("rig95_"+key+"_"+side);if(arm!=null){bool gone=game.State.Injuries.Contains(side=="l"?"braccioS":"braccioD");DrawPiece95(arm,r,parent,side=="l"?pivotL:pivotR,gone?0:side=="l"?aL:aR,Vector2.Zero,Colors.White);}}
        DrawSetTransform(Vector2.Zero);
    }
    private void DrawCombatEffects95(){
        if(battle==null)return;string k=BattleArt75();var p=EnemyProfile95(k);
        if(heroAttack75>0){float t=HeroProgress95;float f=Mathf.Max(0,1-Math.Abs(t-.56f)/.17f);if(f>0){var tex=Art("limb75_"+k+"_0");if(tex!=null){float s=Math.Min(680f/tex.GetWidth(),398f/tex.GetHeight());Vector2 at=new Vector2(835,458)+EnemyTarget95(tex.GetWidth()*s,tex.GetHeight()*s,target75,k);
            var color=new Color(.88f,.79f,.61f,f*.55f);string weapon=HeroWeapon95();
            if(weapon=="coltello")DrawLine(at+new Vector2(-70,17),at+new Vector2(20,-6),color,3,true);
            else if(weapon=="bastone")DrawArc(at+new Vector2(-30,8),69,2.6f,5.7f,30,color,3,true);
            else if(weapon=="martello"){DrawArc(at+new Vector2(-16,-49),63,3.3f,6.1f,28,color,4,true);}
            if(t>.51f&&heroLanded95)Impact95(at,f,false);else if(t>.51f&&!heroLanded95)Text("MANCATO",at.X-32,at.Y-25,18,new Color(.85f,.82f,.72f,f));
        }}}
        if(enemyAttack75>0){float t=EnemyProgress95;float f=Mathf.Max(0,1-Math.Abs(t-.56f)/.21f);Text(p.Name,570,482,17,Gold);
            if(f>0){var at=new Vector2(350,290);var color=new Color(.64f,.52f,.40f,f*.48f);
                if(p.Style is 0 or 8){for(int i=0;i<3;i++)DrawArc(new Vector2(640,448),34+i*25+t*175,Mathf.Pi,Mathf.Tau,48,color,2,true);}
                else if(p.Style==1){for(int i=0;i<12;i++){float u=i/12f;DrawEllipse(710-350*u*t,282+Mathf.Sin(i*4+t*7)*28,3,2,new Color(.56f,.57f,.35f,f*.55f));}}
                else if(p.Style==3){DrawArc(at+new Vector2(12,-28),42,-.2f,2.7f,30,color,3,true);}
                else if(p.Style==9){DrawArc(at+new Vector2(-5,0),57,1,4.1f,32,color,2,true);DrawArc(at+new Vector2(12,0),54,-1.7f,1.5f,32,color,2,true);}
                else{int count=p.Style is 5 or 7?3:1;for(int i=0;i<count;i++)DrawArc(at+new Vector2(85+i*13,-7+i*12),86+i*9,2.2f,4.7f,30,color,3,true);}
                if(enemyResolved95&&enemyLanded82)Impact95(at,f,enemyGuard95);else if(enemyResolved95&&!enemyLanded82)Text("SCHIVATO",280,242,18,new Color(.85f,.82f,.72f,f));
            }
        }
    }
    private void Impact95(Vector2 at,float f,bool guard){for(int i=0;i<9;i++){float a=i*Mathf.Tau/9;var d=new Vector2(Mathf.Cos(a),Mathf.Sin(a));float r=12+(1-f)*33;DrawLine(at+d*r,at+d*(r+9),new Color(.80f,.68f,.48f,f*(guard?.80f:.56f)),2,true);}}
    private void CheckCombat95(){int n=0;void C(bool a,string m){if(!a)throw new Exception(m);n++;}
        string[] profiles={"z74_heavy","z74_cyst","z74_thin","z74_eye","z74_hunch","z74_flayed","falciante84","ossuary","bell","stitch","sexton","corrupted"};
        C(profiles.Select(k=>EnemyProfile95(k).Name).Distinct().Count()==12,"distinct choreography");
        foreach(var k in profiles){C(EnemyProfile95(k).Duration>.75f,"duration "+k);C(Art("intact82_"+k)!=null,"texture "+k);}
        foreach(var hero in Enum.GetValues<HeroId>())for(int mask=0;mask<16;mask++)foreach(string piece in new[]{"full","body","l","r"})C(Art("rig95_"+hero.ToString().ToLowerInvariant()+"_"+mask.ToString("D2")+"_"+piece)!=null,"rig");
        foreach(var hero in Enum.GetValues<HeroId>()){game.StartNew(hero,Difficulty.Sopravvivenza);ResetEventRules();StartBattle(new WorldEnemy{Kind="zombie",Id="check95_1",X=14,Y=10});battle!.Phase=1;int before=battle.Parts["torace"];StartAttack75(0);UpdateBattle75(.20f);C(battle.Parts["torace"]==before&&pending75==0,"windup no damage");UpdateBattle75(.40f);C(pending75<0&&heroAttack75>0&&enemyPending95,"impact queues reply");int body=game.State.Body;UpdateBattle75(.5f);C(enemyAttack75>0&&!enemyResolved95,"enemy windup");UpdateBattle75(.1f);C(game.State.Body==body,"no early damage");UpdateBattle75(.8f);C(enemyResolved95,"enemy impact");UpdateBattle75(1f);C(!BattleBusy75,"turn released");}
        game.StartNew(HeroId.Gladia,Difficulty.Sopravvivenza);StartBattle(new WorldEnemy{Kind="zombie",Id="check95_1"});battle!.Parts["braccioS"]=0;battle.Parts["braccioD"]=0;int b=game.State.Body;RequestEnemy95();UpdateBattle75(2);C(game.State.Body==b,"no arms no damage");
        GD.Print($"COMBAT95_OK {n}");GetTree().Quit();}
}
