// Ohne gültigen Token zurück zum Login
if (!Api.getToken()) {
  window.location.href = "login.html";
}

document.getElementById("abmelden-btn").addEventListener("click", () => {
  Api.clearToken();
  window.location.href = "login.html";
});

const GERAETE_TYPEN = ["Server", "Workstation", "Notebook", "Drucker", "Printserver", "WLAN", "Router", "Switch", "VPN", "VLAN"];

const kundenAnsicht = document.getElementById("kunden-ansicht");
const kundeDetailAnsicht = document.getElementById("kunde-detail-ansicht");
let aktuellerKunde = null;
let aktuellerTypFilter = "";

function escapeHtml(text) {
  const div = document.createElement("div");
  div.textContent = text ?? "";
  return div.innerHTML;
}

// --- Kundenliste (Startseite) ---

async function kundenLadenUndAnzeigen(suche) {
  const fehlerFeld = document.getElementById("kunde-fehler");
  fehlerFeld.textContent = "";
  try {
    const kunden = await Api.kundenListe(suche);
    const tbody = document.getElementById("kunden-liste");
    tbody.innerHTML = "";
    kunden.forEach((k) => {
      const zeile = document.createElement("tr");
      zeile.innerHTML = `<td>${escapeHtml(k.name)}</td><td>${escapeHtml(k.email)}</td><td>${escapeHtml(k.telefonnummer)}</td><td>${escapeHtml(k.adresse)}</td><td><button class="sekundaer">Bearbeiten</button></td>`;
      zeile.addEventListener("click", () => kundeDetailAnzeigen(k));
      tbody.appendChild(zeile);
    });
  } catch (err) {
    fehlerFeld.textContent = err.message;
  }
}

document.getElementById("kunde-neu-btn").addEventListener("click", () => {
  document.getElementById("kunde-formular").style.display = "grid";
  document.getElementById("kunde-neu-btn").style.display = "none";
});

document.getElementById("kunde-abbrechen-btn").addEventListener("click", () => {
  document.getElementById("kunde-formular").reset();
  document.getElementById("kunde-formular").style.display = "none";
  document.getElementById("kunde-neu-btn").style.display = "inline-block";
  document.getElementById("kunde-fehler").textContent = "";
});

document.getElementById("kunde-formular").addEventListener("submit", async (e) => {
  e.preventDefault();
  const fehlerFeld = document.getElementById("kunde-fehler");
  fehlerFeld.textContent = "";

  const daten = {
    name: document.getElementById("kunde-name").value,
    email: document.getElementById("kunde-email").value || null,
    telefonnummer: document.getElementById("kunde-telefon").value || null,
    adresse: document.getElementById("kunde-adresse").value || null,
  };

  try {
    await Api.kundeErstellen(daten);
    document.getElementById("kunde-abbrechen-btn").click();
    await kundenLadenUndAnzeigen(document.getElementById("kunde-suche").value);
  } catch (err) {
    fehlerFeld.textContent = err.message;
  }
});

let sucheTimeout;
document.getElementById("kunde-suche").addEventListener("input", (e) => {
  clearTimeout(sucheTimeout);
  sucheTimeout = setTimeout(() => kundenLadenUndAnzeigen(e.target.value), 250);
});

// --- Kundendetail: Menü-Steuerung ---

document.querySelectorAll(".detail-menu-btn").forEach((btn) => {
  btn.addEventListener("click", () => panelAnzeigen(btn.dataset.panel));
});

function panelAnzeigen(name) {
  document.querySelectorAll(".detail-panel").forEach((p) => (p.style.display = "none"));
  document.querySelectorAll(".detail-menu-btn").forEach((b) => b.classList.remove("aktiv"));
  document.getElementById(`panel-${name}`).style.display = "block";
  document.querySelector(`.detail-menu-btn[data-panel="${name}"]`).classList.add("aktiv");

  if (name === "standorte") standorteLadenUndAnzeigen();
  if (name === "geraete") geraeteLadenUndAnzeigen();
  if (name === "ip") ipUebersichtLadenUndAnzeigen();
}

async function kundeDetailAnzeigen(kunde) {
  aktuellerKunde = kunde;
  aktuellerTypFilter = "";
  geraetBearbeitenModusBeenden();
  document.getElementById("standort-formular").style.display = "none";
  document.getElementById("standort-formular").reset();
  document.getElementById("standort-neu-btn").style.display = "inline-block";
  document.getElementById("detail-kunde-name").textContent = kunde.name;

  document.getElementById("info-name").value = kunde.name ?? "";
  document.getElementById("info-email").value = kunde.email ?? "";
  document.getElementById("info-telefon").value = kunde.telefonnummer ?? "";
  document.getElementById("info-adresse").value = kunde.adresse ?? "";

  kundenAnsicht.style.display = "none";
  kundeDetailAnsicht.style.display = "block";
  panelAnzeigen("info");
}

