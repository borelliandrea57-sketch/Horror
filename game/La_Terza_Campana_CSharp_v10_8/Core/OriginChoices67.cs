namespace LaTerzaCampana.Core;
public static class OriginChoices67
{
    public sealed record Alternative(string Label,string Result,int Body=0,int Mind=0,int Hunger=0,string? Spend=null);
    public static Alternative[] Extra(HeroId hero,int chapter)=> (hero,chapter) switch {
        (HeroId.Gladia,0)=>[new("Dona tutto il pane","Lasci tutto il pane. La madre abbassa lo sguardo: non ha nulla da offrirti.\n−2 Pane (fino a esaurimento)",Spend:"all_pane"),new("Passa oltre senza parlare","Ti allontani. La ciotola rimane vuota.\n−5 Mente",Mind:-5)],
        (HeroId.Gladia,1)=>[new("Veglia accanto al letto","Rimani fino all’alba. La febbre non passa. Non impari alcun nuovo rito.\n−8 Corpo · −8 Fame",Body:-8,Hunger:-8)],
        (HeroId.Gladia,2)=>[new("Sfida Braka a viso aperto","Gridi contro la voce. Il silenzio che segue non è una vittoria.\n−14 Mente · −5 Corpo",Body:-5,Mind:-14),new("Resta immobile e in silenzio","Non rispondi. Il fuoco si spegne da solo. Nessuna ricompensa.")],
        (HeroId.Ambrosh,0)=>[new("Non rispondere al tutore","Lasci che il libro resti chiuso. Nessun dono, nessun giuramento.")],
        (HeroId.Ambrosh,1)=>[new("Sopporta la crisi senza rimedi","Aspetti che passi. Il riposo non arriva.\n−8 Mente · −5 Corpo",Body:-5,Mind:-8),new("Lascia la stanza","Non impari nessuna tecnica nuova.")],
        (HeroId.Ambrosh,2)=>[new("Ignora la voce della dea","Ti allontani dal giardino. Nessun nuovo dono.")],
        (HeroId.Roderick,0)=>[new("Brucia i tuoi appunti","Getti le carte nel fuoco. I ricordi restano.\n−5 Mente",Mind:-5)],
        (HeroId.Roderick,1)=>[new("Lascia libera la creatura","Apri il passaggio. La creatura scompare. Nessun patto."),new("Fuggi dal cortile","Le sbarre vibrano alle tue spalle.\n−7 Mente",Mind:-7)],
        (HeroId.Roderick,2)=>[new("Attraversa l’arco senza pregare","L’arco ti restituisce al mondo. Non ricevi alcun dono.")],
        _=>[]};
    public static bool Apply(GameRules g,int chapter,int index){if(chapter<0||chapter>2||index<0)return false;var s=g.State;string h=s.Hero.ToString().ToLowerInvariant();string key=$"{h}_origin_choice_{chapter}";
        if(index<2)return s.Hero switch {HeroId.Gladia=>GladiaOrigin.Apply(g,chapter,index==0),HeroId.Ambrosh=>AmbroshOrigin.Apply(g,chapter,index==0),_=>RoderickOrigin.Apply(g,chapter,index==0)};
        var extras=Extra(s.Hero,chapter);if(index-2>=extras.Length||!s.StoryFlags.Add(key))return false;var e=extras[index-2];s.StoryFlags.Add($"{key}_option_{index+1}");s.Body=Math.Clamp(s.Body+e.Body,1,100);s.Mind=Math.Clamp(s.Mind+e.Mind,0,100);s.Hunger=Math.Clamp(s.Hunger+e.Hunger,0,100);if(e.Spend=="all_pane")s.Inventory["pane"]=0;return true;}
}
