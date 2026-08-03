// Ohne gültigen Token zurück zum Login
if (!Api.getToken()) {
  window.location.href = "login.html";
}

document.getElementById("abmelden-btn").addEventListener("click", () => {
  Api.clearToken();
  window.location.href = "login.html";
});

const kundenAnsicht = document.getElementById("kunden-ansicht");
const geraeteAnsicht = document.getElementById("geraete-ansicht");
let aktuellerKundeId = null;

// --- Kunden ---

async function kundenLadenUndAnzeigen() {
  const fehlerFeld = document.getElementById("kunde-fehler");
  fehlerFeld.textContent = "";
  try {
    const kunden = await Api.kundenListe();
    const tbody = document.getElementById("kunden-liste");
    tbody.innerHTML = "";
    kunden.forEach((k) => {
      const zeile = document.createElement("tr");
      zeile.innerHTML = `<td>${escapeHtml(k.name)}</td><td>${escapeHtml(k.email ?? "")}</td><td>${escapeHtml(k.telefonnummer ?? "")}</td><td>${escapeHtml(k.adresse ?? "")}</td>`;
      zeile.addEventListener("click", () => geraeteAnzeigen(k.id, k.name));
      tbody.appendChild(zeile);
    });
  } catch (err) {
    fehlerFeld.textContent = err.message;
  }
}

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
    e.target.reset();
    await kundenLadenUndAnzeigen();
  } catch (err) {
    fehlerFeld.textContent = err.message;
  }
});

// --- Geräte ---

async function geraeteAnzeigen(kundeId, kundeName) {
  aktuellerKundeId = kundeId;
  document.getElementById("geraete-kunde-name").textContent = `Geräte – ${kundeName}`;
  kundenAnsicht.style.display = "none";
  geraeteAnsicht.style.display = "block";
  await geraeteLadenUndAnzeigen();
}

document.getElementById("zurueck-btn").addEventListener("click", () => {
  geraeteAnsicht.style.display = "none";
  kundenAnsicht.style.display = "block";
  aktuellerKundeId = null;
});

async function geraeteLadenUndAnzeigen() {
  const fehlerFeld = document.getElementById("geraet-fehler");
  fehlerFeld.textContent = "";
  try {
    const geraete = await Api.geraeteListe(aktuellerKundeId);
    const tbody = document.getElementById("geraete-liste");
    tbody.innerHTML = "";
    geraete.forEach((g) => {
      const zeile = document.createElement("tr");
      zeile.innerHTML = `
        <td>${escapeHtml(g.typ)}</td>
        <td>${escapeHtml(g.name)}</td>
        <td>${escapeHtml(g.ip ?? "")}</td>
        <td>${escapeHtml(g.seriennummer ?? "")}</td>
        <td><button class="loeschen" data-id="${g.id}">Löschen</button></td>
      `;
      zeile.querySelector(".loeschen").addEventListener("click", async (ev) => {
        ev.stopPropagation();
        if (confirm(`Gerät "${g.name}" wirklich löschen?`)) {
          await Api.geraetLoeschen(aktuellerKundeId, g.id);
          await geraeteLadenUndAnzeigen();
        }
      });
      tbody.appendChild(zeile);
    });
  } catch (err) {
    fehlerFeld.textContent = err.message;
  }
}

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
    await Api.geraetErstellen(aktuellerKundeId, daten);
    e.target.reset();
    await geraeteLadenUndAnzeigen();
  } catch (err) {
    fehlerFeld.textContent = err.message;
  }
});

function escapeHtml(text) {
  const div = document.createElement("div");
  div.textContent = text;
  return div.innerHTML;
}

kundenLadenUndAnzeigen();