document.getElementById("zurueck-btn").addEventListener("click", () => {
  kundeDetailAnsicht.style.display = "none";
  kundenAnsicht.style.display = "block";
  aktuellerKunde = null;
  kundenLadenUndAnzeigen(document.getElementById("kunde-suche").value);
});

// --- Allgemeine Info ---

document.getElementById("info-formular").addEventListener("submit", async (e) => {
  e.preventDefault();
  const fehlerFeld = document.getElementById("info-fehler");
  fehlerFeld.textContent = "";

  const daten = {
    name: document.getElementById("info-name").value,
    email: document.getElementById("info-email").value || null,
    telefonnummer: document.getElementById("info-telefon").value || null,
    adresse: document.getElementById("info-adresse").value || null,
  };

  try {
    await Api.kundeBearbeiten(aktuellerKunde.id, daten);
    aktuellerKunde = { ...aktuellerKunde, ...daten };
    document.getElementById("detail-kunde-name").textContent = aktuellerKunde.name;
  } catch (err) {
    fehlerFeld.textContent = err.message;
  }
});

// --- Standorte ---

async function standorteLadenUndAnzeigen() {
  const fehlerFeld = document.getElementById("standort-fehler");
  fehlerFeld.textContent = "";
  try {
    const standorte = await Api.standorteListe(aktuellerKunde.id);
    const tbody = document.getElementById("standorte-liste");
    tbody.innerHTML = "";
    standorte.forEach((s) => {
      const zeile = document.createElement("tr");
      zeile.innerHTML = `<td>${escapeHtml(s.name)}</td><td>${escapeHtml(s.adresse)}</td><td><button class="loeschen">Löschen</button></td>`;
      zeile.querySelector(".loeschen").addEventListener("click", async () => {
        if (confirm(`Standort "${s.name}" wirklich löschen?`)) {
          await Api.standortLoeschen(aktuellerKunde.id, s.id);
          await standorteLadenUndAnzeigen();
        }
      });
      tbody.appendChild(zeile);
    });
  } catch (err) {
    fehlerFeld.textContent = err.message;
  }
}

document.getElementById("standort-neu-btn").addEventListener("click", () => {
  document.getElementById("standort-formular").style.display = "grid";
  document.getElementById("standort-neu-btn").style.display = "none";
});

document.getElementById("standort-abbrechen-btn").addEventListener("click", () => {
  document.getElementById("standort-formular").reset();
  document.getElementById("standort-formular").style.display = "none";
  document.getElementById("standort-neu-btn").style.display = "inline-block";
  document.getElementById("standort-fehler").textContent = "";
});

document.getElementById("standort-formular").addEventListener("submit", async (e) => {
  e.preventDefault();
  const fehlerFeld = document.getElementById("standort-fehler");
  fehlerFeld.textContent = "";

  const daten = {
    name: document.getElementById("standort-name").value,
    adresse: document.getElementById("standort-adresse").value || null,
  };

  try {
    await Api.standortErstellen(aktuellerKunde.id, daten);
    document.getElementById("standort-abbrechen-btn").click();
    await standorteLadenUndAnzeigen();
  } catch (err) {
    fehlerFeld.textContent = err.message;
  }
});

// --- Geräte (mit Typ-Filter-Menü) ---

function typFilterAufbauen() {
  const container = document.getElementById("typ-filter");
  container.innerHTML = '<button class="typ-chip aktiv" data-typ="">Alle</button>';
  GERAETE_TYPEN.forEach((typ) => {
    const btn = document.createElement("button");
    btn.className = "typ-chip";
    btn.dataset.typ = typ;
    btn.textContent = typ;
    container.appendChild(btn);
  });
  container.querySelectorAll(".typ-chip").forEach((chip) => {
    chip.addEventListener("click", () => {
      aktuellerTypFilter = chip.dataset.typ;
      container.querySelectorAll(".typ-chip").forEach((c) => c.classList.remove("aktiv"));
      chip.classList.add("aktiv");
      geraeteLadenUndAnzeigen();
    });
  });

  const select = document.getElementById("geraet-typ");
  select.innerHTML = "";
  GERAETE_TYPEN.forEach((typ) => {
    const option = document.createElement("option");
    option.value = typ;
    option.textContent = typ;
    select.appendChild(option);
  });
}
typFilterAufbauen();

let geraetImBearbeitungsModus = null;

