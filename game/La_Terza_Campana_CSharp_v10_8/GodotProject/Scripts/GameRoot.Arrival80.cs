using System;
using Godot;
using LaTerzaCampana.Core;
public partial class GameRoot {
    private int arrivalBeat80=-1;
    private void BeginArrival80(){if(game.State.StoryFlags.Contains("arrival80_done"))return;arrivalBeat80=0;messageTimer=0;walking=false;ShowArrival80();}
    private void ShowArrival80(){string h=Catalog.Heroes[game.State.Hero].Name;string portrait="portrait_"+game.State.Hero.ToString().ToLowerInvariant();switch(arrivalBeat80){
        case 0:Say("NARRATORE","Il rumore del treno è scomparso. Ti ritrovi nella piazza di Dùsterbedragna, senza ricordare di essere sceso. L'aria è immobile.");break;
        case 1:Say(h,"Come sono arrivato qui? Un attimo fa ero sul treno. Quel portone sembra chiuso da anni…",portrait);break;
        case 2:Say("IL CAPOSTAZIONE","Hai detto che eri sul treno? Non ne so nulla. Non vedo arrivare un convoglio da così tanto tempo che ho smesso di contarli.","portrait_stationmaster");break;
        case 3:Say(h,"Allora come ho fatto a uscire dalla stazione? Non ricordo di aver attraversato quella porta.",portrait);break;
        case 4:Say("IL CAPOSTAZIONE","Non saprei dirtelo. Ti ho visto nella piazza, tutto qui. Non ti ho visto scendere da nessun treno.","portrait_stationmaster");break;
        case 5:Say("NARRATORE","Ti volti verso la stazione. Il portone è sprangato; la ruggine ha saldato gli anelli del battente. Non riesci a spiegarti come tu possa essere qui.");break;
    }}
    private void AdvanceArrival80(){if(arrivalBeat80<0)return;if(++arrivalBeat80>=6){arrivalBeat80=-1;game.State.StoryFlags.Add("arrival80_done");CloseDialogue();}else ShowArrival80();QueueRedraw();}
    private void CheckArrival80(){int n=0;void C(bool b,string m){if(!b)throw new Exception(m);n++;}foreach(var h in Enum.GetValues<HeroId>()){hero=h;BeginNew60();screen=Screen.Prologue;proStep=7;PrologueAdvance(true);C(screen==Screen.Dialogue&&arrivalBeat80==0,"mandatory arrival");var pos=visibleHero;for(int i=0;i<6;i++){C(screen==Screen.Dialogue&&arrivalBeat80==i&&visibleHero==pos,"locked dialogue");AdvanceArrival80();}C(screen==Screen.World&&arrivalBeat80==-1&&game.State.StoryFlags.Contains("arrival80_done"),"unlocks");C(GameRules.FromJson(game.ToJson()).State.StoryFlags.Contains("arrival80_done"),"save flag");}GD.Print($"ARRIVAL80_OK {n}");GetTree().Quit();}
}
