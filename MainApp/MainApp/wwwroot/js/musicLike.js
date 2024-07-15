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

    document.querySelectorAll('a[data-form-method="post"]').forEach(link => {
        link.addEventListener('click', function (event) {
            event.preventDefault();
            const url = this.href;
            const isLiked = this.getAttribute('data-liked') === 'true';

            sendPostRequest(url, () => {
                if (isLiked) {
                    const newHref = this.href.replace('unlike', 'like');
                    this.href = newHref;
                    this.innerText = 'Лайк';
                    this.setAttribute('data-liked', 'false');
                } else {
                    const newHref = this.href.replace('like', 'unlike');
                    this.href = newHref;
                    this.innerText = 'НеЛайк';
                    this.setAttribute('data-liked', 'true');
                }
            });
        });
    });
});

window.addEventListener('pageshow', function (event) {
    if (event.persisted || performance.getEntriesByType("navigation")[0].type === 'back_forward') {
        window.location.reload();
    }
});