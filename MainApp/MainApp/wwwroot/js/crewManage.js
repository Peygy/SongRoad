document.getElementById('loadCrewBtn').addEventListener('click', function () {
    fetch('/api/crew/moder')
        .then(response => response.json())
        .then(data => {
            const apiDataDiv = document.getElementById('apiData');
            apiDataDiv.innerHTML = '';

            Promise.all(data.map(user => {
                const userDiv = document.createElement('div');
                userDiv.setAttribute('id', user.id);
                userDiv.setAttribute('class', 'first');
                apiDataDiv.appendChild(userDiv);

                const userNameDiv = document.createElement('div');
                userNameDiv.innerHTML = `${user.userName}`;
                userDiv.appendChild(userNameDiv);

                const userBan = document.createElement('button');
                userBan.textContent = "Remove";
                userBan.addEventListener('click', () => {
                    removeModerFromUser(user.id);
                });
                userDiv.appendChild(userBan);
            })).catch(error => console.error('Ошибка: ', error));
        }).catch(error => console.error('Ошибка: ', error));
});

function madeModerFromUser(userId) {
    return fetch(`/api/crew/moder/${userId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json;charset=utf-8'
        }
    })
        .then(response => response.json())
        .then(result => {
            if (result) {
                const userDiv = document.getElementById(`${userId}`);
                userDiv.remove()
            }
        }).catch(error => {
            console.error('Ошибка при выдаче полномочий модератора: ', error);
        });
}

function removeModerFromUser(userId) {
    return fetch(`/api/crew/moder/${userId}`, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json;charset=utf-8'
        }
    })
        .then(response => response.json())
        .then(result => {
            if (result) {
                const userDiv = document.getElementById(`${userId}`);
                userDiv.remove()
            }
        }).catch(error => {
            console.error('Ошибка при удалении полномочий модератора: ', error);
        });
}