// Main JavaScript for Bookstore API Documentation

document.addEventListener('DOMContentLoaded', function() {
    initializeNavigation();
    initializeSearch();
    initializeSidebar();
});

// Initialize navigation active state
function initializeNavigation() {
    const currentPage = window.location.pathname.split('/').pop() || 'index.html';
    const navLinks = document.querySelectorAll('.navbar a');
    
    navLinks.forEach(link => {
        const href = link.getAttribute('href');
        if (href === currentPage || (currentPage === '' && href === 'index.html')) {
            link.classList.add('active');
        } else {
            link.classList.remove('active');
        }
    });
}

// Initialize search functionality
function initializeSearch() {
    const searchInput = document.getElementById('searchInput');
    if (!searchInput) return;

    searchInput.addEventListener('input', function(e) {
        const searchTerm = e.target.value.toLowerCase();
        const sidebarLinks = document.querySelectorAll('.sidebar-link');
        
        sidebarLinks.forEach(link => {
            const text = link.textContent.toLowerCase();
            if (text.includes(searchTerm) || searchTerm === '') {
                link.style.display = 'flex';
            } else {
                link.style.display = 'none';
            }
        });
    });
}

// Initialize sidebar link active state
function initializeSidebar() {
    const sidebarLinks = document.querySelectorAll('.sidebar-link');
    
    sidebarLinks.forEach(link => {
        link.addEventListener('click', function() {
            sidebarLinks.forEach(l => l.classList.remove('active'));
            this.classList.add('active');
        });
    });

    // Set active link based on scroll position
    window.addEventListener('scroll', function() {
        updateActiveSidebarLink();
    });
}

// Update active sidebar link based on scroll position
function updateActiveSidebarLink() {
    const sections = document.querySelectorAll('section');
    const sidebarLinks = document.querySelectorAll('.sidebar-link');
    
    let current = '';
    sections.forEach(section => {
        const sectionTop = section.offsetTop;
        if (pageYOffset >= sectionTop - 200) {
            current = section.getAttribute('id');
        }
    });

    sidebarLinks.forEach(link => {
        link.classList.remove('active');
        if (link.getAttribute('href') === '#' + current) {
            link.classList.add('active');
        }
    });
}

// Smooth scroll for anchor links
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function(e) {
        const href = this.getAttribute('href');
        if (href !== '#') {
            e.preventDefault();
            const target = document.querySelector(href);
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        }
    });
});

// Copy code to clipboard functionality
function initializeCodeCopy() {
    const codeBlocks = document.querySelectorAll('.docbox code');
    
    codeBlocks.forEach(block => {
        const copyButton = document.createElement('button');
        copyButton.textContent = 'Copy';
        copyButton.className = 'copy-btn';
        copyButton.style.cssText = `
            position: absolute;
            top: 10px;
            right: 10px;
            padding: 5px 10px;
            background: var(--primary-color);
            color: white;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            font-size: 0.85rem;
            opacity: 0;
            transition: opacity 0.3s ease;
        `;
        
        block.parentElement.style.position = 'relative';
        block.parentElement.appendChild(copyButton);
        
        block.parentElement.addEventListener('mouseenter', () => {
            copyButton.style.opacity = '1';
        });
        
        block.parentElement.addEventListener('mouseleave', () => {
            copyButton.style.opacity = '0';
        });
        
        copyButton.addEventListener('click', () => {
            const text = block.textContent;
            navigator.clipboard.writeText(text).then(() => {
                copyButton.textContent = 'Copied!';
                setTimeout(() => {
                    copyButton.textContent = 'Copy';
                }, 2000);
            });
        });
    });
}

// Initialize code copy on page load
document.addEventListener('DOMContentLoaded', initializeCodeCopy);

// Add keyboard shortcuts
document.addEventListener('keydown', function(e) {
    // Ctrl/Cmd + K for search
    if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
        e.preventDefault();
        const searchInput = document.getElementById('searchInput');
        if (searchInput) {
            searchInput.focus();
        }
    }
    
    // Escape to clear search
    if (e.key === 'Escape') {
        const searchInput = document.getElementById('searchInput');
        if (searchInput && searchInput === document.activeElement) {
            searchInput.value = '';
            searchInput.dispatchEvent(new Event('input'));
        }
    }
});

// Add animation to elements on scroll
function initializeScrollAnimations() {
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -100px 0px'
    };

    const observer = new IntersectionObserver(function(entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.animation = 'fadeIn 0.6s ease-out forwards';
                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);

    document.querySelectorAll('.feature-card, .getting-started-card, .endpoint-card').forEach(el => {
        observer.observe(el);
    });
}

document.addEventListener('DOMContentLoaded', initializeScrollAnimations);

// Mobile menu toggle
function initializeMobileMenu() {
    const navbar = document.querySelector('.navbar');
    if (!navbar) return;

    const headerContainer = document.querySelector('.header-container');
    if (!headerContainer) return;

    // Create the hamburger toggle button
    const toggleBtn = document.createElement('button');
    toggleBtn.className = 'mobile-menu-toggle';
    toggleBtn.setAttribute('aria-label', 'Toggle navigation menu');
    toggleBtn.setAttribute('aria-expanded', 'false');
    toggleBtn.innerHTML = '<span></span><span></span><span></span>';
    headerContainer.appendChild(toggleBtn);

    // Create the backdrop overlay
    const backdrop = document.createElement('div');
    backdrop.className = 'nav-backdrop';
    document.body.appendChild(backdrop);

    function openMenu() {
        navbar.classList.add('active');
        toggleBtn.classList.add('active');
        backdrop.classList.add('active');
        toggleBtn.setAttribute('aria-expanded', 'true');
        document.body.style.overflow = 'hidden';
    }

    function closeMenu() {
        navbar.classList.remove('active');
        toggleBtn.classList.remove('active');
        backdrop.classList.remove('active');
        toggleBtn.setAttribute('aria-expanded', 'false');
        document.body.style.overflow = '';
    }

    // Toggle on hamburger click
    toggleBtn.addEventListener('click', function() {
        if (navbar.classList.contains('active')) {
            closeMenu();
        } else {
            openMenu();
        }
    });

    // Close on backdrop click
    backdrop.addEventListener('click', closeMenu);

    // Close when a nav link is clicked
    const navLinks = navbar.querySelectorAll('a');
    navLinks.forEach(function(link) {
        link.addEventListener('click', closeMenu);
    });

    // Close on Escape key
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape' && navbar.classList.contains('active')) {
            closeMenu();
        }
    });

    // Close menu if window is resized above mobile breakpoint
    window.addEventListener('resize', function() {
        if (window.innerWidth > 768 && navbar.classList.contains('active')) {
            closeMenu();
        }
    });
}

document.addEventListener('DOMContentLoaded', initializeMobileMenu);

// Utility function to highlight code syntax
function highlightCode() {
    const codeBlocks = document.querySelectorAll('.docbox code');
    codeBlocks.forEach(block => {
        // Basic syntax highlighting for JSON
        let html = block.innerHTML;
        html = html.replace(/(".*?")/g, '<span style="color: #90ee90;">$1</span>');
        html = html.replace(/(\d+)/g, '<span style="color: #ffa500;">$1</span>');
        block.innerHTML = html;
    });
}

document.addEventListener('DOMContentLoaded', highlightCode);

// Log initialization
console.log('Bookstore API Documentation loaded successfully');