async function geraeteLadenUndAnzeigen() {
  const fehlerFeld = document.getElementById("geraet-fehler");
  fehlerFeld.textContent = "";
  try {
    const geraete = await Api.geraeteListe(aktuellerKunde.id);
    const gefiltert = aktuellerTypFilter ? geraete.filter((g) => g.typ === aktuellerTypFilter) : geraete;

    const tbody = document.getElementById("geraete-liste");
    tbody.innerHTML = "";
    gefiltert.forEach((g) => {
      const zeile = document.createElement("tr");
      zeile.innerHTML = `
        <td>${escapeHtml(g.typ)}</td>
        <td>${escapeHtml(g.name)}</td>
        <td>${escapeHtml(g.ip)}</td>
        <td>${escapeHtml(g.seriennummer)}</td>
        <td>
          <button class="sekundaer bearbeiten">Bearbeiten</button>
          <button class="loeschen">Löschen</button>
        </td>
      `;
      zeile.querySelector(".bearbeiten").addEventListener("click", () => geraetBearbeitenModusStarten(g));
      zeile.querySelector(".loeschen").addEventListener("click", async () => {
        if (confirm(`Gerät "${g.name}" wirklich löschen?`)) {
          await Api.geraetLoeschen(aktuellerKunde.id, g.id);
          if (geraetImBearbeitungsModus?.id === g.id) geraetBearbeitenModusBeenden();
          await geraeteLadenUndAnzeigen();
        }
      });
      tbody.appendChild(zeile);
    });
  } catch (err) {
    fehlerFeld.textContent = err.message;
  }
}

document.getElementById("geraet-neu-btn").addEventListener("click", () => {
  geraetFormularAnzeigen();
});

function geraetFormularAnzeigen() {
  document.getElementById("geraet-formular").style.display = "grid";
  document.getElementById("geraet-formular-titel").style.display = "block";
  document.getElementById("geraet-neu-btn").style.display = "none";
}

function geraetBearbeitenModusStarten(g) {
  geraetImBearbeitungsModus = g;
  geraetFormularAnzeigen();
  document.getElementById("geraet-formular-titel").textContent = `Gerät bearbeiten: ${g.name}`;
  document.getElementById("geraet-formular-submit").textContent = "Änderungen speichern";

  document.getElementById("geraet-typ").value = g.typ;
  document.getElementById("geraet-name").value = g.name ?? "";
  document.getElementById("geraet-bezeichnung").value = g.bezeichnung ?? "";
  document.getElementById("geraet-ip").value = g.ip ?? "";
  document.getElementById("geraet-seriennummer").value = g.seriennummer ?? "";
}

function geraetBearbeitenModusBeenden() {
  geraetImBearbeitungsModus = null;
  document.getElementById("geraet-formular-titel").textContent = "Gerät hinzufügen";
  document.getElementById("geraet-formular-submit").textContent = "Gerät hinzufügen";
  document.getElementById("geraet-formular").reset();
  document.getElementById("geraet-formular").style.display = "none";
  document.getElementById("geraet-formular-titel").style.display = "none";
  document.getElementById("geraet-neu-btn").style.display = "inline-block";
  document.getElementById("geraet-fehler").textContent = "";
}

document.getElementById("geraet-abbrechen-btn").addEventListener("click", geraetBearbeitenModusBeenden);

document.getElementById("geraet-formular").addEventListener("submit", async (e) => {
  e.preventDefault();
  const fehlerFeld = document.getElementById("geraet-fehler");
  fehlerFeld.textContent = "";

  const daten = {
    typ: document.getElementById("geraet-typ").value,
    name: document.getElementById("geraet-name").value,
    bezeichnung: document.getElementById("geraet-bezeichnung").value || null,
    ip: document.getElementById("geraet-ip").value || null,
    seriennummer: document.getElementById("geraet-seriennummer").value || null,
  };

  try {
    if (geraetImBearbeitungsModus) {
      await Api.geraetBearbeiten(aktuellerKunde.id, geraetImBearbeitungsModus.id, daten);
    } else {
      await Api.geraetErstellen(aktuellerKunde.id, daten);
    }
    geraetBearbeitenModusBeenden();
    await geraeteLadenUndAnzeigen();
  } catch (err) {
    fehlerFeld.textContent = err.message;
  }
});

// --- IP-Übersicht ---

async function ipUebersichtLadenUndAnzeigen() {
  try {
    const geraete = await Api.geraeteListe(aktuellerKunde.id);
    const tbody = document.getElementById("ip-liste");
    tbody.innerHTML = "";
    geraete.forEach((g) => {
      const zeile = document.createElement("tr");
      zeile.innerHTML = `<td>${escapeHtml(g.name)}</td><td>${escapeHtml(g.typ)}</td><td>${escapeHtml(aktuellerKunde.name)}</td><td>${escapeHtml(g.ip)}</td>`;
      tbody.appendChild(zeile);
    });
  } catch (err) {
    console.error(err);
  }
}

kundenLadenUndAnzeigen();
