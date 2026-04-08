// Search functionality for Bookstore API Documentation

class DocumentationSearch {
    constructor() {
        this.searchIndex = [];
        this.initializeSearch();
    }

    initializeSearch() {
        this.buildSearchIndex();
        this.setupSearchListeners();
    }

    buildSearchIndex() {
        // Index all sections and content
        const sections = document.querySelectorAll('section');
        
        sections.forEach(section => {
            const id = section.getAttribute('id');
            const title = section.querySelector('h2')?.textContent || '';
            const content = section.textContent;
            
            this.searchIndex.push({
                id,
                title,
                content,
                element: section
            });
        });
    }

    setupSearchListeners() {
        const searchInput = document.getElementById('searchInput');
        if (!searchInput) return;

        searchInput.addEventListener('input', (e) => {
            this.performSearch(e.target.value);
        });

        // Highlight search term in results
        searchInput.addEventListener('keyup', (e) => {
            if (e.key === 'Enter') {
                this.highlightSearchResults(e.target.value);
            }
        });
    }

    performSearch(query) {
        if (!query.trim()) {
            this.showAllSections();
            return;
        }

        const results = this.searchIndex.filter(item => {
            const searchTerm = query.toLowerCase();
            return item.title.toLowerCase().includes(searchTerm) ||
                   item.content.toLowerCase().includes(searchTerm);
        });

        this.displaySearchResults(results);
    }

    displaySearchResults(results) {
        const sections = document.querySelectorAll('section');
        
        sections.forEach(section => {
            const id = section.getAttribute('id');
            const found = results.some(r => r.id === id);
            section.style.display = found ? 'block' : 'none';
        });
    }

    showAllSections() {
        const sections = document.querySelectorAll('section');
        sections.forEach(section => {
            section.style.display = 'block';
        });
    }

    highlightSearchResults(query) {
        if (!query.trim()) return;

        const sections = document.querySelectorAll('section');
        const searchTerm = query.toLowerCase();

        sections.forEach(section => {
            const paragraphs = section.querySelectorAll('p, h3, h4, li');
            paragraphs.forEach(para => {
                const text = para.textContent;
                if (text.toLowerCase().includes(searchTerm)) {
                    const regex = new RegExp(`(${query})`, 'gi');
                    para.innerHTML = text.replace(regex, '<mark style="background-color: rgba(30, 144, 255, 0.3); padding: 2px 4px; border-radius: 3px;">$1</mark>');
                }
            });
        });
    }
}

// Initialize search when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    new DocumentationSearch();
});

// Global search function
function globalSearch(query) {
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.value = query;
        searchInput.dispatchEvent(new Event('input'));
    }
}

// Export for use in other scripts
if (typeof module !== 'undefined' && module.exports) {
    module.exports = DocumentationSearch;
}
