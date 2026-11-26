using System;
using System.Linq;
using Godot;
using LaTerzaCampana.Core;
public partial class GameRoot {
    private bool enemyLanded82;
    private void DrawSkip82(){Panel(1040,80,224,32,.85f);Text("F10 — SALTA INTRO",1052,102,15,Gold);}
    private void SkipIntro82(){if(screen==Screen.Origin){if(EmileOrigin103)FinishEmile103(false);else FinishOrigin69();return;}if(screen==Screen.Prologue){proStep=7;PrologueAdvance(true);}}
    private (float X,float Y,float Angle,float Sx,float Sy) EnemyMove82(string k,float p){float q=p*p;return k switch{
        "falciante84"=>(-156*q,-12*p,-.32f*p,1+.10f*p,1-.04f*p),
        "z74_heavy"=>(-108*q,8*p,-.10f*p,1+.18f*p,1-.13f*p),
        "z74_cyst"=>(-65*p,-8*p,.06f*Mathf.Sin(p*11),1+.15f*p,1+.05f*p),
        "z74_thin"=>(-172*q,-28*p,-.27f*p,1-.03f*p,1+.10f*p),
        "z74_eye"=>(-108*p,5*p,.19f*p,1+.08f*p,1-.05f*p),
        "z74_hunch"=>(-123*q,-41*p,-.19f*p,1+.08f*p,1-.08f*p),
        "z74_flayed"=>(-153*p,-9*p,-.24f*p,1+.12f*p,1+.05f*p),
        "ossuary"=>(-145*p,-24*p,-.14f*p,1+.20f*p,1-.10f*p),
        "bell"=>(-55*p,17*p,.04f*p,1+.19f*p,1-.18f*p),
        "stitch"=>(-135*q,4*p,-.20f*p,1+.11f*p,1-.12f*p),
        "sexton"=>(-92*p,-54*p,.12f*p,1+.04f*p,1+.07f*p),
        _=>(-125*p,-4*p,-.12f*p,1+.04f*p,1+.03f*p)};}
    private void DrawAttackTrace82(string k,float p,Vector2 at){float a=p*.30f;Color c=enemyLanded82?new Color(.72f,.48f,.30f,a):new Color(.67f,.65f,.57f,a*.65f);
        if(k is "z74_heavy" or "bell"){for(int i=0;i<3;i++)DrawArc(at+new Vector2(-35,80),30+i*17+p*22,0,Mathf.Pi,32,c,2);}
        else if(k=="z74_cyst"){for(int i=0;i<9;i++)DrawCircle(at+new Vector2(-i*10,Mathf.Sin(i*3+tick)*18),1+i%3,new Color(.55f,.59f,.31f,a));}
        else{for(int i=0;i<(k=="ossuary"||k=="z74_flayed"?3:1);i++)DrawArc(at+new Vector2(-40-i*14,-8+i*13),65+i*10,1.5f,3.6f,36,c,2);}}
    private void Check82(){int n=0;void C(bool ok,string msg){if(!ok)throw new Exception(msg);n++;}
        foreach(var h in new[]{HeroId.Gladia,HeroId.Ambrosh,HeroId.Roderick}){hero=h;BeginNew60();int b=game.State.Body;SkipIntro82();C(screen==Screen.Prologue&&proStep==0&&game.State.Body==b,"origin skip");SkipIntro82();C(screen==Screen.Dialogue&&arrivalBeat80==0,"prologue skip arrival");}
        game.StartNew(HeroId.Roderick,Difficulty.Sopravvivenza);ResetEventRules();screen=Screen.World;EnterMap("casa_olmo",14,16);var hole=CurrentMap().Props.First(p=>p.Id=="olmo_hole82");C(Art("olmo_hole82")!=null,"hole texture");C(!Blocked("casa_olmo",10,4),"hole accessible");InteractProp(hole);C(choices.Count==2&&!game.HasItem("chiave_bassa"),"optional search");choices[0].Do();C(game.HasItem("chiave_bassa"),"key obtained");InteractProp(hole);C(choices.Count==0&&game.State.Inventory["chiave_bassa"]==1,"once only");CloseDialogue();EnterMap("strada_bassa",15,4);InteractDoor(CurrentMap().Doors.First(d=>d.Id=="locked_basso_1"));C(choices.Count==2,"key usable");choices[0].Do();C(game.State.Map=="officina_bassa","unlocks south");
        foreach(var kind in new[]{"ossuary","stitch","sexton","bell","corrupted","zombie"})for(int i=1;i<=(kind=="zombie"?6:1);i++){StartBattle(new WorldEnemy{Kind=kind,Id="test82_"+i,X=14,Y=10});string k=BattleArt75();C(Art("intact82_"+k)!=null,"intact "+k);for(int j=0;j<6;j++)C(Art("limb75_"+k+"_"+j)!=null,"layer");var a=EnemyMove82(k,.8f);C(a.X<0&&a.Sx>0,"attack movement");}
        C(Art("waiting88")!=null,"waiting entity");for(int i=0;i<8;i++)C(Art("waiting88_"+i)!=null,"waiting frame "+i);C(Art("map88_casa_olmo")!=null,"new cavity map");for(int i=0;i<16;i++)C(Art("roderick_rear82_"+i.ToString("D2"))!=null,"rear pose");GD.Print($"UPDATE82_OK {n}");GetTree().Quit();}
}
