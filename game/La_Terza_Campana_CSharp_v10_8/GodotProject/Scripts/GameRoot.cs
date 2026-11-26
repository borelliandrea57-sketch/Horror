using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using LaTerzaCampana.Core;

/// <summary>
/// Godot-native C# version. Artwork and map layout derive from La Terza Campana v1.3.
/// No browser, HTML, JavaScript, or embedded web runtime is used at game runtime.
/// </summary>
public partial class GameRoot : Node2D
{
    private enum Screen { Title, Hero, Difficulty, Origin, Fountain, Prologue, World, Dialogue, Inventory, Combat, Cutscene, Pause, Prayer, Dead, Ending }
    private const int W=1280, H=720, T=44;
    private const string SavePath="user://terza_campana_v81.json";
    private readonly Color Cream=Color.FromHtml("eee0c7"), Gold=Color.FromHtml("c8ab79"), Red=Color.FromHtml("b56763");
    private readonly Dictionary<string,Texture2D?> images=new();
    private Dictionary<string,WorldMap> maps=new();
    private GameRules game=new(HeroId.Gladia,Difficulty.Sopravvivenza);
    private HeroId hero=HeroId.Gladia;
    private Difficulty difficulty=Difficulty.Sopravvivenza;
    private Screen screen=Screen.Title, returnScreen=Screen.World;
    private Font font=ThemeDB.FallbackFont;
    private int proStep=0, invTab=0, dir=0, animFrame=0, selectedInventory=0;
    private float tick=0f, moveTimer=0f, enemyTimer=0f, messageTimer=0f, shakeTime=0f;
    private bool walking=false, debugHitboxes=false, editCollision=false;
    private string toast="", speaker="", speech="", speechPortrait="", deathReason="", ending="";
    private Vector2 visibleHero=Vector2.Zero, camera=Vector2.Zero;
    private readonly List<Choice> choices=new();
    private readonly Dictionary<string,Vector2I> enemyPositions=new();
    private readonly Dictionary<string,int> enemyFacing=new();
    private readonly HashSet<string> alertedEnemies=new();
    private readonly Dictionary<string,AudioStream> soundStreams=new();
    private AudioStreamPlayer? soundPlayer;
    private AudioStreamPlayer? monsterPlayer;
    private AudioStreamPlayer? sanityPlayer;
    private readonly Random rng=new();
    private Battle? battle;
    private bool previewMode=false;
    private float introTime=0f;
    private bool introChargeSoundPlayed=false;
    private float ossuaryAttackTime=0f;
    private const float IntroDuration=5.05f;
    private int previewFrames=0;
    private string previewFile="";
    private record Choice(string Title,Action Do);
    private record Battle(string Kind,string Id)
    {
        public int Turn=1;
        public bool Guard=false;
        public int Phase=0;
        public bool Armed;
        public string Log="La creatura ti osserva. Il tuo corpo ricorda già il dolore.";
        public Dictionary<string,int> Parts=new(){{"torace",90},{"testa",48},{"braccioS",32},{"braccioD",32},{"gambaS",21},{"gambaD",21}};
    }
    private static readonly string[] Parts={"torace","testa","braccioS","braccioD","gambaS","gambaD"};
    private static readonly string[] ItemKeys={"bende","erbe","infuso","pane","mela","conserva","chiave_stazione","chiave_bassa","lettera_scarlatta","grimaldello","chiave_cisterne"};
    private static readonly string[] SlotLabels={"ARMA","CORPO","TESTA","ACCESSORIO"};
    private static readonly GearSlot[] Slots={GearSlot.Arma,GearSlot.Corpo,GearSlot.Testa,GearSlot.Accessorio};
    private const string IntroHeader="IL CONVOGLIO DELLE 03:17";

