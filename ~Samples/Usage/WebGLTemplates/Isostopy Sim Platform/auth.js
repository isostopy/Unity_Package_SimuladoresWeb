(() => {
  const ENDPOINT_ME = 'https://test.isostopyserver.net/api/users/me';
  const AUTH_FAIL_STATUSES = new Set([401, 403, 419]); // estados que SÍ confirman token inválido

  function clearToken() {
    // borra cookie en cliente; el servidor debería tener /logout para hacerlo "de verdad"
    document.cookie = 'directus_token=; Max-Age=0; path=/';
  }

  /**
   * checkSession(options)
   * - redirectIfAuth: a dónde ir si SÍ hay sesión (login)
   * - redirectIfAnon: a dónde ir si NO hay sesión (páginas protegidas)
   * - clearOnAuthFail: borrar cookie si el server devuelve 401/403/419 (def. true)
   * - clearOnNetworkError: borrar cookie si hay error de red/CORS (def. false)
   * Devuelve true/false según autenticación.
   */
  async function checkSession({
    redirectIfAuth = null,
    redirectIfAnon = null,
    clearOnAuthFail = true,
    clearOnNetworkError = false
  } = {}) {
    try {
      const res = await fetch(ENDPOINT_ME, {
        method: 'GET',
        credentials: 'include',
        cache: 'no-store'
      });

      if (res.ok) {
        if (redirectIfAuth) {
          window.location.replace(redirectIfAuth);
          return true;
        }
        return true;
      }

      // Solo limpiamos si es un "fallo de auth" contundente
      if (clearOnAuthFail && AUTH_FAIL_STATUSES.has(res.status)) {
        clearToken();
      }

      if (redirectIfAnon) {
        window.location.replace(redirectIfAnon);
      }
      return false;

    } catch (err) {
      // Error de red / CORS / timeout: NO borrar cookie por defecto
      if (clearOnNetworkError) {
        clearToken();
      }
      if (redirectIfAnon) {
        window.location.replace(redirectIfAnon);
      }
      return false;
    }
  }

  window.authGuard = { checkSession, clearToken };
})();