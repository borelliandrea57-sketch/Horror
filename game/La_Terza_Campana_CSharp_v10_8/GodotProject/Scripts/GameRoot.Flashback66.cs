using System;
using System.Collections.Generic;
using Godot;
public partial class GameRoot
{
    private record RodObject66(Vector2I At,string Name,string Detail);
    private static readonly RodObject66[][] RodObjects66={
        new RodObject66[]{new(new(10,8),"Appunti del fronte","Disegni di esseri incontrati in guerra. Alcuni recano date che devono ancora arrivare."),new(new(18,8),"Specchio incrinato","Il tuo riflesso si volta un istante prima di te."),new(new(21,10),"Cassa dei cimeli","Una medaglia, un distintivo e una lettera senza destinatario. Sono reali, almeno per ora.")},
        new RodObject66[]{new(new(9,8),"Gabbie arrugginite","Dalle sbarre pendono catene rotte. Qualcosa le ha piegate dall'interno."),new(new(16,7),"Ciotola e cuccia","La creatura ha imparato a temere gli uomini. Un patto può sostituire le catene."),new(new(20,10),"Pozza scura","Nell'acqua vedi due creature. Quando alzi lo sguardo ne resta una sola.")},
        new RodObject66[]{new(new(10,10),"Cerchio del velo","Ogni linea ritorna al proprio inizio. Nyarlatoth ti insegna a lasciare un varco."),new(new(20,10),"Colonna degli occhi","Gli occhi scolpiti seguono qualcosa che passa dietro di te."),new(new(15,9),"Altare senza volto","Tre volti, una voce: non sei obbligato a credere a tutto ciò che vedi.")}
    };
    private float flashClock66,rodInspect66,ambDelirium66;
    private string rodTitle66="",rodText66="",ambThought66="";
    private int ambLast66=-1;
    private bool rodDebug66;
    private static readonly string[] AmbDeliriumLines66={"Ambrosh: La lussuria mi assale anche qui, davanti all’altare. Non è un ordine a cui devo obbedire.","Nuwdanìe: Ogni forma ti ricorda la procreazione.  —  Ambrosh: Basta. Il mio corpo è mio.","Ambrosh: Persino le radici mi sembrano simboli osceni. So che è soltanto un delirio."};
    private void ResetFlash66(){flashClock66=0;rodInspect66=0;ambDelirium66=0;ambLast66=-1;}
    private bool RodBlocked66(Vector2I p){if(p==OriginNpc60)return true;int x=p.X,y=p.Y;return !(originChapter switch{
        0=>y is 7 or 8 && x>=9&&x<=21 || y>=9&&y<=12&&x>=10&&x<=25,
        1=>y is 6 or 7 && x>=10&&x<=19 || y>=8&&y<=10&&x>=9&&x<=21 || y>=11&&y<=12&&x>=9&&x<=25,
        _=>y==8&&x>=10&&x<=18 || y>=9&&y<=10&&x>=8&&x<=21 || y>=11&&y<=12&&x>=8&&x<=25});}
    private RodObject66? RodNearby66(){foreach(var obj in RodObjects66[originChapter])if(originPos==obj.At)return obj;return null;}
    private bool RodInspectAction66(){if(originMode is not (1 or 4)||originMode==1&&originPos==OriginStand60||originMode==4&&OriginNear60(OriginExit60))return false;var obj=RodNearby66();if(obj==null)return false;rodTitle66=obj.Name;rodText66=obj.Detail;rodInspect66=5.5f;PlaySound("mind_whisper",-15);return true;}
    private void UpdateFlash66(float dt){flashClock66+=dt;rodInspect66=Math.Max(0,rodInspect66-dt);ambDelirium66=Math.Max(0,ambDelirium66-dt);if(IsAmbrosh63&&originMode is 1 or 4&&flashClock66>7&&ambLast66!=originChapter){ambLast66=originChapter;ambDelirium66=6;ambThought66=AmbDeliriumLines66[originChapter];PlaySound("mind_whisper",-8);}}
    private void DrawFlashVignette66()
    {
        float alpha=IsRoderick64?1f:IsAmbrosh63?.99f:.97f;
        float w=OW*OT,h=OH*OT;
        Image("flashback_vignette66",new Rect2(OX,OY,w,h),alpha+.035f*Mathf.Sin(tick*.85f));
        Rect(OX,OY,w,h,IsRoderick64?new Color(.09f,.045f,.13f,.075f):IsAmbrosh63?new Color(.23f,.075f,.065f,.045f):new Color(.23f,.16f,.07f,.04f));
        // Vintage flashback wash: warm sepia tone, slight film fade and surface wear.
        Rect(OX,OY,w,h,new Color(.44f,.31f,.16f,IsRoderick64?.12f:IsAmbrosh63?.17f:.19f));
        Rect(OX,OY,w,h,new Color(.81f,.71f,.50f,.065f+.012f*Mathf.Sin(tick*.6f)));
        for(int y=(int)OY+2;y<OY+h;y+=5)
            DrawLine(new Vector2(OX,y),new Vector2(OX+w,y),new Color(.08f,.05f,.03f,.085f),1f);
        for(int i=0;i<95;i++)
        {
            float px=OX+38+Mathf.PosMod(i*97.3f+tick*(2.1f+(i%5)*.17f),w-76);
            float py=OY+34+Mathf.PosMod(i*53.7f+tick*(1.3f+(i%4)*.09f),h-68);
            float r=1.1f+(i%3)*.55f;
            var c=i%5==0?new Color(.18f,.11f,.08f,.19f):new Color(.96f,.91f,.80f,.16f);
            DrawCircle(new Vector2(px,py),r,c);
        }
        if(Mathf.Sin(tick*.72f)>.55f)
        {
            for(int i=0;i<3;i++)
            {
                float sx=OX+162+i*287+Mathf.Sin(tick*.33f+i)*18;
                float sy=OY+52+(i*73)%180;
                DrawLine(new Vector2(sx,sy),new Vector2(sx+8,sy+h*.56f),new Color(.92f,.83f,.70f,.19f),1.15f);
            }
        }
        if(flashClock66<.8f)Rect(OX,OY,w,h,new Color(0,0,0,(1-flashClock66/.8f)*.85f));
    }
    private void DrawRodAtmos66(){if(originChapter==0){foreach(var p in new[]{new Vector2(297,257),new Vector2(741,200),new Vector2(1202,133)})for(int j=0;j<5;j++)DrawCircle(p,14+j*9,new Color(.9f,.53f,.25f,.022f+.008f*Mathf.Sin(tick*7+j)));}
        for(int i=0;i<(originChapter==0?22:42);i++){float x=OX+65+(i*103.2f+tick*(3+i%3))%(OW*OT-130),y=OY+75+(i*41.8f+tick*(1+i%4))%(OH*OT-135);DrawCircle(new Vector2(x,y),.7f+i%3*.5f,originChapter==2?new Color(.64f,.49f,.78f,.26f):new Color(.73f,.64f,.48f,.18f));}
        if(originChapter>0){for(int j=0;j<5;j++){var p=new Vector2(280+j*180+Mathf.Sin(tick*.2f+j)*65,490+j%2*50);DrawEllipse(p.X,p.Y,140,24,new Color(.58f,.54f,.65f,.022f));}}
        if(originChapter==2&&Mathf.Sin(tick*.75f)>.92f)DrawNyar76(new Rect2(947,242,102,124),.35f);
    }
    private void DrawRodInspect66(){if(rodInspect66>0){Panel(24,518,1232,125,.95f);Inscribe(rodTitle66,51,557,22,InkGold);InscribedParagraph(rodText66,51,588,1175,21,25,2,Parchment);}else if(RodNearby66() is {} o){Rect(25,613,510,36,new Color(.04f,.03f,.025f,.92f));Inscribe("E — "+o.Name,38,638,19,InkGold);}}
    private void DrawRodDebug66(){if(!rodDebug66)return;for(int y=0;y<OH;y++)for(int x=0;x<OW;x++){var r=new Rect2(OriginCenter60(new(x,y))-new Vector2(OT/2f,OT/2f),new Vector2(OT,OT));if(RodBlocked66(new(x,y)))DrawRect(r,new Color(.8f,.12f,.12f,.22f));else DrawRect(r,new Color(.4f,.8f,.5f,.27f),false,1);}}
    private void DrawAmbRelics66(){Vector2[] p=originChapter==0?new[]{new Vector2(442,258),new Vector2(792,252)}:originChapter==1?new[]{new Vector2(523,242),new Vector2(682,251)}:new[]{new Vector2(430,327),new Vector2(820,328)};for(int i=0;i<p.Length;i++){Image("fertility_relic66_"+(i+1),new Rect2(p[i].X-13,p[i].Y-38,26,38));}if(ambDelirium66>0){float a=Mathf.Min(.32f,ambDelirium66*.11f);Rect(OX,OY,OW*OT,OH*OT,new Color(.36f,.045f,.09f,a*.45f));for(int i=0;i<3;i++){var q=new Vector2(310+i*310,356+Mathf.Sin(tick*1.7f+i)*14);Image("fertility_relic66_3",new Rect2(q.X-28,q.Y-75,56,80),a);}}}
    private void DrawAmbDelirium66(){if(ambDelirium66<=0||ambInspectTime65>0)return;Panel(24,533,1232,103,.91f);Inscribe("DELIRIO — NUWDANÌE",51,565,19,Clot);InscribedParagraph(ambThought66,51,597,1174,20,24,2,Parchment);}
    private void CheckFlash66(){int count=0;void C(bool ok,string label){if(!ok)throw new Exception(label);count++;}hero=LaTerzaCampana.Core.HeroId.Roderick;BeginNew60();for(int ch=0;ch<3;ch++){LoadOrigin60(ch);var seen=new HashSet<Vector2I>{OriginSpawn60};var q=new Queue<Vector2I>();q.Enqueue(OriginSpawn60);while(q.Count>0){var p=q.Dequeue();foreach(var d in new[]{Vector2I.Up,Vector2I.Down,Vector2I.Left,Vector2I.Right})if(!RodBlocked66(p+d)&&seen.Add(p+d))q.Enqueue(p+d);}C(seen.Contains(OriginStand60)&&seen.Contains(OriginExit60),"Roderick route");foreach(var obj in RodObjects66[ch]){C(seen.Contains(obj.At),obj.Name);originMode=1;originPos=obj.At;C(RodInspectAction66()&&rodTitle66==obj.Name,"inspection "+obj.Name);}}hero=LaTerzaCampana.Core.HeroId.Ambrosh;BeginNew60();for(int ch=0;ch<3;ch++){LoadOrigin60(ch);originMode=1;UpdateFlash66(8);C(ambDelirium66>0&&ambLast66==ch,"delirium begins");UpdateFlash66(7);C(ambDelirium66==0,"delirium expires");UpdateFlash66(8);C(ambDelirium66==0,"no duplicate");}GD.Print($"FLASH66_OK {count}");GetTree().Quit();}
}
