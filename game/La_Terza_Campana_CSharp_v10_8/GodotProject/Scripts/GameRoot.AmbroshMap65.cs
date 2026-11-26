using System;
using Godot;
public partial class GameRoot
{
    private bool ambDebug65;
    private float ambInspectTime65;
    private string ambInspectTitle65="",ambInspectText65="";
    private record AmbInspect65(Vector2I At,string Title,string Detail);
    private static readonly AmbInspect65[][] AmbObjects65={
        new AmbInspect65[]{new(new(8,8),"Biblioteca votiva","Volumi rilegati in pelle: genealogie, preghiere e nascite annotate con inchiostro dorato."),new(new(17,7),"Braciere rituale","Il profumo dell’incenso si è insinuato nelle pietre della casa."),new(new(21,8),"Reliquiario","Il vetro custodisce ex voto fallici, simboli di fecondità. Ambrosh distoglie lo sguardo.")},
        new AmbInspect65[]{new(new(9,10),"Letto e baule","Il baule è chiuso. Le coperte sono ripiegate con un ordine quasi ossessivo."),new(new(12,6),"Banco dei rimedi","Radici essiccate, ampolle, mortai e un quaderno di dosi. Ambrosh conosce ogni etichetta."),new(new(20,8),"Specchio votivo","La cornice è intagliata con tralci e simboli fallici. Ambrosh: «La mia lussuria non decide chi sono.»")},
        new AmbInspect65[]{new(new(9,9),"Vasca delle ninfee","L’acqua è ferma, eppure le foglie si muovono. Un profumo dolciastro sale dal fondo."),new(new(17,7),"Fiori della dea","I gigli hanno invaso perfino le fughe delle pietre."),new(new(18,10),"Radici nel selciato","Le radici assumono forme falliche nel delirio. Ambrosh chiude gli occhi; quando li riapre, sono soltanto radici.")}
    };
    private bool AmbroshBlocked65(Vector2I p)
    {
        if(p==OriginNpc60)return true;
        int x=p.X,y=p.Y;
        bool open=originChapter switch{
            0=> y==7&&x>=7&&x<=19 || y is 8 or 9&&x>=7&&x<=25 || y>=10&&y<=12&&x>=8&&x<=25,
            1=> y==6&&x>=10&&x<=16 || y==7&&x>=9&&x<=21 || y is 8 or 9&&x>=9&&x<=21 || y>=10&&y<=12&&x>=8&&x<=25,
            _=> y==7&&x>=9&&x<=19 || y>=8&&y<=10&&x>=9&&x<=18 || y==11&&x>=9&&x<=19 || y==12&&x>=9&&x<=25
        };
        return !open;
    }
    private void ResetAmbroshMap65(){ambInspectTime65=0;ambInspectTitle65="";ambInspectText65="";}
    private AmbInspect65? NearbyAmbroshObject65(){foreach(var item in AmbObjects65[originChapter])if(originPos==item.At)return item;return null;}
    private bool AmbroshInspect65(){if(originMode is not (1 or 4))return false;if(originMode==1&&originPos==OriginStand60||originMode==4&&OriginNear60(OriginExit60))return false;var obj=NearbyAmbroshObject65();if(obj==null)return false;ambInspectTitle65=obj.Title;ambInspectText65=obj.Detail;ambInspectTime65=5.5f;return true;}
    private void UpdateAmbroshMap65(float dt){ambInspectTime65=Math.Max(0,ambInspectTime65-dt);}
    private void DrawAmbroshInspect65(){if(ambInspectTime65>0){Panel(24,518,1232,125,.94f);Inscribe(ambInspectTitle65,51,557,22,InkGold);InscribedParagraph(ambInspectText65,51,588,1175,21,25,2,Parchment);}else if(NearbyAmbroshObject65() is { } obj){Rect(25,613,510,36,new Color(.04f,.03f,.025f,.92f));Inscribe("E — "+obj.Title,38,638,19,InkGold);}}
    private void DrawAmbroshCollision65(){for(int y=0;y<OH;y++)for(int x=0;x<OW;x++){var p=OriginCenter60(new(x,y));var r=new Rect2(p-new Vector2(OT/2f,OT/2f),new Vector2(OT,OT));if(AmbroshBlocked65(new(x,y)))DrawRect(r,new Color(.8f,.12f,.12f,.23f));else DrawRect(r,new Color(.4f,.8f,.5f,.27f),false,1);}}
    // Fine light motes, candle flicker, drifting pollen and water glints are layered on the painted map.
    private void DrawAmbroshAtmosphere65(){
        if(originChapter<2){Vector2[] lights=originChapter==0?new[]{new Vector2(280,218),new Vector2(485,167),new Vector2(840,150),new Vector2(1190,238)}:new[]{new Vector2(395,164),new Vector2(714,89),new Vector2(1230,237)};
            for(int i=0;i<lights.Length;i++){var p=lights[i];float a=.035f+.014f*Mathf.Sin(tick*8.9f+i*2.4f);for(int r=58;r>=12;r-=8)DrawCircle(p,r,new Color(.95f,.58f,.23f,a*.35f));}
            for(int i=0;i<18;i++){float x=OX+55+(i*143.7f+tick*(2+i%3))%(OW*OT-110),y=OY+60+(i*39.6f+Mathf.Sin(tick*.4f+i)*14)%(OH*OT-120);DrawCircle(new Vector2(x,y),.9f,new Color(.92f,.75f,.48f,.19f+.08f*Mathf.Sin(tick+i)));}}
        else {for(int i=0;i<35;i++){float x=OX+35+(i*97.4f+tick*(5+i%3))%(OW*OT-70),y=OY+80+(i*41.8f+tick*(2+i%4))%(OH*OT-145);DrawCircle(new Vector2(x,y),1+i%2*.45f,new Color(.87f,.80f,.65f,.20f+.09f*Mathf.Sin(tick+i)));}
            foreach(var p in new[]{new Vector2(225,447),new Vector2(1036,414),new Vector2(646,311)}){for(int i=0;i<3;i++){float r=3+(tick*9+i*13)%39;DrawArc(p,r,.1f,Mathf.Pi-.1f,24,new Color(.72f,.77f,.64f,.16f*(1-r/43)),1);}}}
    }
}
