using System;
using System.Collections.Generic;
using Godot;
public partial class GameRoot
{
    private AudioStreamPlayer? piano79, ambience79, spot79, steps79;
    private readonly Dictionary<string,AudioStreamWav> audio79=new();
    private string ambientKey79="";
    private float spotTimer79=3f;
    private int stepIndex79;
    private readonly Random audioRng79=new();
    private static readonly string[] audioNames79={"music_piano79","amb79_street","amb79_rail","amb79_house","amb79_chapel","amb79_bell","amb79_metal","amb79_creak","amb79_drip","amb79_wind","amb79_step_stone0","amb79_step_stone1","amb79_step_wood0","amb79_step_wood1","amb79_step_gravel0","amb79_step_gravel1"};
    private void InitAudio79()
    {
        foreach(string key in audioNames79)
        {
            string external=System.IO.Path.Combine(OS.GetExecutablePath().GetBaseDir(),"v8_assets",key+".wav");
            AudioStreamWav? wav=System.IO.File.Exists(external)?AudioStreamWav.LoadFromFile(external):ResourceLoader.Exists("res://Assets/"+key+".wav")?GD.Load<AudioStreamWav>("res://Assets/"+key+".wav"):null;
            if(wav==null)continue;
            if(key=="music_piano79"||key is "amb79_street" or "amb79_rail" or "amb79_house" or "amb79_chapel")
            {wav.LoopBegin=0;wav.LoopEnd=(int)Math.Round(wav.GetLength()*wav.MixRate);wav.LoopMode=AudioStreamWav.LoopModeEnum.Forward;}
            audio79[key]=wav;
        }
        piano79=new AudioStreamPlayer{VolumeDb=-16};AddChild(piano79);
        ambience79=new AudioStreamPlayer{VolumeDb=-24};AddChild(ambience79);
        spot79=new AudioStreamPlayer{VolumeDb=-20};AddChild(spot79);
        steps79=new AudioStreamPlayer{VolumeDb=-19};AddChild(steps79);
        if(audio79.TryGetValue("music_piano79",out var piano)){piano79.Stream=piano;piano79.Play();}
    }
    private string AmbientKey79()=>screen switch
    {
        Screen.Title or Screen.Hero or Screen.Difficulty or Screen.Dead or Screen.Ending=>"",
        Screen.Origin=>"house", Screen.Prologue=>"rail",
        _=>game.State.Map switch {"piazza" or "strada" or "strada_bassa" or "vicolo"=>"street","binari" or "galleria" or "station" or "sala"=>"rail","cappella"=>"chapel",_=>"house"}
    };
    private void UpdateAudio79(float dt)
    {
        if(piano79==null||ambience79==null||spot79==null||steps79==null)return;
        bool focused=DisplayServer.WindowIsFocused();
        piano79.StreamPaused=!focused;
        ambience79.StreamPaused=spot79.StreamPaused=steps79.StreamPaused=!focused||screen is Screen.Pause or Screen.Prayer;
        float target=screen switch{Screen.Combat=>-21,Screen.Cutscene=>-24,Screen.Prologue or Screen.Origin=>-19,Screen.Inventory or Screen.Pause or Screen.Prayer=>-23,Screen.Dead=>-25,_=>-15};
        if(screen==Screen.Origin&&EmileOrigin103)target=emileScene103==4&&emileBeat103>=4?-34:emileScene103==5?-25:-24;
        if(clinicAttack96)target=clinicClock96<2.0f?-40:-33;
        piano79.VolumeDb=Mathf.MoveToward(piano79.VolumeDb,target,dt*(clinicAttack96?32:8));
        string key=AmbientKey79();
        if(key!=ambientKey79)
        {
            ambience79.Stop();spot79.Stop();ambientKey79=key;spotTimer79=3f+(float)audioRng79.NextDouble()*3;
            if(audio79.TryGetValue("amb79_"+key,out var stream)){ambience79.Stream=stream;ambience79.VolumeDb=-36;ambience79.Play();}
        }
        ambience79.VolumeDb=Mathf.MoveToward(ambience79.VolumeDb,clinicAttack96?-34:screen==Screen.Combat||screen==Screen.Cutscene?-31:-24,dt*10);
        if(!focused||screen!=Screen.World)return;
        spotTimer79-=dt;if(spotTimer79>0)return;
        spotTimer79=7f+(float)audioRng79.NextDouble()*12f;
        string[] pool=key switch{"street"=>new[]{"wind","bell"},"rail"=>new[]{"metal","drip","bell"},"chapel"=>new[]{"bell","creak"},_=>new[]{"creak","drip","metal"}};
        if(audio79.TryGetValue("amb79_"+pool[audioRng79.Next(pool.Length)],out var effect))
        {spot79.Stream=effect;spot79.VolumeDb=-19-(float)audioRng79.NextDouble()*5;spot79.PitchScale=.94f+(float)audioRng79.NextDouble()*.12f;spot79.Play();}
    }
    private void StepAudio79()
    {
        if(steps79==null||screen!=Screen.World)return;
        string material=game.State.Map switch{"binari" or "galleria"=>"gravel","casa_olmo" or "casa_olmo_servizio" or "casa_sarto" or "rifugio_basso" or "magazzino_basso"=>"wood",_=>"stone"};
        if(audio79.TryGetValue("amb79_step_"+material+(stepIndex79++%2),out var step)){steps79.Stream=step;steps79.PitchScale=.94f+(float)audioRng79.NextDouble()*.13f;steps79.Play();}
    }
    private void CheckAudio79()
    {
        int n=0;void C(bool ok,string name){if(!ok)throw new Exception(name);n++;}
        foreach(string key in audioNames79){C(audio79.ContainsKey(key),"missing "+key);C(audio79[key].GetLength()>0,"duration "+key);}
        C(audio79["music_piano79"].GetLength()>75,"music length");C(audio79["music_piano79"].LoopMode==AudioStreamWav.LoopModeEnum.Forward,"music loop");
        screen=Screen.World;foreach(var (map,key) in new[]{("strada","street"),("binari","rail"),("casa_olmo","house"),("cappella","chapel")}){EnterMap(map,4,4);C(AmbientKey79()==key,"zone "+map);UpdateAudio79(.1f);C(ambientKey79==key,"switch "+map);}
        GD.Print($"AUDIO79_OK {n}");GetTree().Quit();
    }
}
