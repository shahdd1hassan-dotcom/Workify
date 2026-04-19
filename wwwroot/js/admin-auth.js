(function () {
  var STORAGE_KEY = "workifyAdminSession";
  var USERNAME = "admin";
  var PASSWORD = "Workify@2026";

  function getSession() {
    try {
      return JSON.parse(localStorage.getItem(STORAGE_KEY) || "null");
    } catch (error) {
      return null;
    }
  }

  function setSession(session) {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
  }

  function clearSession() {
    localStorage.removeItem(STORAGE_KEY);
  }

  function isAuthed() {
    var session = getSession();
    return Boolean(session && session.authed === true);
  }

  function requireAdminAccess() {
    var loginPath = "admin-login.html";
    var currentPath = window.location.pathname.toLowerCase();
    if (!isAuthed() && !currentPath.endsWith(loginPath.toLowerCase())) {
      window.location.href = loginPath;
    }
  }

  function handleLogin(form) {
    var usernameInput = form.querySelector("[name='username']");
    var passwordInput = form.querySelector("[name='password']");
    var alertBox = document.getElementById("admin-login-alert");
    var username = (usernameInput && usernameInput.value || "").trim();
    var password = passwordInput && passwordInput.value || "";

    if (username === USERNAME && password === PASSWORD) {
      setSession({ authed: true, username: USERNAME, role: "admin" });
      window.location.href = "admin-dashboard.html";
      return;
    }

    if (alertBox) {
      alertBox.textContent = "Invalid admin username or password.";
      alertBox.classList.add("show");
    }
  }

  function handleLogout() {
    clearSession();
    window.location.href = "admin-login.html";
  }

  function bootstrap() {
    var loginPath = "admin-login.html";
    var currentPath = window.location.pathname.toLowerCase();

    window.adminAuth = {
      USERNAME: USERNAME,
      PASSWORD: PASSWORD,
      clearSession: clearSession,
      handleLogin: handleLogin,
      handleLogout: handleLogout,
      isAuthed: isAuthed,
      requireAdminAccess: requireAdminAccess,
    };

    if (isAuthed() && currentPath.endsWith(loginPath.toLowerCase())) {
      window.location.href = "admin-dashboard.html";
      return;
    }

    requireAdminAccess();
  }

  document.addEventListener("DOMContentLoaded", bootstrap);
})();