    public override void _Ready()
    {
        font=ThemeDB.FallbackFont;
        soundPlayer=new AudioStreamPlayer{VolumeDb=-6};AddChild(soundPlayer);
        monsterPlayer=new AudioStreamPlayer{VolumeDb=-8};AddChild(monsterPlayer);
        sanityPlayer=new AudioStreamPlayer{VolumeDb=-17};AddChild(sanityPlayer);
        eventAudio=new AudioStreamPlayer{VolumeDb=-6};AddChild(eventAudio);
        foreach(string sound in new[]{"door_open","door_lock","door_hit","monster_cry","monster_move","monster_attack","mind_whisper","mind_moan","mind_scrape","cutscene_shriek","ritual_hum","event_shots","event_burst","event_zombies","event_brass","event_drag","event_heartbeat","event_drone","event_bell","cat_buzz63","swallow78","bird_drop78"})
        {
            string path=$"res://Assets/sfx_{sound}.wav";
            if(ResourceLoader.Exists(path))
            {
                var stream=GD.Load<AudioStream>(path);
                if(stream!=null)soundStreams[sound]=stream;
            }
            else
            {
                string external8=System.IO.Path.Combine(OS.GetExecutablePath().GetBaseDir(),"v8_assets","sfx_"+sound+".wav");
                if(System.IO.File.Exists(external8)){var stream=AudioStreamWav.LoadFromFile(external8);if(stream!=null)soundStreams[sound]=stream;}
                else if(sound=="cat_buzz63"){
                    string external=System.IO.Path.Combine(OS.GetExecutablePath().GetBaseDir(),"v5_assets","sfx_cat_buzz63.wav");
                    if(System.IO.File.Exists(external)){var stream=AudioStreamWav.LoadFromFile(external);if(stream!=null)soundStreams[sound]=stream;}
                }
            }
        }
        string desktopIcon=System.IO.Path.Combine(OS.GetExecutablePath().GetBaseDir(),"v5_assets","icon.png");
        if(System.IO.File.Exists(desktopIcon))
        {
            var iconImage=Godot.Image.LoadFromFile(desktopIcon);
            if(iconImage!=null&&!iconImage.IsEmpty())DisplayServer.WindowSetIcon(iconImage);
        }
        // External map corrections make the existing, tested Windows Mono pack
        // updatable without rebuilding the engine itself.
        string worldJson=Godot.FileAccess.GetFileAsString("res://Data/maps.json");
        string worldOverride=System.IO.Path.Combine(OS.GetExecutablePath().GetBaseDir(),"v4_assets","maps_v4.json");
        if(System.IO.File.Exists(worldOverride))worldJson=System.IO.File.ReadAllText(worldOverride);
        maps=WorldCatalog.Read(worldJson);
        InstallPentesileaNpc();InstallCat63();InstallUndead74();Install84();InitAudio79();InitOmens83();
        ResetEventRules();
        foreach(var id in new[]{"title","prologue_train","prologue_wounded","prologue_eclipse","portrait_gladia","portrait_ambrosh","portrait_roderick"}) Art(id);
        previewMode=OS.GetCmdlineUserArgs().Any(arg=>arg.StartsWith("--ltc-preview="));
        GD.Print($"La Terza Campana v10.1 C#: caricate {maps.Count} mappe, gioco e dialoghi nativi.");
        if(OS.GetCmdlineUserArgs().Contains("--ltc-emile103-check")){try{CheckEmile103();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-anatema101-check")){try{CheckAnatema101();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-stats100-check")){try{CheckStats100();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-key99-check")){try{Check99();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-clinic96-check")){try{CheckClinic96();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-hole93-check")){try{CheckHole93();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-return91-check")){try{Check91();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-refuge89-check")){try{CheckRefuge89();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-gait87-check")){try{Check87();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-persist86-check")){try{Check86();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-room85-check")){try{CheckRoom85();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-84-check")){try{Check84();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-prayer83-check")){try{Check83();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-update82-check")){try{Check82();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-map81-check")){try{CheckMap81();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-audio79-check")){try{CheckAudio79();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-arrival80-check")){try{CheckArrival80();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-corridor77-check")){try{CheckCorridor77();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-nyar76-check")){try{CheckNyar76();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-combat95-check")){try{CheckCombat95();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-battle75-check")){try{CheckBattle75();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-undead74-check")){try{CheckUndead74();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-statue73-check")){try{CheckStatue73();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-fix70-check")){try{CheckFix70();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-narrative-check")){try{CheckNarrative69();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-choices-check")){try{CheckChoices67();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-flash-check")){try{CheckFlash66();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-roderick-check")){try{CheckRoderick64();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-ambrosh-check")){try{CheckAmbrosh63();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-detail-check")){try{CheckDetail61();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-origin-check")){try{CheckOrigin60();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-survival-check")){try{CheckSurvival59();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        if(OS.GetCmdlineUserArgs().Contains("--ltc-ai-check")){try{CheckEnemy58();}catch(Exception ex){GD.PushError(ex.ToString());GetTree().Quit(1);}return;}
        foreach(string arg in OS.GetCmdlineUserArgs())
        {
            if(!arg.StartsWith("--ltc-preview="))continue;
            string target=arg.Substring("--ltc-preview=".Length);
            game.StartNew(HeroId.Gladia,Difficulty.Sopravvivenza);ResetEventRules();
            hero=HeroId.Gladia;difficulty=Difficulty.Sopravvivenza;
            if(target=="clinic96"){screen=Screen.World;EnterMap("casa_medico",17,10);enemyTimer=100f;}
            else if(target.StartsWith("fatal98")||target=="fatal97"){screen=Screen.World;EnterMap("casa_medico",17,10);ClinicStep96(16,13);clinicClock96=target.EndsWith("crawl")?4.05f:target.EndsWith("fall")?2.85f:1.57f;clinicImpact97=true;game.State.Body=0;}
            else if(target=="patient96"){screen=Screen.World;EnterMap("casa_medico",17,10);InspectPatient96();clinicClock96=2f;}
            else if(target=="return91"){screen=Screen.World;EnterMap("strada",17,4);game.State.StoryFlags.Add("street_from_station");ReturnFromStreet91();messageTimer=0;}
            else if(target=="arrival80"){screen=Screen.World;EnterMap("station",10,14);BeginArrival80();AdvanceArrival80();AdvanceArrival80();}
            else if(target=="station"){screen=Screen.World;EnterMap("station",10,14);}
            else if(target=="refuge89"){screen=Screen.World;EnterMap("rifugio_basso",10,5);}
            else if(target=="hole93"){screen=Screen.World;EnterMap("casa_olmo",10,4);InspectHole93("Cavità insolita");holeClock93=2f;}
            else if(target=="letter89"){screen=Screen.World;EnterMap("rifugio_basso",3,10);InteractProp(CurrentMap().Props.First(p=>p.Id=="decor87_rifugio_basso_2"));choices[0].Do();}
            else if(target=="piazza"){screen=Screen.World;EnterMap("piazza",8,27);}
            else if(target=="stats100"){screen=Screen.Inventory;EnterMap("station",10,14);invTab=4;}
            else if(target=="inventory"){screen=Screen.Inventory;EnterMap("station",10,14);invTab=0;}
            else if(target=="skills"){screen=Screen.Inventory;EnterMap("station",10,14);invTab=2;}
            else if(target=="equipment"){screen=Screen.Inventory;EnterMap("station",10,14);invTab=1;}
            else if(target=="sala"){screen=Screen.World;EnterMap("sala",13,15);}
            else if(target=="sala_hitbox"){screen=Screen.World;EnterMap("sala",13,10);debugHitboxes=true;}
            else if(target=="sala_ingresso"){screen=Screen.World;EnterMap("sala",13,15);}
            else if(target=="station_barrel"){screen=Screen.World;EnterMap("station",11,17);}
            else if(target=="station_occlusion"){screen=Screen.World;EnterMap("station",11,17);game.State.TileX=11;game.State.TileY=16;visibleHero=TileCenter(11,16);}
            else if(target=="sala_scale"){screen=Screen.World;EnterMap("sala",23,6);dir=3;Interact();}
            else if(target=="station_door"){screen=Screen.World;EnterMap("station",17,9);dir=3;Interact();}
            else if(target=="prologue"){screen=Screen.Prologue;proStep=1;}
            else if(target=="prologue_first"){screen=Screen.Prologue;proStep=0;}
            else if(target=="prologue_wounded"){screen=Screen.Prologue;proStep=3;}
            else if(target=="prologue_eclipse"){screen=Screen.Prologue;proStep=4;}
            else if(target=="difficulty"){screen=Screen.Difficulty;}
            else if(target=="difficulty_extreme"){screen=Screen.Difficulty;difficulty=Difficulty.DeSade;}
            else if(target=="piazza_layers"){screen=Screen.World;EnterMap("piazza",20,23);}
            else if(target=="binari"){screen=Screen.World;EnterMap("binari",18,6);}
            else if(target=="binari_monster"){screen=Screen.World;EnterMap("binari",26,19);}
            else if(target=="station_intro") {screen=Screen.World;EnterMap("sala",13,15);StartStationIntro();introTime=2.4f;}
            else if(target=="station_return") {screen=Screen.World;EnterMap("sala",13,15);game.State.StoryFlags.Add("station_levatrice_seen");}
            else if(target=="corrupted_npc") {screen=Screen.World;EnterMap("strada",20,13);}
            else if(target=="corrupted_dialogue") {screen=Screen.World;EnterMap("strada",20,13);InteractNpc(CurrentMap().Npcs.First(n=>n.Id=="corrupted_wanderer"));}
            else if(target=="sanity_50") {screen=Screen.World;EnterMap("strada",20,13);game.State.Mind=45;}
            else if(target=="sanity_30") {screen=Screen.World;EnterMap("strada",20,13);game.State.Mind=25;}
            else if(target=="sanity_10") {screen=Screen.World;EnterMap("strada",20,13);game.State.Mind=8;}
            else if(target=="corrupted_battle") {screen=Screen.World;EnterMap("strada",20,13);game.State.StoryFlags.Add("corrupted_hostile");RebuildEnemies();StartBattle(new WorldEnemy{Id="corrupted_wanderer",Kind="corrupted",X=21,Y=14});}
            else if(target=="post_escape") {screen=Screen.World;game.State.StoryFlags.Add("station_levatrice_seen");EnterMap("station",17,10);Say("Gladia","MA CHE DIAVOLO ERA QUELLO?! Che co-cosa sta succedendo... me lo sono immaginato?!","portrait_gladia");}
            else if(target=="cat63"||target=="cat64"){screen=Screen.World;EnterMap("strada",13,20);messageTimer=0;}
            else if(target=="street57") {screen=Screen.World;EnterMap("strada",17,18);}
            else if(target=="house57") {screen=Screen.World;EnterMap("casa_olmo",14,16);}
            else if(target=="pentesilea") {screen=Screen.World;EnterMap("casa_olmo",9,10);for(int i=0;i<12;i++)eventRules!.OnTileChanged();StartPentesileaCutscene();eventTime=11f;}
            else if(target=="pentesilea58") {screen=Screen.World;EnterMap("casa_olmo",14,16);for(int i=0;i<12;i++)eventRules!.OnTileChanged();StartPentesileaCutscene();eventTime=11f;}
            else if(target=="zombie58") {screen=Screen.World;EnterMap("casa_olmo",23,9);game.State.StoryFlags.Add("pentesilea_met");game.State.DefeatedEnemies.Add("casa_olmo:olmo_undead_1");game.State.DefeatedEnemies.Add("casa_olmo:olmo_undead_2");RebuildEnemies();enemyTimer=100f;}
            else if(target=="stillness") {screen=Screen.World;EnterMap("casa_olmo",9,10);StartStillnessDeath();eventTime=2f;}
            else if(target=="waiting94"){screen=Screen.World;EnterMap("strada",17,18);StartStillnessDeath();eventTime=1.85f;}
            else if(target=="waiting82"){screen=Screen.World;EnterMap("station",10,14);StartStillnessDeath();eventTime=1.25f;}
            else if(target=="prayer83"){screen=Screen.World;EnterMap("piazza",10,12);OpenPrayer83();}
            else if(target=="rear82"){game.StartNew(HeroId.Roderick,Difficulty.Sopravvivenza);StartBattle(new WorldEnemy{Id="olmo_undead_6",Kind="zombie",X=14,Y=10});battle!.Phase=1;battle.Parts["braccioS"]=0;target75=0;}
            else if(target.StartsWith("combat95")){game.StartNew(HeroId.Ambrosh,Difficulty.Sopravvivenza);StartBattle(new WorldEnemy{Id="preview_heavy4",Kind="zombie",X=14,Y=10});if(target.EndsWith("hero")){heroAttack75=HeroDuration95*.43f;heroLanded95=true;target75=0;}else{enemyDuration95=EnemyProfile95(BattleArt75()).Duration;enemyAttack75=enemyDuration95*.73f;enemyResolved95=false;}}
            else if(target=="battle75"){StartBattle(new WorldEnemy{Id="preview_ossuary",Kind="ossuary",X=29,Y=20});battle!.Phase=1;target75=2;battle.Parts["gambaD"]=0;}
            else if(target=="ossuary_battle"){screen=Screen.Combat;StartBattle(new WorldEnemy{Id="preview_ossuary",Kind="ossuary",X=29,Y=20});}

            else if(target=="combat59"){screen=Screen.World;EnterMap("casa_olmo",14,16);StartBattle(new WorldEnemy{Id="olmo_undead_4",Kind="zombie",X=5,Y=5});battle!.Phase=1;}
            else if(target=="pentesilea59"){screen=Screen.World;EnterMap("casa_olmo",14,16);for(int i=0;i<12;i++)eventRules!.OnTileChanged();StartPentesileaCutscene();eventTime=4f;}
            else if(target=="street87"){screen=Screen.World;EnterMap("strada_bassa",15,15);enemyTimer=100f;}
            else if(target=="olmo87"){screen=Screen.World;EnterMap("casa_olmo",8,15);enemyTimer=100f;}
            else if(target=="archive87"){screen=Screen.World;EnterMap("archivio_basso",12,8);enemyTimer=100f;}
            else if(target=="house59"){screen=Screen.World;EnterMap("casa_olmo",14,16);}
            else if(target=="battle"){screen=Screen.Combat;StartBattle(new WorldEnemy{Id="preview",Kind="stitch",X=13,Y=12});}
            else if(target.StartsWith("statue73")){screen=Screen.World;EnterMap("station",10,14);InteractProp(CurrentMap().Props.Find(p=>p.Id=="station_fountain")!);fountainClock61=target.EndsWith("top")?7.1f:target.EndsWith("mid")?3.2f:.55f;}
            else if(target.StartsWith("fontana")){screen=Screen.World;EnterMap("piazza",20,23);if(target.EndsWith("low"))game.State.Mind=15;OpenFountain61();fountainClock61=2f;}
            else if(target.StartsWith("emile103")){hero=HeroId.Anatema;BeginNew60();LoadEmile103(Math.Clamp(int.Parse(target.Substring(8)),0,5));EmilePhase103(1);emileBeat103=emileScene103==4?5:emileScene103==5?4:1;flashClock66=2;}
            else if(target.StartsWith("narrative69")){hero=target.EndsWith("amb")?HeroId.Ambrosh:target.EndsWith("rod")?HeroId.Roderick:HeroId.Gladia;BeginNew60();originPos=OriginStand60;originVisual=OriginCenter60(originPos);originMode=2;OriginChoose67(0);if(target.EndsWith("reaction")){originClock=.45f;}else{AdvanceNarrative69();AdvanceNarrative69();}}
            else if(target.StartsWith("roderick")){hero=HeroId.Roderick;BeginNew60();LoadOrigin60(target.EndsWith("2")?1:target.EndsWith("3")?2:0);originMode=2;originPos=OriginStand60;originVisual=OriginCenter60(originPos);originClock=2f;}
            else if(target.StartsWith("ambrosh")){hero=HeroId.Ambrosh;BeginNew60();LoadOrigin60(target.EndsWith("2")?1:target.EndsWith("3")?2:0);originMode=2;originPos=OriginStand60;originVisual=OriginCenter60(originPos);originClock=2f;}
            else if(target.StartsWith("origin")){BeginNew60();int ch=target.EndsWith("2")?1:target.EndsWith("3")?2:0;LoadOrigin60(ch);originMode=2;originPos=OriginStand60;originVisual=OriginCenter60(originPos);originClock=2f;}
            else if(target=="hero")screen=Screen.Hero;
            previewFile="/mnt/data/LTC_v3_preview_"+target+".png";previewFrames=62;
        }
        QueueRedraw();
    }
    private void PlaySound(string id,float volumeDb=0)
    {
        if(!soundStreams.TryGetValue(id,out var stream))return;
        var player=id.StartsWith("event_",StringComparison.Ordinal)?eventAudio:id.StartsWith("mind_",StringComparison.Ordinal)?sanityPlayer: id.StartsWith("monster_",StringComparison.Ordinal)?monsterPlayer:soundPlayer;
        if(player==null)return;
        player.Stop();player.Stream=stream;player.VolumeDb=(id.StartsWith("mind_",StringComparison.Ordinal)?-17:id.StartsWith("monster_",StringComparison.Ordinal)?-8:-6)+volumeDb;
        player.Play();
    }
    private Texture2D? Art(string id)
    {
        if(string.IsNullOrEmpty(id)) return null;
        if(images.TryGetValue(id,out var texture)) return texture;
        string v5dir=System.IO.Path.Combine(OS.GetExecutablePath().GetBaseDir(),"v5_assets");
        foreach(string v5ext in new[]{"png","jpg"})
        {
            string filename=System.IO.Path.Combine(v5dir,id+"."+v5ext);
            if(!System.IO.File.Exists(filename))continue;
            var source=Godot.Image.LoadFromFile(filename);
            if(source!=null&&!source.IsEmpty())
            {
                texture=ImageTexture.CreateFromImage(source);
                images[id]=texture;
                return texture;
            }
        }
        string v8dir=System.IO.Path.Combine(OS.GetExecutablePath().GetBaseDir(),"v8_assets");
        foreach(string ext in new[]{"png","jpg"})
        {
            string path=$"res://Assets/{id}.{ext}";
            if(ResourceLoader.Exists(path)) {texture=GD.Load<Texture2D>(path);break;}
            string external8=System.IO.Path.Combine(v8dir,id+"."+ext);
            if(System.IO.File.Exists(external8))
            {
                var img=Godot.Image.LoadFromFile(external8);
                if(img!=null&&!img.IsEmpty()){texture=ImageTexture.CreateFromImage(img);break;}
            }
        }
        // External foreground art can be loaded as native ImageTexture, even if
        // its PNG was not imported into the previous version's original PCK.
        if(texture==null&&id.StartsWith("foreground_",StringComparison.Ordinal))
        {
            string external=System.IO.Path.Combine(OS.GetExecutablePath().GetBaseDir(),"v4_assets",id+".png");
            if(System.IO.File.Exists(external))
            {
                var img=Godot.Image.LoadFromFile(external);
                if(img!=null&&!img.IsEmpty())texture=ImageTexture.CreateFromImage(img);
            }
        }
        images[id]=texture;
        return texture;
    }
    private void Image(string id,Rect2 dst,float alpha=1f)
    {
        var tex=Art(id);if(tex==null) return;
        DrawTextureRect(tex,dst,false,new Color(1,1,1,alpha));
    }
    private void Rect(float x,float y,float w,float h,Color c) => DrawRect(new Rect2(x,y,w,h),c);
    private void Frame(float x,float y,float w,float h,Color? col=null,float thickness=1.6f) => DrawRect(new Rect2(x,y,w,h),col??Gold,false,thickness);
    private void Panel(float x,float y,float w,float h,float alpha=.94f)
    {
        Rect(x,y,w,h,new Color(.075f,.066f,.063f,alpha));
        Frame(x,y,w,h,new Color(.52f,.43f,.33f,.8f),2);
        Frame(x+5,y+5,w-10,h-10,new Color(.3f,.24f,.2f,.75f),1);
    }
    private void Text(string text,float x,float y,int size=22,Color? color=null,int max=0)
    {
        DrawString(font,new Vector2(x,y),text,HorizontalAlignment.Left,max>0?max:-1,size,color??Cream);
    }
    private void Center(string text,float x,float y,int size=22,Color? color=null)
    {
        float width=font.GetStringSize(text,HorizontalAlignment.Left,-1,size).X;
        Text(text,x-width/2,y,size,color);
    }
    private void Paragraph(string text,float x,float y,float width,int size=19,int space=31,Color? color=null,int limit=8)
    {
        float lineY=y;var words=(text??"").Replace("\n"," \n ").Split(' ',StringSplitOptions.RemoveEmptyEntries);
        string line="";int lines=0;
        foreach(var word in words)
        {
            if(word=="\n") {Text(line,x,lineY,size,color);line="";lineY+=space;if(++lines>=limit)return;continue;}
            string test=line.Length==0?word:line+" "+word;
            if(font.GetStringSize(test,HorizontalAlignment.Left,-1,size).X>width&&line.Length>0)
            {Text(line,x,lineY,size,color);lineY+=space;line=word;if(++lines>=limit)return;}
            else line=test;
        }
        if(line.Length>0&&lines<limit)Text(line,x,lineY,size,color);
    }
    private void Bar(string title,float x,float y,float value,Color tint)
    {
        Text(title,x,y+15,18,Gold);Rect(x+78,y+3,132,13,Color.FromHtml("2c2928"));
        Rect(x+78,y+3,132*Mathf.Clamp(value/100f,0,1),13,tint);
        Text($"{Math.Ceiling(value)}",x+220,y+15,17);
    }
    private string HeroKey()=>game.State.Hero.ToString().ToLowerInvariant();
    private string InjuryKey()
    {
        var s=game.State;int mask=(s.Injuries.Contains("braccioS")?1:0)|
            (s.Injuries.Contains("braccioD")?2:0)|(s.Injuries.Contains("gambaS")?4:0)|
            (s.Injuries.Contains("gambaD")?8:0);
        return HeroKey()+"_"+mask.ToString("D2");
    }
    private void Sprite(string key,Vector2 worldPos,int row,int frame,int cellW=44,int cellH=90,float width=50,float height=88)
    {
        var tex=Art(key);if(tex==null)return;
        var size=tex.GetSize();if(size.X<cellW*8||size.Y<cellH*4)return;
        float sx=worldPos.X-camera.X, sy=worldPos.Y-camera.Y;
        if(sx< -110||sx>W+110||sy< -110||sy>H+160)return;
        DrawEllipse(sx,sy+15,width*.23f,5f,new Color(.04f,.03f,.025f,.34f));
        DrawTextureRectRegion(tex,new Rect2(sx-width/2,sy-height+15,width,height),
            new Rect2(frame*cellW,row*cellH,cellW,cellH));
    }
    private void DrawEllipse(float x,float y,float rx,float ry,Color col)
    {
        var points=new Vector2[24];for(int i=0;i<points.Length;i++){float a=i*Mathf.Tau/points.Length;points[i]=new Vector2(x+Mathf.Cos(a)*rx,y+Mathf.Sin(a)*ry);}
        DrawColoredPolygon(points,col);
    }
    public override void _Draw()
    {
        Rect(0,0,W,H,Color.FromHtml("100d0c"));
        switch(screen)
        {
            case Screen.Title: DrawTitleV3();break;
            case Screen.Hero: DrawHeroV3();break;
            case Screen.Difficulty: DrawDifficultyV3();break;
            case Screen.Origin: DrawOrigin60();break;
            case Screen.Fountain: DrawFountain61();break;
            case Screen.Prologue: DrawPrologue();break;
            case Screen.World: DrawWorld();break;
            case Screen.Dialogue: if(clinicPortrait96){DrawClinicPortrait96();DrawDialogue();if(clinicClock96<.65f)Rect(0,0,W,H,new Color(0,0,0,1-clinicClock96/.65f));}else if(clinicAttack96){DrawClinicAttack96();}else{if(holeScene93)DrawHole93();else DrawWorld();DrawDialogue();if(holeScene93&&holeClock93<.85f)Rect(0,0,W,H,new Color(0,0,0,1-holeClock93/.85f));}break;
            case Screen.Inventory: DrawInventoryV3();break;
            case Screen.Combat: DrawCombat();break;
            case Screen.Cutscene: if(eventScene!=ExtraScene.None)DrawExtraScene();else DrawStationIntro();break;
            case Screen.Pause: DrawWorld();DrawPause();break;
            case Screen.Prayer: DrawWorld();DrawPrayer83();break;
            case Screen.Dead:DrawEnd(false);break;
            case Screen.Ending:DrawEnd(true);break;
        }
        if(screen is Screen.Origin or Screen.Prologue)DrawSkip82();
        if(messageTimer>0&&screen!=Screen.Origin&&screen!=Screen.Dialogue&&screen!=Screen.Combat&&screen!=Screen.Prologue&&screen!=Screen.Cutscene&&screen!=Screen.Dead&&screen!=Screen.Prayer)
        {Panel(222,583,836,63,.95f);Center(toast,640,623,19);}
    }
    private void DrawTitle()
    {
        Image("title",new Rect2(0,0,W,H));
        Rect(0,0,W,H,new Color(.035f,.028f,.024f,.25f));
        Panel(780,112,444,514,.92f);
        Center("LA TERZA CAMPANA",1000,194,35,Gold);
        Center("UN RACCONTO DI FAME E CENERE",1000,229,15);
        Text("NUOVA PARTITA      [INVIO]",817,329,23);
        Text("CONTINUA                   [L]",817,393,23);
        Text("ESCI                           [ESC]",817,457,22);
        Paragraph("Godot C# · mappe e sistemi ripresi dalla versione precedente",815,560,350,16,24,Gold);
    }
    private void DrawHero()
    {
        Center("SCEGLI CHI SOPRAVVIVERÀ",640,83,37,Gold);
        foreach(var id in Enum.GetValues<HeroId>())
        {
            int n=(int)id;float x=72+n*395;
            Panel(x,126,356,467,.9f);
            Image("portrait_"+id.ToString().ToLowerInvariant(),new Rect2(x+14,144,328,302));
            if(hero==id)Frame(x,126,356,467,Gold,4);
            Center(Catalog.Heroes[id].Name,x+178,492,27);
            Center($"Corpo {Catalog.Heroes[id].StartingBody} · Mente {Catalog.Heroes[id].StartingMind}",x+178,532,18);
            Paragraph(Catalog.Heroes[id].Description,x+21,564,320,16,22,Gold,2);
        }
        Center("A / D o frecce cambia · INVIO conferma · ESC indietro",640,668,20);
    }
    private void DrawDifficulty()
    {
        Center("QUANTO DESIDERI SOFFRIRE?",640,91,37,Gold);
        foreach(var d in Enum.GetValues<Difficulty>())
        {
            float y=147+(int)d*158;Panel(105,y,1070,138);
            if(d==difficulty)Frame(105,y,1070,138,Gold,3);
            var m=Catalog.Difficulties[d];var col=d==Difficulty.DeSade?Color.FromHtml("b73435"):Cream;
            Text(d switch {Difficulty.Facile=>"FACILE",Difficulty.DeSade=>"DE SADE — EXTREMA",_=>"SOPRAVVIVENZA"},146,y+45,32,col);
            Text($"Consumo Fame ×{m.HungerRate:0.00}   ·   Attacco nemici ×{m.EnemyAttack:0.00}   ·   Difesa nemici ×{m.EnemyDefense:0.00}",147,y+94,19,Gold);
            if(d==Difficulty.DeSade) for(int i=0;i<10;i++) Rect(935+i*18,y+8,7,10+i%3*11,Color.FromHtml("782020"));
        }
        Center("W/S seleziona · INVIO inizia il prologo · ESC indietro",640,685,20);
    }
    private (string heading,string story,string art,string portrait,string speaker,bool decision) PrologueInfo()
    {
        return proStep switch
        {
            0=>("L'ULTIMA FERMATA","Il convoglio delle 03:17 attraversa la pianura senza fermarsi. Oltre i finestrini il paesaggio si ripete, sempre uguale. Un uomo ha iniziato a contare i battiti delle ruote.","prologue_train","","",false),
            1=>("IL CONTROLLO","Il Controllore ti tende la mano. «Il biglietto, prego. Siamo arrivati a un posto che non compare sull'orario.» Al posto degli occhi, due bottoni scuri sono cuciti nelle orbite vuote. La sua uniforme odora di acqua stagnante.","prologue_controller_close","portrait_controller","Il Controllore",true),
            2=>("QUALCUNO BUSSA","Dalla carrozza accanto proviene un colpo secco. Poi un altro, come dita contro il vetro. Nel corridoio una luce rossa si accende senza elettricità.","prologue_wounded","","",false),
            3=>("IL PASSEGGERO","«Fatemi uscire, vi prego!» Una mano insanguinata appare dietro il vetro della porta. La serratura è scheggiata; aprirla potrebbe ferirti.","prologue_wounded","portrait_passenger","Un Passeggero",true),
            4=>("L'ECLISSI","Fuori il sole scompare dietro un disco nero. Persone in piedi tra le rotaie guardano il treno passare, immobili. Non riesci a vedere le loro facce.","prologue_eclipse","","",false),
            5=>("LA VOCE","Una donna invisibile sussurra il tuo nome. «Ti ricordi la terza campana? Quando la sentirai, sarà troppo tardi per ricordare.»","prologue_eclipse","portrait_voice","La Voce",true),
            6=>("STAZIONE DI DÙSTERBEDRAGNA","I freni gemono. Il treno è arrivato, ma nessuno ha annunciato la fermata. Dal portone della stazione filtra una luce gialla, simile a una ferita aperta.","prologue_train","","",false),
            _=>("UN PASSO FUORI DAL TRENO","Il ricordo si interrompe prima che tu riesca a raggiungere la porta. Quando riapri gli occhi, senti il freddo del selciato sotto i piedi.","prologue_train","","",false)
        };
    }
    private void DrawPrologue()
    {
        var p=PrologueInfo();
        // The portrait and cinematic view use the same illustration.
        Image(p.art,new Rect2(0,0,W,proStep==1?560:H));
        Rect(0,0,W,84,new Color(.015f,.012f,.012f,.40f));
        Image("prologue_panel_v5",new Rect2(0,370,W,350));
        Inscribe(IntroHeader,74,427,19,InkGold);
        SootRule(72,443,1134);
        Inscribe(p.heading,76,490,37,Parchment);
        float paragraphX=82,paragraphWidth=1100;
        if(p.portrait.Length>0)
        {
            Image(p.portrait,new Rect2(78,506,140,136));
            ForgedCorner(78,506,1,1);ForgedCorner(218,642,-1,-1);
            paragraphX=246;paragraphWidth=927;
        }
        InscribedParagraph(p.story,paragraphX,536,paragraphWidth,22,29,4,Parchment);
        if(p.decision)
        {
            string a=proStep==1?"[1] MOSTRA IL BIGLIETTO":proStep==3?"[1] APRI LA PORTA":"[1] RISPONDI ALLA VOCE";
            string b=proStep==1?"[2] NASCONDI IL BIGLIETTO":proStep==3?"[2] VAI OLTRE":"[2] RESTA IN SILENZIO";
            Inscribe(a,249,655,20,InkGold);Inscribe(b,690,655,20,InkGold);
            Inscribe("OGNI SCELTA LASCIA UNA CICATRICE",831,687,15,Clot);
        }
        else Inscribe("INVIO / E  —  CONTINUA",940,685,20,InkGold);
    }
    private WorldMap CurrentMap()=>maps.GetValueOrDefault(game.State.Map)??maps["station"];
    private Vector2 TileCenter(int x,int y)=>new((x+.5f)*T,(y+.5f)*T);
    private static bool Contains(double[] r,float x,float y)
        => r.Length>=4&&x>=r[0]&&x<r[0]+r[2]&&y>=r[1]&&y<r[1]+r[3];
    private static bool PolygonContains(List<double[]> polygon,float x,float y)
    {
        bool inside=false;
        for(int i=0,j=polygon.Count-1;i<polygon.Count;j=i++)
        {
            var a=polygon[i];var b=polygon[j];if(a.Length<2||b.Length<2)continue;
            if(((a[1]>y)!=(b[1]>y))&&(x<(b[0]-a[0])*(y-a[1])/(b[1]-a[1])+a[0]))inside=!inside;
        }
        return inside;
    }
    private bool Blocked(string mapId,int x,int y)
    {
        if(!maps.TryGetValue(mapId,out var m))return true;
        string id=$"{mapId}:{x}:{y}";
        if(game.State.CollisionEdits.TryGetValue(id,out bool collision))return collision;
        if(x<1||y<1||x>=m.W-1||y>=m.H-1)return true;
        float fx=x+.5f,fy=y+.5f;
        if(m.Blocks.Any(r=>Contains(r,fx,fy)))return true;
        if(m.SceneryBlocks.Any(r=>Contains(r,fx,fy)))return true;
        if(m.Polygons.Any(poly=>PolygonContains(poly,fx,fy)))return true;
        foreach(var p in m.Props)
        {
            double[]? r=p.Footprint;
            if(r==null&&p.Solid?.Length>=4)r=new[]{p.X+p.Solid[0],p.Y+p.Solid[1],p.Solid[2],p.Solid[3]};
            if(r!=null&&Contains(r,fx,fy))return true;
        }
        return false;
    }
    private void RebuildEnemies()
    {
        RememberEnemies86();
        enemyPositions.Clear();enemyFacing.Clear();alertedEnemies.Clear();enemyBrains.Clear();recentStepNoise=0;
        enemyState86=game.State;enemyMap86=game.State.Map;
        foreach(var e in CurrentMap().Enemies)
            if((e.Kind!="corrupted"||game.State.StoryFlags.Contains("corrupted_hostile"))
                &&!game.State.DefeatedEnemies.Contains(game.State.Map+":"+e.Id))
            {
                var pos=new Vector2I(e.X,e.Y);int facing=0;
                if(game.State.EnemyLocations86.TryGetValue(game.State.Map+":"+e.Id,out var old86)){var remembered=new Vector2I(old86.X,old86.Y);if(!Blocked(game.State.Map,remembered.X,remembered.Y)&&!enemyPositions.Values.Contains(remembered)){pos=remembered;facing=Math.Clamp(old86.Facing,0,3);}}
                if(Blocked(game.State.Map,pos.X,pos.Y)||enemyPositions.Values.Contains(pos)){var free=NearbyFree86(pos,false);if(free.HasValue)pos=free.Value;else continue;}
                enemyPositions[e.Id]=pos;
                enemyFacing[e.Id]=facing;enemyBrains[e.Id]=new EnemyBrain(pos,TileCenter(pos.X,pos.Y));
            }
    }
    private void EnterMap(string id,int x,int y)
    {
        if(!maps.ContainsKey(id)) {Notify("Questa stanza non è stata ancora ricostruita.");return;}
        game.State.Map=id;MigrateClinic96();MigrateStreetCollisions();Migrate59();Migrate81();Migrate90();Migrate91();ResetStreetPatrol();game.State.TileX=Math.Clamp(x,2,maps[id].W-3);game.State.TileY=Math.Clamp(y,2,maps[id].H-3);
        if(Blocked(id,game.State.TileX,game.State.TileY))
        {
            bool found=false;
            for(int r=1;r<11&&!found;r++)for(int a=-r;a<=r&&!found;a++)for(int b=-r;b<=r;b++)
            {
                int px=game.State.TileX+a,py=game.State.TileY+b;
                if(!Blocked(id,px,py)){game.State.TileX=px;game.State.TileY=py;found=true;break;}
            }
        }
        visibleHero=TileCenter(game.State.TileX,game.State.TileY);dir=0;RebuildEnemies();
        if(enemyPositions.Values.Contains(new Vector2I(game.State.TileX,game.State.TileY))&&NearbyFree86(new Vector2I(game.State.TileX,game.State.TileY),true) is Vector2I free86){game.State.TileX=free86.X;game.State.TileY=free86.Y;visibleHero=TileCenter(free86.X,free86.Y);}
        eventRules?.EnterMap(id);eventAudio?.Stop();ResetStreetOmens78();ResetOmens83();if(id=="casa_medico"&&ClinicRoom96(game.State.TileX,game.State.TileY))game.State.StoryFlags.Add("clinic96_entered");
        if(id=="piazza"&&game.State.StoryFlags.Add("piazza_intro"))Notify("Il selciato è coperto di segni simili a impronte... dall'interno.");
        else if(id=="strada")Notify("VIA DEL DISTRETTO BASSO — Molte porte sono chiuse, alcune si possono sfondare. Il vicolo a sud sembra scendere ancora.");
        else if(id=="strada_bassa")Notify("SOTTOSTRADA — L’acqua ristagna nelle cisterne. Dietro le imposte chiuse non si sente nulla.");
        else if(id=="binari")Notify("Tre binari, ma nessuno porta verso casa.");
        if(id=="sala"&&!previewMode&&!game.State.StoryFlags.Contains("station_levatrice_seen"))StartStationIntro();
        QueueRedraw();
    }
    private void Notify(string text,float duration=4.1f)
    {
        toast=text;messageTimer=duration;QueueRedraw();
    }
    private void Say(string name,string body,string portrait="",params Choice[] options)
    {
        holeScene93=false;clinicPortrait96=false;clinicAttack96=false;speaker=name;speech=body;speechPortrait=portrait;
        choices.Clear();choices.AddRange(options);
        returnScreen=screen==Screen.Combat?Screen.Combat:Screen.World;
        screen=Screen.Dialogue;QueueRedraw();
    }
    private void CloseDialogue() {bool fatal=clinicAttack96&&game.State.Body<=0;holeScene93=false;clinicPortrait96=false;clinicAttack96=false;screen=returnScreen;choices.Clear();if(fatal)Die("L’appendice ti trascina verso la lettiga. L’ultima cosa che senti è un fruscio di petali.");QueueRedraw();}
    private string PortraitOf(WorldNpc npc)
    {
        if(npc.Portrait.Length>0)return npc.Portrait;
        return npc.Id switch {"stationmaster"=>"portrait_stationmaster","oldtraveller"=>"portrait_oldtraveller","keeper"=>"portrait_keeper_new","woman"=>"portrait_ada","survivor"=>"portrait_neri","corrupted_wanderer"=>"portrait_corrupted","priest"=>"portrait_priest",_=>""};
    }
    private string NpcAtlas(WorldNpc npc)=>npc.Id switch
    {
        "stationmaster"=>"anim_stationmaster","oldtraveller"=>"anim_keeper","keeper"=>"anim_keeper",
        "woman"=>"anim_ada_00","survivor"=>"anim_neri","corrupted_wanderer"=>"anim_corrupted","priest"=>"anim_priest",_=>"anim_keeper"
    };
    private string EnemyAtlas(string kind)=>"anim_"+kind;
    private static float DistanceToRect(float x,float y,double[] r)
    {
        float dx=Math.Max(Math.Max((float)r[0]-x,0),x-(float)(r[0]+r[2]));
        float dy=Math.Max(Math.Max((float)r[1]-y,0),y-(float)(r[1]+r[3]));
        return Mathf.Sqrt(dx*dx+dy*dy);
    }
    private record Target(string Type,string Id,string Name,object Entity,float Score);
    private Target? ClosestTarget()
    {
        float x=game.State.TileX+.5f,y=game.State.TileY+.5f;
        Vector2 forward=dir switch {0=>Vector2.Down,1=>Vector2.Left,2=>Vector2.Right,_=>Vector2.Up};
        float tx=x+forward.X*1.1f,ty=y+forward.Y*1.1f;
        var options=new List<Target>();
        foreach(var npc in CurrentMap().Npcs)
        {
            if(HideCorruptedNpc(npc.Id)||HidePentesileaNpc(npc.Id))continue;
            var r=new double[]{npc.X,npc.Y,1,1};
            float n=DistanceToRect(tx,ty,r)+.09f,actual=DistanceToRect(x,y,r);
            if(actual<=2.35f)options.Add(new("npc",npc.Id,npc.Name,npc,n));
        }
        foreach(var d in CurrentMap().Doors)
        {
            if(HidePentesileaDoor(d.Id))continue;
            var r=d.InteractArea??new double[]{d.X-.3,d.Y-.3,1.6,1.6};
            float n=DistanceToRect(tx,ty,r)+.22f,actual=DistanceToRect(x,y,r);
            if(actual<=2.05f)options.Add(new("door",d.Id,d.Label,d,n));
        }
        foreach(var p in CurrentMap().Props)
        {
            var r=p.InteractArea??new[]{p.X-.45,p.Y-.4,1.9,1.9};
            float n=DistanceToRect(tx,ty,r)+.18f,actual=DistanceToRect(x,y,r);
            if(actual<=1.85f)options.Add(new("prop",p.Id,p.Name,p,n));
        }
        AddCorpseTargets(options,x,y,tx,ty);
        return options.OrderBy(a=>a.Score).FirstOrDefault(a=>a.Score<2.15f);
    }
    private void DrawWorld()
    {
        var m=CurrentMap();var state=game.State;
        Vector2 mapSize=new(m.W*T,m.H*T);
        camera=new Vector2(
            mapSize.X<W?-(W-mapSize.X)/2f:Mathf.Clamp(visibleHero.X-W/2f,0,mapSize.X-W),
            mapSize.Y<H?-(H-mapSize.Y)/2f:Mathf.Clamp(visibleHero.Y-H/2f,0,mapSize.Y-H));
        Image(state.Map=="casa_medico"&&Art("map96_casa_medico")!=null?"map96_casa_medico":state.Map=="station"&&Art("map99_station")!=null?"map99_station":state.Map=="strada_bassa"&&Art("map90_strada_bassa")!=null?"map90_strada_bassa":state.Map=="rifugio_basso"&&Art("map89_rifugio_basso")!=null?"map89_rifugio_basso":state.Map=="casa_olmo"&&Art("map88_casa_olmo")!=null?"map88_casa_olmo":Art("map87_"+state.Map)!=null?"map87_"+state.Map:"map_"+state.Map,new Rect2(-camera,mapSize));
        if(state.Map=="casa_olmo"&&!HidePentesileaDoor("olmo_service"))
        {Vector2 at=TileCenter(14,1)-camera;Rect(at.X-19,at.Y-38,38,59,new Color(.12f,.075f,.055f));Frame(at.X-19,at.Y-38,38,59,Gold,1);}
        if(state.Map=="casa_olmo_servizio")
        {Vector2 at=TileCenter(10,13)-camera;Rect(at.X-21,at.Y-20,42,46,new Color(.12f,.075f,.055f));Frame(at.X-21,at.Y-20,42,46,Gold,1);}
        if(state.Map=="casa_medico")DrawClinicMap96();DrawRitualDecals(state.Map);DrawCat63(state.Map);
        if(state.Map is "casa_olmo" or "casa_olmo_servizio")Image("decay_"+state.Map,new Rect2(-camera,mapSize));
        if(Art("map_"+state.Map)==null) DrawFallbackMap(m);
        DrawOlmo75(state.Map);DrawFallingDust(state.Map);DrawOpenLocks87();DrawTraps84();
        foreach(var d in m.Doors)
        {
            if(d.Hp<=0)continue;
            bool broken=state.DoorDamage.GetValueOrDefault(state.Map+":"+d.Id)>=d.Hp;
            float x=d.X*T-camera.X+T/2f,y=d.Y*T-camera.Y+T/2f;
            if(broken) DrawRect(new Rect2(x-15,y-23,30,44),new Color(.08f,.06f,.05f,.72f));
            else if(state.DoorDamage.GetValueOrDefault(state.Map+":"+d.Id)>0)DrawLine(new Vector2(x-10,y-17),new Vector2(x+12,y+18),Red,3);
        }
        // Each character, standalone prop and painted foreground cut-out is rendered at
        // its actual floor-contact Y coordinate.  A barrel IN FRONT of the heroine now
        // covers her body; when she walks in front, it stays behind her.
        var sprites=new List<(float depth,Action draw)>();
        foreach(var p in m.Props)
        {
            if(p.PaintOnly)continue;
            string key=p.ArtFile.Length>0?p.ArtFile:p.Art.Length>0?"env_"+p.Art:"";
            var tex=Art(key);if(tex==null)continue;
            float w=tex.GetWidth()*(float)Math.Min(1.5,p.Scale),h=tex.GetHeight()*(float)Math.Min(1.5,p.Scale);
            float x=(float)(p.X+.5)*T-camera.X-w/2, y=(float)(p.Y+1)*T-camera.Y-h+9;
            if(x>W||y>H||x+w<0||y+h<0)continue;
            sprites.Add(((float)p.Y+1f,()=>{if(state.Map=="rifugio_basso"&&p.Solid!=null)DrawEllipse(x+w/2,y+h-4,Math.Min(w*.42f,28),7,new Color(.025f,.019f,.014f,.44f));DrawTextureRect(tex,new Rect2(x,y,w,h),false);}));
        }
        AddPaintedForeground(sprites,state.Map);
        if(state.Map=="sala"&&state.StoryFlags.Contains("station_levatrice_seen"))
        {
            var corpse=Art("corpse_railworker");
            if(corpse!=null)
            {
                Vector2 at=TileCenter(13,8);
                sprites.Add((8.30f,()=>DrawTextureRect(corpse,new Rect2(at.X-camera.X-77,at.Y-camera.Y-16,154,56),false)));
            }
            if(screen==Screen.Cutscene)
            {
                var monster=Art("anim_levatrice");
                if(monster!=null)
                {
                    float advance=introTime<=2.1f?0f:Mathf.Clamp((introTime-2.1f)/1.75f,0,1)*4.9f;
                    float yTile=8.2f+advance;
                    int row=introTime<2.1f?0:1;
                    int index=(int)(tick*(row==0?5f:13f))%8;
                    Vector2 at=new(13.5f*T,(yTile+.5f)*T);
                    sprites.Add((yTile+.4f,()=>DrawTextureRectRegion(monster,new Rect2(at.X-camera.X-68,at.Y-camera.Y-101,136,133),
                        new Rect2(index*128,row*132,128,132))));
                }
            }
        }
        foreach(var npc in m.Npcs)
        {
            if(npc.Id=="woman"&&state.CompanionJoined)continue;
            if(HideCorruptedNpc(npc.Id)||HidePentesileaNpc(npc.Id))continue;
            int x=npc.X,y=npc.Y;string atlas=NpcAtlas(npc);
            if(npc.Id=="pentesilea")
                sprites.Add((y+.5f,()=>DrawPentesilea(TileCenter(x,y),0,0)));
            else if(npc.Id=="corrupted_wanderer")
                sprites.Add((streetNpcVisual.Y/T,()=>Sprite(atlas,streetNpcVisual,streetNpcFacing,streetNpcMoving?(int)(tick*8)%8:0,80,94,65,89)));
            else if(NpcModel61(npc.Id) is string model) sprites.Add((y+.5f,()=>DrawWorldNpc61(model,TileCenter(x,y))));
            else sprites.Add((y+.5f,()=>Sprite(atlas,TileCenter(x,y),0,(int)(tick*1.7f)%8)));
        }
        foreach(var e in m.Enemies)
        {
            if(!enemyPositions.TryGetValue(e.Id,out var pos))continue;
            string atlas=EnemyAtlas59(e);int ex=pos.X,ey=pos.Y;
            int facing=enemyFacing.GetValueOrDefault(e.Id,0);
            float width=e.Kind=="ossuary"?86:e.Kind=="corrupted"?64:e.Kind=="zombie"?88:75,height=e.Kind=="ossuary"?100:e.Kind=="corrupted"?90:89;
            var at=enemyBrains.TryGetValue(e.Id,out var b)?b.Visual:TileCenter(ex,ey);
            bool moving=at.DistanceTo(TileCenter(ex,ey))>1.5f;
            bool chasing=b!=null&&b.Memory>0;
            sprites.Add((at.Y/T,()=>DrawEnemyGait87(e,at,facing,moving,chasing,width,height)));
        }
        if(state.CompanionJoined)
        {
            Vector2 companion=visibleHero+new Vector2(-T*.82f,T*.23f);
            sprites.Add((state.TileY+.7f,()=>DrawNpc61("ada",companion,dir,(int)(tick*6)%8,68,100,true,walking)));
        }
        Vector2 heroInScene=visibleHero;
        int heroFacing=dir,heroFrame=clinicAttack96&&clinicImpact97?2:walking?((int)(tick*12)%8):0;
        if(screen==Screen.Cutscene&&eventScene==ExtraScene.None&&introTime>3.45f)
        {
            float retreat=Mathf.Clamp((introTime-3.45f)/1.45f,0,1);
            heroInScene+=new Vector2(0,retreat*T*2.1f);
            heroFacing=0;heroFrame=(int)(introTime*17)%8;
        }
        AddPentesileaSceneSprites(sprites);
        AddCorpses59(sprites);AddFamiliar64(sprites);
        if(eventScene!=ExtraScene.Stillness)
            sprites.Add((state.TileY+.5f+(heroInScene.Y-visibleHero.Y)/T,()=>Sprite("anim_"+InjuryKey(),heroInScene,heroFacing,heroFrame)));
        else if(eventTime<3.5f)
            sprites.Add((state.TileY+.5f,()=>Sprite("anim_"+InjuryKey(),heroInScene,heroFacing,0,44,90,Mathf.Max(3,50-eventTime*13),88+eventTime*48)));
        foreach(var it in sprites.OrderBy(it=>it.depth))it.draw();
        DrawStreetOmens78();DrawStreet83();DrawDarkness59();DrawVision64();DrawCurse83();
        if(debugHitboxes&&screen!=Screen.Cutscene)DrawHitboxes(m);
        if(screen!=Screen.Cutscene&&!clinicAttack96)DrawWorldHudV3(m);
        DrawMindEffects();
        if(screen==Screen.World)DrawStillnessWarning();
    }
    private void DrawFallbackMap(WorldMap m)
    {
        // Supplemental fallback for the chapel and underground gallery while full art is being ported.
        for(int x=0;x<m.W;x++)for(int y=0;y<m.H;y++)
        {
            bool obstacle=Blocked(game.State.Map,x,y);
            Color tint=((x*7+y*13)%6==0)?Color.FromHtml("625951"):Color.FromHtml("534b46");
            if(obstacle)tint=Color.FromHtml("302b29");
            Rect(x*T-camera.X,y*T-camera.Y,T,T,tint);
            Frame(x*T-camera.X+1,y*T-camera.Y+1,T-2,T-2,new Color(.12f,.11f,.1f,.28f));
        }
    }
    private void DrawHitboxes(WorldMap m)
    {
        int minX=Math.Max(0,(int)(camera.X/T)),maxX=Math.Min(m.W-1,(int)((camera.X+W)/T)+1);
        int minY=Math.Max(0,(int)(camera.Y/T)),maxY=Math.Min(m.H-1,(int)((camera.Y+H)/T)+1);
        for(int y=minY;y<=maxY;y++)for(int x=minX;x<=maxX;x++)
        {
            float sx=x*T-camera.X,sy=y*T-camera.Y;
            if(Blocked(game.State.Map,x,y))
            {
                if(editCollision)Rect(sx,sy,T,T,new Color(.66f,.12f,.10f,.12f));
                Frame(sx+1,sy+1,T-2,T-2,new Color(.95f,.27f,.18f,.63f),1.1f);
            }
            else if(editCollision)Frame(sx+1,sy+1,T-2,T-2,new Color(.78f,.74f,.56f,.16f),.7f);
        }
    }
    private void DrawDialogue()
    {
        Panel(24,489,1232,217,.98f);
        if(speechPortrait=="portrait_pentesilea")Image(speechPortrait,new Rect2(52,504,138,184));
        else if(speechPortrait.Length>0)Image(speechPortrait,new Rect2(38,504,172,184));
        float left=speechPortrait.Length>0?233:59;
        Text(speaker,left,533,25,Gold);
        FormattedParagraph(speech,left,565,1180-left,20,29,3);
        if(choices.Count>0)
        {
            if(choices.Count==4)
                for(int i=0;i<4;i++)Text($"[{i+1}] {choices[i].Title}",left+(i%2)*475,660+(i/2)*24,16,Gold);
            else for(int i=0;i<Math.Min(3,choices.Count);i++) Text($"[{i+1}] {choices[i].Title}",left+i*315,685,16,Gold);
        }
        else Text("E / INVIO — CONTINUA",1015,688,16,Gold);
    }
    private void DrawInventory()
    {
        var s=game.State;Panel(39,29,1202,660,.98f);
        string[] tabs={"OGGETTI","EQUIPAGGIAMENTO","ABILITÀ","DIARIO"};
        for(int i=0;i<4;i++)
        {
            float x=60+i*292;Rect(x,42,278,51,i==invTab?Color.FromHtml("584138"):Color.FromHtml("282221"));
            Frame(x,42,278,51,i==invTab?Gold:Color.FromHtml("76685a"));
            Center(tabs[i],x+139,75,20);
        }
        Image("portrait_"+HeroKey(),new Rect2(66,119,245,248));
        var doll=Art("doll_"+InjuryKey());
        if(doll!=null)DrawTextureRect(doll,new Rect2(74,378,113,280),false);
        Text(Catalog.Heroes[s.Hero].Name,193,412,28,Gold);
        Text($"CORPO  {s.Body}",193,454,19);Text($"MENTE  {s.Mind}",193,487,19);
        Text($"FAME  {(int)s.Hunger}",193,520,19);
        if(s.Bleeding)Text("SANGUINAMENTO",190,561,17,Red);
        if(s.Injuries.Count>0)Text($"ARTI PERSI: {s.LostLimbs84.Count} · FERITI: {s.Injuries.Count-s.LostLimbs84.Count}",190,593,15,Red);
        var eq=game.GetEquipmentStats();
        Text($"ATTACCO +{eq.Attack}    DIFESA +{eq.Defense}",193,624,16,Gold);
        if(invTab==0)
        {
            Text("PROVVISTE E OGGETTI",393,149,27,Gold);
            for(int i=0;i<ItemKeys.Length;i++)
            {
                string id=ItemKeys[i];float x=392+(i%2)*408,y=176+(i/2)*105;
                Panel(x,y,398,98,.84f);
                string name=Catalog.Items.GetValueOrDefault(id,id);
                Text($"[{i+1}]  {name}",x+16,y+36,19);
                Text($"{s.Inventory.GetValueOrDefault(id,0)}x",x+344,y+36,19,Gold);
                Text(id switch {"bende"=>"Ferma il sangue e cura.","erbe"=>"Riduce l'infezione.","infuso"=>"Ristora la Mente.","pane" or "mela" or "conserva"=>"Sazia la Fame.",_=>"Oggetto della storia."},x+18,y+72,16,Gold);
            }
            Text("1–6 usa l'oggetto selezionato",393,646,18,Gold);
        }
        else if(invTab==1)
        {
            Text("EQUIPAGGIAMENTO",402,147,29,Gold);
            for(int i=0;i<4;i++)
            {
                var slot=Slots[i];string id=s.Equipped.GetValueOrDefault(slot,"");
                string name=Catalog.Equipment.GetValueOrDefault(id)?.DisplayName??"Nessuno";
                float y=174+i*110;Panel(393,y,810,100,.86f);
                Text($"[{i+1}]   {SlotLabels[i]}",412,y+33,21,Gold);
                Text(name,621,y+33,22);
                string buff=Catalog.Equipment.TryGetValue(id,out var gear)?$"ATK +{gear.Attack} · DEF +{gear.Defense} · MENTE +{gear.MindProtection}":"Slot vuoto";
                Text(buff,623,y+71,17);
            }
            Text("1–4 cambia oggetto · equipaggiamento trovato nelle case",395,651,17,Gold);
        }
        else if(invTab==2)
        {
            Text(Catalog.Heroes[s.Hero].Name.ToUpperInvariant()+" — ABILITÀ",397,145,27,Gold);
            int i=0;foreach(var skill in game.GetSkills())
            {
                float y=178+i*111;Panel(389,y,813,102,.87f);
                Text($"[{i+1}] {skill}",408,y+37,22);
                Text(i==0?"PARLA: compagni, NPC e nemici":i==1?"Capacità di supporto":i==2?"Abilità fisica": "Magia o concentrazione",408,y+74,18,Gold);i++;
            }
            Text("In battaglia puoi usare le abilità e scegliere il bersaglio.",400,660,17);
        }
        else
        {
            Text("DIARIO DI DÙSTERBEDRAGNA",391,152,27,Gold);
            var entries=new List<string>{"Il convoglio delle 03:17 non compare su alcun registro.",
                "Protagonista: "+Catalog.Heroes[s.Hero].Name+" · difficoltà: "+s.Difficulty,
                s.StoryFlags.Contains("ticket_shown")?"Hai mostrato il biglietto al Controllore.":"Hai nascosto il biglietto.",
                s.StoryFlags.Contains("passenger_helped")?"Hai aiutato il passeggero ferito. Forse lo ritroverai.":"Hai ignorato il passeggero, e la sua voce ti segue.",
                s.StoryFlags.Contains("voice_answered")?"Hai risposto alla voce durante l'eclissi.":"Non hai risposto alla voce della notte.",
                game.HasItem("chiave_stazione")?"La chiave della Stazione Centrale è in tuo possesso.":"Il portone della stazione è chiuso. Non sai come sei arrivato nella piazza.",
                s.CompanionJoined?"Ada ora ti accompagna.":"Una donna di nome Ada si aggira nella piazza.",
                s.DefeatedEnemies.Contains("galleria:bell1")?"Hai fermato il Cuore della Campana.":"La terza campana continua a suonare da sotto la città."};
            for(int i=0;i<entries.Count;i++)Paragraph("• "+entries[i],398,211+i*53,778,18,24,Cream,2);
        }
        Text("TAB cambia scheda  ·  I/ESC chiude  ·  F5 prega e salva",84,672,18,Gold);
    }
    private void DrawCombat()
    {
        if(battle==null)return;
        Image("battle_ossuary_bg",new Rect2(0,0,W,510),.86f);
        Rect(0,0,W,510,new Color(.05f,.04f,.04f,.14f));
        DrawBattleHero59();DrawBattleFamiliar64();DrawEnemy75();DrawCombatEffects95();
        DrawBattleAnatomy59();
        Panel(12,500,1256,206,.96f);
        string title=battle.Kind switch {"falciante"=>"IL FALCIANTE DELL’OLMO","stitch"=>"IL CUCITO","sexton"=>"IL SAGRESTANO","bell"=>"IL CUORE DELLA CAMPANA","ossuary"=>"L'OSSARIO ERRANTE","corrupted"=>"IL VIANDANTE CORROTTO","zombie"=>Z74(battle.Id).Name,_=>"CREATURA"};
        if(battle.Kind=="ossuary")
        {
            Rect(16,10,365,41,new Color(.05f,.03f,.03f,.85f));
            Text(title,32,40,28,Gold);
        }
        else Center(title,640,38,31,Gold);
        var s=game.State;
        Bar("CORPO",408,523,s.Body,Color.FromHtml("aa5954"));
        Bar("MENTE",741,523,s.Mind,Color.FromHtml("768b9b"));
        Text("TURNO "+battle.Turn,1127,541,18,Gold);
        Frame(376,505,1,195,Color.FromHtml("6e6052"));
        if(battle.Phase==0)
        {
            string[] cmds={"[1] ATTACCA","[2] ABILITÀ","[3] DIFENDI","[4] OGGETTI",$"[5] FUGGI ({FleeChance100():P0})"};
            for(int i=0;i<cmds.Length;i++)Text(cmds[i],43,546+i*32,20);
        }
        else if(battle.Phase==1)
        {
            Text("SCEGLI UNA PARTE DEL CORPO",42,527,17,Gold);
            for(int i=0;i<Parts.Length;i++){var part=Parts[i];if(i==target75)Rect(23,531+i*25,345,24,new Color(.48f,.32f,.14f,.48f));Text($"[{i+1}] {PartName(part)}  {(battle.Parts[part]==0?"FUORI USO":battle.Parts[part]+" PV")}",38,548+i*25,16,battle.Parts[part]==0?Red:i==target75?Gold:Cream);}
        }
        else if(battle.Phase==2)
        {
            Text("ABILITÀ DEL PERSONAGGIO",38,529,18,Gold);
            int i=0;foreach(var name in game.GetSkills())Text($"[{++i}] {name}",38,559+(i-1)*35,18);
        }
        else if(battle.Phase==3)
        {
            Text("TENTA DI COMUNICARE",37,528,19,Gold);
            Text("[1] Chiedi chi sei",37,571,18);Text("[2] Implora",37,609,18);Text("[3] Cerca di calmarlo",37,649,18);
        }
        else if(battle.Phase==4)
        {
            Text("USA UN OGGETTO",38,528,19,Gold);
            for(int i=0;i<6;i++)Text($"[{i+1}] {Catalog.Items[ItemKeys[i]]}  {s.Inventory.GetValueOrDefault(ItemKeys[i],0)}x",36,554+i*26,16);
        }
        Paragraph(battle.Log.Replace("\n"," "),407,575,807,18,25,Cream,4);
        Text(battle.Phase==1?"↑ ↓ seleziona · E / INVIO attacca · ESC indietro":"ESC torna alle azioni · Elimina testa o torace per vincere",407,685,15,Gold);
    }
    private string PartName(string key)=>key switch {"torace"=>"Torace","testa"=>"Testa","braccioS"=>"Braccio sinistro","braccioD"=>"Braccio destro","gambe"=>"Gambe","gambaS"=>"Gamba sinistra","gambaD"=>"Gamba destra",_=>key};
    private void DrawPause()
    {
        Rect(0,0,W,H,new Color(0,0,0,.67f));Panel(345,142,590,448);
        Center("PAUSA",640,214,49,Gold);
        Center("[1] Riprendi",640,302,24);Center("[2] Prega e salva",640,379,24);Center("[3] Torna al menu",640,457,24);
        Center("ESC per riprendere",640,537,17,Gold);
    }
    private void DrawEnd(bool good)
    {
        Image("prologue_eclipse",new Rect2(0,0,W,H),.48f);Rect(0,0,W,H,new Color(0,0,0,.75f));
        if(!good&&stillnessDeath){Center("SEI RIMASTO FERMO ABBASTANZA",640,205,35,Gold);Center("PERCHÉ LUI TI TROVASSE.",640,252,35,Gold);Center("Il mondo non dimentica chi smette di attraversarlo.",640,360,22);Center("INVIO — RITORNA AL MENU",640,611,22,Gold);return;}
        Center(good?(ending=="luce"?"LA CITTÀ RICORDA IL SOLE":"LA CITTÀ DIMENTICA IL SUONO"):"IL SILENZIO TI HA TROVATO",640,236,44,Gold);
        Paragraph(good?"Il sigillo si spezza. La campana tace, ma la città porterà a lungo i segni della tua scelta.":deathReason,243,307,815,25,43,Cream,4);
        Center("INVIO — RITORNA AL MENU",640,611,22,Gold);
    }
    // Persistent one-shot encounter. No respawn, even after leaving and returning.
    private void StartStationIntro()
    {
        if(!game.State.StoryFlags.Add("station_levatrice_seen"))return;
        game.State.Mind=Math.Max(0,game.State.Mind-17);
        introTime=0f;introChargeSoundPlayed=false;
        screen=Screen.Cutscene;
        PlaySound("cutscene_shriek",-7);
    }
    private void FinishStationIntro()
    {
        introTime=0f;
        screen=Screen.World;
        EnterMap("station",17,10); // The protagonist stumbles back through the central doors.
        string line=game.State.Hero switch
        {
            HeroId.Gladia=>"MA CHE DIAVOLO ERA QUELLO?! Che co-cosa sta succedendo... me lo sono immaginato?!",
            HeroId.Ambrosh=>"No... impossibile. Quella cosa era sul cadavere. Ho visto tutto. Perché il mio cuore non smette di tremare?",
            _=>"Che razza di abominio...?! Non ero preparato a questo. Non tornerò là dentro senza pensarci."
        };
        Say(Catalog.Heroes[game.State.Hero].Name,line,"portrait_"+HeroKey());
    }
    private void DrawStationIntro()
    {
        DrawWorld();
        float t=introTime;
        // The creature turns, strikes the floor and rushes toward the doorway.
        if(t>=2.15f&&t<4.1f)
        {
            float strength=Mathf.Clamp((t-2.15f)/1.1f,0,1);
            Rect(0,0,W,H,new Color(.15f,.015f,.02f,strength*.10f));
            if(t>3.1f)
            {
                for(int j=0;j<4;j++)DrawLine(new Vector2(410+j*60,125),new Vector2(355+j*75,530),new Color(.86f,.57f,.47f,.22f),3);
            }
        }
        Rect(0,0,W,72,new Color(.025f,.018f,.018f,.86f));
        Rect(0,615,W,105,new Color(.025f,.018f,.018f,.91f));
        string scene=t<1.35f?"Qualcosa è chino sul corpo di un ferroviere. Ti arriva un rumore di ossa sotto il ferro.":
           t<2.15f?"Le teste nella gabbia si voltano verso di te, una dopo l'altra.":
           t<3.8f?"LA LEVATRICE DEL FERRO TI HA VISTO.  CORRI!":
           "Indietreggi, poi ti getti verso il portone. Alle tue spalle le zampe battono sul marmo.";
        Text("STAZIONE CENTRALE · PRIMO INCONTRO",35,43,20,Gold);
        Paragraph(scene,37,658,1180,23,31,Cream,2);
    }
    private void DrawFallingDust(string map)
    {
        if(map!="sala"&&map!="station"&&map!="binari")return;
        // Falling wall plaster/dust starts at the arch supports, not randomly
        // across the floor: all particles remain independent of sprite depth.
        int[] columns=map=="sala"?new[]{2,5,10,15,22,24}:
                     map=="station"?new[]{5,9,12,21,26,30}:new[]{2,8,15,23,31};
        float top=map=="sala"?3.1f:map=="station"?5.2f:2.0f;
        float fall=map=="sala"?7.0f:5.2f;
        for(int i=0;i<36;i++)
        {
            float xTile=columns[i%columns.Length]+.16f*(float)Math.Sin(tick*.5f+i*2.4f);
            float yTile=top+(i*.77f+tick*(.23f+i%4*.07f))%fall;
            float x=xTile*T-camera.X,y=yTile*T-camera.Y;
            if(x<0||x>W||y<0||y>H)continue;
            float opacity=(.12f+(i%6)*.024f);
            DrawLine(new Vector2(x,y),new Vector2(x-1,y+3+i%4),new Color(.93f,.78f,.59f,opacity),1.2f);
        }
    }
    private bool TrySouthStreetExit(Vector2I movement)
    {
        if(movement!=Vector2I.Down)return false;
        var s=game.State;
        // The roofs of the station courtyard block the old south row. The exit is
        // triggered *before* collision when the player reaches the open corridor.
        if(s.Map=="station"&&s.TileY>=21&&s.TileX>=15&&s.TileX<=18)
        {s.StoryFlags.Add("street_from_station");EnterMap("strada",17,4);return true;}
        // Also preserve the original HTML game's city exit below the town square.
        if(s.Map=="piazza"&&s.TileY>=29&&s.TileX>=7&&s.TileX<=39)
        {s.StoryFlags.Remove("street_from_station");EnterMap("strada",17,4);return true;}
        if(s.Map=="strada"&&s.TileY>=21&&s.TileX>=14&&s.TileX<=20)
        {EnterMap("strada_bassa",15,4);return true;}
        return false;
    }
    private void FormattedParagraph(string text,float x,float y,float width,int size,int lineHeight,int limit)
    {
        // **x1 Nome oggetto** appears in visibly heavier letters, rather than
        // printing Markdown asterisks as literal characters.
        bool bold=false;float cx=x,cy=y;int lines=0;
        foreach(string segment in (text??"").Split("**",StringSplitOptions.None))
        {
            foreach(string word in segment.Split(' ',StringSplitOptions.RemoveEmptyEntries))
            {
                float advance=font.GetStringSize(word+" ",HorizontalAlignment.Left,-1,size).X;
                if(cx+advance>x+width&&cx>x){cx=x;cy+=lineHeight;if(++lines>=limit)return;}
                if(bold)
                {
                    Color ink=Color.FromHtml("f4ddb4");
                    DrawString(font,new Vector2(cx+1.3f,cy),word,HorizontalAlignment.Left,-1,size,ink);
                    DrawString(font,new Vector2(cx-.8f,cy),word,HorizontalAlignment.Left,-1,size,ink);
                    DrawString(font,new Vector2(cx,cy),word,HorizontalAlignment.Left,-1,size,ink);
                }
                else Text(word,cx,cy,size,Cream);
                cx+=advance;
            }
            bold=!bold;
        }
    }
    public override void _Process(double delta)
    {
        float dt=(float)Math.Min(delta,.2);tick+=dt;UpdateAudio79(dt);if(screen==Screen.Dialogue&&holeScene93)holeClock93+=dt;if(screen==Screen.Dialogue&&clinicPortrait96)clinicClock96+=dt;UpdateClinicFatal97(dt);
        if(eventAudio!=null)eventAudio.StreamPaused=!DisplayServer.WindowIsFocused()||screen==Screen.Inventory||screen==Screen.Pause||editCollision;
        if(previewFrames>0&&--previewFrames==0)
        {
            var screenshot=GetViewport().GetTexture().GetImage();
            if(screenshot!=null){screenshot.SavePng(previewFile);GD.Print("PREVIEW_SAVED: "+previewFile);}
            GetTree().Quit();return;
        }
        if(messageTimer>0)messageTimer-=dt;
        if(screen==Screen.Combat&&!(previewMode&&previewFile.Contains("combat95")))UpdateBattle75(dt);
        if(screen!=Screen.World)presence83?.Stop();
        if(screen==Screen.Prayer){UpdatePrayer83(dt);QueueRedraw();return;}
        if(screen==Screen.Origin){UpdateOrigin60(dt);QueueRedraw();return;}
        if(screen==Screen.Fountain){fountainClock61+=dt;QueueRedraw();return;}
        if(ossuaryAttackTime>0)ossuaryAttackTime=Math.Max(0f,ossuaryAttackTime-dt);
        if(screen==Screen.Cutscene&&eventScene!=ExtraScene.None)
        {
            if(DisplayServer.WindowIsFocused())UpdateExtraScene(dt);
        }
        else if(screen==Screen.Cutscene)
        {
            introTime+=dt;
            if(introTime>2.10f&&!introChargeSoundPlayed)
            {introChargeSoundPlayed=true;PlaySound("monster_cry",1);}
            if(introTime>=IntroDuration)FinishStationIntro();
        }
        if(screen==Screen.World&&DisplayServer.WindowIsFocused()&&!editCollision)
        {
            if(eventRules?.IntroPending==true){StartPentesileaCutscene();QueueRedraw();return;}
            movedThisFrame=false;
            Walk(dt);
            if(screen!=Screen.World){QueueRedraw();return;}
            if(!movedThisFrame)AdvanceStillness(delta);
            if(screen!=Screen.World){QueueRedraw();return;}
            UpdateStreetPatrol(dt);
            UpdateEnemyVisuals(dt);
            UpdateMindEffects(dt);UpdateBraka60(dt);UpdateNuwdanie63(dt);UpdateNyarlatoth64(dt);UpdateStreetOmens78(dt);UpdateOmens83(dt);UpdateCompulsion84(dt);
            enemyTimer-=dt;
            if(enemyTimer<=0){MoveEnemies();enemyTimer=game.State.Map=="binari"?.19f:game.State.Map is "casa_medico" or "strada_bassa"?.26f:.37f;}
            if(screen!=Screen.World){QueueRedraw();return;}
            game.AdvanceMinutes(delta*3.0); // Long sessions gradually exhaust hunger, modified by difficulty.
            if(game.State.Hunger<=0){game.State.Body=Math.Max(0,game.State.Body-(int)(delta*2));}
            if(game.State.Body<=0)Die("Il tuo corpo ha smesso di rispondere. La città non se ne accorge.");
        }
        QueueRedraw();
    }
    private void Walk(float dt)
    {
        Vector2I movement=Vector2I.Zero;
        if(Input.IsKeyPressed(Key.W)||Input.IsKeyPressed(Key.Up))movement=Vector2I.Up;
        else if(Input.IsKeyPressed(Key.S)||Input.IsKeyPressed(Key.Down))movement=Vector2I.Down;
        else if(Input.IsKeyPressed(Key.A)||Input.IsKeyPressed(Key.Left))movement=Vector2I.Left;
        else if(Input.IsKeyPressed(Key.D)||Input.IsKeyPressed(Key.Right))movement=Vector2I.Right;
        walking=movement!=Vector2I.Zero;
        if(movement==Vector2I.Down)dir=0;
        else if(movement==Vector2I.Left)dir=1;
        else if(movement==Vector2I.Right)dir=2;
        else if(movement==Vector2I.Up)dir=3;
        moveTimer-=dt;
        if(walking&&moveTimer<=0)
        {
            int x=game.State.TileX+movement.X, y=game.State.TileY+movement.Y;
            if(TrySouthStreetExit(movement)){movedThisFrame=true;return;}
            if(!Blocked(game.State.Map,x,y)&&!DormantOlmoZombieAt(x,y)&&!(game.State.Map=="casa_olmo"&&!game.State.StoryFlags.Contains("pentesilea_met")&&enemyPositions.Values.Contains(new Vector2I(x,y)))&&!CurrentMap().Npcs.Any(n=>n.X==x&&n.Y==y&&!HidePentesileaNpc(n.Id)&&!HideCorruptedNpc(n.Id)&&!(n.Id=="woman"&&game.State.CompanionJoined)))
            {
                game.State.TileX=x;game.State.TileY=y;movedThisFrame=true;StepAudio79();eventAudio?.Stop();TriggerTrap84(x,y);if(screen!=Screen.World)return;
                var touched=CurrentMap().Enemies.FirstOrDefault(e=>enemyPositions.TryGetValue(e.Id,out var ep)&&ep==new Vector2I(x,y));
                if(touched!=null){game.State.TileX-=movement.X;game.State.TileY-=movement.Y;movedThisFrame=false;StartBattle(touched);return;}
                if(ClinicStep96(x,y))return;
                if(TransitFromWalk())return;
                if(eventRules?.OnTileChanged()==LaTerzaCampana.Events.EventSignal.StartPentesileaIntro)
                {visibleHero=TileCenter(x,y);StartPentesileaCutscene();return;}
            }
            moveTimer=MoveDelay84();
        }
        visibleHero=visibleHero.Lerp(TileCenter(game.State.TileX,game.State.TileY),Mathf.Min(1f,dt*15));
    }
    private bool TransitFromWalk()
    {
        var s=game.State;
        if(s.Map=="piazza"&&s.TileY>=30&&s.TileX>=7&&s.TileX<=39){s.StoryFlags.Remove("street_from_station");EnterMap("strada",17,4);return true;}
        if(s.Map=="station"&&s.TileY>=21&&s.TileX>=15&&s.TileX<=18){s.StoryFlags.Add("street_from_station");EnterMap("strada",17,4);return true;}
        if(s.Map=="strada"&&s.TileY<=3&&s.TileX>=14&&s.TileX<=20)
        {
            ReturnFromStreet91();
            return true;
        }
        if(s.Map=="strada"&&s.TileY>=22&&s.TileX>=11&&s.TileX<=22){EnterMap("strada_bassa",17,4);return true;}
        if(s.Map=="strada_bassa"&&s.TileY<=2&&s.TileX>=16&&s.TileX<=18){EnterMap("strada",17,21);return true;}
        if(s.Map=="vicolo"&&s.TileY>=18&&s.TileX>=10&&s.TileX<=14){EnterMap("piazza",7,28);return true;}
        return false;
    }
    private void MoveEnemies()=>MoveEnemies58();
    private void StartBattle(WorldEnemy enemy)
    {
        ResetBattle75();summonTurns64=0;battle=new Battle(enemy.Kind,enemy.Id);
        if(enemy.Kind=="sexton")battle.Parts=new(){{"torace",120},{"testa",65},{"braccioS",46},{"braccioD",46},{"gambaS",25},{"gambaD",25}};
        else if(enemy.Kind=="bell")battle.Parts=new(){{"torace",220},{"testa",118},{"braccioS",75},{"braccioD",75},{"gambaS",55},{"gambaD",55}};
        else if(enemy.Kind=="zombie")battle.Parts=new(){{"torace",55},{"testa",32},{"braccioS",19},{"braccioD",19},{"gambaS",12},{"gambaD",12}};
        else if(enemy.Kind=="falciante")battle.Parts=new(){{"torace",105},{"testa",54},{"braccioS",35},{"braccioD",42},{"gambaS",28},{"gambaD",28}};
        else if(enemy.Kind=="corrupted")battle.Parts=new(){{"torace",82},{"testa",47},{"braccioS",28},{"braccioD",28},{"gambaS",18},{"gambaD",18}};
        else if(enemy.Kind=="ossuary")battle.Parts=new(){{"torace",148},{"testa",89},{"braccioS",63},{"braccioD",63},{"gambaS",36},{"gambaD",36}};
        LoadBattleInjuries59(enemy);
        ossuaryAttackTime=0f;
        if(enemy.Kind=="ossuary")PlaySound("monster_cry");
        screen=Screen.Combat;QueueRedraw();
    }
    private void WinBattle()
    {
        if(battle==null)return;
        if(heroAttack75>0||enemyAttack75>0){winPending95=true;return;}
        string id=game.State.Map+":"+battle.Id;
        if(!game.State.StoryFlags.Contains("spared_"+battle.Id))RecordCorpse59(battle.Id,battle.Kind);
        game.State.EnemyParts.Remove(id);
        game.State.DefeatedEnemies.Add(id);enemyPositions.Remove(battle.Id);
        if(battle.Kind=="bell")game.State.StoryFlags.Add("bell_defeated");
        if(battle.Kind=="corrupted")game.State.StoryFlags.Add("corrupted_defeated");
        string msg=battle.Kind=="bell"?"Il Cuore della Campana tace. La porta nella galleria ora ti riconoscerà.":"La creatura crolla. Il silenzio che segue sembra peggiore.";
        battle=null;screen=Screen.World;Notify(msg,5);
    }
    private void EnemyTurn()
    {
        if(battle?.Kind=="ossuary")
        {
            ossuaryAttackTime=.70f;
            PlaySound("monster_attack");
        }
        if(battle==null)return;
        var b=battle;var s=game.State;
        if(FamiliarAttack64())return;
        int livingArms=(b.Parts["braccioS"]>0?1:0)+(b.Parts["braccioD"]>0?1:0);
        int baseAtk=b.Kind=="falciante"?19:b.Kind=="bell"?21:b.Kind=="sexton"?16:b.Kind=="corrupted"?12:b.Kind=="zombie"?Z74(b.Id).Attack:13;
        var stats=CombatStats100.Effective(game);
        int damage=CombatStats100.IncomingDamage(game.EnemyAttack(baseAtk),b.Kind=="bell"?stats.ArcaneDefense:stats.Defense,rng.Next(0,6));
        if(livingArms==0)damage=b.Kind=="bell"?Math.Max(1,damage/3):0;
        else if(b.Armed&&b.Parts["braccioD"]==0)damage=Math.Max(1,damage/3);
        else if(livingArms==1)damage=Math.Max(1,damage/2);
        if(b.Parts["gambaS"]==0&&b.Parts["gambaD"]==0)damage/=2;
        string actionLog=b.Log;
        double chance=EnemyHitChance78(b,livingArms);
        bool guarded84=b.Guard;
        if(b.Guard){damage=damage==0?0:Math.Max(1,damage/3);chance-=.14;b.Guard=false;}
        bool landed=RollHit78(chance,out bool graze);if(!landed)damage=0;else if(graze&&damage>0)damage=Math.Max(1,damage/2);
        s.Body=Math.Max(0,s.Body-damage);if(livingArms>0||b.Kind=="bell"){enemyLanded82=damage>0;PlaySound("monster_attack",b.Kind=="zombie"?-5:-1);}
        if(b.Kind=="zombie"&&damage>0)zombieSwing74=tick;
        if(damage>0&&rng.NextDouble()<.15&&livingArms>0){s.Bleeding=true;b.Log=$"La creatura ti colpisce. Perdi {damage} Corpo e inizi a sanguinare.";}
        else b.Log=damage==0?"Il nemico manca il colpo.":graze?$"Un colpo di striscio ti sottrae {damage} Corpo.":$"Un colpo ti strappa {damage} Corpo.";
        b.Log=actionLog+"\n"+b.Log;
        if(b.Kind=="zombie"&&damage>0&&rng.NextDouble()<Z74(b.Id).Infection){s.Infection=Math.Min(100,s.Infection+8);b.Log+=" La ferita si infetta (+8 infezione).";}
        if(damage>0&&!graze&&b.Kind=="falciante"&&b.Parts["braccioD"]>0&&!guarded84&&rng.NextDouble()<.22)
        {
            var available=Parts.Skip(2).Where(l=>!s.LostLimbs84.Contains(l)).ToArray();
            if(available.Length>0){string limb=available[rng.Next(available.Length)];if(game.LoseLimb84(limb))b.Log+=" Perdita permanente: "+PartName(limb)+". "+(game.NoArms84?"Non puoi più impugnare armi.":limb.StartsWith("gamba")?"Il movimento è compromesso.":"La presa è indebolita.");}
        }
        else if(damage>0&&rng.NextDouble()<.045&&livingArms>0)
        {
            string injured=new[]{"braccioD","braccioS","gambaD","gambaS"}[rng.Next(4)];
            if(s.Injuries.Add(injured))b.Log+=" Hai perso l'uso di un arto: "+PartName(injured)+".";
        }
        if(s.Bleeding)s.Body=Math.Max(0,s.Body-3);
        SaveBattleInjuries59();b.Turn++;b.Phase=0;
        if(s.Body<=0)Die("Le tue ferite non smettono di sanguinare. Il nemico si allontana senza voltarsi.");
    }
    private void AttackPart(int i)
    {
        if(battle==null||i<0||i>=Parts.Length)return;
        var b=battle;string p=Parts[i];
        if(b.Parts[p]<=0){b.Log="Quella parte è già stata distrutta.";return;}
        int power=CombatStats100.Effective(game).Attack+rng.Next(0,9);
        power=Math.Max(2,power-(int)game.EnemyDefense(b.Kind=="bell"?7:3));
        double hitChance=PlayerHitChance78(b,p);
        bool graze=false;
        if(!RollHit78(hitChance,out graze))
        {
            b.Log=$"Miri al {PartName(p).ToLowerInvariant()}, ma il colpo va a vuoto.";
            RequestEnemy95();return;
        }
        heroLanded95=true;
        if(graze)power=Math.Max(1,(int)(power*.45f));
        b.Parts[p]=Math.Max(0,b.Parts[p]-power);if(b.Kind=="zombie")zombieHit74=tick;
        b.Log=(graze?$"Sfiori il {PartName(p).ToLowerInvariant()}.":"Colpisci "+PartName(p).ToLowerInvariant()+".")+ $" Infliggi {power} danni. " + (graze?"La ferita è superficiale.":"La carne si apre come una cucitura.");
        if(b.Parts[p]==0)
        {
            if(p=="torace"||p=="testa"){WinBattle();return;}
            b.Log+=p=="braccioD"&&b.Armed?" L’arma cade a terra.":p.StartsWith("gamba")?" La gamba cede: il nemico rallenta.":" L’arto cede. Il nemico perde l’uso del braccio.";
        }
        RequestEnemy95();
    }
    private void BattleTalk(int n)
    {
        if(battle==null)return;
        battle.Log=n switch {0=>"La creatura inclina la testa come se riconoscesse la tua voce.",1=>"Le tue suppliche si perdono dentro un rumore di metallo.",_=>"Per un momento i suoi muscoli si rilassano."};
        double chance=n==2?.32:n==0?.14:.10;
        if(rng.NextDouble()<chance){game.State.StoryFlags.Add("spared_"+battle.Id);WinBattle();Notify("Le parole hanno interrotto lo scontro. La creatura si ritira.",4);}
        else RequestEnemy95();
    }
    private void BattleSkill(int n)
    {
        if(battle==null)return;
        var s=game.State;
        if(n==0){battle.Phase=3;return;}
        if(IsRoderick64){RoderickSkill64(n);return;}
        if(s.Hero==HeroId.Anatema){AnatemaSkill101(n);return;}
        string name=game.GetSkills()[Math.Clamp(n,0,3)];
        int cost=n==1?12:n==2?8:22;
        if(name=="Sussurro di Braka")cost=28;
        if(name=="Litania di contenimento")cost=16;
        if(name=="Respiro di contenimento")cost=8;
        if(name=="Disciplina del corpo")cost=10;
        if(name=="Sovraccarico vitale")cost=28;
        if(name=="Sigillo della soglia")cost=16;
        if(s.Mind<cost){battle.Log="Non hai Mente sufficiente per "+name+".";return;}
        s.Mind-=cost;
        if(n==1&&s.Hero==HeroId.Gladia){int heal=GladiaOrigin.Heal(game,true);battle.Log=$"{name}: recuperi fino a {heal} Corpo. Il sanguinamento si ferma.";RequestEnemy95();}
        else if(n==1&&s.Hero==HeroId.Ambrosh&&name=="Respiro di contenimento"){battle.Guard=true;s.Mind=Math.Min(100,s.Mind+14);battle.Log="Riprendi il controllo del respiro. Recuperi lucidità e ti prepari a difenderti.";RequestEnemy95();}
        else if(n==1&&s.Hero==HeroId.Ambrosh&&name=="Disciplina del corpo"){battle.Guard=true;s.Body=Math.Min(100,s.Body+18);battle.Log="Disciplina del corpo: recuperi 18 Corpo e ti difendi.";RequestEnemy95();}
        else if(n==1&&s.Hero==HeroId.Ambrosh){battle.Log="Osservi il nemico: la testa è più fragile delle braccia.";battle.Parts["testa"]=Math.Max(1,battle.Parts["testa"]-16);RequestEnemy95();}
        else if(n==1){s.Body=Math.Min(100,s.Body+17);battle.Guard=true;battle.Log="Il tuo respiro si fa più saldo.";RequestEnemy95();}
        else
        {
            int bonus=n==3?42:24;
            string part=n==3?"torace":"testa";
            if(name=="Sussurro di Braka")bonus=58;
            else if(name=="Sovraccarico vitale")bonus=56;
            else if(name=="Laccio votivo"){bonus=30;part="gambaS";battle.Guard=true;}
            else if(name=="Sigillo della soglia"){bonus=35;part="testa";battle.Guard=true;}
            else if(name=="Litania di contenimento"){bonus=32;part="testa";battle.Guard=true;}
            else if(name=="Sigillo del focolare"){bonus=32;part=battle.Armed&&battle.Parts["braccioD"]>0?"braccioD":"torace";battle.Guard=true;}
            bool physical=name is "Taglio netto" or "Fendente metodico";
            var stats=CombatStats100.Effective(game);
            bonus=CombatStats100.SkillDamage(bonus,physical?stats.Attack:stats.ArcaneAttack,(int)game.EnemyDefense(physical?6:CombatStats100.ArcaneDefense(battle.Kind)));
            battle.Parts[part]=Math.Max(0,battle.Parts[part]-bonus);
            battle.Log=$"{name}: {bonus} danni al {PartName(part).ToLowerInvariant()}.";
            if(battle.Parts[part]<=0&&(part=="testa"||part=="torace")){WinBattle();return;}
            RequestEnemy95();
        }
    }
    private void Die(string why){if(screen==Screen.Combat&&enemyAttack75>0){deathPending95=why;return;}deathReason=why;screen=Screen.Dead;}
    private bool UseItem(string id,bool inCombat=false)
    {
        var s=game.State;if(s.Inventory.GetValueOrDefault(id,0)<1){Notify("Non possiedi questo oggetto.");return false;}
        bool used=true;
        switch(id)
        {
            case "bende":s.Body=Math.Min(100,s.Body+(s.Hero==HeroId.Gladia?43:33));s.Bleeding=false;break;
            case "erbe":s.Body=Math.Min(100,s.Body+20);s.Infection=Math.Max(0,s.Infection-25);break;
            case "infuso":s.Mind=Math.Min(100,s.Mind+30);break;
            case "pane":s.Hunger=Math.Min(100,s.Hunger+40);break;
            case "mela":s.Hunger=Math.Min(100,s.Hunger+25);break;
            case "conserva":s.Hunger=Math.Min(100,s.Hunger+50);break;
            case "grimaldello":Notify("Il grimaldello si usa davanti a una porta chiusa.");return false;
            default:used=false;break;
        }
        if(!used){Notify("Non puoi consumare questo oggetto.");return false;}
        s.Inventory[id]--;
        string text="Hai usato: "+Catalog.Items[id]+".";
        if(inCombat&&battle!=null){battle.Log=text;RequestEnemy95();}
        else Notify(text);
        return true;
    }
    private void CycleEquipment(int index)
    {
        var slot=Slots[index];var s=game.State;
        if(slot==GearSlot.Arma&&game.NoArms84){s.Equipped.Remove(slot);Notify("Non hai più braccia con cui impugnare un’arma.");return;}
        var possible=new List<string>{""};
        possible.AddRange(s.OwnedEquipment.Where(id=>Catalog.Equipment.TryGetValue(id,out var e)&&e.Slot==slot).OrderBy(x=>x));
        string old=s.Equipped.GetValueOrDefault(slot,"");
        int next=(possible.IndexOf(old)+1)%possible.Count;
        if(next==0)s.Equipped.Remove(slot);
        else game.Equip(possible[next]);
        Notify("Equipaggiato: "+(next==0?"nessuno":Catalog.Equipment[possible[next]].DisplayName));
    }
    private void WorldSkill(int n)
    {
        var s=game.State;
        if(n==0)
        {
            if(s.CompanionJoined)Say("Ada","Non sono morta. Non ancora. È questo che mi spaventa di più.","portrait_ada");
            else
            {
                var npc=ClosestTarget();
                if(npc!=null&&npc.Type=="npc")InteractNpc((WorldNpc)npc.Entity);
                else Notify("Non c'è nessuno con cui parlare.");
            }
        }
        else if(n==1&&s.Hero==HeroId.Ambrosh&&s.StoryFlags.Contains("ambrosh_ritual_control")){Notify("Il respiro di contenimento si usa in combattimento.");}
        else if(n==1&&s.Hero==HeroId.Ambrosh&&s.StoryFlags.Contains("ambrosh_physical_control")){if(s.Mind>=10){s.Mind-=10;s.Body=Math.Min(100,s.Body+18);Notify("Disciplina del corpo: recuperi Corpo.");}else Notify("Mente insufficiente.");}
        else if(n==1&&s.Hero==HeroId.Gladia){if(s.Mind>=12){s.Mind-=12;int heal=GladiaOrigin.Heal(game,false);Notify(game.GetSkills()[1]+$": recuperi fino a {heal} Corpo.");}else Notify("Mente insufficiente.");}
        else if(s.Hero==HeroId.Roderick&&n==1){ActivateVision64();}
        else if(s.Hero==HeroId.Roderick&&n==3&&s.FamiliarKind!=""){Say("Patto della follia","Vuoi sciogliere il legame con il tuo famiglio?","",new Choice("Liberalo",()=>{s.FamiliarKind="";CloseDialogue();Notify("Il famiglio torna nel buio.");}),new Choice("Conservalo",CloseDialogue));}
        else Notify("Questa abilità è disponibile durante i combattimenti.");
    }
    private void Interact()
    {
        var target=ClosestTarget();
        if(target==null){Notify("Non c'è nulla da esaminare davanti a te.");return;}
        if(target.Type=="npc")InteractNpc((WorldNpc)target.Entity);
        else if(target.Type=="door")InteractDoor((WorldDoor)target.Entity);
        else if(target.Type=="corpse")LootCorpse59((string)target.Entity);
        else InteractProp((WorldProp)target.Entity);
    }
    private bool HasLockpickDoorOption(WorldDoor d,string key)
    {
        if(game.State.Inventory.GetValueOrDefault("grimaldello",0)<1)return false;
        if(d.Id=="stationhall")return !game.CanOpenCentralStationDoor();
        if(game.State.Map=="sala"&&d.Id=="alley")return !game.State.StoryFlags.Contains("opened93:"+key);
        if(game.State.Map=="strada_bassa"&&d.Id.StartsWith("locked_",StringComparison.Ordinal)&&!game.State.StoryFlags.Contains("opened81:"+key))return true;
        return d.Hp>0&&game.State.DoorDamage.GetValueOrDefault(key)<d.Hp;
    }
    private void UseLockpickOnDoor(WorldDoor d,string key)
    {
        if(!HasLockpickDoorOption(d,key)){Say(d.Label,"Non puoi usare il grimaldello qui.");return;}
        game.State.Inventory["grimaldello"]--;
        PlaySound("door_open");
        if(game.State.Map=="strada_bassa"&&d.Id.StartsWith("locked_",StringComparison.Ordinal)&&!game.State.StoryFlags.Contains("opened81:"+key))
        {
            game.State.StoryFlags.Add("opened81:"+key);
            Say(d.Label,"Il metallo cede con un clic soffocato. Il grimaldello si spezza dentro il meccanismo, ma la porta è aperta.","",new Choice("Entra",()=>{PlaySound("door_open");EnterMap(d.To,(int)d.Spawn[0],(int)d.Spawn[1]);}),new Choice("Resta qui",CloseDialogue));
            return;
        }
        if(d.Id=="stationhall"||game.State.Map=="sala"&&d.Id=="alley")
        {
            game.State.StoryFlags.Add("opened93:"+key);
            Say(d.Label,"Lavori sulla serratura arrugginita finché non senti il nottolino cedere. Il grimaldello è inutilizzabile, ma il portone si apre.","",new Choice("Apri il portone",()=>{PlaySound("door_open");EnterMap(d.To,(int)d.Spawn[0],(int)d.Spawn[1]);}),new Choice("Resta qui",CloseDialogue));
            return;
        }
        game.State.DoorDamage[key]=Math.Max(d.Hp,game.State.DoorDamage.GetValueOrDefault(key));
        Say(d.Label,"Scassini la serratura con pazienza. Il grimaldello si consuma, ma la porta ora cede senza fare resistenza.","",new Choice("Apri la porta",()=>{PlaySound("door_open");EnterMap(d.To,(int)d.Spawn[0],(int)d.Spawn[1]);}),new Choice("Resta qui",CloseDialogue));
    }
    private void InteractDoor(WorldDoor d)
    {
        var s=game.State;
        if(s.Map=="strada"&&d.Id=="return_square"){PlaySound("door_open");ReturnFromStreet91();return;}
        string key=s.Map+":"+d.Id;
        if(s.Map=="station"&&d.Id=="stationhall"&&!game.CanOpenCentralStationDoor())
        {
            PlaySound("door_lock");
            if(HasLockpickDoorOption(d,key))
            {
                Say("Portone della Stazione Centrale","Il portone non si muove. La serratura è fredda, arrugginita e chiusa a chiave. Potresti usare la chiave… oppure tentare con un grimaldello consumabile.","",new Choice("Usa un grimaldello",()=>UseLockpickOnDoor(d,key)),new Choice("Allontanati",CloseDialogue));
            }
            else Say("Portone della Stazione Centrale","Il portone non si muove. La serratura è fredda, arrugginita e chiusa a chiave. Serve una chiave.");return;
        }
        if(s.Map=="sala"&&d.Id=="alley"&&!s.StoryFlags.Contains("ticket_shown")&&!s.StoryFlags.Contains("late_ticket")&&!s.StoryFlags.Contains("registry")&&!s.StoryFlags.Contains("opened93:"+key))
        {PlaySound("door_lock");if(HasLockpickDoorOption(d,key))Say("Uscita di servizio","La catena è chiusa da un lucchetto.","",new Choice("Usa un grimaldello",()=>UseLockpickOnDoor(d,key)),new Choice("Allontanati",CloseDialogue));else Say("Uscita di servizio","Una catena nuova blocca la porta. Il Capostazione non ti ha autorizzato: controlla il registro degli arrivi o torna a parlare con lui.");return;}
        if(s.Map=="strada_bassa"&&d.Id.StartsWith("locked_",StringComparison.Ordinal)&&!s.StoryFlags.Contains("opened81:"+key))
        {
            PlaySound("door_lock");
            var opts=new List<Choice>();
            if(d.Id=="locked_basso_1"&&game.HasItem("chiave_bassa"))opts.Add(new Choice("Usa la chiave",()=>Unlock81(d,key)));
            if(d.Id=="locked_basso_2"&&game.HasItem("chiave_cisterne"))opts.Add(new Choice("Usa la chiave dell’archivio",()=>Unlock81(d,key)));
            if(s.StoryFlags.Contains("street81_lockpick"))opts.Add(new Choice("Scassina il lucchetto",()=>Unlock81(d,key)));
            if(game.State.Inventory.GetValueOrDefault("grimaldello",0)>0)opts.Add(new Choice("Usa un grimaldello",()=>UseLockpickOnDoor(d,key)));
            opts.Add(new Choice("Allontanati",CloseDialogue));
            Say(d.Label,"Il lucchetto è troppo robusto per essere sfondato. Occorre la chiave adatta, conoscere lo scassinaggio oppure consumare un grimaldello.","",opts.ToArray());return;
        }
        if(d.Hp>0&&s.DoorDamage.GetValueOrDefault(key)<d.Hp)
        {
            int self=PredictDoorSelfDamage81(key);
            var opts=new List<Choice>();
            if(game.State.Inventory.GetValueOrDefault("grimaldello",0)>0)opts.Add(new Choice("Usa un grimaldello",()=>UseLockpickOnDoor(d,key)));
            opts.Add(new Choice($"Colpisci (−{self} Corpo)",()=>{CloseDialogue();AttackDoor(d);}));
            opts.Add(new Choice("Allontanati",CloseDialogue));
            Say(d.Label,$"La porta è rinforzata e chiusa. Resistenza {d.Hp-s.DoorDamage.GetValueOrDefault(key)}. Ogni colpo ti costa più del precedente. Perderai {self} Corpo se colpisci ancora. Un grimaldello consumabile eviterebbe parte del dolore.","",opts.ToArray());return;
        }
        PlaySound("door_open");
        EnterMap(d.To,(int)d.Spawn[0],(int)d.Spawn[1]);
    }
    private int DoorHitCount81(string key)=>game.State.DoorDamage.GetValueOrDefault(key+":hits");
    private int PredictDoorSelfDamage81(string key)
    {
        int hits=DoorHitCount81(key);
        return 2+Math.Max(0,hits)*2;
    }
    private void AttackDoor(WorldDoor d)
    {
        if(d.Hp<=0||d.Id.StartsWith("locked_",StringComparison.Ordinal))return;
        string key=game.State.Map+":"+d.Id;
        if(game.State.DoorDamage.GetValueOrDefault(key)>=d.Hp)return;
        PlaySound("door_hit");
        int amount=Math.Max(2,9+game.GetEquipmentStats().Attack*2-d.Armor*3);
        int selfDamage=PredictDoorSelfDamage81(key);
        game.State.DoorDamage[key]=game.State.DoorDamage.GetValueOrDefault(key)+amount;
        game.State.DoorDamage[key+":hits"]=DoorHitCount81(key)+1;
        game.State.Hunger=Math.Max(0,game.State.Hunger-1*Catalog.Difficulties[game.State.Difficulty].HungerRate);
        game.State.Body=Math.Max(0,game.State.Body-selfDamage);
        if(selfDamage>=4&&rng.NextDouble()<.22)game.State.Bleeding=true;
        if(game.State.Body<=0){Die("Insisti a colpire finché le braccia cedono e il dolore ti spegne davanti alla porta.");return;}
        string pain=game.State.Bleeding?" Le nocche si spaccano e il sangue ti cola dalle mani.":" Le braccia protestano ad ogni urto.";
        if(game.State.DoorDamage[key]>=d.Hp)Say("Porta sfondata",$"Il legno cede con un gemito. La porta è aperta e resterà così anche dopo il salvataggio. Ti sei però inflitto {selfDamage} Corpo colpendola.{pain}");
        else Say("Porta rinforzata",$"La colpisci con tutte le tue forze. Infliggi {amount} danni e perdi {selfDamage} Corpo. Rimangono {d.Hp-game.State.DoorDamage[key]} punti di resistenza.{pain}");
    }
    private void InteractNpc(WorldNpc npc)
    {
        var s=game.State;string name=npc.Name,portrait=PortraitOf(npc);
        switch(npc.Id)
        {
            case "pentesilea": TalkPentesilea();break;
            case "archivio_unknown99": Say("????","L’individuo solleva appena lo sguardo. Non dice altro.");break;
            case "stationmaster":
                if(s.StoryFlags.Contains("ticket_shown")||s.StoryFlags.Contains("late_ticket"))Say(name,"Non ti ho mai visto su quel treno. Eppure il tuo biglietto è in regola. Che strano... Hai il permesso per l'uscita di servizio.",portrait);
                else Say(name,"Non ti ho mai visto su quel treno. Non risulti fra i passeggeri. L'uscita di servizio è chiusa finché non avrò una prova.",portrait,
                    new Choice("Mostra il biglietto ora",()=>{s.StoryFlags.Add("late_ticket");s.Mind=Math.Max(0,s.Mind-7);Say(name,"Il Capostazione strappa metà del documento e la restituisce. «Non ti ho mai visto sul treno. Eppure il biglietto è autentico». Ora hai il permesso.",portrait);}),
                    new Choice("Cerca il registro",()=>Say(name,"Nella sala d'attesa c'è un registro degli arrivi. Trova il tuo nome.",portrait)));
                break;
            case "oldtraveller":
                if(s.StoryFlags.Contains("passenger_helped")&&s.StoryFlags.Add("passenger_reward"))
                {game.AddItem("erbe");Say(name,"Qualcuno ha lasciato delle erbe per te. C'è scritto: «Non tutte le mani che chiedono aiuto appartengono ai vivi».",portrait);}
                else if(s.StoryFlags.Contains("passenger_ignored")&&s.StoryFlags.Add("passenger_regret"))
                {s.Mind=Math.Max(0,s.Mind-6);Say(name,"Quell'uomo che hai ignorato è sceso dal treno dietro di te. Lo senti ancora bussare. Perdi Mente.",portrait);}
                else Say(name,"Alcuni passeggeri scendono qui ogni notte. Nessuno ricorda di essere salito.",portrait);
                break;
            case "keeper":Say(name,"La campana è sottoterra. La cappella custodisce un sigillo; cerca prima quello.",portrait);break;
            case "woman":Say("Ada","Non andrò da sola nella galleria. Vuoi che venga con te?",portrait,
                new Choice("Unisciti al gruppo",()=>{s.CompanionJoined=true;Say("Ada","Verrò con te. Non so se possiamo tornare indietro.",portrait);}),
                new Choice("Resta qui",CloseDialogue));break;
            case "survivor":Say("Neri","Posso scambiare una fasciatura con un pezzo di pane. Nessuno vuole più mangiare qui.",portrait,
                new Choice("Scambia pane per benda",()=>{if(s.Inventory.GetValueOrDefault("pane")>0){s.Inventory["pane"]--;game.AddItem("bende");Say("Neri","Non guardare troppo a lungo la statua.",portrait);}else Say("Neri","Non hai pane.",portrait);}),
                new Choice("Lascia stare",CloseDialogue));break;
            case "corrupted_wanderer":
                Say("Il Viandante Corrotto",
                    "L'uomo tiene le mani lontane da sé. «Non sono malato... È la campana. Mi risuona nelle ossa». Si sforza di non avvicinarsi.",portrait,
                    new Choice("Chiedi della campana",()=>Say(name,"«Quando suona, le case si ricordano i loro morti. Non varcare le porte segnate». La voce si spezza in un rantolo.",portrait)),
                    new Choice("Offrigli una parola gentile",()=>{s.StoryFlags.Add("corrupted_spared");Say(name,"La sua mano smette di tremare. «C'è ancora qualcuno che mi ascolta...»",portrait);}),
                    new Choice("Chiamalo un cadavere",()=>MakeCorruptedHostile()));
                break;
            case "priest":Say(name,s.StoryFlags.Contains("voice_answered")?"La voce ti ha marchiato. L'altare riconoscerà il tuo nome e chiederà meno Mente.":"Il sigillo è sull'altare. Non toccarlo se non vuoi sacrificare qualcosa.",portrait);break;
            default:Say(name,"Il viaggiatore evita il tuo sguardo.",portrait);break;
        }
    }
    private void LootContainer(WorldProp p)
    {
        var loot=game.SearchContainer(game.State.Map+":"+p.Id,rng,game.State.Map is "strada" or "vicolo" or "casa_olmo" or "casa_olmo_servizio" or "strada_bassa" or "rifugio_basso" or "magazzino_basso" or "officina_bassa" or "archivio_basso" or "casa_medico");
        if(loot.AlreadyOpened)Say(p.Name,"Hai già rovistato qui. Non è rimasto niente.");
        else if(loot.Quantity==0)Say(p.Name,"Rovisti fra il legno marcio, ma non trovi nulla.");
        else Say(p.Name,$"Tra gli scarti trovi: **x{loot.Quantity} {loot.DisplayName}**.");
    }
    private void InteractProp(WorldProp p)
    {
        var s=game.State;string id=p.Id;if(id=="patient96"){InspectPatient96();return;}
        if(s.Map=="rifugio_basso"&&InteractRefuge89(p))return;
        if(id.StartsWith("trap84_")){InspectTrap84(p);return;}
        if(id=="basso_training"){
            if(s.StoryFlags.Contains("street81_lockpick")){Say(p.Name,"Hai già studiato il manuale.");return;}
            Say(p.Name,"Il manuale spiega l'uso degli attrezzi del fabbro. Puoi dedicare del tempo a studiarlo.","",new Choice("Studia (−5 Fame)",()=>{s.StoryFlags.Add("street81_lockpick");s.Hunger=Math.Max(0,s.Hunger-5);Say(p.Name,"Hai appreso l'abilità Scassinaggio. Ora puoi aprire i lucchetti del Sottostrada.");}),new Choice("Lascia stare",CloseDialogue));return;}
        if(id=="olmo_hole82"||id=="basso_key"){InspectHole93(p.Name);return;}
        if(id=="dead_cat63"){Say("Gatto morto",s.Mind<30?"Per un istante credi che il fianco si muova. Sono soltanto le mosche. Forse.":"Un piccolo gatto giace immobile sul selciato. Il ronzio delle mosche è l’unico suono che gli rimane.");return;}
        if(id=="station_refuse_bin")
        {
            var loot=game.SearchStationBin();
            if(loot.AlreadyOpened)Say("Bidone della stazione","È già stato rovistato. Rimangono vecchi orari ferroviari e una bottiglia incrinata.");
            else Say("Bidone della stazione","Nel fondo del bidone, sotto una rivista bagnata, trovi: **x1 Chiave della Stazione Centrale**.");
            return;
        }
        if(id=="desk01")
        {
            if(s.StoryFlags.Add("registry"))
            {if(s.Hero==HeroId.Ambrosh)s.Mind=Math.Min(100,s.Mind+6);Say("Registro della stazione","Tra gli arrivi di domani trovi il tuo nome. In margine una combinazione permette di aprire l'uscita di servizio.");}
            else Say("Registro della stazione","Il tuo nome è scritto accanto a una data che non è ancora arrivata.");return;
        }
        if(id=="olmo_desk92")
        {
            bool foundDocs=s.StoryFlags.Contains("olmo_docs92");
            bool foundPick=s.StoryFlags.Contains("olmo_pick92");
            if(foundDocs&&foundPick){Say("Scrivania dell’Olmo","I cassetti sono già stati svuotati. Rimane odore di muffa, cera e inchiostro marcito.");return;}
            var opts=new List<Choice>();
            if(!foundDocs)opts.Add(new Choice("Esamina i documenti",()=>{s.StoryFlags.Add("olmo_docs92");ReadOlmoDocs93();}));
            if(!foundPick)opts.Add(new Choice("Fruga nel doppiofondo",()=>{s.StoryFlags.Add("olmo_pick92");game.AddItem("grimaldello");Say("Scrivania dell’Olmo","Dal doppiofondo estrai un astuccio fradicio con dentro **x1 Grimaldello consumabile**. Potrai usarlo su una porta chiusa, ma si spezzerà all’uso.");}));
            opts.Add(new Choice("Lascia stare",CloseDialogue));
            Say("Scrivania dell’Olmo","La scrivania è gonfia d’umidità. Tra i cassetti bloccati senti carta secca e ferro sottile.","",opts.ToArray());return;
        }
        if(id=="well"&&s.Map=="piazza"){OpenFountain61();return;}
        if(id=="station_fountain"){OpenThreeMothers73(p);return;}
        if(id=="well")
        {Say(id=="station_fountain"?"Fontana delle Tre Madri":"La Statua dei Senzavoce",p.Note.Length>0?p.Note:"La pietra è viscida. Qualcosa sotto la superficie respira in silenzio.");return;}
        if(id=="chest"||id=="corpse")
        {
            string key=s.Map+":"+id;
            if(s.SearchedContainers.Add(key)){game.AddItem(id=="chest"?"infuso":"bende");Say(p.Name,$"Tra i resti trovi: **x1 {(id=="chest"?"Infuso amaro":"Fasciatura sporca")}**.");}
            else Say(p.Name,"Non rimane niente.");return;
        }
        if(id=="altar")
        {
            if(s.StoryFlags.Contains("sigillo")){Say("Altare senza santo","L'impronta del sigillo è ancora calda.");return;}
            int cost=s.StoryFlags.Contains("voice_answered")?12:20;
            Say("Altare senza santo","Un disco di cera nera attende sull'altare. Per prenderlo devi offrire qualcosa.","",
                new Choice($"Sacrifica {cost} Mente",()=>{s.Mind=Math.Max(0,s.Mind-cost);s.StoryFlags.Add("sigillo");Say("Sigillo","La cera porta inciso il tuo nome. Il sigillo ora ti appartiene.");}),
                new Choice("Sacrifica 20 Corpo",()=>{s.Body=Math.Max(1,s.Body-20);s.StoryFlags.Add("sigillo");Say("Sigillo","La pietra beve il tuo sangue. Il sigillo ora ti appartiene.");}),
                new Choice("Allontanati",CloseDialogue));return;
        }
        if(id=="gate")
        {
            if(!s.StoryFlags.Contains("bell_defeated")){Say(p.Name,"La porta respira. Il Cuore della Campana deve essere fermato.");return;}
            if(!s.StoryFlags.Contains("sigillo")){Say(p.Name,"La porta non ha maniglia. Occorre un sigillo.");return;}
            Say(p.Name,"Puoi spezzare il sigillo o custodire il suono.","",
                new Choice("Spezza il sigillo",()=>{ending="luce";screen=Screen.Ending;}),
                new Choice("Conserva il sigillo",()=>{ending="silenzio";screen=Screen.Ending;}));return;
        }
        var gearDrops=new Dictionary<string,string>{{"olmo_dresser","corazza"},{"olmo_trunk","cappuccio"},{"medic_table","coltello"},
            {"medic_shelf","medaglione"},{"sarto_lockbox","elmo"},{"sarto_bench","giacca"},{"gear_tool_rail","bastone"},{"cantina_chest","bastone"}};
        if(gearDrops.TryGetValue(id,out var gearId))
        {
            string key=s.Map+":"+id;
            if(s.SearchedContainers.Add(key)){s.OwnedEquipment.Add(gearId);Say(p.Name,"Trovi: **x1 "+Catalog.Equipment[gearId].DisplayName+"**. Puoi equipaggiarlo nella scheda EQUIPAGGIAMENTO.");}
            else Say(p.Name,"Hai già controllato questo mobile.");return;
        }
        if(id.Contains("barrel")||id.Contains("barile")||id.Contains("crate")||id.Contains("cassa")||p.Kind=="barrel"||p.Kind=="crate")
        {LootContainer(p);return;}
        Say(p.Name,p.Note.Length>0?p.Note:"L'oggetto porta i segni di un utilizzo che non riesci a comprendere.");
    }
    private static int Digit(Key key)=>key switch
    {
        Key.Key1 or Key.Kp1=>1,Key.Key2 or Key.Kp2=>2,Key.Key3 or Key.Kp3=>3,Key.Key4 or Key.Kp4=>4,Key.Key5 or Key.Kp5=>5,
        Key.Key6 or Key.Kp6=>6,Key.Key7 or Key.Kp7=>7,Key.Key8 or Key.Kp8=>8,Key.Key9 or Key.Kp9=>9,_=>0
    };
    private void PrologueAdvance(bool first)
    {
        if(proStep==1)game.ShowTicket(first);
        if(proStep==3)game.HelpPassenger(first);
        if(proStep==5)game.AnswerVoice(first);
        proStep++;
        if(proStep>=8){screen=Screen.World;EnterMap("station",10,14);BeginArrival80();}
    }
    private void InventoryKey(int n,Key key)
    {
        if(key==Key.I||key==Key.Escape){screen=Screen.World;return;}
        if(key==Key.Tab||key==Key.D||key==Key.Right){invTab=(invTab+1)%5;return;}
        if(key==Key.A||key==Key.Left){invTab=(invTab+4)%5;return;}
        if(n==7&&invTab==3&&game.State.StoryFlags.Contains("olmo_docs92")){ReadOlmoDocs93(true);return;}
        if(n==9&&(invTab==0||invTab==3)&&game.HasItem("lettera_scarlatta")){ReadScarlet89(true);return;}
        if(n<1)return;
        if(invTab==0&&n<=ItemKeys.Length)UseItem(ItemKeys[n-1]);
        else if(invTab==1&&n<=4)CycleEquipment(n-1);
        else if(invTab==2&&n<=4){screen=Screen.World;WorldSkill(n-1);}
    }
    private void BattleKey(int n,Key key)
    {
        if(battle==null||BattleBusy75)return;
        if(key==Key.Escape){battle.Phase=0;return;}
        if(battle.Phase==1){if(key==Key.Up||key==Key.W){SelectTarget75(-1);return;}if(key==Key.Down||key==Key.S){SelectTarget75(1);return;}if(key==Key.Enter||key==Key.KpEnter||key==Key.E||key==Key.Space){StartAttack75(target75);return;}if(n>=1&&n<=Parts.Length){StartAttack75(n-1);return;}}
        if(n==0)return;
        if(battle.Phase==0)
        {
            if(n==1){battle.Phase=1;target75=0;if(battle.Parts[Parts[0]]<=0)SelectTarget75(1);}
            else if(n==2)battle.Phase=2;
            else if(n==3){battle.Guard=true;battle.Log="Ti prepari a ricevere il prossimo colpo.";RequestEnemy95();}
            else if(n==4)battle.Phase=4;
            else if(n==5)
            {
                AttemptFlee100(rng.NextDouble());
            }
        }
        else if(battle.Phase==1&&n<=Parts.Length)StartAttack75(n-1);
        else if(battle.Phase==2&&n<=4)BattleSkill(n-1);
        else if(battle.Phase==3&&n<=3)BattleTalk(n-1);
        else if(battle.Phase==4&&n<=6)UseItem(ItemKeys[n-1],true);
    }
    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if(@event is not InputEventKey keyEvent||!keyEvent.Pressed||keyEvent.Echo)return;
        var key=keyEvent.Keycode==Key.None?keyEvent.PhysicalKeycode:keyEvent.Keycode;int n=Digit(key);
        bool ok=key==Key.Enter||key==Key.KpEnter||key==Key.Space||key==Key.E;
        if(key==Key.F2&&screen is (Screen.Title or Screen.Hero)){debugAnatema101=true;hero=HeroId.Anatema;screen=Screen.Hero;QueueRedraw();return;}
        if(key==Key.F10&&(screen is Screen.Origin or Screen.Prologue)){SkipIntro82();QueueRedraw();return;}
        if(key==Key.F5&&(screen==Screen.World||screen==Screen.Inventory||screen==Screen.Pause)){Save();return;}
        switch(screen)
        {
            case Screen.Title:
                if(key==Key.Up||key==Key.W)titleSelection=(titleSelection+2)%3;
                else if(key==Key.Down||key==Key.S)titleSelection=(titleSelection+1)%3;
                else if(ok)
                {
                    if(titleSelection==0)screen=Screen.Hero;
                    else if(titleSelection==1)Load();
                    else GetTree().Quit();
                }
                else if(key==Key.L)Load();
                else if(key==Key.Escape)GetTree().Quit();
                break;
            case Screen.Hero:
                if(ok)screen=Screen.Difficulty;
                else if(key==Key.Escape)screen=Screen.Title;
                else if(key==Key.A||key==Key.Left)hero=(HeroId)(((int)hero+HeroCount101-1)%HeroCount101);
                else if(key==Key.D||key==Key.Right)hero=(HeroId)(((int)hero+1)%HeroCount101);
                break;
            case Screen.Difficulty:
                if(ok)BeginNew60();
                else if(key==Key.Escape)screen=Screen.Hero;
                else if(key==Key.W||key==Key.Up)difficulty=(Difficulty)(((int)difficulty+2)%3);
                else if(key==Key.S||key==Key.Down)difficulty=(Difficulty)(((int)difficulty+1)%3);
                break;
            case Screen.Origin: OriginKey60(n,key,ok);break;
            case Screen.Fountain: if(ok||key==Key.Escape)AdvanceFountain73(key==Key.Escape);break;
            case Screen.Prologue:
                if(PrologueInfo().decision){if(n==1||n==2)PrologueAdvance(n==1);}
                else if(ok)PrologueAdvance(true);
                break;
            case Screen.World:
                if(key==Key.I){screen=Screen.Inventory;invTab=0;}
                else if(key==Key.E)Interact();
                else if(key==Key.F2)debugHitboxes=!debugHitboxes;
                else if(key==Key.F6){editCollision=!editCollision;debugHitboxes=editCollision||debugHitboxes;Notify(editCollision?"EDITOR HITBOX ATTIVO — clicca per aggiungere/togliere ostacoli.":"Editor hitbox disattivato.");}
                else if(key==Key.Escape)screen=Screen.Pause;
                break;
            case Screen.Dialogue:
                if(clinicAttack96)break;
                if(clinicPortrait96&&clinicClock96<.65f){clinicClock96=.65f;break;}
                if(holeScene93){if(holeClock93<.85f){holeClock93=.85f;break;}if(key==Key.Escape){CloseDialogue();break;}if(choices.Count>0&&n>=1&&n<=choices.Count){choices[n-1].Do();break;}if(choices.Count==0&&ok)CloseDialogue();break;}
                if(arrivalBeat80>=0){if(ok)AdvanceArrival80();break;}
                if(pentesileaDialogue&&choices.Count==0){if(ok)NextPentesileaLine();break;}
                if(choices.Count>0){if(n>=1&&n<=choices.Count){var choice=choices[n-1];choice.Do();}}
                else if(ok||key==Key.Escape)CloseDialogue();
                break;
            case Screen.Inventory:InventoryKey(n,key);break;
            case Screen.Prayer: PrayerKey83(n,key,ok);break;
            case Screen.Combat:BattleKey(n,key);break;
            case Screen.Cutscene:
                if(key==Key.Escape&&eventScene==ExtraScene.None)FinishStationIntro();
                break;
            case Screen.Pause:
                if(n==1||key==Key.Escape)screen=Screen.World;
                else if(n==2)Save();
                else if(n==3)screen=Screen.Title;
                break;
            case Screen.Dead:case Screen.Ending:if(ok)screen=Screen.Title;break;
        }
        QueueRedraw();
    }
    public override void _UnhandledInput(InputEvent @event)
    {
        if(@event is not InputEventMouseButton mb||!mb.Pressed||mb.ButtonIndex!=MouseButton.Left)return;
        Vector2 click=mb.Position;
        if(screen==Screen.Prayer){if(click.Y>=540&&click.Y<=650){PrayerKey83(click.X<640?1:2,Key.None,false);}return;}
        if(screen==Screen.Origin){OriginMouse60(click);QueueRedraw();return;}
        if(screen==Screen.Fountain){AdvanceFountain73();return;}
        if(screen==Screen.World&&editCollision)
        {
            int x=(int)Math.Floor((click.X+camera.X)/T),y=(int)Math.Floor((click.Y+camera.Y)/T);
            if(x>0&&y>0&&x<CurrentMap().W-1&&y<CurrentMap().H-1)
            {
                string id=$"{game.State.Map}:{x}:{y}";
                game.State.CollisionEdits[id]=!Blocked(game.State.Map,x,y);
                Notify($"Collisione modificata a [{x}, {y}]. Salva con F5.",3);
            }
            return;
        }
        if(screen==Screen.Combat&&click.X<375&&click.Y>=535&&click.Y<700&&battle!=null&&(battle.Phase==0||battle.Phase==1))
        {int i=(int)((click.Y-(battle.Phase==1?535:530))/(battle.Phase==1?25:32))+1;BattleKey(i,Key.None);QueueRedraw();return;}
        if(screen==Screen.Dialogue&&clinicAttack96)return;
        if(screen==Screen.Dialogue&&clinicPortrait96){if(clinicClock96<.65f){clinicClock96=.65f;return;}if(click.Y>=665&&choices.Count>0){int index=(int)((click.X-59)/315);if(click.X>=59&&index>=0&&index<choices.Count)choices[index].Do();}else if(click.Y>=489&&choices.Count==0)CloseDialogue();return;}
        if(screen==Screen.Dialogue&&holeScene93){if(holeClock93<.85f){holeClock93=.85f;return;}if(click.Y>=665&&choices.Count>0){int index=(int)((click.X-59)/315);if(click.X>=59&&index>=0&&index<choices.Count)choices[index].Do();}else if(click.Y>=489&&choices.Count==0)CloseDialogue();return;}
        if(screen==Screen.Dialogue&&arrivalBeat80>=0&&click.Y>=489){AdvanceArrival80();return;}
        if(screen==Screen.Dialogue&&pentesileaDialogue&&click.Y>=489)
        {
            if(choices.Count==0)NextPentesileaLine();
            else if(click.Y>=665){int index=(int)((click.X-233)/315);if(click.X>=233&&index>=0&&index<choices.Count)choices[index].Do();}
            return;
        }
        if(screen==Screen.Title)
        {
            if(click.X>745&&click.Y>380&&click.Y<610)
            {
                titleSelection=Math.Clamp((int)((click.Y-390)/66),0,2);
                if(titleSelection==0)screen=Screen.Hero;
                else if(titleSelection==1)Load();
                else GetTree().Quit();
            }
        }
        else if(screen==Screen.Hero)
        {
            if(click.Y>126&&click.Y<590){int index=Math.Clamp((int)((click.X-72)/(HeroCount101==4?294f:395f)),0,HeroCount101-1);hero=(HeroId)index;}
            else if(click.Y>595)screen=Screen.Difficulty;
        }
        else if(screen==Screen.Difficulty && click.X >= 176 && click.X <= 1104 && click.Y >= 151 && click.Y <= 607)
        {
            // Click one of the three *vertical* rows, rather than columns.
            int row = Math.Clamp((int)((click.Y - 155) / 160), 0, 2);
            difficulty = (Difficulty)row;
            // An actual clickable choice: begin the prologue on the same click.
            BeginNew60();
        }
        else if(screen==Screen.Inventory&&click.Y>44&&click.Y<112&&click.X>435)
            invTab=click.X<590?0:click.X<805?1:click.X<945?2:click.X<1060?3:4;
        QueueRedraw();
    }
    private void Save(){OpenPrayer83();}
    private void Load()
    {
        if(!Godot.FileAccess.FileExists(SavePath)){Notify("Non esiste ancora un salvataggio C#.");return;}
        try
        {
            string json=Godot.FileAccess.GetFileAsString(SavePath);
            game=GameRules.FromJson(json);curseClock83=Math.Max(12f,65f-Math.Min(15,game.State.Prayers83)*4f);curseFlash83=0;ResetRoderick64();
            if(!maps.ContainsKey(game.State.Map))game.State.Map="station";
            ResetEventRules();
            hero=game.State.Hero;difficulty=game.State.Difficulty;
            screen=Screen.World;EnterMap(game.State.Map,game.State.TileX,game.State.TileY);Notify("Dùsterbedragna ti aspettava.");
        }
        catch(Exception ex){GD.PushError(ex.ToString());Notify("Il salvataggio non è compatibile o è danneggiato.");}
    }
}
