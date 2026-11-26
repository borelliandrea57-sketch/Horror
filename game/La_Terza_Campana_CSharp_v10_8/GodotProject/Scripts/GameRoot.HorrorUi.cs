using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using LaTerzaCampana.Core;

/// <summary>Visual-only rework: illuminated manuscript interface, no boxes of buttons.</summary>
public partial class GameRoot
{
    private Font? _gothicFont;
    private int titleSelection=0;
    private Font GothicFont
    {
        get
        {
            if (_gothicFont==null)
            {
                var f=new SystemFont();
                f.FontNames=new string[]{"Georgia","EB Garamond","Noto Serif","DejaVu Serif"};
                _gothicFont=f;
            }
            return _gothicFont;
        }
    }
    private readonly Color Parchment=Color.FromHtml("ddc6a7");
    private readonly Color InkGold=Color.FromHtml("b49365");
    private readonly Color Dust=Color.FromHtml("a58f78");
    private readonly Color Clot=Color.FromHtml("8e3e38");
    private void Inscribe(string value,float x,float y,int size=23,Color? tint=null)
    {
        var c=tint??Parchment;
        DrawString(GothicFont,new Vector2(x+1.2f,y+1.2f),value,HorizontalAlignment.Left,-1,size,new Color(.03f,.02f,.02f,.87f));
        DrawString(GothicFont,new Vector2(x,y),value,HorizontalAlignment.Left,-1,size,c);
    }
    private void InscribeCentered(string value,float cx,float y,int size=23,Color? tint=null)
    {
        var width=GothicFont.GetStringSize(value,HorizontalAlignment.Left,-1,size).X;
        Inscribe(value,cx-width/2,y,size,tint);
    }
    private void InscribedParagraph(string input,float x,float y,float width,int size=19,int spacing=28,int limit=7,Color? tint=null)
    {
        int used=0;string line="";
        foreach(var word in (input??"").Split(' ',StringSplitOptions.RemoveEmptyEntries))
        {
            string next=line.Length>0?line+" "+word:word;
            if(GothicFont.GetStringSize(next,HorizontalAlignment.Left,-1,size).X>width&&line.Length>0)
            {
                Inscribe(line,x,y+spacing*used,size,tint);
                line=word;if(++used>=limit)return;
            }
            else line=next;
        }
        if(line.Length>0&&used<limit)Inscribe(line,x,y+spacing*used,size,tint);
    }
    private void SootRule(float x,float y,float w,bool selected=false)
    {
        Color c=selected?InkGold:new Color(.43f,.32f,.25f,.63f);
        DrawLine(new Vector2(x,y),new Vector2(x+w,y),c,selected?1.8f:1f);
        DrawCircle(new Vector2(x,y),selected?3.8f:2.4f,c);
        DrawCircle(new Vector2(x+w,y),selected?3.8f:2.4f,c);
        if(selected)DrawLine(new Vector2(x+12,y+3),new Vector2(x+w-12,y+3),new Color(.46f,.18f,.16f,.65f),.8f);
    }
    private void Halo(float cx,float cy,float scale=1f)
    {
        var ring=new Color(.58f,.36f,.19f,.45f);
        DrawArc(new Vector2(cx,cy),45*scale,0,Mathf.Tau,64,ring,1.6f);
        DrawArc(new Vector2(cx,cy),52*scale,0,Mathf.Tau,64,new Color(.34f,.16f,.13f,.55f),.9f);
        for(int i=0;i<12;i++)
        {
            float a=i*Mathf.Tau/12f;
            var a1=new Vector2(Mathf.Cos(a),Mathf.Sin(a));
            DrawLine(new Vector2(cx,cy)+a1*55*scale,new Vector2(cx,cy)+a1*(65+(i%3)*4)*scale,ring,1f);
        }
        DrawLine(new Vector2(cx-18*scale,cy),new Vector2(cx+18*scale,cy),ring,1.1f);
        DrawLine(new Vector2(cx,cy-22*scale),new Vector2(cx,cy+22*scale),ring,1.1f);
        DrawCircle(new Vector2(cx,cy),3.5f*scale,Clot);
    }
    private void ForgedCorner(float x,float y,int flipX,int flipY)
    {
        var c=new Color(.63f,.47f,.30f,.7f);
        DrawLine(new Vector2(x,y+flipY*39),new Vector2(x,y),c,1.5f);
        DrawLine(new Vector2(x,y),new Vector2(x+flipX*39,y),c,1.5f);
        DrawArc(new Vector2(x+flipX*10,y+flipY*10),9,0,Mathf.Tau,18,new Color(.43f,.20f,.18f,.62f),1);
    }
    private void DrawTitleV3()
    {
        Image("title",new Rect2(0,0,W,H));
        // No inset black rectangle: an irregular ink gradient settles over the right side of the painting.
        for(int i=0;i<154;i++)
        {
            float x=510+i*5.2f;
            float progress=i/153f;
            Rect(x,0,6,H,new Color(.045f,.030f,.025f,.14f+progress*.77f));
        }
        Rect(1070,0,210,H,new Color(.03f,.021f,.019f,.62f));
        Halo(947,99,.45f);
        InscribeCentered("LA",955,183,30,Parchment);
        InscribeCentered("TERZA",955,246,58,Parchment);
        InscribeCentered("CAMPANA",955,304,57,Parchment);
        SootRule(757,333,398,true);
        InscribeCentered("UN RACCONTO DI FAME E CENERE",955,365,18,InkGold);
        string[] labels={"NUOVA PARTITA", "CONTINUA", "ESCI"};
        for(int i=0;i<labels.Length;i++)
        {
            float y=436+i*66;
            bool selected=i==titleSelection;
            if(selected)
            {
                DrawLine(new Vector2(740,y-15),new Vector2(765,y-15),Clot,3);
                DrawCircle(new Vector2(756,y-15),5,InkGold);
            }
            Inscribe(labels[i],795,y,selected?33:29,selected?Parchment:Dust);
            if(selected)SootRule(795,y+13,299,true);
        }
        InscribeCentered("W/S  SCEGLI     •     INVIO  CONFERMA",957,674,17,InkGold);
        Inscribe("v9.6",1180,706,16,Dust);
        Inscribe("LE CENERI NON DIMENTICANO",38,694,14,Dust);
    }
    private void DrawHeroV3()
    {
        Image("title",new Rect2(0,0,W,H),.42f);Rect(0,0,W,H,new Color(.025f,.018f,.018f,.72f));
        InscribeCentered(HeroCount101==4?"QUATTRO VITE, UNA SOLA CITTÀ":"TRE VITE, UNA SOLA CITTÀ",640,86,43,Parchment);SootRule(218,112,844,true);
        float step=HeroCount101==4?294:394;float width=HeroCount101==4?262:338;
        for(int n=0;n<HeroCount101;n++){
            var id=(HeroId)n;float x=(HeroCount101==4?62:94)+n*step;float mid=x+width/2+5;
            Image("portrait_"+id.ToString().ToLowerInvariant(),new Rect2(x+5,149,width,334));
            ForgedCorner(x+5,149,1,1);ForgedCorner(x+width+5,149,-1,1);ForgedCorner(x+5,483,1,-1);ForgedCorner(x+width+5,483,-1,-1);
            SootRule(x+7,502,width-4,hero==id);if(hero==id)DrawCircle(new Vector2(mid,522),4,Clot);
            InscribeCentered(Catalog.Heroes[id].Name.ToUpperInvariant(),mid,552,32,hero==id?Parchment:Dust);
            InscribedParagraph(Catalog.Heroes[id].Description,x+10,588,width-12,17,22,4,Dust);
        }
        InscribeCentered("A / D  SCEGLI   •   INVIO  CONFERMA   •   ESC  INDIETRO",640,700,18,InkGold);
    }
    private void DrawDifficultyV3()
    {
        // Three vertically stacked, individually illustrated difficulty choices.
        // Text lives in the artwork, but actual selection/controls are native C#.
        Image("difficulty_v5", new Rect2(0, 0, W, H));
        float y = 155 + (int)difficulty * 160;
        var ink = difficulty == Difficulty.DeSade ? Color.FromHtml("c44a45") : Color.FromHtml("d9c098");
        Frame(173, y - 6, 934, 139, ink, 3.1f);
        ForgedCorner(177, y - 4, 1, 1);
        ForgedCorner(1102, y - 4, -1, 1);
        ForgedCorner(177, y + 128, 1, -1);
        ForgedCorner(1102, y + 128, -1, -1);
    }
    private void DrawWorldHudV3(WorldMap m)
    {
        Rect(0,0,1280,71,new Color(.041f,.031f,.028f,.86f));
        SootRule(18,67,1245);
        Inscribe(m.Name,24,31,23,InkGold);
        Inscribe(Catalog.Heroes[game.State.Hero].Name+" — "+game.State.Difficulty+" · Dùsterbedragna",27,56,17,Dust);
        var s=game.State;
        Bar("CORPO",472,19,s.Body,Color.FromHtml("8d514f"));
        Bar("MENTE",733,19,s.Mind,Color.FromHtml("727e86"));
        Bar("FAME",987,19,(float)s.Hunger,Color.FromHtml("96825b"));
        Rect(0,648,1280,72,new Color(.041f,.029f,.025f,.83f));
        SootRule(18,653,1245);
        var target=ClosestTarget();
        if(target!=null)Inscribe("✦  E — "+target.Name,30,694,23,Parchment);
        else Inscribe("WASD  CAMMINA      E  ESAMINA      I  DIARIO      F2  COLLISIONI",30,693,19,Dust);
        if(debugHitboxes)Inscribe("MAPPA DI COLLISIONE",990,698,17,Clot);
        if(editCollision)Inscribe("EDITOR ATTIVO  •  clicca per modificare",420,636,18,Clot);
    }
    private void DrawPortraitPlate()
    {
        Image("portrait_"+HeroKey(),new Rect2(69,169,286,313));
        // Antiqued corner filigree rather than a standard rectangular UI panel.
        ForgedCorner(69,169,1,1);ForgedCorner(355,169,-1,1);
        ForgedCorner(69,482,1,-1);ForgedCorner(355,482,-1,-1);
        SootRule(81,496,264,true);
        InscribeCentered(Catalog.Heroes[game.State.Hero].Name.ToUpperInvariant(),214,531,33,Parchment);
        var s=game.State;
        Inscribe("CORPO",77,566,16,Dust);Inscribe($"{s.Body}",315,566,21);
        SootRule(76,575,274);
        Inscribe("MENTE",77,600,16,Dust);Inscribe($"{s.Mind}",315,600,21);
        SootRule(76,607,274);
        Inscribe("FAME",77,635,16,Dust);Inscribe($"{(int)s.Hunger}",315,635,21);
        if(s.Bleeding)Inscribe("SANGUINA",77,662,17,Clot);
        else if(s.Injuries.Count>0)Inscribe($"ARTI PERDUTI: {s.Injuries.Count}",77,662,17,Clot);
        else Inscribe("IL CORPO RICORDA",77,662,17,InkGold);
    }
    private void DrawInventoryV3()
    {
        Image("ui_manuscript",new Rect2(0,0,W,H));
        DrawPortraitPlate();
        string[] tabs={"INVENTARIO", "EQUIPAGGIAMENTO", "ABILITÀ", "DIARIO", "STATISTICHE"};
        float[] tx={446,596,810,950,1065};
        for(int i=0;i<tabs.Length;i++)
        {
            bool selected=invTab==i;
            int size=i==1?18:19;
            Inscribe(tabs[i],tx[i],85,size,selected?Parchment:Dust);
            if(selected)SootRule(tx[i],99,GothicFont.GetStringSize(tabs[i],HorizontalAlignment.Left,-1,size).X,true);
        }
        SootRule(438,122,755);
        var s=game.State;
        if(invTab==0)
        {
            Inscribe("Le provviste dei superstiti",465,176,33,InkGold);
            Inscribe("Ciò che porti con te comincia a pesare più di quanto dovrebbe.",467,205,18,Dust);
            int drawn=0;
            int stride=ItemKeys.Count(id=>s.Inventory.GetValueOrDefault(id,0)>0)>10?38:42;
            for(int i=0;i<ItemKeys.Length;i++)
            {
                string id=ItemKeys[i];int count=s.Inventory.GetValueOrDefault(id,0);
                if(count<=0)continue;
                int y=238+drawn*stride;drawn++;
                Inscribe(i<9?$"{i+1:D2}":"—",458,y,19,Clot);
                Inscribe(Catalog.Items.GetValueOrDefault(id,id),503,y,26,Parchment);
                Inscribe($"{count}x",1129,y,22,InkGold);
                var desc=id switch{"bende"=>"Tiene unite le ferite che dovrebbero restare aperte.","erbe"=>"L'odore del verde è quasi insopportabile.","infuso"=>"Cancella un ricordo e calma il tremore.","pane"=>"Crosta dura, sapore di farina umida.","mela"=>"La polpa è scura dove nessuno l'ha toccata.","conserva"=>"La carne galleggia in un grasso immobile.","lettera_scarlatta"=>"Un sigillo rosso. Premi 9 per rileggere la lettera.","grimaldello"=>"Si consuma aprendo una porta chiusa.","chiave_cisterne"=>"La chiave stretta dalla donna dei fiori.",_=>"La serratura riconosce chi la porta."};
                Inscribe(desc,504,y+24,17,Dust);
                SootRule(457,y+37,705);
            }
            if(drawn==0)Inscribe("Non è rimasto niente, tranne la fame.",485,308,24,Dust);
            Inscribe("NUMERI 1–6  •  USA UN OGGETTO    9  •  LEGGI LETTERA",470,684,17,InkGold);
        }
        else if(invTab==1)
        {
            Inscribe("Le cose che ti tengono in vita",463,177,32,InkGold);
            Inscribe("Ogni protezione ha il prezzo del proprio peso.",468,206,18,Dust);
            for(int i=0;i<4;i++)
            {
                var slot=Slots[i];string id=s.Equipped.GetValueOrDefault(slot,"");
                string name=Catalog.Equipment.TryGetValue(id,out var equipped)?equipped.DisplayName:"Nulla da offrire";
                float y=264+i*89;
                Inscribe($"{i+1:00}",457,y,18,Clot);
                Inscribe(SlotLabels[i],507,y,19,InkGold);
                Inscribe(name,694,y,26,Parchment);
                if(equipped!=null)Inscribe($"ATT +{equipped.Attack}   DIF +{equipped.Defense}   MENTE +{equipped.MindProtection}",697,y+25,16,Dust);
                else Inscribe("La pelle non basta contro ciò che attende.",697,y+25,16,Dust);
                SootRule(458,y+40,708);
            }
            var bonus=game.GetEquipmentStats();
            Inscribe($"PROTEZIONE  •  Attacco +{bonus.Attack}    Difesa +{bonus.Defense}",479,652,19,InkGold);
            Inscribe("NUMERI 1–4  •  CAMBIA EQUIPAGGIAMENTO",470,685,16,Dust);
        }
        else if(invTab==2)
        {
            Inscribe("Arti e rituali",465,175,37,InkGold);
            Inscribe("Alcuni gesti si imparano soltanto sopravvivendo.",468,205,18,Dust);
            int i=0;
            foreach(var skill in game.GetSkills())
            {
                float y=264+i*98;
                DrawArc(new Vector2(477,y-4),22,0,Mathf.Tau,32,new Color(.55f,.37f,.24f,.73f),1.6f);
                InscribeCentered((i+1).ToString(),477,y+2,21,Clot);
                Inscribe(skill,524,y,31,Parchment);
                var desc=i switch {0=>"Una voce può salvarti o condannarti. Parla a vivi e mostri.",1=>"Una conoscenza strappata al dolore. Consuma Mente.",2=>"La forza è una forma di preghiera disperata.",_=>"La magia domanda sempre qualcosa in cambio."};
                desc=skill switch {"Visione irrazionale"=>"Rivela le presenze vicine; in battaglia mette in guardia.","Richiamo del velo"=>"Evoca un aiutante temporaneo in combattimento.","Patto della follia"=>"Doma Non-morti o Cuciti feriti. Richiede una gamba fuori uso.","Mani benedette"=>"Cura più potente, ferma il sangue e riduce l’infezione.","Sigillo del focolare"=>"Colpisce il braccio armato o il torace, poi ti mette in guardia.","Sussurro di Braka"=>"Danni arcani al torace. Costa 28 Mente.","Litania di contenimento"=>"Danni arcani alla testa e guardia. Costa 16 Mente.",_=>desc};
                Inscribe(desc,526,y+29,18,Dust);
                SootRule(460,y+51,702);i++;
            }
            Inscribe("In battaglia seleziona ABILITÀ per usare i rituali.",472,686,17,InkGold);
        }
        else if(invTab==4)DrawStats100();
        else
        {
            Inscribe("Il registro dei giorni perduti",463,172,34,InkGold);
            string[] diary={
                "Il convoglio delle 03:17 non compare su alcun registro.",
                "Viaggiatore: "+Catalog.Heroes[s.Hero].Name+". Prova: "+s.Difficulty+".",
                s.StoryFlags.Contains("ticket_shown")?"Hai consegnato il biglietto. Ti hanno riconosciuto.":"Hai nascosto il biglietto. Qualcuno ti sta cercando.",
                s.StoryFlags.Contains("passenger_helped")?"Hai aiutato l'uomo dietro la porta. Sanguinavi.":"Le suppliche di quell'uomo tornano quando ti fermi.",
                s.StoryFlags.Contains("voice_answered")?"Hai pronunciato una risposta durante l'eclissi.":"Il silenzio durante l'eclissi non è bastato.",
                game.HasItem("chiave_stazione")?"La chiave della stazione è nelle tue mani.":"Il portone è chiuso. Come sei arrivato nella piazza?",
                s.CompanionJoined?"Ada cammina dietro di te, senza fare domande.":"Ada è ancora sola, nella città.",
                s.StoryFlags.Contains("bell_defeated")?"Il Cuore della Campana non batte più.":"Sotto le pietre qualcosa batte ancora."};
            for(int i=0;i<diary.Length;i++)
            {
                float y=225+i*55;
                Inscribe($"{i+1:00}",460,y,18,Clot);
                InscribedParagraph(diary[i],511,y,660,20,23,2,Dust);
                SootRule(462,y+27,700);
            }
            if(game.State.StoryFlags.Contains("olmo_docs92"))Inscribe("7  •  DOCUMENTI DELL’OLMO",480,665,17,InkGold);
            if(game.HasItem("lettera_scarlatta"))Inscribe("9  •  RILEGGI LA LETTERA SCARLATTA",480,685,17,InkGold);
        }
        Inscribe("TAB   VOLTA PAGINA      •      I / ESC   CHIUDI      •      F5   PREGA E SALVA",444,708,15,InkGold);
    }
}
