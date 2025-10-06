// wwwroot/js/site.js

// 1. Function for Mouse Over (Hover) Event
function mouseOverEffect(element) {
    // Already defined in CSS using :hover, but this function can add custom JS logic
    // For demonstration, let's change the text color temporarily via JS
    element.style.color = 'darkred';
    // The CSS transform and box-shadow handle the primary visual effect
}

// 2. Function for Mouse Out Event
function mouseOutEffect(element) {
    // Restore original text color
    element.style.color = ''; // Reverts to the CSS default color
}

// 3. JavaScript to Produce HTML5 Elements (Run on Page Load)
document.addEventListener('DOMContentLoaded', function() {
    const generateButton = document.getElementById('generate-btn');
    const dynamicContentArea = document.getElementById('dynamic-content-area');

    if (generateButton && dynamicContentArea) {
        generateButton.addEventListener('click', function() {
            // Remove existing fact if present
            const existingFact = document.getElementById('ai-fact');
            if (existingFact) {
                existingFact.remove();
            }

            // Data for the facts
            const facts = [
                "AI-powered systems can predict patient deterioration 48 hours in advance.",
                "Generative AI has helped reduce preclinical drug development timelines by up to 12 months.",
                "AI is being used to create synthetic, yet realistic, medical images for training purposes."
            ];

            // Select a random fact
            const randomFact = facts[Math.floor(Math.random() * facts.length)];

            // Create new HTML5 elements
            const factParagraph = document.createElement('p'); // <p> element
            factParagraph.id = 'ai-fact';
            factParagraph.textContent = 'Fact: ' + randomFact;
            factParagraph.classList.add('mt-3', 'alert', 'alert-info'); // Add Bootstrap styling

            // Append elements
            dynamicContentArea.appendChild(factParagraph);
        });
    }
});