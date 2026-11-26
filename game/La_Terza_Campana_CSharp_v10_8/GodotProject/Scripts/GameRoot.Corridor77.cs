using System;
using Godot;
using LaTerzaCampana.Core;
public partial class GameRoot
{
    // Only Roderick's first memory. The apparition is a rendering layer, not a traversable room.
    private float CorridorOpacity77()
    {
        if(!IsRoderick64||originChapter!=0)return 0;
        float Ease(float t){t=Mathf.Clamp(t,0,1);return t*t*(3-2*t);}
        return originMode switch {
            7 when beat69>=1 => Ease(originClock/1.25f),
            2 => 1f,
            3 => originChoice67==0?1f:1-Ease(originClock/2.1f),
            11 or 8 => originChoice67==0?.84f:0,
            9 => originChoice67==0&&followChoice69!=1?.84f*(1-Ease(originClock/3.5f)):originChoice67==0?.84f*(1-Ease(originClock/1.5f)):0,
            _=>0
        };
    }
    private void DrawImpossibleCorridor77()
    {
        float a=CorridorOpacity77();if(a<=.001f)return;
        var npc=OriginCenter60(OriginNpc60);
        float widen=originMode==3&&originChoice67==0?Mathf.Sin(Mathf.Clamp(originClock/2f,0,1)*Mathf.Pi)*.18f:0;
        float pulse=1+.012f*Mathf.Sin(tick*1.7f)+widen;
        float w=310*pulse,h=320*pulse;
        var rect=new Rect2(npc.X-w/2,npc.Y+22-h,w,h);
        Image("roderick_corridor77",rect,a);
        // Dim drifting motes around the opening; no hard flash or new collision.
        for(int i=0;i<15;i++){
            float x=npc.X+Mathf.Sin(i*4.17f+tick*.27f)*(w*.43f);
            float y=rect.Position.Y+35+Mathf.PosMod(i*39f-tick*(9+i%3),h-50);
            DrawCircle(new Vector2(x,y),.8f+i%3*.45f,new Color(.58f,.45f,.68f,a*.20f));
        }
    }
    private void CheckCorridor77(){int n=0;void C(bool b,string m){if(!b)throw new Exception(m);n++;}
        hero=HeroId.Roderick;BeginNew60();C(CorridorOpacity77()==0,"hidden at start");originMode=1;C(CorridorOpacity77()==0,"hidden exploration");originMode=7;beat69=0;C(CorridorOpacity77()==0,"hidden initial dialogue");beat69=1;originClock=.6f;C(CorridorOpacity77()>0&&CorridorOpacity77()<1,"fade in");originMode=2;C(CorridorOpacity77()==1,"visible during choice");
        foreach(int i in new[]{0,1,2}){originMode=3;originChoice67=i;originClock=2.3f;C(i==0?CorridorOpacity77()==1:CorridorOpacity77()==0,"choice response");}
        LoadOrigin60(1);originMode=2;C(CorridorOpacity77()==0,"not in next scene");hero=HeroId.Gladia;BeginNew60();originMode=2;C(CorridorOpacity77()==0,"not other hero");GD.Print($"CORRIDOR77_OK {n}");GetTree().Quit();
    }
}
