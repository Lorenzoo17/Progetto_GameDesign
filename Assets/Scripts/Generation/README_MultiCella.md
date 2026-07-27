# Generazione del dungeon

## Modalità di layout

`DungeonGenerator > Layout Mode`:

### `FullMaze` (comportamento storico)

Maze DFS che riempie **tutte** le celle della griglia. `size` = numero esatto di stanze
(3x3 → 9 stanze) e la pianta è sempre un quadrato pieno.

### `Organic` (stile Binding of Isaac)

La griglia diventa solo il **contenitore massimo**: metti `size` abbondante (es. 9x9) e
decidi il numero di stanze con `Min Rooms` / `Max Rooms`. La pianta cresce a macchia dal
centro e viene fuori irregolare.

La regola che dà la forma tipica di TBOI: **una nuova stanza può toccare una sola stanza
già esistente**. Questo evita i blocchi compatti, mantiene la pianta ad albero (nessun
anello) e garantisce che ci siano sempre dei vicoli ciechi per boss / tesoro / negozio.

```
                B
        2   .     .
      . 2 S . 1 1 .
      X         X
```
`S` = start, `B` = boss, `X` = speciali, `1`/`2` = stanze multi-cella

| Parametro | Effetto |
|---|---|
| `Min Rooms` / `Max Rooms` | quante stanze ha il piano (si estrae a caso nell'intervallo) |
| `Branch Chance` | basso (0.3) = dungeon lungo e serpeggiante, alto (0.7) = compatto e ramificato |
| `Generation Attempts` | quante volte riprovare se non si raggiunge il minimo |

Il limite pratico è circa metà delle celle della griglia (per via della regola di adiacenza):
per 12–14 stanze serve almeno una 9x9. Se chiedi troppo, il generatore lo segnala e limita.

**Questa è anche la risposta al problema delle stanze multi-cella**: in `FullMaze` la griglia
è tutta occupata e piccola, quindi c'è poco margine; in `Organic` puoi tenere una griglia
grande con la pianta rada, e le forme 2x1 / a L trovano posto facilmente
(verificato su 4000 generazioni 9x9: in media 1.8 stanze multi-cella piazzate su 2 richieste,
0 solo nell'1.3% dei casi).

Altre due cose cambiate insieme al layout:

- la **boss room** è ora la dead-end più lontana **come numero di stanze da attraversare**
  (prima era la distanza dal centro della griglia, che su una pianta irregolare non
  significa niente);
- **powerup e vendor** vengono preferibilmente messi in vicoli ciechi, così non capitano
  in mezzo al percorso principale.

> Nota: con una griglia grande la minimappa disegna su un'area più ampia. Se serve,
> abbassa `Spacing` / `Room Size` in `DungeonMinimapUI`.

---

# Stanze multi-cella (2x1, a L, ...)

## Idea

Il maze DFS **non è cambiato**: continua a scavare una cella alla volta e a produrre
`status[4]` per ogni cella (0=Up, 1=Down, 2=Right, 3=Left).

Dopo il maze c'è una nuova **fase di merge**: alcune celle adiacenti *già collegate fra loro*
vengono fuse in un'unica stanza più grande. Le porte interne al gruppo vengono rimosse,
quelle esterne restano dove il maze le aveva messe.

```
MazeGenerator()                 -> labirinto identico a prima
  |
  +- riserva start / boss / powerup / vendor   (restano sempre 1x1)
  +- PlaceMultiCellRooms()      <-- NUOVO: piazza le forme sulle celle libere
  +- spawn stanze 1x1
  +- spawn stanze multi-cella
```

Con `requireInternalConnection = true` (default) una forma viene piazzata solo se le sue
celle erano già connesse dal maze: la struttura del labirinto resta identica, non si creano
scorciatoie. Disattivandolo si ottengono anelli e percorsi alternativi.

Verificato su 24.000 dungeon generati (griglie da 3x3 a 6x5): dungeon sempre connesso,
porte sempre simmetriche, porte interne sempre rimosse, boss room sempre dead-end.

## Convenzioni

| | |
|---|---|
| Direzioni | `0 = Up`, `1 = Down`, `2 = Right`, `3 = Left` |
| Griglia | `x` cresce verso destra, `y` cresce verso il **basso** |
| Offset cella (1,0) | posizione locale `(+offset.x, 0)` = `(+23.4, 0)` |
| Offset cella (0,1) | posizione locale `(0, -offset.y)` = `(0, -23.4)` |

Esempi di forma:

```
2x1 orizzontale : (0,0) (1,0)
2x1 verticale   : (0,0) (0,1)
a L             : (0,0) (1,0) (0,1)
```

La cella `(0,0)` è l'**ancora** ed è obbligatoria: corrisponde all'origine del prefab.

## Come creare una stanza multi-cella

**Non serve configurare niente nell'inspector: basta rispettare i nomi.**
`RoomBehaviour` ricava forma, muri, porte e camera bounds dalla gerarchia.

1. Duplica una stanza esistente per ogni cella e mettile come figli di un unico root,
   **chiamandole `Cell_<x>_<y>`** (`Cell_0_0`, `Cell_1_0`, `Cell_0_1`, ...) e posizionandole
   a multipli di `23.4` (vedi `Room_2x1` / `Room_L` come esempio).
   `Cell_0_0` è obbligatoria ed è l'ancora, in posizione locale `(0,0)`.
2. Dentro ogni cella devono esistere, come già in `Room_1`:
   `ClosedUp` / `ClosedDown` / `ClosedRight` / `ClosedLeft`,
   `DoorUp` / `DoorBottom` / `DoorRight` / `DoorLeft`, e opzionalmente `CameraBounds`.
3. Sul root: **un `RoomBehaviour`** e **un `BoxCollider2D` trigger per cella**
   (più eventuali collider "ponte" sui passaggi, per non avere buchi di copertura).
4. `roomBounds` = il `BoxCollider2D` che racchiude tutta la stanza (usato dalla camera).
5. Trascina il prefab in `DungeonGenerator > Multi Cell Rooms` (**non** in `Normal Rooms`).

La lista `cells` nell'inspector serve solo se vuoi **forzare** una configurazione diversa
da quella dedotta dai nomi: se è vuota, vince la gerarchia.

I lati che confinano con un'altra cella della stessa stanza vengono riconosciuti da soli:
niente muro, niente porta, passaggio libero.

## Camera

- Se una cella **non** ha `cameraBounds`, la camera usa `roomBounds` (tutta la stanza)
  e scorre liberamente: è quello che vuoi per una 2x1.
- Se una cella **ha** `cameraBounds`, quando il player è in quella cella la camera si
  clampa lì: serve per la stanza a L, altrimenti il bounding box inquadrerebbe
  l'angolo vuoto.

`Room_2x1` è configurata senza `cameraBounds` per cella, `Room_L` con.

## Parametri nell'inspector del DungeonGenerator

| Campo | Significato |
|---|---|
| `Multi Cell Rooms` | prefab delle stanze grandi. Se `Min/Max Position` sono entrambi `(0,0)` la regola è considerata non impostata e la stanza può uscire ovunque |
| `Shape Override` (per riga) | forza la forma senza leggerla dal prefab. Vuoto = forma dal prefab. È la via d'uscita se il prefab non viene letto: es. `(0,0)` e `(1,0)` per una 2x1 |
| `Max Multi Cell Rooms` | quante stanze grandi al massimo per dungeon |
| `Multi Cell Room Chance` | probabilità di provare un piazzamento su ogni cella candidata |
| `Require Internal Connection` | `true` = mantiene il labirinto perfetto, `false` = ammette scorciatoie |

## Prefab di prova generati

`Room_2x1.prefab` e `Room_L.prefab` sono composte clonando `Room_1` una volta per cella.
Sono **funzionanti così come sono** (i muri interni vengono disattivati a runtime e resta
il vano porta), ma sono pensate come base: in Unity apri il prefab e ritocca le tilemap
`Wall` / `Ground` / `Limit` sul giunto per far leggere le celle come un unico ambiente.

Ogni cella ha le sue tilemap sotto un proprio `Grid` (`Cell_0_0/BigRoom`, `Cell_1_0/BigRoom`, ...):
si disegnano esattamente come prima, la griglia resta allineata ai tile.
La `NavMeshSurface` è una sola per stanza (sotto `Cell_0_0/Navmesh`) e copre tutto.

## Se le stanze multi-cella non spawnano

Il generatore ora logga la fase di merge (`logMultiCellPlacement`, attivo di default).
In Play mode filtra la console per `[MultiCell]`:

| Messaggio | Causa |
|---|---|
| `'Room_2x1' NON ha una RoomBehaviour` | prefab importato male → Reimport, poi **Tools > Dungeon > Configura celle multi-cella** |
| `'Room_2x1' viene letta come stanza 1x1` | mancano i figli `Cell_<x>_<y>` sul root del prefab |
| `La lista 'Multi Cell Rooms' e' vuota` | prefab non assegnati nell'inspector |
| `maxMultiCellRooms = 0` / `multiCellRoomChance = 0` | i valori di default non sono stati applicati all'oggetto già esistente in scena: impostali a mano |
| `Stanze multi-cella piazzate: 0 / 2` | non c'era spazio: griglia troppo piccola o troppe celle riservate |

### Tool di editor

**Tools > Dungeon > Configura celle multi-cella** ricostruisce la lista `cells` leggendo la
gerarchia, ed è il modo affidabile per configurare una stanza (è Unity stessa a scrivere il dato).
Seleziona il prefab nel Project e lancialo. Convenzione richiesta:

- un figlio diretto per cella, chiamato `Cell_<x>_<y>` (es. `Cell_0_0`, `Cell_1_0`)
- dentro ogni cella: `ClosedUp` / `ClosedDown` / `ClosedRight` / `ClosedLeft`,
  `DoorUp` / `DoorBottom` / `DoorRight` / `DoorLeft`, e `CameraBounds` (opzionale)

Il tool assegna i `cameraBounds` per cella solo se la forma **non** è un rettangolo pieno
(quindi sì per la L, no per la 2x1).

**Tools > Dungeon > Log forma stanza selezionata** stampa quante celle vengono lette da un prefab.

## Compatibilità

Tutte le stanze 1x1 esistenti continuano a funzionare senza modifiche: se `cells` è vuoto,
`RoomBehaviour` usa il vecchio setup `blocks` / `doors`. Non è stato toccato nessun campo
serializzato esistente.
