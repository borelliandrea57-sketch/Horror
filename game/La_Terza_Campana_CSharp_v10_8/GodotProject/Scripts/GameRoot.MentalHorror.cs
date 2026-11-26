using System;
using System.Linq;
using Godot;
using LaTerzaCampana.Core;

/// <summary>
/// State-driven horror: mind tiers are derived from the saved Mind stat, not from
/// ephemeral flags. A restored mind immediately removes visual hallucinations.
/// All hallucinations are non-colliding visuals, not real enemies.
/// </summary>
public partial class GameRoot
{
    private float darknessMindTimer=0f;
    private float madnessAudioTimer=15f;
    private int previousMindTier=0;
    private float mindPhase=0f;
    private int MentalTier => game.State.Mind<10?3:game.State.Mind<30?2:game.State.Mind<50?1:0;

    private bool HideCorruptedNpc(string npcId) =>
        npcId=="corrupted_wanderer" &&
        (game.State.StoryFlags.Contains("corrupted_hostile") ||
         game.State.StoryFlags.Contains("corrupted_defeated"));

    private void MakeCorruptedHostile()
    {
        var s=game.State;
        s.StoryFlags.Add("corrupted_hostile");
        CloseDialogue();
        var enemy=CurrentMap().Enemies.FirstOrDefault(e=>e.Id=="corrupted_wanderer");
        if(enemy!=null)
        {
            var npc=CurrentMap().Npcs.FirstOrDefault(n=>n.Id==enemy.Id);
            enemyPositions[enemy.Id]=npc!=null?new Vector2I(npc.X,npc.Y):new Vector2I(enemy.X,enemy.Y);
            PlaySound("monster_cry",-9);
            StartBattle(enemy);
        }
    }

    private void UpdateMindEffects(float delta)
    {
        var s=game.State;
        mindPhase+=delta;
        // Only genuinely unlit future areas drain Mind. Daylit streets do not.
        bool veryDark=s.Map is "galleria" or "casa_cantina" or "casa_olmo" or "casa_medico" or "magazzino_basso" or "binari";
        if(veryDark)
        {
            darknessMindTimer+=delta;
            if(darknessMindTimer>=9f)
            {
                darknessMindTimer-=9f;
                s.Mind=Math.Max(0,s.Mind-1);
            }
        }
        else darknessMindTimer=0;

        int tier=MentalTier;
        if(tier!=previousMindTier)
        {
            previousMindTier=tier;
            madnessAudioTimer=4.5f;
            if(tier==1)Notify("Un respiro basso, alle tue spalle. Ma non c'è nessuno.",4.4f);
            else if(tier==2)Notify("Qualcosa si muove sotto la superficie della realtà.",4.3f);
            else if(tier==3)Notify("Non guardare negli angoli. Loro sanno che li vedi.",4.8f);
        }
        if(tier==0){madnessAudioTimer=15;return;}
        madnessAudioTimer-=delta;
        if(madnessAudioTimer<=0)
        {
            string fx=tier==1?"mind_whisper":tier==2?(rng.Next(2)==0?"mind_whisper":"mind_moan"):
                new[]{"mind_whisper","mind_moan","mind_scrape"}[rng.Next(3)];
            PlaySound(fx,-3);
            madnessAudioTimer=(tier==1?16:tier==2?11:8)+rng.Next(0,9);
        }
    }

    // Decals are purely pictorial: they never introduce invisible hitboxes.
    private void DrawRitualDecals(string currentMap)
    {
        if(currentMap!="strada")return;
        var positions=new (string art,float x,float y,float size,float alpha)[]
        {
            ("ritual_seal_1",8.35f,6.65f,124f,.67f),
            ("ritual_seal_2",26.25f,11.10f,113f,.63f),
            
            ("ritual_seal_4",30.45f,19.15f,112f,.56f),
        };
        foreach(var p in positions)
        {
            var tex=Art(p.art);if(tex==null)continue;
            float x=(p.x+.5f)*T-camera.X-p.size/2;
            float y=(p.y+.5f)*T-camera.Y-p.size/2;
            if(x>W||x+p.size<0||y>H||y+p.size<0)continue;
            DrawTextureRect(tex,new Rect2(x,y,p.size,p.size),false,new Color(.9f,.60f,.56f,p.alpha));
        }
    }

    private void DrawMindEffects()
    {
        if(screen is not Screen.World and not Screen.Dialogue)return;
        int tier=MentalTier;
        if(tier<2)return;
        float phase=mindPhase;
        // Fixed low opacity texture corruption; avoid flashing full-screen effects.
        int stripes=tier==2?4:7;
        for(int i=0;i<stripes;i++)
        {
            float pos=(i*109 + 24 + Mathf.Sin(phase*.65f+i*2.1f)*21 + phase*4f)%H;
            float x=25+Mathf.Sin(i*2.38f+phase*.9f)*25;
            var tint=i%2==0?new Color(.49f,.11f,.15f,.08f):new Color(.38f,.41f,.46f,.048f);
            Rect(x,pos,W-44,1.5f+(i%3),tint);
        }
        // Small beetles crawl over the *screen*, not on a physical tile.
        int bugs=tier==2?3:8;
        for(int i=0;i<bugs;i++)
        {
            float x=((i*163f + 90f + phase*(9f+i%3*6f))% (W+60))-30f;
            float y=((i*137f + 110f + Mathf.Sin(phase*(.71f+i*.04f))*54f)%(H+20))-10f;
            var body=new Color(.025f,.015f,.012f,.48f);
            DrawCircle(new Vector2(x,y),3.1f,body);
            DrawCircle(new Vector2(x+2,y-5),2.3f,body);
            for(int limb=0;limb<3;limb++)
            {
                float cy=y+limb*3-4;
                DrawLine(new Vector2(x-2,cy),new Vector2(x-6-(limb%2),cy+Mathf.Sin(phase*5f+i+limb)*3),body,1.1f);
                DrawLine(new Vector2(x+2,cy),new Vector2(x+6+(limb%2),cy-Mathf.Sin(phase*5f+i+limb)*3),body,1.1f);
            }
        }
        if(tier<3)return;
        // No hitbox and no combat target; an occasional apparition only.
        float cycle=phase%13.0f;
        if(cycle>4.2f)return;
        float visibility=Mathf.Clamp(cycle/.8f,0f,1f)*Mathf.Clamp((4.2f-cycle)/1.5f,0f,1f);
        var ghost=Art("anim_levatrice");
        if(ghost!=null)
        {
            float x=W*.71f+Mathf.Sin(phase*.8f)*27f;
            float y=H*.36f+Mathf.Sin(phase*1.45f)*11f;
            DrawTextureRectRegion(ghost,new Rect2(x,y,120,138),new Rect2(0,0,128,132),
                new Color(.62f,.42f,.43f,.43f*visibility));
        }
    }
}
