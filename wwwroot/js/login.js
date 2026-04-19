const loginTab = document.getElementById('login-tab');
const registerTab = document.getElementById('register-tab');
const loginForm = document.getElementById('login-form');
const registerForm = document.getElementById('register-form');

function setAuthTab(tabName) {
  const showLogin = tabName === 'login';

  loginTab.classList.toggle('active', showLogin);
  loginTab.setAttribute('aria-selected', String(showLogin));
  registerTab.classList.toggle('active', !showLogin);
  registerTab.setAttribute('aria-selected', String(!showLogin));

  loginForm.classList.toggle('active', showLogin);
  registerForm.classList.toggle('active', !showLogin);
}

function syncTabFromHash() {
  if (window.location.hash === '#register') {
    setAuthTab('register');
  } else {
    setAuthTab('login');
  }
}

loginTab.addEventListener('click', () => {
  window.location.hash = '#login';
  setAuthTab('login');
});

registerTab.addEventListener('click', () => {
  window.location.hash = '#register';
  setAuthTab('register');
});

window.addEventListener('hashchange', syncTabFromHash);
syncTabFromHash();

loginForm.addEventListener('submit', (event) => {
  event.preventDefault();
  // Simulate login success and redirect to onboarding
  window.location.href = 'onboarding.html';
});

registerForm.addEventListener('submit', (event) => {
  event.preventDefault();

  const password = document.getElementById('register-password');
  const confirm = document.getElementById('register-confirm-password');

  if (password.value !== confirm.value) {
    alert('Passwords do not match.');
    return;
  }

  // Simulate register success and redirect to onboarding
  window.location.href = 'onboarding.html';
});
