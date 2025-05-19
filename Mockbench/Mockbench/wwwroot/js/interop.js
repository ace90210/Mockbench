
window.appInterops = {
    highlightElement: function (elementId, language) {
        // Ensure hljs is loaded
        if (typeof hljs === 'undefined') {
            console.error('Highlight.js (hljs) not loaded.');
            return;
        }

        try {
            let element = document.getElementById(elementId);
            if (element) {
                // Clear previous highlighting if any (hljs might add its own spans)
                // A common approach is to set textContent first if the content is dynamic
                // However, Blazor re-renders the content, so this might not be strictly needed
                // if the `<code>` tag's content is correctly bound.

                // If a language is specified and supported by hljs
                if (language && hljs.getLanguage(language)) {
                    element.className = 'language-' + language; // Ensure class is set for hljs
                    hljs.highlightElement(element);
                } else {
                    // Fallback to auto-detection if language is not specified or not supported
                    // Note: auto-detection can be less accurate.
                    hljs.highlightElement(element);
                }
                console.log('Highlighting applied to:', elementId, 'with language:', language || '(auto)');
            } else {
                console.warn('Element with ID not found for highlighting:', elementId);
            }
        } catch (e) {
            console.error('Error during syntax highlighting:', e);
        }
    },

    prettyPrintJson: function (jsonString) {
        try {
            if (!jsonString || jsonString.trim() === "") {
                return ""; // Handle empty or whitespace-only strings
            }
            const jsonObj = JSON.parse(jsonString);
            return JSON.stringify(jsonObj, null, 2); // 2 spaces for indentation
        } catch (e) {
            console.error('Error parsing or stringifying JSON for pretty print:', e);
            return "Error: Invalid JSON - " + e.message; // Return error message instead of original string to indicate failure
        }
    }
};
