using System;
using System.Linq;
using System.Collections.Generic;
using Godot;
using LaTerzaCampana.Core;
public partial class GameRoot
{
    private bool IsAmbrosh63=>game.State.Hero==HeroId.Ambrosh;
    private float nuwTimer63=50,catBuzz63;
    private static readonly string[] AmbTitle63={"La casa della benedizione","La stanza del contenimento","Il giardino di Nuwdanìe"};
    private static readonly string[] AmbIntro63={
        "Anni prima di Dùsterbedragna. Alla nascita, la famiglia accolse le caratteristiche intersessuali di Ambrosh come un segno sacro. Lo consacrarono a Nuwdanìe, dea della procreazione. Ora adulto, egli sa quanto costi quel dono.",
        "La benedizione non conosce tregua. Una lussuria intrusiva e incontrollabile minaccia la sua libertà. Ogni oggetto innocuo sembra diventare un’allusione. La famiglia gli ha imposto dispositivi di contenimento e una disciplina ferrea. Ambrosh desidera soltanto essere padrone di sé.",
        "Nel giardino votivo i fiori crescono anche sulle pietre. Nuwdanìe non è una madre benevola: la sua volontà è generare senza fine. Ambrosh sente la dea chiamarlo per nome."};
    private static readonly string[] AmbQuestion63={"Il tutore attende una risposta. Vuoi accettare gli insegnamenti del culto o rivendicare la tua autonomia?","Il familiare posa i rimedi sul tavolo. Come vuoi affrontare la crisi?","Nuwdanìe attende. Accetti il suo potere, lo respingi o ti allontani?"};
    private static readonly string[,] AmbChoice63={{"Accetta il ruolo votivo","Rivendica la tua autonomia"},{"Impara la disciplina rituale","Prepara i rimedi"},{"Accogli il potere della dea","Chiudi la soglia"}};
    private static readonly string[,] AmbResult63={{"Memorizzi le parole del culto, ma non rinunci alla tua volontà.\n+1 Infuso · Laccio votivo","Ti prepari a viaggiare senza chiedere il permesso.\n+1 Pane · +8 Corpo"},{"Impari a riconoscere la crisi prima che ti travolga.\n+1 Bende · Respiro di contenimento","Conservi nella borsa le provviste del guaritore.\n+2 Infusi · +1 Erbe · Disciplina del corpo"},{"La vita fremisce nelle pietre. La dea ti lascia una parte della propria forza.\n−12 Mente · +4 Corpo · Sovraccarico vitale","Stringi il medaglione. La voce non tace, ma resta oltre la soglia.\nMedaglione equipaggiato · Sigillo della soglia"}};
    private void DrawAmbroshOrigin63(){
        Rect(0,0,W,H,new Color(.025f,.019f,.016f));Image("origin_ambrosh_"+(originChapter+1),new Rect2(OX,OY,OW*OT,OH*OT));
        DrawAmbroshAtmosphere65();DrawAmbRelics66();
        var npc=ReactionNpc69(OriginCenter60(OriginNpc60));
        if(originChapter==0)DrawNpc61("keeper",npc,0,(int)(tick*3)%8,68,100);
        if(originChapter==1)DrawNpc61("mother",npc,0,(int)(tick*3)%8,64,94);
        float gesture=ReactionTilt69();
        OriginSprite60("anim_ambrosh_00",originVisual,originFacing,originWalking&&(originMode is 1 or 4 or 6)?(int)(tick*12)%8:0,44,90,50,88,gesture);
        if(originChapter==2&&originMode is 2 or 3 or 6){Rect(OX,OY,OW*OT,OH*OT,new Color(.22f,.35f,.14f,.12f+.04f*Mathf.Sin(tick*2)));for(int i=0;i<28;i++){var p=npc+new Vector2(Mathf.Sin(i*2.37f+tick*.35f)*190,Mathf.Cos(i*3.2f+tick*.45f)*105);DrawCircle(p,1+i%3,new Color(.83f,.83f,.45f,.36f));}}
        if(originMode==3&&originClock<2){for(int i=0;i<15;i++){float a=i*Mathf.Tau/15+originClock;DrawCircle(originVisual+new Vector2(Mathf.Cos(a)*30,-24+Mathf.Sin(a)*18),2,new Color(.82f,.75f,.42f,.6f*(1-originClock/2)));}}
        if(originMode==1){var p=OriginCenter60(OriginStand60);DrawArc(p+new Vector2(0,12),13+Mathf.Sin(tick*3)*2,0,Mathf.Tau,32,new Color(.83f,.68f,.40f,.7f),1.5f);}
        DrawReaction69();DrawFlashVignette66();if(ambDebug65)DrawAmbroshCollision65();
        Rect(0,0,W,68,new Color(.035f,.025f,.02f,.95f));Inscribe("AMBROSH / PRIMA DI DÙSTERBEDRAGNA",34,28,18,InkGold);Inscribe(AmbTitle63[originChapter],34,54,25,Parchment);Inscribe($"RICORDO {originChapter+1} / 3",1010,43,19,InkGold);
        if(originMode==1){Rect(0,655,W,65,new Color(.035f,.025f,.02f,.96f));Inscribe(originHint70>0?"Avvicinati al punto dorato, poi premi E.":"Avvicinati al punto segnato  •  E interagisci",32,682,23,Parchment);Inscribe("WASD / FRECCE • F2",1000,705,17,InkGold);DrawAmbroshInspect65();DrawAmbDelirium66();return;}
        if(DrawNarrative69())return;
        if(originMode==6)return;
        if(originMode==5){Rect(0,0,W,H,new Color(0,0,0,.85f));InscribeCentered("IL DONO SENZA MISURA",640,251,46,Parchment);InscribedParagraph("Quando Ambrosh salì sul convoglio delle 03:17, portava nel sangue una dea che non conosceva misura. Per la prima volta, la destinazione era una sua scelta.",250,333,780,25,38,4,Parchment);InscribeCentered("IL CONVOGLIO DELLE 03:17  •  RICORDO IN DISSOLVENZA",640,605,23,InkGold);return;}
        Panel(24,472,1232,235,.97f);Inscribe(originMode==0?"MEMORIA":originMode==2&&originChapter==2?"NUWDANÌE — LA DEA ANTICA":originMode==3?"LA SCELTA RESTA":"AMBROSH",54,512,23,InkGold);
        InscribedParagraph(originMode==0?AmbIntro63[originChapter]:originMode==2?AmbQuestion63[originChapter]:ChoiceResult67(AmbResult63),56,548,1160,originMode==2?20:22,originMode==2?24:29,3,Parchment);
        if(originMode==2)DrawChoices67(AmbChoice63);else Inscribe("INVIO / E — CONTINUA",933,683,20,InkGold);
    }
    private void UpdateNuwdanie63(float dt){
        if(game.State.Map=="strada"){var p=CatPos63(game.State.Map);catBuzz63-=dt;if(visibleHero.DistanceTo(p)<T*3&&catBuzz63<=0){catBuzz63=3;PlaySound("cat_buzz63",-12);}}
        if(!IsAmbrosh63||!game.State.StoryFlags.Contains("ambrosh_origin_done"))return;
        nuwTimer63-=dt;if(nuwTimer63>0)return;nuwTimer63=game.State.StoryFlags.Contains("ambrosh_nuwdanie_mark")?48:85;
        if(messageTimer<=0){string[] lines={"Nuwdanìe: Ogni radice cerca una via.","Nuwdanìe: Perché temi ciò che cresce?","Ambrosh: La mia volontà non appartiene a nessuna dea."};Notify(lines[rng.Next(lines.Length)],4);PlaySound("mind_whisper",-7);}
    }
    private Vector2 CatPos63(string map)=>map=="strada"?TileCenter(12,20):TileCenter(8,17);
    private void InstallCat63(){if(maps.TryGetValue("strada",out var m)&&!m.Props.Any(p=>p.Id=="dead_cat63"))m.Props.Add(new WorldProp{Id="dead_cat63",Name="Gatto morto",X=12,Y=20,PaintOnly=true});}
    private void DrawCat63(string map){if(map!="strada")return;var p=CatPos63(map)-camera;DrawEllipse(p.X,p.Y+7,32,10,new Color(.065f,.046f,.036f,.38f));Image("dead_cat63",new Rect2(p.X-34,p.Y-15,68,28));for(int i=0;i<6;i++){float a=tick*(3.1f+i*.26f)+i*2.4f;var f=p+new Vector2(Mathf.Cos(a)*(14+i*3),Mathf.Sin(a*1.3f)*(6+i*1.4f)-9);DrawCircle(f,1.2f,new Color(.07f,.06f,.05f,.95f));DrawLine(f-new Vector2(2,1),f+new Vector2(2,-1),new Color(.64f,.58f,.48f,.7f),1);}}
    private void CheckAmbrosh63(){hero=HeroId.Ambrosh;int count=0;void Check(bool c,string n){if(!c)throw new Exception(n);count++;}for(int mask=0;mask<8;mask++){BeginNew60();Check(screen==Screen.Origin,"starts origin");for(int ch=0;ch<3;ch++){LoadOrigin60(ch);var seen=new HashSet<Vector2I>{OriginSpawn60};var q=new Queue<Vector2I>();q.Enqueue(OriginSpawn60);while(q.Count>0){var p=q.Dequeue();foreach(var d in new[]{Vector2I.Up,Vector2I.Down,Vector2I.Left,Vector2I.Right})if(!OriginBlocked60(p+d)&&seen.Add(p+d))q.Enqueue(p+d);}Check(seen.Contains(OriginStand60)&&seen.Contains(OriginExit60),"reachable");foreach(var obj in AmbObjects65[ch]){Check(seen.Contains(obj.At),"inspection point reachable: "+obj.Title);originMode=1;originPos=obj.At;Check(AmbroshInspect65()&&ambInspectTitle65==obj.Title,"inspection opens: "+obj.Title);}Check(AmbroshBlocked65(new Vector2I(3,3)),"upper wall blocked");Check(AmbroshBlocked65(ch==2?new Vector2I(5,9):new Vector2I(4,10)),"large furnishing blocked");originMode=2;OriginChoose60((mask&(1<<ch))!=0);Check(originMode==3&&!AmbroshOrigin.Apply(game,ch,true),"one-time reward");originMode=4;UpdateOrigin60(.1f);UpdateOrigin60(2.7f);}Check(originMode==5,"ending");OriginInteract60();Check(screen==Screen.Prologue&&game.State.StoryFlags.Contains("ambrosh_origin_done"),"transition");var loaded=GameRules.FromJson(game.ToJson());Check(loaded.GetSkills().SequenceEqual(game.GetSkills()),"save skills");}Check(!Blocked("strada",12,20),"cat reachable");GD.Print($"AMBROSH63_OK {count}");GetTree().Quit();}
}
