using Godot;
public partial class GameRoot
{
    private static readonly int[] NyarFrames76={0,1,2,3,2,1};
    private int NyarFrame76(float time)=>NyarFrames76[((int)(time*5.6f)%6+6)%6];
    private void DrawNyar76(Rect2 area,float opacity)
    {
        float phase=tick*2.1f;
        float scale=1f+.024f*Mathf.Sin(phase);
        var center=area.GetCenter()+new Vector2(Mathf.Sin(phase*.63f)*2.5f,Mathf.Sin(phase)*4f);
        var size=area.Size*scale;
        var dest=new Rect2(center-size*.5f,size);
        // A localized shadow anchors the hovering entity in the scene.
        DrawEllipse(center.X,area.End.Y-2,area.Size.X*.28f,5,new Color(.025f,.012f,.04f,opacity*.23f));
        var tex=Art("nyar76_"+NyarFrame76(tick))??Art("nyar_familiar64");
        if(tex!=null)DrawTextureRect(tex,dest,false,new Color(1,.96f,1,opacity));
        for(int i=0;i<5;i++)
        {
            float angle=tick*.48f+i*Mathf.Tau/5f;
            var p=center+new Vector2(Mathf.Cos(angle)*area.Size.X*.4f,Mathf.Sin(angle*1.2f)*area.Size.Y*.39f);
            DrawCircle(p,1.1f,new Color(.56f,.37f,.70f,opacity*(.12f+.08f*Mathf.Sin(tick*2+i))));
        }
    }
    private void CheckNyar76()
    {
        for(int i=0;i<4;i++)if(Art("nyar76_"+i)==null)throw new System.Exception("Missing entity frame "+i);
        for(int i=0;i<6;i++)if(NyarFrame76((i+.2f)/5.6f)!=NyarFrames76[i])throw new System.Exception("Frame sequence");
        GD.Print("NYAR76_OK 10");GetTree().Quit();
    }
}
