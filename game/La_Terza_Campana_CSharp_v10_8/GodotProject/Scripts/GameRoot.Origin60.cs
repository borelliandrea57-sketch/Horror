using System;
using Godot;
using LaTerzaCampana.Core;

public partial class GameRoot
{
    // Origin runs in its own simulation: no hunger, hostile AI or stillness clock.
    private const int OT=44, OX=24, OY=40, OW=28, OH=14;
    private int originChapter,originMode,originFacing;
    private bool originWalking,originFirst;
    private int originChoice67, originSelection70;
    private float originClock,originMove,originVoiceTimer,originHint70;
    private Vector2I originPos;
    private Vector2 originVisual;
    private Vector2I OriginSpawn60=>new(13,12);
    private Vector2I OriginStand60=>IsAmbrosh63?originChapter==0?new(13,9):originChapter==1?new(15,8):new(14,8):IsRoderick64?new(13,9):GladiaStand67();
    private Vector2I OriginNpc60=>IsAmbrosh63?originChapter==0?new(13,7):originChapter==1?new(15,6):new(14,6):IsRoderick64?new(13,7):GladiaNpc67();
    private Vector2I OriginExit60=>new(24,12);
    private static readonly string[] OriginTitles60={"Il vicolo dei bisognosi","La stanza della febbre","La legnaia"};
    private static readonly string[] OriginIntro60={
        "Prima di Dùsterbedragna. Ogni mattina Gladia portava pane e conforto alle famiglie del quartiere. La chiamavano l’angelo del focolare.",
        "Quello stesso giorno. Una donna giace a letto, divorata dalla febbre. Gladia non ha mai abbandonato chi soffre.",
        "Quella sera. Sin dalla nascita, Gladia è legata a Braka, il dio antico della devastazione. La voce conosce ogni suo pensiero."};
    private static readonly string[] OriginQuestion60={"Nella borsa restano poche provviste. La bambina aspetta una risposta. Cosa fai?","La febbre non cede. Puoi tentare una medicazione, il rito del focolare o restare a vegliare.","Braka ti offre la sua forza. Come rispondi alla voce?"};
    private static readonly string[,] OriginChoices60={{"Dona il tuo pane","Conserva le provviste"},{"Resta e medica la donna","Invoca il sigillo del focolare"},{"Ascolta la voce di Braka","Prega e respingi l’impulso"}};
    private static readonly string[,] OriginResults60={{"La bambina sorride. La madre ti dona delle erbe in cambio.\n−1 pane · +1 erbe · +4 Mente · Misericordia","Conservi le razioni per le prossime visite.\n+1 pane · +4 Corpo"},{"La febbre si attenua. Ti bruciano le mani, ma la donna respira meglio.\n+1 bende · −4 Corpo · Mani benedette","Il calore del braciere si raccoglie fra le tue dita. La donna trova un poco di sollievo.\n+1 infuso · Sigillo del focolare"},{"Per un istante le ombre sembrano inchinarsi. La voce ti ha lasciato un segno.\n−10 Mente · +3 Corpo · Sussurro di Braka","Stringi il medaglione e ripeti le parole della tua infanzia. Braka non tace, ma arretra.\n+6 Mente · Medaglione · Litania di contenimento"}};
    private void BeginNew60()
    {
        arrivalBeat80=-1;curseClock83=30;curseFlash83=0;game.StartNew(hero,difficulty);ResetEventRules();ResetRoderick64();proStep=0;originVoiceTimer=35;
        if(hero==HeroId.Anatema){BeginEmile103();return;}
        if(hero==HeroId.Gladia){game.State.StoryFlags.Add("gladia_origin_started");LoadOrigin60(0);}
        else if(hero==HeroId.Ambrosh){game.State.StoryFlags.Add("ambrosh_origin_started");LoadOrigin60(0);}
        else {game.State.StoryFlags.Add("roderick_origin_started");LoadOrigin60(0);}
    }
    private static Vector2 OriginCenter60(Vector2I p)=>new(OX+(p.X+.5f)*OT,OY+(p.Y+.5f)*OT);
    private void LoadOrigin60(int chapter)
    {
        originChapter=chapter;ResetAmbroshMap65();ResetFlash66();ResetGladia67();originMode=0;originPos=OriginSpawn60;originVisual=OriginCenter60(originPos);originFacing=3;originClock=0;originMove=0;originWalking=false;originSelection70=0;ResetNarrative69();screen=Screen.Origin;
    }
    private bool OriginBlocked60(Vector2I p)
    {
        if(IsAmbrosh63)return AmbroshBlocked65(p);
        if(IsRoderick64)return RodBlocked66(p);
        return GladiaBlocked67(p);
    }
    private bool OriginNear60(Vector2I target)=>Math.Abs(originPos.X-target.X)+Math.Abs(originPos.Y-target.Y)<=2;
    private void OriginInteract60()
    {
        if(originMode==0){originMode=1;originClock=0;return;}
        if(originMode==4){StartFade69();return;} // recover legacy state, never require an exit
        if(originMode==5){FinishOrigin69();return;}
        if(originMode!=1)return;
        if(originPos==OriginStand60 || (OriginNear60(OriginStand60) && !OriginBlocked60(OriginStand60)))
        {originPos=OriginStand60;originMode=6;originClock=0;originWalking=true;originFacing=3;return;}
        if(IsAmbrosh63&&AmbroshInspect65())return;
        if(IsRoderick64&&RodInspectAction66())return;
        if(!IsAmbrosh63&&!IsRoderick64&&GladiaInspect67())return;
        // E gives immediate guidance even when no object is in range.
        originHint70=2.5f;

    }
    private void OriginChoose60(bool first)=>OriginChoose67(first?0:1);
    private void OriginChoose67(int index)
    {
        if(originMode!=2||!OriginChoices67.Apply(game,originChapter,index))return;
        originChoice67=index;originFirst=index==0;BeginReaction69();originMode=3;originClock=0;originWalking=false;
        if(originChapter==2)PlaySound(index==0?"event_drone":"ritual_hum",-5);
        else PlaySound("door_open",-15);
    }
    private void OriginKey60(int n,Key key,bool ok)
    {
        if(EmileOrigin103){EmileKey103(n,key,ok);return;}
        if(IsRoderick64&&key==Key.F2){rodDebug66=!rodDebug66;return;}
        if(IsAmbrosh63&&key==Key.F2){ambDebug65=!ambDebug65;return;}
        if(!IsAmbrosh63&&!IsRoderick64&&key==Key.F2){gladiaDebug67=!gladiaDebug67;return;}
        if(key==Key.Escape&&originMode==0&&originChapter==0){screen=Screen.Difficulty;return;}
        if(originMode is 2 or 8){
            int count=originMode==2?ChoiceCount67():Story69.Replies.Length;
            if(key is Key.Left or Key.Up or Key.A or Key.W)originSelection70=(originSelection70+count-1)%count;
            else if(key is Key.Right or Key.Down or Key.D or Key.S)originSelection70=(originSelection70+1)%count;
            else if(n>=1&&n<=count||ok){int selected=n>=1&&n<=count?n-1:originSelection70;if(originMode==2)OriginChoose67(selected);else ChooseFollow69(selected);}
            return;
        }
        if(NarrativeKey69(n,ok))return;
        if(ok)OriginInteract60();
    }
    private void OriginMouse60(Vector2 p)
    {
        if(EmileOrigin103){EmileMouse103(p);return;}
        if(NarrativeMouse69(p))return;
        if(originMode==2){for(int i=0;i<ChoiceCount67();i++)if(ChoiceRect67(i).HasPoint(p)){OriginChoose67(i);break;}}
        else if(originMode is 0 or 3 or 5)OriginInteract60();
    }
    private void UpdateOrigin60(float dt)
    {
        if(EmileOrigin103){UpdateEmile103(dt);return;}
        originClock+=dt;originHint70=Math.Max(0,originHint70-dt);UpdateFlash66(dt);gladiaInspect67=Math.Max(0,gladiaInspect67-dt);
        if(IsAmbrosh63)UpdateAmbroshMap65(dt);
        if(originMode==4){StartFade69();return;}
        if(UpdateNarrative69(dt))return;
        if(originMode==6){originVisual=originVisual.MoveToward(OriginCenter60(OriginStand60)+new Vector2(0,-18),dt*130);if(originVisual.DistanceTo(OriginCenter60(OriginStand60)+new Vector2(0,-18))<1){originPos=OriginStand60;BeginDialogue69();if(originChapter==2)PlaySound("mind_whisper",-2);}return;}
        if(originMode!=1)return;
        Vector2I d=Vector2I.Zero;
        if(Input.IsKeyPressed(Key.W)||Input.IsKeyPressed(Key.Up))d=Vector2I.Up;
        else if(Input.IsKeyPressed(Key.S)||Input.IsKeyPressed(Key.Down))d=Vector2I.Down;
        else if(Input.IsKeyPressed(Key.A)||Input.IsKeyPressed(Key.Left))d=Vector2I.Left;
        else if(Input.IsKeyPressed(Key.D)||Input.IsKeyPressed(Key.Right))d=Vector2I.Right;
        originMove-=dt;originWalking=d!=Vector2I.Zero;
        if(originWalking){originFacing=d.Y>0?0:d.X<0?1:d.X>0?2:3;if(originMove<=0){if(!OriginBlocked60(originPos+d))originPos+=d;originMove=.13f;}}
        originVisual=originVisual.Lerp(OriginCenter60(originPos),Mathf.Min(1,dt*15));
        if(originMode==1&&originPos==OriginStand60){originMode=6;originClock=0;originWalking=false;originFacing=3;}
    }
    private void OriginSprite60(string atlas,Vector2 pos,int row,int frame,int cw,int ch,float w,float h,float tilt=0)
    {
        var tex=Art(atlas);if(tex==null)return;
        DrawEllipse(pos.X,pos.Y+10,w*.22f,4,new Color(.03f,.02f,.02f,.48f));
        if(tilt!=0){DrawSetTransform(pos,tilt);DrawTextureRectRegion(tex,new Rect2(-w/2,-h+10,w,h),new Rect2(frame*cw,row*ch,cw,ch));DrawSetTransform(Vector2.Zero);}
        else DrawTextureRectRegion(tex,new Rect2(pos.X-w/2,pos.Y-h+10,w,h),new Rect2(frame*cw,row*ch,cw,ch));
    }
    private void DrawOrigin60()
    {
        if(EmileOrigin103){DrawEmile103();return;}
        if(IsRoderick64){DrawRoderickOrigin64();return;}
        if(IsAmbrosh63){DrawAmbroshOrigin63();return;}
        Rect(0,0,W,H,new Color(.025f,.019f,.016f));
        Image("origin_gladia_"+(originChapter+1),new Rect2(OX,OY,OW*OT,OH*OT));
        DrawGladiaAtmos67();
        Vector2 npc=ReactionNpc69(OriginCenter60(OriginNpc60));
        if(originChapter==0){DrawNpc61("mother",npc,0,(int)(tick*3)%8,68,100);DrawNpc61("girl",npc+new Vector2(-43,10),0,(int)(tick*4+2)%8,55,79);}
        if(originChapter==1)DrawPatient61(npc+new Vector2(0,-67));
        float gesture=ReactionTilt69();
        OriginSprite60("anim_gladia_00",originVisual,originFacing,originWalking&&(originMode==1||originMode==4||originMode==6)?(int)(tick*12)%8:0,44,90,50,88,gesture);
        if(originMode==3&&originClock<2.4f)
        {
            float t=Mathf.Clamp(originClock/1.5f,0,1);
            if(originChapter==0&&(originFirst||originChoice67==2)){var bread=Art("prop_item_pane");if(bread!=null){var at=originVisual.Lerp(npc+new Vector2(-37,-9),t)+new Vector2(0,-30-Mathf.Sin(t*Mathf.Pi)*23);DrawTextureRect(bread,new Rect2(at.X-15,at.Y-12,30,24),false,new Color(1,1,1,1-t*.6f));}}
            if(originChoice67<2&&(originChapter==1||originChapter==2&&!originFirst)){for(int i=0;i<16;i++){float a=i*Mathf.Tau/16+t*.7f;var at=(originChapter==1?npc:originVisual)+new Vector2(Mathf.Cos(a)*(13+38*t),-28+Mathf.Sin(a)*(9+20*t));DrawCircle(at,2.8f*(1-t)+.7f,new Color(.86f,.70f,.38f,(1-t)*.68f));}}
        }
        if(originChapter==2&&originMode is 2 or 3 or 6){float a=.13f+.05f*Mathf.Sin(tick*2);Rect(OX,OY,OW*OT,OH*OT,new Color(.21f,.018f,.02f,a));for(int i=0;i<18;i++){var at=npc+new Vector2(Mathf.Sin(i*2.37f+tick*.3f)*100,Mathf.Cos(i*3.2f+tick*.4f)*48-30);DrawCircle(at,1.6f+i%3,new Color(.82f,.24f,.14f,.2f));}}
        if(originMode==1){Vector2 p=OriginCenter60(OriginStand60);DrawArc(p+new Vector2(0,12),13+Mathf.Sin(tick*3)*2,0,Mathf.Tau,32,new Color(.83f,.68f,.40f,.7f),1.5f);}
        DrawReaction69();DrawFlashVignette66();if(gladiaDebug67)DrawGladiaDebug67();
        Rect(0,0,W,68,new Color(.035f,.025f,.02f,.95f));Inscribe("GLADIA  /  PRIMA DI DÙSTERBEDRAGNA",34,28,18,InkGold);Inscribe(OriginTitles60[originChapter],34,54,25,Parchment);Inscribe($"RICORDO {originChapter+1} / 3",1010,43,19,InkGold);
        if(originMode==1){Rect(0,655,W,65,new Color(.035f,.025f,.02f,.96f));string goal=originHint70>0?"Avvicinati al punto dorato, poi premi E.":"Avvicinati "+(originChapter==0?"alla famiglia":originChapter==1?"al letto":"al ceppo")+"  •  E interagisci";Inscribe(goal,32,682,23,Parchment);Inscribe("WASD / FRECCE • F2",1000,705,17,InkGold);DrawGladiaInspect67();return;}
        if(DrawNarrative69())return;
        if(originMode==6)return;
        if(originMode==5){Rect(0,0,W,H,new Color(0,0,0,.85f));InscribeCentered("PIETÀ E ROVINA",640,251,46,Parchment);InscribedParagraph("Prima di Dùsterbedragna, Gladia era già divisa fra la cura e la devastazione. Quando salì sul convoglio delle 03:17, portò con sé entrambe.",250,333,780,25,38,4,Parchment);InscribeCentered("IL CONVOGLIO DELLE 03:17  •  RICORDO IN DISSOLVENZA",640,605,23,InkGold);return;}
        Panel(24,472,1232,235,.97f);
        string label=originMode==0?"MEMORIA":originMode==2&&originChapter==2?"BRAKA — IL DIO ANTICO":originMode==3?"LA SCELTA RESTA":"GLADIA";
        Inscribe(label,54,512,23,originChapter==2&&originMode==2?Clot:InkGold);
        string text=originMode==0?OriginIntro60[originChapter]:originMode==2?OriginQuestion60[originChapter]:ChoiceResult67(OriginResults60);
        InscribedParagraph(text,56,548,1160,originMode==2?20:22,originMode==2?24:29,3,Parchment);
        if(originMode==2)DrawChoices67(OriginChoices60);
        else Inscribe("INVIO / E — CONTINUA",933,683,20,InkGold);
    }
    private void CheckOrigin60()
    {
        hero=HeroId.Gladia;BeginNew60();int tests=0;
        void Check(bool c,string desc){if(!c)throw new Exception(desc);tests++;}
        for(int ch=0;ch<3;ch++){LoadOrigin60(ch);var visited=new System.Collections.Generic.HashSet<Vector2I>{OriginSpawn60};var q=new System.Collections.Generic.Queue<Vector2I>();q.Enqueue(OriginSpawn60);while(q.Count>0){var p=q.Dequeue();foreach(var d in new[]{Vector2I.Up,Vector2I.Down,Vector2I.Left,Vector2I.Right}){var next=p+d;if(!OriginBlocked60(next)&&visited.Add(next))q.Enqueue(next);}}Check(visited.Contains(OriginStand60),"target reachable");Check(visited.Contains(OriginExit60),"exit reachable");Check(!OriginBlocked60(OriginSpawn60),"spawn clear");originMode=2;OriginChoose60(ch!=1);Check(originMode==3,"choice advances");Check(!GladiaOrigin.Apply(game,ch,false),"no duplicate");originMode=4;UpdateOrigin60(.1f);UpdateOrigin60(2.7f);}
        Check(originMode==5,"ending");OriginInteract60();Check(screen==Screen.Prologue&&game.State.StoryFlags.Contains("gladia_origin_done"),"prologue");Check(game.GetSkills()[2]=="Sigillo del focolare"&&game.GetSkills()[3]=="Sussurro di Braka","skill slots independent");GD.Print($"ORIGIN60_OK {tests}");GetTree().Quit();
    }
    private void UpdateBraka60(float dt)
    {
        if(game.State.Hero!=HeroId.Gladia||!game.State.StoryFlags.Contains("gladia_origin_done"))return;
        originVoiceTimer-=dt;if(originVoiceTimer>0)return;
        originVoiceTimer=game.State.StoryFlags.Contains("gladia_braka_mark")?45+rng.Next(30):85+rng.Next(35);
        string[] lines={"Braka: La misericordia è soltanto una forma più lenta di rovina.","Braka: Quanto ancora reggeranno queste mani?","Braka: Anche il fuoco che scalda può divorare ogni cosa."};
        if(messageTimer<=0){Notify(lines[rng.Next(lines.Length)],4);PlaySound("mind_whisper",-6);}
    }
}
