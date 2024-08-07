﻿document.getElementById('loadMusicBtn').addEventListener('click', function () {
    loadMusic();
});

let allTracks = [];

function loadMusic(trackId = '', style = '') {
    let url = '/api/crew/music';
    if (trackId) {
        url += `/${trackId}`;
    } else if (style) {
        url += `/filter/style/${style}`;
    }

    fetch(url)
        .then(response => response.json())
        .then(data => {
            if (allTracks.length === 0) {
                allTracks = data;
            }

            const apiDataDiv = document.getElementById('apiData');
            // Удаление всех элементов, кроме элементов с id searchInput и styleFilter
            Array.from(apiDataDiv.children).forEach(child => {
                if (child.id !== 'searchInput' && child.id !== 'styleFilter') {
                    apiDataDiv.removeChild(child);
                }
            });

            if (!document.getElementById('searchInput') && !document.getElementById('styleFilter')) {
                createSearchAndFilterBars(apiDataDiv);
            }

            displayTracks(apiDataDiv, data)
        }).catch(error => console.error('Ошибка: ', error));
};

function displayTracks(apiDataDiv, tracks) {
    if (!Array.isArray(tracks)) {
        createTrackDiv(apiDataDiv, tracks);
        return;
    }

    Promise.all(tracks.map(musicTrack => {
        createTrackDiv(apiDataDiv, musicTrack);
    })).catch(error => console.error('Ошибка: ', error));
}

function createTrackDiv(apiDataDiv, musicTrack) {
    const musicTrackDiv = document.createElement('div');
    musicTrackDiv.setAttribute('id', musicTrack.musicId);
    musicTrackDiv.setAttribute('class', 'first');
    apiDataDiv.appendChild(musicTrackDiv);

    const userNameDiv = document.createElement('div');
    userNameDiv.innerHTML = `Название: ${musicTrack.title}; Стиль: ${musicTrack.style}`;
    musicTrackDiv.appendChild(userNameDiv);

    const userBan = document.createElement('button');
    userBan.textContent = "Удалить";
    userBan.addEventListener('click', () => {
        deleteMusicTrack(musicTrack.musicId);
    });
    musicTrackDiv.appendChild(userBan);
}

function createSearchAndFilterBars(apiDataDiv) {
    const searchInput = document.createElement('input');
    searchInput.setAttribute('type', 'text');
    searchInput.setAttribute('id', 'searchInput');
    searchInput.setAttribute('placeholder', 'Search for a track...');
    apiDataDiv.appendChild(searchInput);

    // Create and add style filter dropdown
    const styleFilter = document.createElement('select');
    styleFilter.setAttribute('id', 'styleFilter');
    const defaultOption = document.createElement('option');
    defaultOption.value = '';
    defaultOption.textContent = 'all';
    styleFilter.appendChild(defaultOption);
    apiDataDiv.appendChild(styleFilter);

    // Load styles for filter dropdown
    loadStyles();

    searchInput.addEventListener('keypress', function (event) {
        if (event.key === 'Enter') {
            const query = searchInput.value;
            console.log(query);
            const trackId = getTrackIdByTitle(query);
            if (trackId) {
                loadMusic(trackId);
            } else {
                alert("Трек не найден!")
                loadMusic('', '');
            }
        }
    });

    styleFilter.addEventListener('change', function () {
        const style = styleFilter.value;
        loadMusic('', style);
    });
}

function getTrackIdByTitle(title) {
    const track = allTracks.find(track => track.title.toLowerCase() === title.toLowerCase());
    return track ? track.musicId : null;
}

function loadStyles() {
    fetch('/api/crew/music/styles')
        .then(response => response.json())
        .then(styles => {
            const styleFilter = document.getElementById('styleFilter');
            styles.forEach(style => {
                const option = document.createElement('option');
                option.value = style.name;
                option.textContent = style.name;
                styleFilter.appendChild(option);
            });
        }).catch(error => console.error('Ошибка загрузки стилей: ', error));
}

function deleteMusicTrack(musicId) {
    return fetch(`/api/crew/music/${musicId}`, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json;charset=utf-8'
        }
    })
        .then(response => response.json())
        .then(result => {
            if (result) {
                const musicTrackDiv = document.getElementById(`${musicId}`);
                musicTrackDiv.remove()
            }
        }).catch(error => {
            console.error('Ошибка при удалении музыкального трека: ', error);
        });
}