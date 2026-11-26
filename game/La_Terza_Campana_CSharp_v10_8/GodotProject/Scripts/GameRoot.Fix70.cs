using System;
using Godot;
using LaTerzaCampana.Core;
public partial class GameRoot {
 private void CheckFix70(){int checks=0;void C(bool ok,string msg){if(!ok)throw new Exception(msg);checks++;}void Press(Key k){_UnhandledKeyInput(new InputEventKey{Pressed=true,Keycode=k});}
 foreach(HeroId h in new[]{HeroId.Gladia,HeroId.Ambrosh,HeroId.Roderick}){hero=h;BeginNew60();for(int ch=0;ch<3;ch++){C(originChapter==ch,"chapter");Press(Key.E);C(originMode==1,"E opens intro");originPos=OriginStand60+Vector2I.Down;originVisual=OriginCenter60(originPos);Press(Key.E);C(originMode==6,"E near target");UpdateOrigin60(1);C(originMode==7,"approach");Press(Key.E);Press(Key.E);C(originMode==2,"E dialogue");Press(Key.Right);Press(Key.E);C(originMode==3&&originChoice67==1,"E selected choice");UpdateOrigin60(6.3f);C(originMode==11,"reaction");UpdateOrigin60(6);if(originMode==8){Press(Key.E);C(originMode==9,"E secondary");UpdateOrigin60(6.3f);}C(originMode==10,"automatic fade");UpdateOrigin60(2.7f);}C(originMode==5,"ending");UpdateOrigin60(6.6f);C(screen==Screen.Prologue,"train without exit");}GD.Print($"FIX70_OK {checks}");GetTree().Quit();}
}