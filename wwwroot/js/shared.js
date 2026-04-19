/* =============================================================
   SHARED.JS — Workify Shared Functionality
   This script runs on EVERY page. It handles:
   1. Loading the header, footer, and sidebar HTML components
   2. Mobile menu toggle
   3. Sidebar collapse/expand
   4. Active link highlighting
   5. Auto-filling the copyright year
   ============================================================= */

document.addEventListener('DOMContentLoaded', function () {
  var componentVersion = '20260326-footer-v2';

  // ---------------------------------------------------------
  // 1. LOAD COMPONENTS
  //    Fetches an HTML file and injects it into a placeholder div.
  //    Usage: put  <div id="header-placeholder"></div>  in your page.
  // ---------------------------------------------------------

  /**
   * Fetches an HTML partial and injects it into the target element.
   * After injection, runs an optional callback (for wiring up events).
   *
   * @param {string} elementId  - The id of the placeholder div
   * @param {string} filePath   - Path to the HTML file (relative to root)
   * @param {Function} callback - Optional function to run after loading
   */
  function loadComponent(elementId, filePath, callback) {
    var placeholder = document.getElementById(elementId);
    if (!placeholder) return; // no placeholder? skip silently

    fetch(filePath + '?v=' + componentVersion, { cache: 'no-store' })
      .then(function (response) {
        if (!response.ok) throw new Error('Failed to load ' + filePath);
        return response.text();
      })
      .then(function (html) {
        placeholder.innerHTML = html;
        if (callback) callback();
      })
      .catch(function (error) {
        console.warn('[Workify] Could not load component:', error.message);
      });
  }

  // Figure out how deep we are in the folder tree so relative paths work.
  // e.g. if we're in /pages/browse-jobs.html, we need "../components/..."
  // but from /index.html we need "./components/..."
  var depth = getPathDepth();
  var prefix = depth === 0 ? './' : '../'.repeat(depth);

  // Load header on every page
  loadComponent('header-placeholder', prefix + 'components/header.html', function () {
    normalizeComponentLinks(document.getElementById('header-placeholder'), prefix);
    initMobileMenu();
    highlightActiveLink();
    updateCopyrightYear();
  });

  // Load footer on every page
  loadComponent('footer-placeholder', prefix + 'components/footer.html', function () {
    normalizeComponentLinks(document.getElementById('footer-placeholder'), prefix);
    updateCopyrightYear();
    
    // Improved icon loading:
    // 1. Try passing the specific root element to ensure we target correctly
    // 2. Check if lucide is available, if not, wait a bit
    var footerEl = document.getElementById('footer-placeholder');
    
    if (typeof lucide !== 'undefined' && footerEl) {
      lucide.createIcons({ root: footerEl });
    } else {
      // Retry once after a short delay in case script is async/defer
      setTimeout(function() {
        if (typeof lucide !== 'undefined' && footerEl) {
           lucide.createIcons({ root: footerEl });
        }
      }, 500);
    }
  });

  // Load sidebar ONLY if the placeholder exists (dashboard pages)
  var sidebarPlaceholder = document.getElementById('sidebar-placeholder');
  if (sidebarPlaceholder) {
    var sidebarComponent = sidebarPlaceholder.getAttribute('data-component') || 'components/sidebar.html';
    loadComponent('sidebar-placeholder', prefix + sidebarComponent, function () {
      normalizeComponentLinks(document.getElementById('sidebar-placeholder'), prefix);
      initSidebarToggle();
      highlightActiveLink();
      if (typeof lucide !== 'undefined') {
        lucide.createIcons();
      }
    });
  }

  function normalizeComponentLinks(rootElement, basePrefix) {
    if (!rootElement) return;

    var routeLinks = rootElement.querySelectorAll('[data-route]');
    routeLinks.forEach(function (link) {
      var route = link.getAttribute('data-route');
      if (!route) return;

      // Keep external links untouched; only normalize internal routes.
      if (/^https?:\/\//i.test(route)) {
        link.setAttribute('href', route);
        return;
      }

      link.setAttribute('href', basePrefix + route);
    });
  }


  // ---------------------------------------------------------
  // 2. MOBILE MENU TOGGLE
  //    Opens/closes the hamburger menu on small screens.
  // ---------------------------------------------------------

  function initMobileMenu() {
    var toggle = document.getElementById('mobile-menu-toggle');
    var menu = document.getElementById('mobile-menu');
    if (!toggle || !menu) return;

    toggle.addEventListener('click', function () {
      // Toggle open class (CSS handles the slide-down animation)
      menu.classList.toggle('open');
      menu.classList.toggle('hidden');

      // Swap the hamburger icon to an X (or back)
      var isOpen = menu.classList.contains('open');
      toggle.setAttribute('aria-label', isOpen ? 'Close menu' : 'Open menu');
    });

    // Close menu when clicking a link inside it
    var menuLinks = menu.querySelectorAll('a');
    menuLinks.forEach(function (link) {
      link.addEventListener('click', function () {
        menu.classList.remove('open');
        menu.classList.add('hidden');
      });
    });
  }


  // ---------------------------------------------------------
  // 3. SIDEBAR COLLAPSE / EXPAND
  //    Toggles the sidebar between full width and icon-only rail.
  // ---------------------------------------------------------

  function initSidebarToggle() {
    var collapseBtn = document.getElementById('sidebar-collapse-toggle');
    var sidebar = document.getElementById('main-sidebar');
    var mainContent = document.querySelector('.main-content');
    if (!collapseBtn || !sidebar) return;

    collapseBtn.addEventListener('click', function () {
      sidebar.classList.toggle('collapsed');

      // Also shift the main content area
      if (mainContent) {
        mainContent.classList.toggle('sidebar-collapsed');
      }
    });
  }


  // ---------------------------------------------------------
  // 4. ACTIVE LINK HIGHLIGHTING
  //    Looks at the current page URL and adds an "active" class
  //    to matching nav links (both header and sidebar).
  // ---------------------------------------------------------

  function highlightActiveLink() {
    // Get the current page filename, e.g. "browse-jobs.html" or "index.html"
    var path = window.location.pathname;
    var currentPage = path.substring(path.lastIndexOf('/') + 1).replace('.html', '');
    var currentHash = (window.location.hash || '').replace('#', '');

    // If we're on the root, treat it as "index"
    if (!currentPage || currentPage === '') currentPage = 'index';

    // Find all links with a data-page attribute and mark the matching one
    var allNavLinks = document.querySelectorAll('[data-page]');
    allNavLinks.forEach(function (link) {
      var linkPage = link.getAttribute('data-page');
      var linkHash = link.getAttribute('data-hash');
      var pageMatches = linkPage === currentPage;
      var hashMatches = !linkHash || linkHash === currentHash;

      if (pageMatches && hashMatches) {
        link.classList.add('active');
      } else {
        link.classList.remove('active');
      }
    });
  }


  // ---------------------------------------------------------
  // 5. COPYRIGHT YEAR
  //    Auto-fills the footer year so it never goes stale.
  // ---------------------------------------------------------

  function updateCopyrightYear() {
    var yearSpan = document.getElementById('footer-year');
    if (yearSpan) {
      yearSpan.textContent = new Date().getFullYear();
    }
  }


  // ---------------------------------------------------------
  // HELPER: Get folder depth from root
  //    Returns 0 for root-level pages, 1 for /pages/*, etc.
  //    This lets us build the right relative path to components.
  // ---------------------------------------------------------

  function getPathDepth() {
    var path = window.location.pathname;
    // This project serves pages from either root or /pages.
    if (/\/pages\//i.test(path)) return 1;
    return 0;
  }

});


// =============================================================
// UTILITY FUNCTIONS (available globally for other page scripts)
// =============================================================

/**
 * Shows a toast notification at the bottom-right of the screen.
 * Make sure you have a <div id="toast" class="toast"></div> in your page,
 * or this function will create one for you.
 *
 * @param {string} message - Text to display
 * @param {string} type    - 'success', 'warning', 'danger', or '' for default
 * @param {number} duration - How long to show (ms), default 3000
 */
function showToast(message, type, duration) {
  type = type || '';
  duration = duration || 3000;

  // Find or create the toast element
  var toast = document.getElementById('toast');
  if (!toast) {
    toast = document.createElement('div');
    toast.id = 'toast';
    toast.className = 'toast';
    document.body.appendChild(toast);
  }

  // Reset classes and set content
  toast.className = 'toast' + (type ? ' toast-' + type : '');
  toast.textContent = message;

  // Show it
  requestAnimationFrame(function () {
    toast.classList.add('show');
  });

  // Auto-hide after duration
  setTimeout(function () {
    toast.classList.remove('show');
  }, duration);
}


/**
 * Formats a number as currency.
 * e.g. formatCurrency(2400) → "$2,400.00"
 *
 * @param {number} amount
 * @param {string} currency - Currency code, default 'USD'
 * @returns {string}
 */
function formatCurrency(amount, currency) {
  currency = currency || 'USD';
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: currency
  }).format(amount);
}


/**
 * Simple time-ago formatter.
 * e.g. timeAgo(new Date('2026-03-10')) → "1 day ago"
 *
 * @param {Date|string} date
 * @returns {string}
 */
function timeAgo(date) {
  var now = new Date();
  var past = new Date(date);
  var seconds = Math.floor((now - past) / 1000);

  var intervals = [
    { label: 'year', seconds: 31536000 },
    { label: 'month', seconds: 2592000 },
    { label: 'week', seconds: 604800 },
    { label: 'day', seconds: 86400 },
    { label: 'hour', seconds: 3600 },
    { label: 'minute', seconds: 60 }
  ];

  for (var i = 0; i < intervals.length; i++) {
    var count = Math.floor(seconds / intervals[i].seconds);
    if (count >= 1) {
      return count + ' ' + intervals[i].label + (count > 1 ? 's' : '') + ' ago';
    }
  }
  return 'just now';
}
