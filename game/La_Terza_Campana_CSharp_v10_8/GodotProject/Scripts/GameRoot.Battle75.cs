using System;
using Godot;
using LaTerzaCampana.Core;
public partial class GameRoot
{
    private int target75, pending75=-1;
    private float heroAttack75, enemyAttack75;
    private const float HeroDuration95=.98f;
    private float enemyDuration95=1.1f;
    private bool enemyPending95, enemyResolved95, winPending95, heroLanded95, enemyGuard95;
    private string deathPending95="";
    private bool BattleBusy75=>pending75>=0||heroAttack75>0||enemyAttack75>0||enemyPending95;
    private string BattleArt75()=>battle!.Kind=="falciante"?"falciante84":battle.Kind=="zombie"?"z74_"+Z74(battle.Id).Key:battle.Kind;
    private void ResetBattle75(){target75=0;pending75=-1;heroAttack75=enemyAttack75=0;enemyPending95=enemyResolved95=winPending95=heroLanded95=enemyGuard95=false;deathPending95="";}
    private void SelectTarget75(int d){if(battle==null)return;for(int i=0;i<Parts.Length;i++){target75=(target75+d+Parts.Length)%Parts.Length;if(battle.Parts[Parts[target75]]>0)break;}}
    private void StartAttack75(int index){if(battle==null||BattleBusy75||index<0||index>=Parts.Length)return;target75=index;if(battle.Parts[Parts[index]]<=0){battle.Log="Quella parte è già fuori uso.";return;}pending75=index;heroAttack75=HeroDuration95;heroLanded95=false;}
    private float HeroProgress95=>heroAttack75>0?1-heroAttack75/HeroDuration95:0;
    private float EnemyProgress95=>enemyAttack75>0?1-enemyAttack75/enemyDuration95:0;
    private void RequestEnemy95(){if(battle==null)return;enemyPending95=true;if(heroAttack75<=0)BeginEnemy95();}
    private void BeginEnemy95(){if(battle==null)return;enemyPending95=false;enemyResolved95=false;enemyLanded82=false;enemyGuard95=battle.Guard;enemyDuration95=EnemyProfile95(BattleArt75()).Duration;enemyAttack75=enemyDuration95;}
    private void UpdateBattle75(float dt){
        if(battle==null||screen!=Screen.Combat){ResetBattle75();return;}
        if(heroAttack75>0){heroAttack75=Math.Max(0,heroAttack75-dt);if(pending75>=0&&(heroAttack75<=HeroDuration95*.49f)){int i=pending75;pending75=-1;AttackPart(i);}
            if(heroAttack75<=0){if(winPending95){winPending95=false;WinBattle();return;}if(enemyPending95)BeginEnemy95();}return;}
        if(enemyAttack75>0){enemyAttack75=Math.Max(0,enemyAttack75-dt);if(!enemyResolved95&&enemyAttack75<=enemyDuration95*.46f){enemyResolved95=true;EnemyTurn();}
            if(enemyAttack75<=0){if(winPending95){winPending95=false;WinBattle();return;}if(deathPending95.Length>0){var reason=deathPending95;deathPending95="";Die(reason);return;}}}
    }
    private void DrawEnemy75()=>DrawEnemyRig95();
    private void DrawOlmo75(string map){if(map!="casa_olmo"&&map!="casa_olmo_servizio")return;
        (string k,int x,int y,float s,float a)[] props=map=="casa_olmo"?new[]{
            ("plate",7,13,.8f,.92f),("broken",8,14,1f,.94f),("wood",9,7,1.1f,.88f),("card",6,6,.7f,.92f),("odd",10,19,.82f,.90f),
            ("plate",22,14,.72f,.92f),("broken",24,16,1.1f,.95f),("wood",25,7,.95f,.88f),("card",20,7,.7f,.90f),("odd",23,19,.9f,.90f),
            ("broken",5,16,.9f,.94f),("card",15,9,.65f,.88f),("wood",20,18,1.05f,.90f),("plate",6,15,.7f,.88f),("broken",24,19,.9f,.92f),
            ("plate",4,4,.86f,.86f),("broken",5,5,.92f,.92f),("wood",4,6,.95f,.83f),("card",7,4,.7f,.90f),("odd",3,7,.82f,.88f),
            ("plate",26,4,.84f,.86f),("broken",25,5,.96f,.92f),("wood",26,6,1.0f,.83f),("card",23,4,.72f,.89f),("odd",27,7,.84f,.88f),
            ("plate",4,21,.86f,.86f),("broken",5,22,.96f,.93f),("wood",4,20,1.0f,.84f),("card",7,21,.72f,.89f),("odd",3,19,.84f,.88f),
            ("plate",26,21,.86f,.86f),("broken",25,22,1f,.93f),("wood",26,20,1.0f,.84f),("card",23,21,.72f,.89f),("odd",27,19,.84f,.88f),
            ("broken",14,4,.85f,.90f),("plate",16,21,.8f,.86f),("card",13,18,.65f,.86f),("wood",16,6,.92f,.84f)
        }:new[]{
            ("plate",6,9,.8f,.90f),("broken",8,8,1f,.94f),("card",12,9,.65f,.88f),("odd",11,6,.9f,.90f),("wood",14,10,1f,.84f),
            ("plate",4,4,.84f,.88f),("broken",15,4,.9f,.92f),("card",4,12,.7f,.88f),("odd",15,12,.9f,.89f),("wood",10,4,.92f,.84f)
        };
        foreach(var o in props){if(map=="casa_olmo"&&o.x>=2&&o.x<=10&&o.y>=12)continue;var t=Art("olmo75_"+o.k);if(t==null)continue;var p=TileCenter(o.x,o.y)-camera;float w=t.GetWidth()*o.s,h=t.GetHeight()*o.s;DrawTextureRect(t,new Rect2(p.X-w/2,p.Y-h/2,w,h),false,new Color(.86f,.81f,.76f,o.a));}
        foreach(var tag in new[]{"tl","tr","bl","br"}){var t=Art("olmo75_corner_"+tag);if(t==null)continue;Vector2 pos=tag switch{ "tl"=>TileCenter(2,2), "tr"=>TileCenter(map=="casa_olmo"?28:17,2), "bl"=>TileCenter(2,map=="casa_olmo"?23:13), _=>TileCenter(map=="casa_olmo"?28:17,map=="casa_olmo"?23:13)};pos-=camera;DrawTextureRect(t,new Rect2(pos.X-56,pos.Y-56,112,112),false,new Color(.94f,.9f,.85f,.93f));}}
    private void CheckBattle75(){int n=0;void C(bool ok,string s){if(!ok)throw new Exception(s);n++;}game.StartNew(HeroId.Gladia,Difficulty.Sopravvivenza);ResetEventRules();EnterMap("casa_olmo",14,16);
        foreach(string kind in new[]{"ossuary","stitch","sexton","bell","corrupted","zombie"})for(int z=1;z<=(kind=="zombie"?6:1);z++){StartBattle(new WorldEnemy{Kind=kind,Id="test75_"+z,X=14,Y=10});for(int i=0;i<6;i++)C(Art("limb75_"+BattleArt75()+"_"+i)!=null,"layer "+kind+i);battle!.Phase=1;BattleKey(0,Key.Down);C(target75==1,"down");BattleKey(0,Key.Up);C(target75==0,"up");battle.Parts[Parts[1]]=0;BattleKey(0,Key.Down);C(target75==2,"skip unavailable");BattleKey(0,Key.E);C(pending75==2,"confirm E");C(BattleBusy75,"lock");ResetBattle75();}
        foreach(string k in new[]{"plate","broken","card","wood","odd"})C(Art("olmo75_"+k)!=null,"decor "+k);GD.Print($"BATTLE75_OK {n}");GetTree().Quit();}
}
