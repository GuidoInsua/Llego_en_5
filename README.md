# 🏃‍♂️ LLEGÓ EN 5’

### TP 2 - Desarrollo Consciente.

**Equipo:** Lucas, Rodrigo, Fatima y Guido  
**Motor:** Unity (2D)  
**Referente jugable:** Limbo / Inside[cite: 1, 2, 3, 4]

---

## 🏙️ Resumen del Concepto

¡Uy no, te quedaste dormido campeón! Dale que tenés que llegar al laburo cuanto antes.

Salí de un salto de la cama, y con una patada ninja atravesá la ventana. Atravesar la ciudad es un reto cotidiano, pero sabés que nada te va a enchufar: saltar las tapas de AySA, un bache en la esquina, esquivar porteros manguereando la vereda o patear la basura que dejaron los de la marcha de ayer.

Si llegás al subte B, tomate una pausa con el vendedor de medias; esta mercancía te puede dar unos power ups. Prestá atención al bajar en tu estación y metele pata. El tiempo corre y hay que llegar antes de que tu jefe "El Trompa" se enoje.

---

## 📌 ETAPA 0 - PREPRODUCCIÓN

### Géneros del Prototipo

- **Principal:** Runner 2D de scroll lateral con obstáculos.
- **Secundario:** Arcade de tiempo (reflejos y mejorar la propia marca).
- **Tono:** Comedia liviana, con las escenas narradas en historieta.

### Referente Elegido y Motivos

**Referente:** LIMBO / INSIDE (Playdead), categoría Puzzles.

**Motivos de la elección:**

1. Es un 2D de scroll lateral donde se avanza superando obstáculos, igual que nuestra idea.
2. Cuenta su historia sin diálogos: con pocas mecánicas logra mucha carga emocional, algo realista para un prototipo chico.
3. Usa pocas mecánicas (moverse, saltar, interactuar) y las explota con el diseño de niveles.
4. El ciclo muerte es inmediato y sin castigo grande, lo que sostiene el ritmo de un juego de obstáculos.
5. Muestra cómo la estética (atmósfera, tensión) surge de mecánicas simples.

_Inspiración adicional:_ Vector, runner de parkour 2D. Aporta la idea de correr y fluir por el escenario. No está en la lista de referentes, por eso el análisis se hace sobre Limbo / Inside.

---

### Análisis con MDA Framework

MDA separa un juego en tres capas: **Mecánicas** (las reglas), **Dinámicas** (lo que ocurre cuando el jugador las usa) y **Estética** (lo que el jugador siente). El diseñador va de M a D a A; el jugador vive el camino inverso.

#### Cuadro Comparativo MDA

| Mecánica                               | Dinámica                       | Emoción / Estética           |
| :------------------------------------- | :----------------------------- | :--------------------------- |
| Salto + movimiento                     | Timing y ritmo                 | Desafío, tensión             |
| Muerte + checkpoint                    | Prueba y error                 | Perseverancia                |
| Empujar objetos y activadores          | Resolver puzzles con la física | Descubrimiento, satisfacción |
| Sin texto ni HUD                       | Lectura del entorno            | Narrativa, curiosidad        |
| Fondo oscuro, niebla y sonido ambiente | Cautela, exploración visual    | Soledad, miedo               |
| Peligros y persecución                 | Tensión y alivio               | Miedo, alivio                |

#### Qué tomamos para nuestro juego

- Pocas mecánicas, bien pulidas.
- Historia contada por el escenario, sin texto largo.
- Reintento inmediato, sin castigo fuerte.
- Atmósfera marcada por luz, color y sonido.
- **Qué cambia:** nuestro tono es el humor y la urgencia (no la soledad ni el miedo), y el material sale de lo cotidiano.

---

## 📋 ETAPA 1 - PLANIFICACIÓN

### a. Experiencia a Diseñar

Una carrera contra el reloj por una mañana porteña. Matías se quedó dormido y tiene que llegar a la oficina: corre, salta y esquiva obstáculos cotidianos (obrero, portero, escaleras) con humor absurdo y costumbrismo argentino. En el medio hay una pausa tranquila en el subte, donde un señor le ofrece una ayuda (tienda). Al llegar, el tiempo final define la reacción del jefe.

