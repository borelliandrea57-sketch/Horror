using System;
using Godot;
using LaTerzaCampana.Core;
public partial class GameRoot {
 private int emileScene103,emileBeat103,emileStage103,emileChoice103;
 private float emileClock103,emileTotal103;
 private bool EmileOrigin103=>game.State.Hero==HeroId.Anatema;
 private AnatemaOrigin103.Scene EmileStory103=>AnatemaOrigin103.Scenes[emileScene103];
 private void BeginEmile103(){game.State.StoryFlags.Add("anatema_origin_started");LoadEmile103(0);}
 private void LoadEmile103(int ch){emileScene103=ch;emileBeat103=0;emileStage103=0;emileChoice103=0;emileClock103=0;emileTotal103=0;ResetFlash66();screen=Screen.Origin;EmileStart104();PlaySound("event_bell",-24);}
 private void FinishEmile103(bool complete){EmileStop104();if(complete)game.State.StoryFlags.Add("anatema_origin_done");screen=Screen.Prologue;proStep=0;}
 private void EmilePhase103(int stage){emileStage103=stage;emileClock103=0;if(stage==1)EmileCue104();}
 private void UpdateEmile103(float dt){emileClock103+=dt;emileTotal103+=dt;EmileUpdate104(dt);flashClock66+=dt;if(emileStage103==0&&emileClock103>=2.5f)EmilePhase103(1);else if(emileStage103==3&&emileClock103>=4.2f)EmilePhase103(4);else if(emileStage103==4&&emileClock103>=3.8f){if(emileScene103<5)LoadEmile103(emileScene103+1);else FinishEmile103(true);}}
 private void AdvanceEmile103(){if(emileStage103==0){EmilePhase103(1);return;}if(emileStage103!=1)return;emileBeat103++;if(emileBeat103>=EmileStory103.Beats.Length){emileBeat103=EmileStory103.Beats.Length-1;EmilePhase103(2);}else{emileClock103=0;EmileCue104();if(EmileStory103.Beats[emileBeat103].Speaker is "BRAKA" or "NYARLATOTH" or "NUWDANÌE")PlaySound("mind_whisper",-16);if(emileScene103==4&&emileBeat103==4)PlaySound("event_drone",-9);}}
 private void ChooseEmile103(int i){if(emileStage103!=2||!AnatemaOrigin103.Apply(game,emileScene103,i))return;emileChoice103=i;EmilePhase103(3);PlaySound(emileScene103==2||emileScene103==4?"ritual_hum":"door_open",-15);}
 private void EmileKey103(int n,Key key,bool ok){if(emileStage103==2){int len=EmileStory103.Options.Length;if(key is Key.Left or Key.Up or Key.A or Key.W)emileChoice103=(emileChoice103+len-1)%len;else if(key is Key.Right or Key.Down or Key.D or Key.S)emileChoice103=(emileChoice103+1)%len;else if(n>=1&&n<=len)ChooseEmile103(n-1);else if(ok)ChooseEmile103(emileChoice103);}else if(ok)AdvanceEmile103();}
 private Rect2 EmileChoiceRect103(int i)=>new(44+(i%2)*600,590+(i/2)*48,584,44);
 private void EmileMouse103(Vector2 p){if(emileStage103==2){for(int i=0;i<EmileStory103.Options.Length;i++)if(EmileChoiceRect103(i).HasPoint(p)){ChooseEmile103(i);return;}}else AdvanceEmile103();}
 private static float EaseEmile103(float x){x=Mathf.Clamp(x,0,1);return x*x*(3-2*x);}
 private void DrawEmile103(){
  Rect(0,0,W,H,new Color(.025f,.019f,.016f));int ch=emileScene103;float t=emileClock103;var scene=EmileStory103;
  var area=new Rect2(24,40,1232,548);if(ch>=4)DrawEmileScene105(ch,area);else Image("emile104_map_"+ch,area);
  // The memories are staged rooms, not navigable world maps. All movement stays on their foreground floor.
  Vector2[] from={new(235,469),new(505,468),new(620,468),new(390,471),new(285,467),new(640,476)};
  Vector2[] to={new(385,429),new(690,432),new(640,385),new(535,431),new(585,427),new(640,405)};
  float approach=emileStage103==0?EaseEmile103(t/2.3f):1;Vector2 p=from[ch].Lerp(to[ch],approach);bool walk=emileStage103==0&&t<2.3f;int facing=walk?(ch==2||ch==5?3:2):3;float tilt=0;
  if(emileStage103==3){float a=Mathf.Sin(Mathf.Clamp(t/2.7f,0,1)*Mathf.Pi);p+=new Vector2((emileChoice103==0?-14:19)*a,4*a);tilt=-.12f*a;}
  if(ch==0){DrawNpc61("mother",new Vector2(472,407),emileBeat103==2||emileStage103==3&&emileChoice103==0?2:1,(int)(tick*3)%8,70,97);DrawNpc61("neri",new Vector2(855,418),1,(int)(tick*3)%8,74,105);}
  if(ch==1){
   // The mother's dialogue remains; the foreground mother sprite is omitted from this memory.
   float appear=emileStage103==0?EaseEmile103(t/2):1f;
   float presence=emileBeat103==3||emileBeat103==4?.46f:.93f;
   float sway=Mathf.Sin(emileTotal103*1.1f)*5,bob=Mathf.Sin(emileTotal103*1.65f)*6;
   var spirit=Art("emile107_nuwdanie");if(spirit!=null){float h=252+Mathf.Sin(emileTotal103*1.3f)*3,w=h*spirit.GetWidth()/spirit.GetHeight();float x=emileBeat103>=3?778:875;
    var center=new Vector2(x+sway,360+bob);float opacity=appear*presence*(.94f+.06f*Mathf.Sin(emileTotal103*2));
    for(int k=5;k>=1;k--)DrawCircle(center-new Vector2(0,105),24+k*13,new Color(.89f,.83f,.72f,opacity*.014f));
    DrawTextureRect(spirit,new Rect2(center.X-w/2,center.Y-h,w,h),false,new Color(1,.98f,.94f,opacity));
    for(int k=0;k<18;k++){float px=center.X+Mathf.Sin(k*2.4f+emileTotal103*.6f)*(30+k%5*10);float py=center.Y-Mathf.PosMod(k*37f+emileTotal103*18,240);DrawCircle(new Vector2(px,py),1+k%2*.45f,new Color(.95f,.91f,.8f,opacity*.25f));}
   }
  }
  if(ch==2){float a=.10f+.05f*Mathf.Sin(emileTotal103*2);for(int j=0;j<4;j++){float scale=.48f-j*.085f;float ww=430*scale,hh=400*scale;Image("emile104_map_2",new Rect2(640-ww/2,238-hh/2,ww,hh),a);}}
  if(ch==3){DrawNpc61("neri",new Vector2(688,415),1,(int)(tick*4)%8,76,105);if(emileBeat103>=3){var bread=p.Lerp(new Vector2(683,377),1-EaseEmile103(t/2));Image("prop_item_pane",new Rect2(bread.X-11,bread.Y-38,22,16));}}
  if(ch==0&&emileBeat103>=2&&emileStage103==1){p+=new Vector2(-6*Mathf.Sin(t*3),2*Mathf.Sin(t*9));tilt=-.09f;}
  float height=ch<2?65:ch<5?83:99;float width=height*44/90;int frame=walk?(int)(tick*10)%8:0;
  if(ch<4)OriginSprite60("anim_anatema_00",p,facing,frame,44,90,width,height,tilt);
  if(ch==2&&emileBeat103>=2){float a=.12f+.10f*Mathf.Sin(emileTotal103*1.8f);var tex=Art("anim_anatema_00");if(tex!=null)DrawTextureRectRegion(tex,new Rect2(630,220,36,74),new Rect2(0,270,44,90),new Color(.65f,.52f,.77f,a));}
  for(int i=0;i<40;i++){float x=48+Mathf.PosMod(i*107.3f+emileTotal103*(3+i%4),1180),y=60+Mathf.PosMod(i*67.1f-emileTotal103*(7+i%3),450);DrawCircle(new Vector2(x,y),.6f+i%3*.4f,new Color(.85f,.66f,.43f,ch==4?.22f:.12f));}
  if(ch==5&&emileStage103==3){float dim=EaseEmile103(t/3);Rect(24,40,1232,548,new Color(.02f,.01f,.01f,dim*.58f));}
  DrawEmileAtmos104(ch);DrawFlashVignette66();
  Rect(24,0,1232,43,new Color(.045f,.031f,.025f,.97f));Inscribe(scene.Title,42,31,25,Parchment);Inscribe(scene.Era+"  •  "+(ch+1)+" / 6",850,30,18,InkGold);
  Panel(24,484,1232,224,.97f);
  string who,body;
  if(emileStage103==0){who="IL RICORDO";body=scene.Era+" — "+scene.Title;}
  else if(emileStage103==1){var b=scene.Beats[emileBeat103];who=b.Speaker;body=b.Text;}
  else if(emileStage103==2){who="EMILÈ";body="Che cosa fai?";}
  else {who="IL RICORDO RESTA";body=scene.Options[emileChoice103].Result;}
  Inscribe(who,48,521,23,InkGold);InscribedParagraph(body,48,551,1170,22,29,3,Parchment);
  if(emileStage103==2){for(int i=0;i<scene.Options.Length;i++){var r=EmileChoiceRect103(i);Panel(r.Position.X,r.Position.Y,r.Size.X,r.Size.Y,.96f);if(i==emileChoice103)Frame(r.Position.X,r.Position.Y,r.Size.X,r.Size.Y,InkGold,2.5f);Inscribe($"[{i+1}] {scene.Options[i].Label}",r.Position.X+14,r.Position.Y+29,20,InkGold);}}
  else if(emileStage103==1)Inscribe("E / INVIO — CONTINUA",925,686,18,InkGold);else if(emileStage103==3)Inscribe("IL RICORDO PROSEGUE…",920,686,18,InkGold);
  if(emileStage103==4){float fade=EaseEmile103(t/1.3f);Rect(0,0,W,H,new Color(.008f,.006f,.005f,fade));if(t>1.3f)InscribedParagraph(scene.Bridge,250,319,780,28,36,4,Parchment);}
 }
 private void CheckEmile103(){int n=0;void C(bool x,string m){if(!x)throw new Exception(m);n++;}hero=HeroId.Anatema;BeginNew60();C(screen==Screen.Origin,"start origin");foreach(string id in new[]{"sob","baby","wail","breath","mother","mother2","rain","choir","funeralbell"}){C(EmileAudio104(id)?.GetLength()>0,"audio "+id);}for(int i=0;i<6;i++){C(emileScene103==i,"auto sequence");C(Art(i==4?"emile107_funeral":(i>=4?"emile105_map_":"emile104_map_")+i)!=null,"art");if(i==1)C(Art("emile107_nuwdanie")!=null,"new goddess");if(i==4)C(emileBell106?.Playing==true,"funeral bells started");UpdateEmile103(2.6f);C(emileStage103==1,"script intro");for(int j=0;j<EmileStory103.Beats.Length;j++)AdvanceEmile103();C(emileStage103==2,"choice");ChooseEmile103(1);C(emileStage103==3,"reaction");C(!AnatemaOrigin103.Apply(game,i,0),"no duplicate");UpdateEmile103(4.3f);C(emileStage103==4,"fade");UpdateEmile103(3.9f);}C(screen==Screen.Prologue&&proStep==0,"train");C(game.State.StoryFlags.Contains("anatema_origin_done"),"complete");var copy=GameRules.FromJson(game.ToJson());C(copy.State.StoryFlags.Contains("anatema103_indesiderato"),"persist trait");C(CombatStats100.Effective(copy).ArcaneDefense>=27,"trait stats");hero=HeroId.Anatema;BeginNew60();SkipIntro82();C(screen==Screen.Prologue&&!game.State.StoryFlags.Contains("anatema_origin_done"),"skip");C(emileVoice104?.Playing==false&&emileBed104?.Playing==false&&emileBell106?.Playing==false,"audio stopped on skip");foreach(var i in System.Linq.Enumerable.Range(0,6))for(int j=0;j<AnatemaOrigin103.Scenes[i].Options.Length;j++){var g=new GameRules(HeroId.Anatema,Difficulty.Sopravvivenza);C(AnatemaOrigin103.Apply(g,i,j),"all options");C(!AnatemaOrigin103.Apply(g,i,j),"once");}GD.Print($"EMILE103_OK {n}");GetTree().Quit();}
}
