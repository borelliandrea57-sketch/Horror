using System;
using System.Linq;
using Godot;
using LaTerzaCampana.Core;
public partial class GameRoot
{
    private record Zombie74(string Key,string Name,int Torso,int Head,int Arm,int Leg,int Attack,float Speed,float Infection,string[] Loot);
    private static readonly Zombie74[] Undead74={
        new("cyst","IL CISTICO",68,36,24,18,12,2.8f,.42f,new[]{"","bende","erbe","infuso"}),
        new("thin","IL CONSUNTO",44,26,17,13,8,4.5f,.08f,new[]{"","","pane","mela"}),
        new("eye","LO SFIGURATO",60,34,22,17,11,3.4f,.14f,new[]{"","bende","conserva"}),
        new("heavy","IL CORPULENTO",98,45,34,24,17,2.0f,.20f,new[]{"","conserva","bende","pane"}),
        new("hunch","IL GOBBO",76,37,29,20,13,3.0f,.25f,new[]{"","erbe","infuso","bende"}),
        new("flayed","LO SCORTICATO",83,39,27,20,16,3.9f,.22f,new[]{"","bende","infuso","conserva"})};
    private static Zombie74 Z74(string id){int i=id.Length>0&&id[^1]>='1'&&id[^1]<='6'?id[^1]-'1':0;return Undead74[i];}
    private void InstallUndead74(){if(!maps.TryGetValue("casa_olmo_servizio",out var m))return;foreach(var (id,x,y) in new[]{("olmo_undead_5",5,8),("olmo_undead_6",13,6)})if(!m.Enemies.Any(e=>e.Id==id)&&!Blocked("casa_olmo_servizio",x,y))m.Enemies.Add(new WorldEnemy{Id=id,Kind="zombie",X=x,Y=y});}
    private float zombieHit74=-10,zombieSwing74=-10;
    private void DrawZombieBattle74(Texture2D tex){float strike=Mathf.Max(0,1-(tick-zombieSwing74)/.35f);float hit=Mathf.Max(0,1-(tick-zombieHit74)/.28f);float w=Math.Min(365,405f*tex.GetWidth()/tex.GetHeight());float h=w*tex.GetHeight()/tex.GetWidth();float x=865-w/2-35*strike+12*hit*Mathf.Sin(tick*55);float y=454-h+Mathf.Sin(tick*1.9f)*2;DrawEllipse(865,455,w*.35f,9,new Color(.015f,.01f,.01f,.48f));DrawTextureRect(tex,new Rect2(x,y,w,h),false,new Color(1,1-hit*.23f,1-hit*.23f));}
    private void PenteShadow74(Vector2 at,float width){for(int i=4;i>=1;i--)DrawEllipse(at.X,at.Y+7,width*(.18f+i*.045f),3+i*1.5f,new Color(.035f,.025f,.019f,.10f));}
    private void CheckUndead74(){int n=0;void C(bool ok,string name){if(!ok)throw new Exception(name);n++;}C(Undead74.Select(x=>x.Key).Distinct().Count()==6,"six variants");game.StartNew(HeroId.Gladia,Difficulty.Sopravvivenza);ResetEventRules();EnterMap("casa_olmo",14,16);for(int i=1;i<=6;i++){string id="olmo_undead_"+i;var z=Z74(id);var e=new WorldEnemy{Id=id,Kind="zombie",X=14,Y=10};StartBattle(e);C(battle!.Parts["torace"]==z.Torso,"variant torso");foreach(var prefix in new[]{"anim_","battle_","corpse_","combat_parts_"})C(Art(prefix+"z74_"+z.Key)!=null,"asset "+prefix+z.Key);C(EnemyAtlas59(e)=="anim_z74_"+z.Key,"matching sprite");}C(maps["casa_olmo_servizio"].Enemies.Count(e=>e.Kind=="zombie")==2,"two service variants");C(Art("anim_pentesilea74")!=null&&Art("anim_pentesilea_fire5974")!=null,"graded pentesilea");GD.Print($"UNDEAD74_OK {n}");GetTree().Quit();}
}