### b. Emociones que Buscamos Producir

1. Urgencia y adrenalina.
2. Humor y risa.
3. Identificación con lo cotidiano argentino.
4. Alivio y calidez en la pausa del subte.
5. Satisfacción al mejorar el tiempo.
6. Tensión y expectativa ante el jefe.

### c. Dinámica Principal

- **Principal:** Carrera contra el reloj (leer los obstáculos bajo presión de tiempo).
- **Secundarias:** Riesgo y recompensa (desviarse por monedas), administración de recursos (qué comprar en el subte) y mejora por repetición (bajar el tiempo).

### d. Mecánicas

- **Carrera automática:** a velocidad fija; el jugador no la controla.
- **Saltar y deslizar:** saltar (obstáculos bajos) y deslizar (obstáculos altos y escalera del subte).
- **Patada:** solo en puntos marcados del recorrido; rompe objetos y revela monedas ocultas.
- **Tropiezo:** al chocar, el personaje se ralentiza cerca de un segundo y sigue. No hay muerte.
- **Reloj:** corre hasta la oficina. No hay puntaje: menos tiempo es mejor.
- **Monedas:** se juntan en la calle y se gastan en el subte.
- **Tienda del subte (tres ítems):**
    - _Velocidad (alfajor):_ golpe de azúcar, vas más rápido.
    - _Head start (caramelo 1/2 hora):_ el reloj tarda unos segundos en arrancar.
    - _Cococho:_ el señor te lleva en la espalda; absorbe un golpe y se pierde al recibirlo.
- **Reacción del jefe:** cuatro estados, del más enojado al más tranquilo, según el tiempo final.

#### Matriz de Mecánicas, Dinámicas y Emociones

| Mecánica                        | Dinámica                         | Emoción / Estética           |
| :------------------------------ | :------------------------------- | :--------------------------- |
| Carrera automática + obstáculos | Lectura de patrones bajo presión | Urgencia, adrenalina         |
| Salto / deslizar                | Timing y precisión               | Satisfacción                 |
| Tropiezo con ralentización      | Presión de tiempo                | Tensión, ganas de reintentar |
| Patada en puntos especiales     | Variedad de ritmo                | Sorpresa, humor              |
| Monedas                         | Riesgo y recompensa              | Ambición                     |
| Tienda del subte                | Administración de recursos       | Alivio, calidez              |
| Reacción del jefe               | Mejora por repetición            | Tensión, risa, orgullo       |
| Historieta y sonido ambiente    | Narrativa implícita              | Humor, identificación        |

### e. Géneros

- **Principal:** Runner 2D de scroll lateral.
- **Secundario:** Arcade de tiempo.
- **Tono:** Comedia / narrativa liviana en historieta.

### f. Objetivos a Corto, Mediano y Largo Plazo

- **Del proyecto (2 meses):**
    - **Corto:** Tener jugable el núcleo (correr, saltar, deslizarse y tropiezo) en un tramo de prueba en Unity.
    - **Mediano:** Nivel 1 completo: calle, subte con tienda, salida, oficina y las cuatro reacciones del jefe.
    - **Largo:** Arte, sonido e historietas integrados, pruebas con otros grupos y build publicada en itch.io.
    - **A futuro (fuera del prototipo):** Nivel 2, llegar tarde a una cita.
- **Del jugador (dentro de la partida):**
    - **Corto:** Esquivar el obstáculo que viene y juntar monedas.
    - **Mediano:** Llegar al subte, elegir el ítem y cruzar la salida.
    - **Largo:** Llegar a la oficina en el menor tiempo y lograr un jefe tranquilo.

### g. Pilares de la Experiencia Jugable

1. **Urgencia fluida:** Controles simples y respuesta inmediata.
2. **Lo cotidiano con humor:** Situaciones reconocibles, llevadas al absurdo.
3. **Pocas mecánicas, bien pulidas:** Lo que no suma al núcleo se deja afuera.
4. **Reintento rápido:** El objetivo es mejorar el tiempo; fallar no castiga.
5. **Historia sin texto largo:** Se cuenta con historieta y sonido ambiente por zona.
6. **Calidez y respeto:** Hacia las personas del juego, incluido el vendedor del subte.

