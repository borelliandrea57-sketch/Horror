namespace LaTerzaCampana.Core;
public sealed record CombatStats(int Attack,int ArcaneAttack,int Defense,int ArcaneDefense,int Agility);
public static class CombatStats100 {
 public static CombatStats Base(HeroId h)=>h switch {HeroId.Gladia=>new(10,16,11,19,14),HeroId.Ambrosh=>new(14,11,16,11,18),HeroId.Anatema=>new(15,0,15,22,18),_=>new(17,20,10,10,13)};
 public static CombatStats Effective(GameRules g){var b=Base(g.State.Hero);var e=g.GetEquipmentStats();var s=g.State;if(s.Hero==HeroId.Anatema){var f=s.StoryFlags;b=b with {Attack=b.Attack+(f.Contains("anatema103_forgia")?2:0),ArcaneDefense=b.ArcaneDefense+(f.Contains("anatema103_lucidita")?3:0)+(f.Contains("anatema103_indesiderato")?2:0)+(f.Contains("anatema103_rinnegato")?1:0)};}int a=s.Injuries.Count(x=>x.StartsWith("braccio")),l=s.Injuries.Count(x=>x.StartsWith("gamba")),lost=s.LostLimbs84.Count(x=>x.StartsWith("gamba"));return new(Math.Max(2,(int)Math.Round((b.Attack+e.Attack)*(a==0?1:a==1?.7:.35))),b.ArcaneAttack,b.Defense+e.Defense,b.ArcaneDefense+e.MindProtection,Math.Max(1,b.Agility-l*3-lost*3-(s.Hunger<25?3:0)-(s.Body<25?2:0)));}
 public static int EnemyAgility(string k,int z=0)=>k switch{"zombie"=>new[]{12,20,15,8,13,18}[Math.Clamp(z,0,5)],"bell"=>9,"ossuary"=>19,"sexton"=>11,"stitch"=>14,"corrupted"=>16,"falciante"=>17,_=>14};
 public static int ArcaneDefense(string k)=>k switch{"bell"=>19,"ossuary"=>13,"sexton"=>16,"stitch"=>8,"falciante"=>7,_=>6};
 public static double FleeChance(int h,int e,Difficulty d)=>Math.Clamp(.55+(h-e)*.025+(d==Difficulty.Facile?.10:d==Difficulty.DeSade?-.12:0),.10,.90);
 public static int SkillDamage(int nominal,int stat,int defense)=>Math.Max(1,nominal+stat-14-defense/2);
 public static int IncomingDamage(double attack,int defense,int bonus)=>Math.Max(1,(int)(attack-defense*.35)+bonus);
}
