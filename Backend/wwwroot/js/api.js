const Api = (() => {
  const BASIS_URL = "/api";

  function getToken() {
    return sessionStorage.getItem("token");
  }

  function setToken(token, benutzername) {
    sessionStorage.setItem("token", token);
    sessionStorage.setItem("benutzername", benutzername);
  }

  function clearToken() {
    sessionStorage.removeItem("token");
    sessionStorage.removeItem("benutzername");
  }

  async function anfrage(pfad, optionen = {}) {
    const kopfzeilen = { "Content-Type": "application/json", ...(optionen.headers || {}) };
    const token = getToken();
    if (token) {
      kopfzeilen["Authorization"] = `Bearer ${token}`;
    }

    const antwort = await fetch(`${BASIS_URL}${pfad}`, { ...optionen, headers: kopfzeilen });

    if (antwort.status === 401) {
      clearToken();
      window.location.href = "login.html";
      throw new Error("Sitzung abgelaufen. Bitte erneut anmelden.");
    }

    if (!antwort.ok) {
      const text = await antwort.text();
      throw new Error(text || `Fehler ${antwort.status}`);
    }

    if (antwort.status === 204) return null;
    return antwort.json();
  }

  return {
    setToken,
    clearToken,
    getToken,
    getBenutzername: () => sessionStorage.getItem("benutzername"),

    login: (benutzername, passwort) =>
      anfrage("/auth/login", { method: "POST", body: JSON.stringify({ benutzername, passwort }) })
        // Backend erwartet PascalCase-Eigenschaften via Model-Binding (case-insensitiv)
        .catch((err) => { throw err; }),

    kundenListe: (suche) => anfrage(`/kunden${suche ? `?suche=${encodeURIComponent(suche)}` : ""}`),
    kundeErstellen: (daten) => anfrage("/kunden", { method: "POST", body: JSON.stringify(daten) }),
    kundeBearbeiten: (id, daten) => anfrage(`/kunden/${id}`, { method: "PUT", body: JSON.stringify(daten) }),
    kundeLoeschen: (id) => anfrage(`/kunden/${id}`, { method: "DELETE" }),

    geraeteListe: (kundeId) => anfrage(`/kunden/${kundeId}/geraete`),
    geraetErstellen: (kundeId, daten) => anfrage(`/kunden/${kundeId}/geraete`, { method: "POST", body: JSON.stringify(daten) }),
    geraetBearbeiten: (kundeId, id, daten) => anfrage(`/kunden/${kundeId}/geraete/${id}`, { method: "PUT", body: JSON.stringify(daten) }),
    geraetLoeschen: (kundeId, id) => anfrage(`/kunden/${kundeId}/geraete/${id}`, { method: "DELETE" }),
  };
})();
