document.addEventListener('DOMContentLoaded', function () {
    const audioPlayer = document.getElementById('audio-player');
    const trackTitle = document.getElementById('track-title');
    const trackCreator = document.getElementById('track-creator');
    const trackStyle = document.getElementById('track-style');
    const playButtons = document.querySelectorAll('.play-button');
    let currentButton = null;
    let title = null;

    playButtons.forEach(button => {
        button.addEventListener('click', function () {
            const src = this.getAttribute('data-src');

            if (src && title !== this.getAttribute('data-title')) {
                sessionStorage.removeItem(title);
            }
            title = this.getAttribute('data-title');

            if (currentButton && currentButton !== this) {
                currentButton.textContent = '▶️ Play';
            }

            if (audioPlayer.src !== src && this.textContent !== '⏹ Stop') {
                const savedTime = sessionStorage.getItem(title) || 0;
                audioPlayer.src = src;
                audioPlayer.currentTime = savedTime;
                audioPlayer.play();
                this.textContent = '⏹ Stop';
            } else if (audioPlayer.paused) {
                audioPlayer.play();
                this.textContent = '⏹ Stop';
            } else {
                sessionStorage.setItem(title, audioPlayer.currentTime);
                audioPlayer.pause();
                this.textContent = '▶️ Play';
            }

            currentButton = this;
            trackTitle.textContent = `Title: ${this.getAttribute('data-title')}`;
            trackCreator.textContent = `Creator: ${this.getAttribute('data-creator')}`;
            trackStyle.textContent = `Style: ${this.getAttribute('data-style')}`;
        });
    });

    audioPlayer.addEventListener('pause', function () {
        if (currentButton && !audioPlayer.ended) {
            currentButton.textContent = '▶️ Play';
            sessionStorage.setItem(title, audioPlayer.currentTime);
        }
    });

    audioPlayer.addEventListener('play', function () {
        if (currentButton) {
            currentButton.textContent = '⏹ Stop';
        }
    });

    audioPlayer.addEventListener('ended', function () {
        if (currentButton) {
            currentButton.textContent = '▶️ Play';
            sessionStorage.removeItem(title);
        }
    });

    audioPlayer.addEventListener('timeupdate', function () {
        if (title && audioPlayer.paused) {
            sessionStorage.setItem(title, audioPlayer.currentTime);
        }
    });

    window.addEventListener('beforeunload', function () {
        sessionStorage.removeItem(title);
    });
});