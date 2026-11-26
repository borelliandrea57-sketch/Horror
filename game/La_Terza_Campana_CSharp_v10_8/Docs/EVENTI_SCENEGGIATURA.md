# La Terza Campana — Pentesilea e il Dio dell'Attesa

**Stato:** pacchetto di preproduzione pronto per l'integrazione in Godot C#. **Non è ancora una build del gioco.**
**Riferimento:** Godot C# v5.5, area esistente `casa_olmo` (*Casa dell'Olmo*) nella *Via del Distretto Basso*.

## A — Incontro obbligatorio con Pentesilea

### Attivazione e persistenza

- Primo ingresso in `casa_olmo` durante una partita in cui non compare `pentesilea_met` nei `StoryFlags`.
- **12 cambi effettivi di casella all'interno dell'abitazione**, non 12 pressioni di un tasto. I passi contro il muro non valgono. Il conteggio riparte da zero se si esce prima dell'evento.
- Alla dodicesima casella: blocca i controlli, non avanzare Mente/Fame o il tempo di immobilità; inizia la scena.
- La scena non può partire durante un dialogo, un combattimento o un inventario. Se il dodicesimo passo è raggiunto nel momento di un cambio di schermata, ritardarla fino al ritorno al mondo.
- Registra `pentesilea_met` soltanto al termine della conversazione iniziale. All'uscita e al rientro non ripetere rumori e spari.

### Sequenza tecnica e regia (durata indicativa 16–22 secondi, escluso dialogo)

| Momento | Inquadratura e animazione | Audio | Effetto sul gioco |
|---|---|---|---|
| 00:00 | Player arrestato a metà corridoio; le lampade tremano | Tre colpi lontani, poi silenzio | Controlli bloccati |
| 00:02 | Una porta interna vibra e polvere cade dallo stipite | Urlo rauco, mobili che cadono | Camera avanza di pochi pixel |
| 00:03–00:07 | 3 raffiche in successione, lampi da una porta socchiusa; ombre di creature cadono | Raffiche, bossoli a terra, gemiti bruscamente interrotti | Breve shake camera a ogni raffica |
| 00:08 | Fumo e particelle in controluce | Ultimo sparo isolato, ottone sul pavimento | La musica si interrompe |
| 00:10 | Pentesilea entra da destra trascinando un'arma smisurata; i capelli seguono il movimento | Passi pesanti, metallo che striscia | Mostra sprite animato |
| 00:12 | Si volta verso il protagonista e solleva lentamente la canna | Sbuffo del meccanismo, tessuto | Passa alla conversazione |

**Contenuto in scena:** tre sagome di non-morti in lontananza, non nemici persistenti. Evitare di generare entità che restino attive dopo la scena.

### Dialogo integrale

**Pentesilea:** «Un altro?»

