document.addEventListener('DOMContentLoaded', function() {
    const likeBtns = document.querySelectorAll('.like-btn');
    
    likeBtns.forEach(btn => {
        btn.addEventListener('click', async function() {
            // Check if user is authenticated (button would be disabled if not)
            if (this.disabled) {
                alert('Please log in to like images.');
                return;
            }

            const imageId = this.getAttribute('data-image-id');
            const likeCountSpan = this.querySelector('.like-count');
            
            try {
                const response = await fetch(`/AIImages/ToggleLike/${imageId}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    }
                });

                if (response.ok) {
                    const data = await response.json();
                    likeCountSpan.textContent = data.likeCount;
                    
                    // Toggle button styling
                    if (data.isLiked) {
                        this.classList.remove('btn-outline-danger');
                        this.classList.add('btn-danger');
                    } else {
                        this.classList.remove('btn-danger');
                        this.classList.add('btn-outline-danger');
                    }
                } else {
                    console.error('Server error:', response.status);
                    alert('Error updating like. Please try again.');
                }
            } catch (error) {
                console.error('Error:', error);
                alert('Error updating like. Please try again.');
            }
        });
    });
});