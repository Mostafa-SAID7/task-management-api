(function () {
  function buildNav() {
    if (document.getElementById('swagger-docs-nav')) return;

    var nav = document.createElement('div');
    nav.id = 'swagger-docs-nav';
    nav.innerHTML =
      '<span class="swagger-nav-label">&#8592; Docs</span>' +
      '<a class="swagger-nav-home" href="/">Overview</a>' +
      '<span class="swagger-nav-sep">|</span>' +
      '<a href="/api-docs.html">API Docs</a>' +
      '<span class="swagger-nav-sep">|</span>' +
      '<a href="/architecture.html">Architecture</a>' +
      '<span class="swagger-nav-sep">|</span>' +
      '<a href="/features.html">Features</a>';

    var topbar = document.querySelector('.swagger-ui .topbar');
    if (topbar) {
      topbar.parentNode.insertBefore(nav, topbar);
    } else {
      document.body.insertBefore(nav, document.body.firstChild);
    }
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', buildNav);
  } else {
    buildNav();
  }
})();
