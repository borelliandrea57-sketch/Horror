using System;
using Godot;
using LaTerzaCampana.Core;
public partial class GameRoot
{
    // 7: dialogue, 8: second decision, 9: second consequence, 10: fade, 11: response.
    private int beat69,followChoice69;
    private Vector2 reactionBase69;
    private OriginNarrative69.Scene Story69=>OriginNarrative69.Resolve(game,originChapter);
    private void ResetNarrative69(){beat69=0;followChoice69=-1;reactionBase69=originVisual;}
    private void Mode69(int m){originMode=m;originClock=0;originWalking=false;originSelection70=0;}
    private void BeginDialogue69(){beat69=0;Mode69(7);}
    private void BeginReaction69(){reactionBase69=originVisual;}
    private void StartFade69(){Mode69(10);PlaySound("event_bell",-17);}
    private void ChooseFollow69(int index){if(originMode!=8||!OriginNarrative69.Apply(game,originChapter,index))return;followChoice69=index;reactionBase69=originVisual;Mode69(9);PlaySound(index==2?"mind_whisper":"ritual_hum",-14);}
    private void AdvanceNarrative69(){
        switch(originMode){
            case 7: if(++beat69>=Story69.Opening.Length)Mode69(2);else originClock=0;break;
            case 3: originVisual=reactionBase69;Mode69(11);break;
            case 11: if(Story69.Replies.Length>0)Mode69(8);else StartFade69();break;
            case 9: originVisual=reactionBase69;StartFade69();break;
        }
    }
    private bool UpdateNarrative69(float dt){
        if(originMode==7||originMode==8)return true; // dialogue and decisions wait for input
        if(originMode is 3 or 9){
            float t=Mathf.Clamp(originClock/2.2f,0,1);float a=Mathf.Sin(t*Mathf.Pi);
            int i=originMode==3?originChoice67:followChoice69;
            bool withdrawal=i>=2 || IsAmbrosh63&&i==1&&originChapter==0;
            var npc=OriginCenter60(OriginNpc60);var dir=(npc-reactionBase69).Normalized();
            originVisual=reactionBase69+dir*(withdrawal?-18:12)*a;
            if(originClock>=6.2f)AdvanceNarrative69();return true;
        }
        if(originMode==11){if(originClock>=5.8f)AdvanceNarrative69();return true;}
        if(originMode==10){if(originClock>=2.6f){if(originChapter<2)LoadOrigin60(originChapter+1);else Mode69(5);}return true;}
        if(originMode==5){if(originClock>=6.5f)FinishOrigin69();return true;}
        return false;
    }
    private void FinishOrigin69(){RecordRebellion101();game.State.StoryFlags.Add(IsRoderick64?"roderick_origin_done":IsAmbrosh63?"ambrosh_origin_done":"gladia_origin_done");screen=Screen.Prologue;proStep=0;}
    private Rect2 FollowRect69(int i)=>new(48+(i%2)*598,603+(i/2)*48,566,44);
    private bool NarrativeKey69(int n,bool ok){if(originMode==8){if(n>=1&&n<=Story69.Replies.Length)ChooseFollow69(n-1);return true;}if(originMode==10)return true;if(originMode is 7 or 11 or 3 or 9){if(ok)AdvanceNarrative69();return true;}return false;}
    private bool NarrativeMouse69(Vector2 p){if(originMode==8){for(int i=0;i<Story69.Replies.Length;i++)if(FollowRect69(i).HasPoint(p)){ChooseFollow69(i);break;}return true;}if(originMode is 7 or 11 or 3 or 9 or 10){NarrativeKey69(0,true);return true;}return false;}
    private float ReactionTilt69(){if(originMode is not (3 or 9))return 0;int i=originMode==3?originChoice67:followChoice69;return Mathf.Sin(Mathf.Clamp(originClock/2.2f,0,1)*Mathf.Pi)*(i>=2?-.15f:.12f);}
    private Vector2 ReactionNpc69(Vector2 at){if(originMode is not (3 or 9))return at;float t=Mathf.Clamp(originClock/2.2f,0,1);int i=originMode==3?originChoice67:followChoice69;return at+new Vector2((i>=2?8:-5)*Mathf.Sin(t*Mathf.Pi),-2*Mathf.Sin(t*Mathf.Pi));}
    private void DrawReaction69(){
        if(originMode is not (3 or 9))return;
        float p=Mathf.Clamp(originClock/2.2f,0,1),a=Mathf.Sin(p*Mathf.Pi);int i=originMode==3?originChoice67:followChoice69;
        var npc=OriginCenter60(OriginNpc60);var at=originVisual+new Vector2(0,-36);
        if(originChapter==2||IsRoderick64&&originChapter==1){
            Color c=IsRoderick64?new Color(.54f,.40f,.77f,.48f*a):IsAmbrosh63?new Color(.69f,.26f,.42f,.44f*a):new Color(.81f,.22f,.13f,.45f*a);
            for(int k=0;k<16;k++){float angle=k*Mathf.Tau/16+p*1.3f;var v=npc+new Vector2(Mathf.Cos(angle)*(18+p*62),-35+Mathf.Sin(angle)*(10+p*28));DrawCircle(v,1.5f+(k%3),c);}
            DrawArc(at,19+p*25,0,Mathf.Tau,40,c,1.5f);
        }else if(IsAmbrosh63&&originChapter==1&&((originMode==3&&i==1)||(originMode==9&&i==2))){var pos=at+new Vector2(20+25*p,8+45*p*p);Image("prop_item_infuso",new Rect2(pos.X-12,pos.Y-15,24,30),a);}
        else if(!IsAmbrosh63&&!IsRoderick64&&originChapter==1&&originMode==3&&i==0){var pos=at.Lerp(npc+new Vector2(0,-45),p);Image("prop_item_bende",new Rect2(pos.X-13,pos.Y-12,26,24),a);}
        else if(i>=2){DrawArc(at,16+p*8,0,Mathf.Pi,24,new Color(.76f,.48f,.38f,.36f*a),1.5f);}
    }
    private bool DrawNarrative69(){
        if(originMode==10){float t=Mathf.Clamp(originClock/1.15f,0,1);Rect(0,0,W,H,new Color(.13f,.09f,.055f,.25f*Mathf.Sin(t*Mathf.Pi)));Rect(0,0,W,H,new Color(.008f,.006f,.005f,t));if(t>.72f)InscribedParagraph(Story69.Bridge,270,325,740,25,35,3,Dust);return true;}
        if(originMode is not (7 or 8 or 9 or 11))return false;
        Panel(24,472,1232,235,.97f);
        string speaker,text;
        if(originMode==7){var b=Story69.Opening[Math.Clamp(beat69,0,Story69.Opening.Length-1)];speaker=b.Speaker;text=b.Text;}
        else if(originMode==11){speaker="NARRATORE";text=Story69.Reactions[Math.Clamp(originChoice67,0,Story69.Reactions.Length-1)];if(Story69.Replies.Length==0)text+="\n«"+Story69.FollowQuestion+"»";}
        else if(originMode==8){speaker=Story69.FollowSpeaker;text=Story69.FollowQuestion;}
        else{speaker="IL RICORDO RESTA";var r=Story69.Replies[Math.Clamp(followChoice69,0,Story69.Replies.Length-1)];text=r.Text;}
        Inscribe(speaker,54,512,23,InkGold);InscribedParagraph(text,56,548,1160,originMode==8?20:22,29,3,Parchment);
        if(originMode==8){for(int i=0;i<Story69.Replies.Length;i++){var r=FollowRect69(i);Panel(r.Position.X,r.Position.Y,r.Size.X,r.Size.Y,.97f);if(i==originSelection70)Frame(r.Position.X,r.Position.Y,r.Size.X,r.Size.Y,InkGold,2.5f);Inscribe($"[{i+1}] {Story69.Replies[i].Label}",r.Position.X+16,r.Position.Y+29,20,InkGold);}}
        else Inscribe(originMode==7?"INVIO / E — CONTINUA":"IL RICORDO PROSEGUE…  •  E",850,683,19,InkGold);
        return true;
    }
    private void CheckNarrative69(){int count=0;void C(bool ok,string msg){if(!ok)throw new Exception(msg);count++;}
        foreach(HeroId h in new[]{HeroId.Gladia,HeroId.Ambrosh,HeroId.Roderick}){
            for(int ch=0;ch<3;ch++){var scene=OriginNarrative69.Get(h,ch);C(scene.Opening.Length>=2,"opening dialogue");C(scene.Reactions.Length==2+OriginChoices67.Extra(h,ch).Length,"reaction per primary choice");for(int j=0;j<scene.Replies.Length;j++){var g=new GameRules(h,Difficulty.Sopravvivenza);var st=g.State;int body=st.Body,mind=st.Mind;double hunger=st.Hunger;var r=scene.Replies[j];C(OriginNarrative69.Apply(g,ch,j),"apply follow");C(!OriginNarrative69.Apply(g,ch,j),"once");C(st.Body==Math.Clamp(body+r.Body,1,100)&&st.Mind==Math.Clamp(mind+r.Mind,0,100)&&st.Hunger==Math.Clamp(hunger+r.Hunger,0,100),"costs");C(GameRules.FromJson(g.ToJson()).State.StoryFlags.Contains($"{h.ToString().ToLowerInvariant()}_origin_follow_{ch}_option_{j+1}"),"persist");}}
            hero=h;BeginNew60();for(int ch=0;ch<3;ch++){C(originChapter==ch,"auto next chapter");originMode=6;originVisual=OriginCenter60(OriginStand60)+new Vector2(0,-18);UpdateOrigin60(.1f);C(originMode==7,"dialogue trigger");for(int i=0;i<Story69.Opening.Length;i++)AdvanceNarrative69();C(originMode==2,"primary choice");OriginChoose67(0);C(originMode==3,"reaction");UpdateOrigin60(6.3f);C(originMode==11,"response");UpdateOrigin60(6f);if(Story69.Replies.Length>0){C(originMode==8,"second choice");ChooseFollow69(2);C(originMode==9,"second consequence");UpdateOrigin60(6.3f);}C(originMode==10,"fade");UpdateOrigin60(2.7f);}
            C(originMode==5,"ending");UpdateOrigin60(6.6f);C(screen==Screen.Prologue,"automatic train");
        }GD.Print($"NARRATIVE69_OK {count}");GetTree().Quit();
    }
}
