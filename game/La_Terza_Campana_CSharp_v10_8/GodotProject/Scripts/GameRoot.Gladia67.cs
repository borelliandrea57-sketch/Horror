using System;
using System.Collections.Generic;
using Godot;
using LaTerzaCampana.Core;
public partial class GameRoot
{
    private bool gladiaDebug67;
    private float gladiaInspect67;
    private string gladiaTitle67="",gladiaText67="";
    private int gladiaDetail67;
    private record GladiaObject67(Vector2I At,string Title,string Detail);
    private static readonly GladiaObject67[][] GladiaObjects67={
        [new(new(10,9),"Ruota e carretto","La ruota non gira più. Il legno conserva i segni di chi trascinava qui le merci."),new(new(13,7),"Altare dei poveri","Una candela per chi non ha più casa. Gladia si ferma un istante, in silenzio."),new(new(20,9),"Banco del mercato","Le stoviglie vuote sono già state lavate. Nessuno ha lasciato avanzi."),new(new(9,11),"Cesto abbandonato","Fra le foglie marce non c'è nulla di commestibile.")],
        [new(new(8,9),"Erbario aperto","Le pagine descrivono piante comuni. Nessuna promette un miracolo."),new(new(11,8),"Candele consumate","Qualcuno è rimasto sveglio tutta la notte a pregare."),new(new(20,8),"Catino d’acqua","L’acqua si è raffreddata. Il panno è già stato utilizzato."),new(new(21,10),"Armadio dei rimedi","Le ampolle sono vuote. Non puoi portare via ciò che non c'è.")],
        [new(new(8,9),"Braciere","Il calore ti ricorda le sere trascorse a preparare da mangiare per gli altri."),new(new(12,8),"Catasta di legna","Ogni tronco ha una forma diversa. Braka ti sussurra che tutti finiranno allo stesso modo."),new(new(19,9),"Porta del cortile","La serratura è fredda. Dietro non si sente alcuna voce umana."),new(new(10,11),"Fango e foglie","L’acqua trascina piccoli frammenti di corteccia. Per un istante li scambi per dita.")]
    };
    private Vector2I GladiaStand67()=>originChapter switch{0=>new(15,8),1=>new(16,8),_=>new(15,8)};
    private Vector2I GladiaNpc67()=>originChapter switch{0=>new(17,7),1=>new(16,6),_=>new(15,7)};
    private bool GladiaBlocked67(Vector2I p){int x=p.X,y=p.Y;if(p==OriginNpc60||originChapter==0&&p==OriginNpc60+Vector2I.Left)return true;return !(originChapter switch{
        0=>y==7&&x>=12&&x<=19 || y==8&&x>=10&&x<=20 || y==9&&x>=8&&x<=21 || y>=10&&y<=12&&x>=7&&x<=25,
        1=>y==8&&x>=10&&x<=20 || y==9&&x>=8&&x<=21 || y>=10&&y<=12&&x>=8&&x<=25,
        _=>y==8&&x>=10&&x<=18 || y==9&&x>=8&&x<=21 || y>=10&&y<=12&&x>=8&&x<=25});}
    private void ResetGladia67(){gladiaInspect67=0;originChoice67=0;}
    private GladiaObject67? GladiaNearby67(){foreach(var item in GladiaObjects67[originChapter])if(item.At==originPos)return item;return null;}
    private bool GladiaInspect67(){if(originMode is not (1 or 4)||originMode==1&&originPos==OriginStand60||originMode==4&&OriginNear60(OriginExit60))return false;var o=GladiaNearby67();if(o==null)return false;gladiaTitle67=o.Title;gladiaText67=o.Detail;gladiaDetail67=Array.IndexOf(GladiaObjects67[originChapter],o)+1;gladiaInspect67=5.5f;PlaySound("door_open",-22);return true;}
    private void DrawGladiaInspect67(){if(gladiaInspect67>0){Panel(24,518,1232,125,.96f);Inscribe(gladiaTitle67,51,557,22,InkGold);InscribedParagraph(gladiaText67,51,588,1020,21,25,2,Parchment);Image($"gladia_detail67_{originChapter+1}_{gladiaDetail67}",new Rect2(1111,527,104,104));}else if(GladiaNearby67() is {} o){Rect(25,613,510,36,new Color(.04f,.03f,.025f,.92f));Inscribe("E — "+o.Title,38,638,19,InkGold);}}
    private void DrawGladiaDebug67(){for(int y=0;y<OH;y++)for(int x=0;x<OW;x++){var r=new Rect2(OriginCenter60(new(x,y))-new Vector2(OT/2f,OT/2f),new Vector2(OT,OT));if(GladiaBlocked67(new(x,y)))DrawRect(r,new Color(.8f,.12f,.12f,.23f));else DrawRect(r,new Color(.4f,.8f,.5f,.27f),false,1);}}
    private void DrawGladiaAtmos67(){Vector2[] lights=originChapter==0?[new(160,225),new(398,166),new(536,255),new(1185,341)]:originChapter==1?[new(552,161),new(140,295),new(1069,244)]:[new(254,295),new(194,363),new(725,153),new(1143,252)];for(int i=0;i<lights.Length;i++){var at=lights[i];for(int j=0;j<6;j++)DrawCircle(at,12+j*9,new Color(.98f,.51f,.19f,.016f+.007f*Mathf.Sin(tick*8+i+j)));}
        for(int i=0;i<32;i++){float x=OX+60+(i*93.1f+tick*(2+i%3))%(OW*OT-120);float y=OY+62+(i*55.4f+tick*(1+i%4))%(OH*OT-124);DrawCircle(new Vector2(x,y),.7f+(i%3)*.3f,new Color(.9f,.74f,.49f,.15f+.08f*Mathf.Sin(tick+i)));}
        if(originChapter==0)for(int i=0;i<9;i++){float r=(tick*9+i*3)%20;DrawArc(new Vector2(480+i*34,401+i%3*26),r,.1f,Mathf.Pi-.1f,20,new Color(.8f,.68f,.54f,.11f*(1-r/20)),1);}
        if(originChapter==2)for(int i=0;i<24;i++){float a=(tick*.18f+i*.047f)%1;DrawCircle(new Vector2(250+Mathf.Sin(i*2.4f+tick)*28,302-a*105),1+i%3*.4f,new Color(.9f,.43f,.12f,(1-a)*.4f));}
    }
    private int ChoiceCount67()=>2+OriginChoices67.Extra(game.State.Hero,originChapter).Length;
    private Rect2 ChoiceRect67(int index)=>new(48+(index%2)*598,ChoiceCount67()>2?603+(index/2)*48:627,566,ChoiceCount67()>2?44:64);
    private string ChoiceResult67(string[,] old)=>originChoice67<2?old[originChapter,originChoice67]:OriginChoices67.Extra(game.State.Hero,originChapter)[originChoice67-2].Result;
    private void DrawChoices67(string[,] old){var extras=OriginChoices67.Extra(game.State.Hero,originChapter);for(int i=0;i<2+extras.Length;i++){var r=ChoiceRect67(i);Panel(r.Position.X,r.Position.Y,r.Size.X,r.Size.Y,.97f);if(i==originSelection70)Frame(r.Position.X,r.Position.Y,r.Size.X,r.Size.Y,InkGold,2.5f);string label=i<2?old[originChapter,i]:extras[i-2].Label;Inscribe($"[{i+1}] {label}",r.Position.X+16,r.Position.Y+(extras.Length>0?29:39),20,InkGold);}}
    private void CheckChoices67(){int count=0;void C(bool ok,string message){if(!ok)throw new Exception(message);count++;}
        foreach(HeroId h in new[]{HeroId.Gladia,HeroId.Ambrosh,HeroId.Roderick})for(int ch=0;ch<3;ch++){var extras=OriginChoices67.Extra(h,ch);for(int i=0;i<2+extras.Length;i++){var g=new GameRules(h,Difficulty.Sopravvivenza);var st=g.State;int body=st.Body,mind=st.Mind;double hunger=st.Hunger;C(OriginChoices67.Apply(g,ch,i),"valid choice");C(!OriginChoices67.Apply(g,ch,i),"no repeat");if(i>=2){var e=extras[i-2];C(st.Body==Math.Clamp(body+e.Body,1,100)&&st.Mind==Math.Clamp(mind+e.Mind,0,100)&&st.Hunger==Math.Clamp(hunger+e.Hunger,0,100),"costs");C(GameRules.FromJson(g.ToJson()).State.StoryFlags.Contains($"{h.ToString().ToLowerInvariant()}_origin_choice_{ch}_option_{i+1}"),"persistent option");}}}
        hero=HeroId.Gladia;BeginNew60();for(int ch=0;ch<3;ch++){LoadOrigin60(ch);var seen=new HashSet<Vector2I>{OriginSpawn60};var q=new Queue<Vector2I>();q.Enqueue(OriginSpawn60);while(q.Count>0){var p=q.Dequeue();foreach(var d in new[]{Vector2I.Up,Vector2I.Down,Vector2I.Left,Vector2I.Right})if(!OriginBlocked60(p+d)&&seen.Add(p+d))q.Enqueue(p+d);}C(seen.Contains(OriginStand60)&&seen.Contains(OriginExit60),"route");foreach(var o in GladiaObjects67[ch]){C(seen.Contains(o.At),o.Title);originMode=1;originPos=o.At;C(GladiaInspect67()&&gladiaTitle67==o.Title,"inspect");}originMode=2;OriginChoose67(2);C(originMode==3&&originChoice67==2,"extra result");originMode=4;UpdateOrigin60(.1f);UpdateOrigin60(2.7f);}C(originMode==5,"ending");OriginInteract60();C(screen==Screen.Prologue,"train");GD.Print($"CHOICES67_OK {count}");GetTree().Quit();}
}