### h. Valores del Equipo y Cómo se Aplican

_Equipo de 4. Todos hacemos de todo; los roles son una guía: Lucas (diseño y parte gráfica), Guido y Rodrigo (programación), Fátima (historia, dinámicas)._

1. **LO NUESTRO, BIEN CONTADO:** Contar Buenos Aires con detalles reales (Subte B, obrero, portero, alfajor, mate, oficina).
2. **POCO, PERO PULIDO:** Pocas mecánicas que se sientan muy bien. Si una idea no mejora la carrera, se deja afuera.
3. **PROBAR RÁPIDO, CORREGIR RÁPIDO:** Ajustar con feedback y no con suposiciones (una build jugable por semana).

---

## 🎮 DISEÑO DEL JUEGO

### Core Loop

- **Segundo a segundo:** Ver el obstáculo ➡️ decidir (saltar, deslizar o patear) ➡️ ejecutar ➡️ seguir corriendo.
- **Minuto a minuto:** Calle ➡️ pausa en el subte (compra) ➡️ salida ➡️ oficina ➡️ tiempo y reacción del jefe ➡️ ganas de repetir.

### Recorrido del Nivel 1

1. **Despertar (historieta):** El despertador marca las 9 y debía levantarse a las 8. Sale por la ventana con una patada.
2. **La calle:** Cae a la vereda y corre esquivando obstáculos de barrio mientras junta monedas.
3. **Escalera del subte B:** Se desliza hasta el andén.
4. **Pausa en el subte:** Sentado en el vagón, un vendedor ambulante se acerca y se compran ítems con monedas.
5. **Salida:** Se baja y sigue corriendo con más obstáculos.
6. **Oficina:** Se muestra el tiempo y el jefe reacciona (cuatro estados).

---

## 🎨 DIRECCIÓN ARTÍSTICA

- **Estilo Visual:** 2D lateral estilo historieta, con contornos marcados (`#2B2B2B`), colores planos y sprites simples y legibles a alta velocidad.
- **Setting:** Buenos Aires en tres zonas (Barrio, Subte B, Centro).

### Paleta de Colores por Zona

- **Barrio:** `#FFD9A0` / `#8EC9E8` / `#E8A76B` (Cálidos de mañana, vereda, obras)
- **Subte B:** `#7A1F1F` / `#E53935` / `#D9A441` (Rojos y ocres)
- **Centro:** `#6E8CA0` / `#3E5566` / `#C7D3DB` (Grises azulados)
- **Oficina:** `#7FA68E` / `#D8DDD3` / `#8B6B4A` (Verdes y grises)
- **Estado del Jefe:** `#D64541` / `#E58A3C` / `#C9C46B` / `#8FD3A8` (De enojado a tranquilo)

### Asociación Mecánica - Color

- **Obstáculo bajo (salto):** Naranja `#FF8A00`
- **Obstáculo alto (deslizamiento):** Azul `#1E7BFF`
- **Obstáculo rompible (patada):** Rojo `#E5262B` + marca visible
- **Monedas:** Dorado `#FFC700`
- **Ítems de la tienda:** Verde `#2ECC71`

### Tipografía & Interfaz

- **Títulos y letreros:** Bangers
- **Globos de diálogo:** Comic Neue
- **Reloj e interfaz:** Oswald Bold
- _HUD:_ Reloj grande arriba, contador de monedas, ícono del ítem activo.

---

## 🔗 Referencias Útiles

- [Canabalt](https://canabalt.com/)[cite: 2]
- [BIT.TRIP RUNNER](https://store.steampowered.com/app/63710/BITTRIP_RUNNER/?l=spanish)[cite: 2]
- [Pizza Tower](https://store.steampowered.com/app/2231450/Pizza_Tower/?l=spanish)[cite: 2]
- [Badland](https://gamerant.com/db/video-game/badland/)[cite: 2]
- [Silhouette Showdown](https://weentermakesgames.itch.io/silhouette-showdown)[cite: 2]
- [Ninja Arashi 2](https://play.google.com/store/apps/details?id=com.blackpanther.ninjaarashi2&hl=es_CR)[cite: 2]
