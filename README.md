# Avatar Conversazionale VR (Unity + MLLM)

Progetto VR sviluppato in **Unity** che implementa un **avatar conversazionale** basato su **MLLM**, con backend Python tramite **Flask**.

---

## Requisiti

* Git
* Unity Hub
* Unity Editor 2022.3.27f1
* Python 3.11.9
* Oculus Meta Quest 3
* Sistema Operativo Windows

---

## Clonare il repository

```bash
git clone https://github.com/Emanuele1087650/Avatar_Conversazionale_VR
cd Avatar_Conversazionale_VR
```

---

## Aprire il progetto in Unity

1. Aprire **Unity Hub**
2. Cliccare su **Open** / **Add**
3. Selezionare la cartella principale del progetto
4. Aprire il progetto con la versione di Unity indicata sotto

> Al primo avvio Unity potrebbe impiegare alcuni minuti per importare le dipendenze.

---

## Avviare il server Flask

Il server backend si trova nella cartella `server` ed è gestito dal file:

```
flask_server_openrouter.py
```

### Passaggi:
Copiare il file .env ricevuto all'interno della cartella 'server'.

Installare le dipendenze:
```bash
pip install flask requests flask_cors dotenv
```

Avviare il server:
```bash
cd server
python flask_server_openrouter.py
```

Il server Flask verrà avviato in locale.

> Assicurarsi di avere installato i package Python richiesti.

---

## Avvio dell'esperienza VR

1. Assicurarsi che il server Flask sia **attivo**
2. Avviare la scena principale del progetto in Unity (all'interno di Assets/Scene).
3. Collegare il visore Oculus.
4. Avviare in **Play Mode**.

L'avatar utilizzerà il server Flask per la comunicazione con il modello MLLM.

---

