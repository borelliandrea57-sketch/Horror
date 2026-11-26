using System;
namespace LaTerzaCampana.Core;
public static class Prayer83
{
    public static string God(HeroId h)=>h switch{HeroId.Gladia=>"Braka",HeroId.Ambrosh=>"Nuwdanìe",_=>"Nyarlatoth"};
    public static string Apply(SaveData s){
        int n=s.Prayers83=Math.Min(1000000,Math.Max(0,s.Prayers83)+1),level=Math.Min(25,n);
        int body=s.Body,mind=s.Mind;double hunger=s.Hunger;
        string line;
        switch(s.Hero){
        case HeroId.Gladia:
            s.Body=Math.Max(1,s.Body-(2+level*3));if(n>=3)s.Bleeding=true;
            line=n==1?"Braka: Ti concedo di conservare questo istante. Lasciami un frammento del tuo corpo.":n==2?"Braka: Le mani che soccorrono impareranno a distruggere.":"Braka: Ogni tua supplica apre una crepa più profonda. Persino la pietà ha un prezzo.";break;
        case HeroId.Ambrosh:
            s.Hunger=Math.Max(0,s.Hunger-(4+level*4));s.Mind=Math.Max(0,s.Mind-(1+level*2));
            line=n==1?"Nuwdanìe: Nulla si conserva senza nutrire qualcos'altro.":n==2?"Nuwdanìe: Puoi contenere il tuo corpo, non ciò che cresce nel pensiero.":"Nuwdanìe: La tua volontà si assottiglia. Le radici reclamano nutrimento.";break;
        default:
            s.Mind=Math.Max(0,s.Mind-(4+level*4));
            line=n==1?"Nyarlatoth: Ricorderai questo istante. Non ti prometto che sarà quello giusto.":n==2?"Nyarlatoth: Ho conservato il ricordo. Quale dei tuoi occhi vuoi che lo osservi?":"Nyarlatoth: Ogni ricordo conservato ne deforma un altro. Non sai più quale mondo sia il tuo.";break;
        }
        s.StoryFlags.Add("prayer83_"+s.Hero.ToString().ToLowerInvariant());
        return line+"\n"+(body!=s.Body?$"Corpo −{body-s.Body}. ":"")+(mind!=s.Mind?$"Mente −{mind-s.Mind}. ":"")+(hunger!=s.Hunger?$"Fame −{hunger-s.Hunger:0.#}. ":"")+(s.Hero==HeroId.Gladia&&n>=3?"Sanguinamento. ":"")+"Il legame si stringe.";
    }
}