*(Sollevi istintivamente le mani. L'arma è più alta della ragazza che la regge. Fra i denti, quando sorride, luccica qualcosa di metallico.)*

**Protagonista:** «Sono vivo! Non sparare!»

**Pentesilea:** «Lo dicono tutti. Poi cominciano a mordere.»

*(Fa un passo avanti. Una cascata di capelli castani le striscia sulle assi sconnesse. Con la punta dello stivale gira il corpo di uno degli assalitori.)*

**Pentesilea:** «Niente occhi lucidi. Respiri. Ti tremano le mani. Va bene: per ora ti credo.»

**Protagonista:** «Chi sei?»

**Pentesilea:** «Pentesilea. Non chiedermi come ho fatto a entrare, perché sto ancora cercando di capirlo.»

**Protagonista:** «Che cosa erano quelle cose?»

**Pentesilea:** «Persone, qualche ora fa. Adesso sono la ragione per cui questo bestione pesa meno quando lo porto carico.»

**Protagonista:** «Hai... un proiettile tra i denti?»

**Pentesilea:** «Un ricordo. Non è stato un appuntamento piacevole.»

*(Da qualche parte nelle fondamenta risuona un colpo lento, come una campana soffocata.)*

**Pentesilea:** «Ascoltami bene. Le strade si muovono. Le porte mentono. E qualunque cosa tu faccia... non restare fermo a fissare il vuoto troppo a lungo.»

**Protagonista:** «Perché?»

**Pentesilea:** «Perché qui c'è qualcosa che nota chi aspetta.»

**Pentesilea:** «Adesso dimmi una cosa. Perché sei entrato proprio in questa casa?»

#### Tre risposte e conseguenze

**1. «Ho sentito gli spari. Volevo aiutare qualcuno.»**

Pentesilea: «Aiutare? Con tutta questa gente che pensa solo a nascondersi? ...Sei una rarità. Cerca di non fartela passare.»

- Aggiungi `pentesilea_met` e `pentesilea_trust_high`; fiducia `+2`.
- Al prossimo incontro: Pentesilea rivela un passaggio verso una stanza di servizio e può offrire una benda.

**2. «Cercavo provviste. Non sapevo ci fosse qualcuno.»**

Pentesilea: «Almeno non racconti favole. Qui perfino gli avanzi hanno imparato a mentire.»

- Aggiungi `pentesilea_met` e `pentesilea_trust_neutral`; fiducia `0`.
- Al prossimo incontro: dialogo neutrale; nessun aiuto iniziale.

**3. «Abbassa quell'arma. Non mi fido di te.»**

Pentesilea: «E io non mi fido delle persone che mi danno ordini mentre ho ancora il dito sul grilletto.»

*(Abbassa il mitra di un palmo, ma continua a seguire ogni movimento del protagonista.)*

- Aggiungi `pentesilea_met` e `pentesilea_trust_low`; fiducia `-2`.
- Al prossimo incontro: opzioni più fredde; nessuna benda. Non iniziare immediatamente un combattimento: la conoscenza iniziale deve essere **obbligatoria**.

**Chiusura comune:**

Pentesilea: «Non ti seguirò per adesso. Ho una porta da chiudere e troppi morti dalla parte sbagliata. Se mi rivedi, annunciati prima di entrare.»

*(Sistema il cinturone, poggia il mitra sulla spalla e sparisce nella stanza laterale. Il corridoio resta percorribile.)*

#### Brevi battute diverse per protagonista

- **Gladia:** «Sei ferita? Posso controllare.» — Pentesilea: «Non è tutto sangue mio. E quello che lo è può aspettare.»
- **Ambrosh:** «Queste creature mostrano una trasformazione impossibile.» — Pentesilea: «L'impossibile non regge molto quando gli spari.»
- **Roderick:** «Quella mitragliatrice pesa più di te.» — Pentesilea: «Eppure sono ancora io a portarla.»

Inserire **una sola** coppia di battute tra «Chi sei?» e «Che cosa erano quelle cose?».

### Incontri successivi

Riaprire il dialogo con Pentesilea, senza ripetere la sparatoria. Opzioni: **Parla**, **Chiedi dei non-morti**, **Chiedi delle porte**, **Congedati**. Lo stato di fiducia modifica il testo e l'eventuale aiuto, ma l'NPC non è reclutabile in questa prima implementazione.

## B — Il Dio dell'Attesa: meccanica anti-immobilità

### Regola esatta

L'evento scatta dopo **120,0 secondi di tempo di gioco attivo** trascorsi senza **alcun vero cambio di casella** del protagonista.

| Condizione | Il timer avanza? | Si azzera? |
|---|---|---|
| Personaggio fermo in esplorazione | Sì | No |
| Si tiene premuto un tasto contro un muro | Sì | No |
| Si cambia realmente casella | No nell'istante del passo | **Sì** |
| Inventario, Equipaggiamento, Abilità, Diario | **No** | No |
| Dialoghi o scelte | No | No |
| Combattimento | No | No |
| Cutscene, transizione, game over, menu Pausa | No | No |
| Editor delle collisioni | No | No |
| Finestra del gioco senza focus | No | No |
| Cambia mappa entrando in un'altra area | No nell'istante del cambio | **Sì** |
| Carica un salvataggio | No | **Sì**, per evitare morti immediate al caricamento |

**Nota importante:** aprire e chiudere l'inventario mette in *pausa*, **non azzera**, il timer. In questo modo l'interfaccia rispetta la pausa del mondo senza diventare un modo per ricominciare da zero.

### Escalation e morte speciale

**00:00–01:29 — L'Attesa.** Nessun avvertimento esplicito in HUD. Solo il rumore normale della stanza.

**01:30 — Qualcuno conta.** Un battito sordo e discontinuo. La polvere cambia direzione. Per una frazione di secondo, un'ombra compare *dietro* il personaggio. Nessun danno.

**01:50 — Il cielo si piega.** L'audio ambientale si abbassa; tre note dissonanti si ripetono. Compaiono sottili linee nere verso il centro dello schermo. La camera perde per un attimo l'allineamento, senza cambiare le hitbox.

**02:00 — Intervento del Dio dell'Attesa.** Il movimento si blocca. Nel silenzio, la luce scompare dal bordo della scena verso il protagonista. Una sagoma smisurata, impossibile da mettere tutta nell'inquadratura, cala come un'ombra di pietra e nervi. Lo sprite del personaggio si allunga contro il suo volere fino a diventare un segno d'inchiostro, poi viene cancellato. Un unico rintocco. **Game Over**.

Testo unico: **«SEI RIMASTO FERMO ABBASTANZA PERCHÉ LUI TI TROVASSE.»**

Sottotitolo: *«Il mondo non dimentica chi smette di attraversarlo.»*

Il Dio dell'Attesa **non è un nemico combattibile**, non dà bottino e non può comparire durante la cutscene di Pentesilea. Dopo la morte, continua dal precedente salvataggio. Nessun salvataggio automatico viene fatto nel momento fatale.

### Interazione con la Mente

Gli effetti sotto 50, 30 e 10 Mente continuano a esistere **in parallelo**, ma non accelerano né rallentano il timer. Sotto 10 Mente, la sagoma può apparire brevemente anche prima dei 90 secondi come *allucinazione puramente visiva*, ma quella visione non provoca la morte speciale. Il game over resta legato ai 120 secondi di vera immobilità.

### Accessibilità e chiarezza

Lo schermo non deve usare lampeggiamenti rapidi e ripetuti. Lo shake camera deve essere limitato e disattivabile. Per provare la meccanica in sviluppo, impostare una durata breve da debug senza modificare il valore reale di 120 secondi.

## C — Risorse e animazioni richieste

| Risorsa | Specifica | Utilizzo |
|---|---|---|
| Ritratto Pentesilea | 1 illustrazione, capelli castani fino al pavimento, adulta, giacca military-lolita e dettagli rossi, calze a rete, mitra smisurato e piccolo dettaglio metallico sul dente | Riquadro dialogo |
| Sprite Pentesilea | 4 direzioni × 4 pose (16 frame), PNG trasparente; allineamento fisso ai piedi | Esplorazione, capelli in movimento |
| Battle sprite Pentesilea | 8 pose: idle, respiro, sollevamento, mira, lampo, rinculo, fumo, riposo | Eventuali combattimenti futuri |
| Silhouette di non-morti | 3 sagome su 2–3 frame ciascuna | Cutscene soltanto |
| Fuoco di bocca, fumo, bossoli | 4–8 frame ciascuno, particelle separabili | Cutscene |
| Entità eldritchiana | 1 illustrazione gigante + 6–8 frame per ingresso, distorsione e cancellazione | Morte anti-idle |
| Player sotto l'entità | 4–6 pose di silhouette deformata, per ogni protagonista oppure effetto shader unico | Game Over |
| Audio | Raffiche brevi, tre gemiti non-morti, pioggia di bossoli, passo con arma, drone cosmico, rintocco finale | Ambienza e cutscene |

### Stato reale dei file inclusi

I tre PNG di **Pentesilea** inclusi nella cartella `Assets/` sono **materiale grafico preparatorio già creato**. Richiedono ancora verifica dei margini, suddivisione uniforme delle celle e rifinitura del canale alfa prima dell'uso diretto nell'atlante Godot. Le sagome dei non-morti, il Dio dell'Attesa e gli effetti audio non sono ancora stati realizzati in questo pacchetto.

## D — Requisiti di collaudo prima della build Windows

1. L'evento parte al dodicesimo **passo valido** in Casa dell'Olmo, una volta sola e non all'undicesimo.
2. Le tre scelte persistono nel salvataggio con l'esito corretto; un rientro non riproduce la sparatoria.
3. Pentesilea non cambia dimensione né compare sopra i muri se deve essere coperta dai mobili; corretto ordinamento verticale.
4. Il timer si interrompe durante l'inventario senza azzerarsi; un tasto premuto contro il muro non lo azzera.
5. Dopo un movimento reale, il timer torna a zero e la sequenza d'avvertimento si ripristina.
6. Il Game Over appare solo al secondo 120 di immobilità attiva, e non durante una cutscene o un menu.
7. Il salvataggio caricato non avvia una morte immediata.
8. Nessun errore C# e nessun oggetto della scena rimane sospeso dopo la sequenza.

