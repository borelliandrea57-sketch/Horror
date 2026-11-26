using System;
using System.Collections.Generic;
using Godot;
public partial class GameRoot
{
    private sealed record ForegroundPiece(string Map,string Art,float X,float Y,float W,float H,float Depth);
    private static readonly ForegroundPiece[] Foregrounds =
    {
        new ForegroundPiece("station","fg91_panel_left",10.9318182f,15.3863636f,0.7272727f,1.8636364f,17.2272727f),
        new ForegroundPiece("station","fg91_basket_left",10.7954545f,17.1590909f,0.7500000f,1.1590909f,18.2954545f),
        new ForegroundPiece("station","fg91_barrel_left",11.4772727f,16.5681818f,1.0227273f,1.6818182f,18.2500000f),
        new ForegroundPiece("station","fg91_crate_high",13.3409091f,15.9772727f,1.8636364f,2.4545455f,18.3863636f),
        new ForegroundPiece("station","fg91_crate_low",12.7272727f,17.6590909f,1.7954545f,2.0227273f,19.6590909f),
        new ForegroundPiece("station","fg91_basket_mid",14.0454545f,18.3636364f,0.9318182f,1.4318182f,19.7727273f),
        new ForegroundPiece("station","fg91_barrel_mid",14.9772727f,17.7272727f,1.0909091f,1.8181818f,19.5454545f),
        new ForegroundPiece("station","fg91_panel_right",16.2954545f,17.0227273f,0.7500000f,1.5681818f,18.5681818f),
        new ForegroundPiece("station","fg91_basket_right",16.2500000f,18.6363636f,0.6590909f,0.9545455f,19.5681818f),
        new ForegroundPiece("station","fg91_barrel_right",16.9318182f,18.0000000f,1.0909091f,1.5227273f,19.5454545f),
        new ForegroundPiece("piazza","fgclean_piazza_barrel",33.90909f,25.27273f,2.52273f,3.81818f,29.06000f),
    };
    private void AddPaintedForeground(List<(float depth,Action draw)> items,string mapId)
    {
        foreach(var f in Foregrounds)
        {
            if(f.Map!=mapId)continue;
            float x=f.X*T-camera.X,y=f.Y*T-camera.Y,w=f.W*T,h=f.H*T;
            if(x>W||y>H||x+w<0||y+h<0)continue;
            var texture=Art(f.Art);if(texture==null)continue;
            items.Add((f.Depth,()=>DrawTextureRect(texture,new Rect2(x,y,w,h),false)));
        }
    }
}
