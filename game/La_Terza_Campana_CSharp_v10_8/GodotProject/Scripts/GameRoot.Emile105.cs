using System;
using Godot;
public partial class GameRoot {
 private void DrawEmileScene105(int ch,Rect2 area){
  float t=emileTotal103;var tex=Art(ch==4?"emile107_funeral":"emile105_map_"+ch);if(tex==null)return;
  // Slow cinematic push-in, with the original illustrated actors integrated in the frame.
  float zoom=1+.022f*EaseEmile103(t/9);float sw=1232/zoom,sh=548/zoom;
  DrawTextureRectRegion(tex,ch==4?new Rect2(24,40,1232,444):area,new Rect2((1232-sw)/2,(548-sh)/2,sw,sh));
  if(ch==4){
   foreach(var q in new[]{new Vector2(408,131),new Vector2(552,124),new Vector2(690,128),new Vector2(855,164)}){float v=.035f+.018f*Mathf.Sin(t*4+q.X);for(int j=0;j<5;j++)DrawCircle(q,10+j*6,new Color(.97f,.54f,.24f,v/(j+1)));}
   for(int i=0;i<32;i++){float x=525+Mathf.PosMod(i*53.7f-t*3,250),y=63+Mathf.PosMod(i*41.1f+t*(39+i%4*9),130);DrawLine(new Vector2(x,y),new Vector2(x-2,y+8),new Color(.65f,.66f,.78f,.10f),1);}
  }else{
   for(int k=0;k<3;k++){float cx=442+k*222;float yy=157+Mathf.Sin(t*2.2f+k)*9;for(int i=0;i<15;i++){float xx=cx-83+Mathf.Sin(t*(2+i*.13f)+i*9+k)*14;float y=yy-24+i*4;float w=153+Mathf.Sin(t*4+i)*25;Rect(xx,y,w,3+i%3,new Color(.018f,.012f,.017f,.62f+(i%3)*.10f));}}
   for(int i=0;i<9;i++){float y=48+Mathf.PosMod(i*71.7f+t*14,410);float dx=Mathf.Sin(t*4+i*7)*14;var src=new Rect2(0,y-40,1232,2+i%3);DrawTextureRectRegion(tex,new Rect2(24+dx,y,1232-Mathf.Abs(dx),src.Size.Y),src,new Color(.74f,.65f,.79f,.42f));}
   float pulse=.035f+.025f*Mathf.Sin(t*1.8f);Rect(24,40,1232,444,new Color(.16f,.045f,.10f,pulse));
   for(int i=0;i<22;i++){float x=50+Mathf.PosMod(i*91.7f+t*(8+i%3),1170),y=72+Mathf.PosMod(i*53.2f-t*(12+i%4),394);DrawCircle(new Vector2(x,y),1+i%2,new Color(.75f,.46f,.5f,.16f));}
  }
 }
}
