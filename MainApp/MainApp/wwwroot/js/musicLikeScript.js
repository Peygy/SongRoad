document.addEventListener('DOMContentLoaded', function () {
    function sendPostRequest(url, callback) {
        fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({})
        })
            .then(response => response.json())
            .then(data => {
                if (data === true) {
                    callback();
                } else {
                    console.error('Failed to like/unlike track');
                }
            })
            .catch(error => {
                console.error('Error:', error);
            });
    }

    // Load initial state from sessionStorage
    document.querySelectorAll('a[data-form-method="post"]').forEach(link => {
        const parentDiv = link.closest('div[id^="track-"]');
        if (parentDiv) {
            const trackId = parentDiv.id;
            const savedState = sessionStorage.getItem(`track-liked-${trackId}`);
            const savedHref = sessionStorage.getItem(`track-href-${trackId}`);
            if (savedState !== null && savedHref !== null) {
                const isLiked = savedState === 'true';
                link.setAttribute('data-liked', isLiked);
                link.href = savedHref;
                link.innerText = isLiked ? 'НеЛайк' : 'Лайк';
            }
        }
    });

    document.querySelectorAll('a[data-form-method="post"]').forEach(link => {
        link.addEventListener('click', function (event) {
            event.preventDefault();
            const url = this.href;
            const isLiked = this.getAttribute('data-liked') === 'true';
            const parentDiv = this.closest('div[id^="track-"]');
            if (parentDiv) {
                const trackId = parentDiv.id;

                sendPostRequest(url, () => {
                    if (isLiked) {
                        const newHref = this.href.replace('unlike', 'like');
                        this.href = newHref;
                        this.innerText = 'Лайк';
                        this.setAttribute('data-liked', 'false');
                        sessionStorage.setItem(`track-liked-${trackId}`, 'false');
                        sessionStorage.setItem(`track-href-${trackId}`, newHref);
                    } else {
                        const newHref = this.href.replace('like', 'unlike');
                        this.href = newHref;
                        this.innerText = 'НеЛайк';
                        this.setAttribute('data-liked', 'true');
                        sessionStorage.setItem(`track-liked-${trackId}`, 'true');
                        sessionStorage.setItem(`track-href-${trackId}`, newHref);
                    }
                });
            }
        });
    });
});