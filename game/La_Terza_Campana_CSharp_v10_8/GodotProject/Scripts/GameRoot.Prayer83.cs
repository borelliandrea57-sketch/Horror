using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using LaTerzaCampana.Core;
public partial class GameRoot
{
    private Screen prayerReturn83;
    private int prayerPhase83;
    private float prayerTime83,curseClock83=30f,curseFlash83;
    private string prayerResult83="";
    private AudioStreamPlayer? presence83,rite83;
    private readonly Dictionary<string,AudioStreamWav> omens83=new();
    private readonly Random omenRng83=new();
    private float presenceClock83=1f,streetClock83=4f,streetLife83;
    private string lastPresence83="";
    private Vector2 streetAt83;
    private int streetType83;
    private static readonly string[] presenceKeys83={"z74_cyst","z74_thin","z74_eye","z74_heavy","z74_hunch","z74_flayed","ossuary","stitch","bell","sexton","corrupted","levatrice","falciante","unknown"};
    private void InitOmens83(){
        presence83=new AudioStreamPlayer{VolumeDb=-28};AddChild(presence83);
        rite83=new AudioStreamPlayer{VolumeDb=-18};AddChild(rite83);
        foreach(string k in presenceKeys83.Concat(new[]{"rite","dust","paper","shutter"})){
            string path=System.IO.Path.Combine(OS.GetExecutablePath().GetBaseDir(),"v8_assets","omen83_"+k+".wav");
            string res="res://Assets/omen83_"+k+".wav";
            var a=System.IO.File.Exists(path)?AudioStreamWav.LoadFromFile(path):ResourceLoader.Exists(res)?GD.Load<AudioStreamWav>(res):null;
            if(a!=null)omens83[k]=a;
        }
    }
    private void OpenPrayer83(){if(screen is not (Screen.World or Screen.Pause or Screen.Inventory))return;prayerReturn83=screen;screen=Screen.Prayer;prayerPhase83=0;prayerTime83=0;prayerResult83="";QueueRedraw();}
    private string PrayerInvocation83()=>game.State.Hero switch{
        HeroId.Gladia=>"Braka, trattieni il fuoco che mi divora. Lascia che io ricordi questo istante.",
        HeroId.Anatema=>"Non chiedo nulla agli dei. Incido questo ricordo nella mia volontà.",
        HeroId.Ambrosh=>"Nuwdanìe, concedimi un momento di quiete. Che la mia volontà sopravviva alla tua benedizione.",
        _=>"Nyarlatoth, custodisci questo ricordo. Fa' che io sappia riconoscerlo quando tornerò."};
    private void PrayerKey83(int n,Key key,bool ok){
        if(prayerPhase83==0){if(n==2||key==Key.Escape){screen=prayerReturn83;return;}if(n==1||ok){prayerPhase83=1;prayerTime83=1.6f;PlayOmen83(rite83,"rite",-18);}}
        else if(prayerPhase83==2&&(ok||n==1||n==2||key==Key.Escape)){screen=prayerReturn83;}
    }
    private void UpdatePrayer83(float dt){if(prayerPhase83!=1)return;prayerTime83-=dt;if(prayerTime83>0)return;CommitPrayer83(SavePath);prayerPhase83=2;}
    private bool CommitPrayer83(string path){RememberEnemies86();string before=game.ToJson();try{
        prayerResult83=game.State.Hero==HeroId.Anatema?"Hai fissato il ricordo senza invocare alcun dio.":Prayer83.Apply(game.State);string full=ProjectSettings.GlobalizePath(path);string temp=full+".tmp";
        System.IO.File.WriteAllText(temp,game.ToJson());System.IO.File.Move(temp,full,true);
        prayerResult83+="\nLa partita è stata salvata.";curseClock83=Math.Min(curseClock83,18f);return true;
        }catch(Exception){game=GameRules.FromJson(before);enemyState86=game.State;ResetEventRules();prayerResult83="La preghiera non ha trovato risposta. Salvataggio fallito; nessun tributo è stato riscosso.";return false;}}
    private void DrawPrayer83(){
        Rect(0,0,W,H,new Color(.025f,.016f,.02f,.74f));Panel(200,116,880,510,.97f);
        string god=Prayer83.God(game.State.Hero);Center("IL PREZZO DEL RICORDO",640,172,30,Gold);
        Image("portrait_"+HeroKey(),new Rect2(235,203,170,188));Frame(235,203,170,188,Gold,1);
        Text(game.State.Hero==HeroId.Anatema?"MEMORIA SENZA DIO":"PREGHIERA A "+god.ToUpperInvariant(),435,238,23,Gold);
        string body=prayerPhase83==0?PrayerInvocation83():prayerPhase83==1?PrayerInvocation83():prayerResult83;
        Paragraph(body,435,279,600,21,31,Cream,6);
        if(prayerPhase83==0){Paragraph(game.State.Hero==HeroId.Anatema?"Conserva questo ricordo senza offrire tributi.":"Il dio ascolta. Ogni preghiera rafforza il legame; il tributo sarà più gravoso della volta precedente.",247,452,780,18,26,Gold,3);Center(game.State.Hero==HeroId.Anatema?"[1 / E] RICORDA E SALVA":"[1 / E] PREGA E SALVA",434,574,20,Gold);Center("[2 / ESC] RINUNCIA",842,574,20,Gold);}
        else if(prayerPhase83==1){float t=1-prayerTime83/1.6f;DrawArc(new Vector2(640,523),33,0,Mathf.Tau*Mathf.Clamp(t,0,1),48,new Color(.75f,.55f,.31f,.75f),2);Center("La voce risponde…",640,583,19,Gold);}
        else Center("E / INVIO — CONTINUA",640,575,20,Gold);
    }
    private void PlayOmen83(AudioStreamPlayer? player,string key,float db){if(player==null||!omens83.TryGetValue(key,out var stream))return;player.Stream=stream;player.VolumeDb=db;player.PitchScale=.96f+(float)omenRng83.NextDouble()*.08f;player.Play();}
    private string PresenceKey83(WorldEnemy e)=>e.Kind=="zombie"?"z74_"+Z74(e.Id).Key:presenceKeys83.Contains(e.Kind)?e.Kind:"unknown";
    private bool Exterior83()=>game.State.Map is "piazza" or "station" or "strada" or "strada_bassa" or "vicolo";
    private void ResetOmens83(){presence83?.Stop();presenceClock83=.7f;lastPresence83="";streetLife83=0;streetClock83=4f+(float)omenRng83.NextDouble()*5f;}
    private void UpdateOmens83(float dt){
        var candidates=CurrentMap().Enemies.Where(e=>enemyPositions.ContainsKey(e.Id)&&!IsDormantOlmoZombie(e.Id)).Select(e=>(e,d:visibleHero.DistanceTo(TileCenter(enemyPositions[e.Id].X,enemyPositions[e.Id].Y))/T)).Where(v=>v.d<=7.5f).OrderBy(v=>v.d).ToList();
        if(candidates.Count==0){presenceClock83=.4f;lastPresence83="";presence83?.Stop();}else{
            var target=candidates.FirstOrDefault(v=>v.e.Id!=lastPresence83);if(target.e==null)target=candidates[0];
            presenceClock83-=dt;if(presenceClock83<=0){lastPresence83=target.e.Id;presenceClock83=4.8f+(float)omenRng83.NextDouble()*4;PlayOmen83(presence83,PresenceKey83(target.e),-23-target.d*1.6f);}}
        if(Exterior83()){
            streetLife83=Math.Max(0,streetLife83-dt);streetClock83-=dt;
            if(streetClock83<=0){streetClock83=7f+(float)omenRng83.NextDouble()*13f;streetType83=omenRng83.Next(4);streetLife83=2.5f;var candidatesAt=new List<Vector2I>();for(int dx=-5;dx<=5;dx++)for(int dy=-4;dy<=4;dy++){int x=game.State.TileX+dx,y=game.State.TileY+dy;if(!Blocked(game.State.Map,x,y)&&Math.Abs(dx)+Math.Abs(dy)>2)candidatesAt.Add(new(x,y));}if(candidatesAt.Count>0){var p=candidatesAt[omenRng83.Next(candidatesAt.Count)];streetAt83=TileCenter(p.X,p.Y);PlayOmen83(rite83,streetType83==0?"dust":streetType83==1?"paper":streetType83==2?"shutter":"dust",-31);}else streetLife83=0;}}
        else streetLife83=0;
        curseFlash83=Math.Max(0,curseFlash83-dt);int n=game.State.Prayers83;if(n<=0)return;curseClock83-=dt;if(curseClock83>0)return;
        curseClock83=Math.Max(12f,65f-Math.Min(15,n)*4f);curseFlash83=3f;
        switch(game.State.Hero){case HeroId.Gladia:game.State.Body=Math.Max(1,game.State.Body-Math.Min(10,n));if(n>=4)game.State.Bleeding=true;Notify("Braka: Senti il fuoco nelle mani. Il debito non è estinto.",4);break;
        case HeroId.Ambrosh:game.State.Hunger=Math.Max(0,game.State.Hunger-Math.Min(12,n*2));Notify("Nuwdanìe: Le pulsioni tornano, più insistenti. Il tuo corpo reclama energia.",4);break;
        default:game.State.Mind=Math.Max(0,game.State.Mind-Math.Min(10,n*2));var at=new Vector2I(game.State.TileX,game.State.TileY);foreach(var d in new[]{new Vector2I(3,0),new Vector2I(-3,0),new Vector2I(0,-3),new Vector2I(0,3)})if(!Blocked(game.State.Map,at.X+d.X,at.Y+d.Y)){phantomTile64=at+d;phantom64=3.5f;break;}Notify("Nyarlatoth: Quel ricordo non è tuo. O forse lo è.",4);break;}
        PlaySound("mind_whisper",-10);
    }
    private void DrawCurse83(){if(curseFlash83<=0)return;float a=Mathf.Sin(Mathf.Clamp(curseFlash83/3f,0,1)*Mathf.Pi)*.13f;Rect(0,0,W,H,game.State.Hero switch{HeroId.Gladia=>new Color(.56f,.12f,.06f,a),HeroId.Ambrosh=>new Color(.43f,.19f,.12f,a),_=>new Color(.24f,.12f,.46f,a)});}
    private void DrawStreet83(){if(!Exterior83()||streetLife83<=0)return;float t=2.5f-streetLife83;Vector2 at=streetAt83-camera;float a=Mathf.Sin(Mathf.Clamp(t/2.5f,0,1)*Mathf.Pi);
        if(streetType83==0){for(int i=0;i<32;i++){float y=-112+(t*43+i*17)%112;float x=Mathf.Sin(i*17+t*1.7f)*(8+i%5*4);DrawCircle(at+new Vector2(x,y),1+i%3*.45f,new Color(.72f,.63f,.5f,a*.36f));}DrawEllipse(at.X,at.Y,23+t*8,6+t*2,new Color(.65f,.55f,.44f,a*.15f));}
        else if(streetType83==1){for(int i=0;i<4;i++){var p=at+new Vector2(t*45+i*12,Mathf.Sin(t*4+i)*9+i*5);DrawSetTransform(p,t*2+i);DrawRect(new Rect2(-4,-2,8,4),new Color(.59f,.49f,.34f,a*.8f));DrawSetTransform(Vector2.Zero);}}
        else if(streetType83==2){for(int i=0;i<5;i++){var p=at+new Vector2(Mathf.Sin(i*3)*25,-25+t*15+i*7);DrawCircle(p,2,new Color(.25f,.20f,.15f,a*.65f));}DrawEllipse(at.X,at.Y-18,30,20,new Color(.13f,.10f,.07f,a*.24f));}
        else{for(int i=0;i<3;i++){var p=at+new Vector2(t*75-70+i*22,-48-i*12);DrawEllipse(p.X,p.Y,13+Mathf.Sin(t*17+i)*4,4,new Color(.04f,.035f,.032f,a*.38f));}}
    }
    private void Check83(){int count=0;void C(bool ok,string why){if(!ok)throw new Exception(why);count++;}foreach(var h in new[]{HeroId.Gladia,HeroId.Ambrosh,HeroId.Roderick}){
        game.StartNew(h,Difficulty.Sopravvivenza);ResetEventRules();screen=Screen.World;EnterMap("piazza",10,10);OpenPrayer83();C(screen==Screen.Prayer&&game.State.Prayers83==0,"confirmation");PrayerKey83(2,Key.None,false);C(screen==Screen.World&&game.State.Prayers83==0,"cancel free");
        string path="user://test83.json";C(CommitPrayer83(path),"first save");var loaded=GameRules.FromJson(Godot.FileAccess.GetFileAsString(path));C(loaded.State.Prayers83==1,"persist first");int b=game.State.Body,m=game.State.Mind;double f=game.State.Hunger;C(CommitPrayer83(path),"second save");C(game.State.Prayers83==2,"cumulative");C(h==HeroId.Gladia?b-game.State.Body==8:h==HeroId.Ambrosh?f-game.State.Hunger==12:m-game.State.Mind==12,"different second cost");
        C(CommitPrayer83(path),"third save");C(game.State.Prayers83==3,"third count");if(h==HeroId.Gladia)C(game.State.Bleeding,"braka bleeding");System.IO.File.Delete(ProjectSettings.GlobalizePath(path));
        curseClock83=0;UpdateOmens83(.1f);C(curseFlash83>0,"recurring curse");}
        foreach(var key in presenceKeys83.Concat(new[]{"rite","dust","paper","shutter"}))C(omens83.ContainsKey(key)&&omens83[key].GetLength()>0,"audio "+key);
        foreach(var map in new[]{"piazza","strada","strada_bassa","vicolo"}){EnterMap(map,8,8);C(Exterior83(),"outdoors "+map);streetClock83=0;UpdateOmens83(.1f);C(streetLife83>0,"ambient event "+map);}
        GD.Print($"PRAYER83_OK {count}");GetTree().Quit();}
}
