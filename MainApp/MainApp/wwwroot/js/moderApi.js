function addWarnToUser(userId) {
    return fetch(`/api/crew/user/warn/${userId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json;charset=utf-8'
        }
    })
        .then(response => response.json())
        .then(warns => {
            const userDiv = document.getElementById(`${userId}`);
            if (warns === 3) {
                const buttons = userDiv.querySelectorAll('button');
                buttons.forEach(button => {
                    button.remove();
                });

                const userBan = document.createElement('button');
                userBan.textContent = "Unban";
                userBan.addEventListener('click', () => {
                    unBanUser(userId);
                });
                userDiv.appendChild(userBan);
            } else {
                userDiv.children[1].textContent = `Warn (${warns}/3)`;
            }
        })
        .catch(error => {
            console.error('Ошибка при добавлении предупред.: ', error);
        });
}

function addBanToUser(userId) {
    return fetch(`/api/crew/user/ban/${userId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json;charset=utf-8'
        }
    })
        .then(response => response.json())
        .then(ban => {
            if (ban) {
                const userDiv = document.getElementById(`${userId}`);
                const buttons = userDiv.querySelectorAll('button');
                buttons.forEach(button => {
                    button.remove();
                });

                const userBan = document.createElement('button');
                userBan.textContent = "Unban";
                userBan.addEventListener('click', () => {
                    unBanUser(userId);
                });
                userDiv.appendChild(userBan);
            }
        })
        .catch(error => {
            console.error('Ошибка при выдаче бана: ', error);
        });
}

function unBanUser(userId) {
    return fetch(`/api/crew/user/unban/${userId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json;charset=utf-8'
        }
    })
        .then(response => response.json())
        .then(ban => {
            if (ban) {
                const userDiv = document.getElementById(`${userId}`);
                const buttons = userDiv.querySelectorAll('button');
                buttons.forEach(button => {
                    button.remove();
                });

                const userWarn = document.createElement('button');
                userWarn.textContent = `Warn (0/3)`;
                userWarn.addEventListener('click', () => {
                    addWarnToUser(userId);
                });
                userDiv.appendChild(userWarn);

                const userBan = document.createElement('button');
                userBan.textContent = "Ban";
                userBan.addEventListener('click', () => {
                    addBanToUser(userId);
                });
                userDiv.appendChild(userBan);
            }
        }).catch(error => {
            console.error('Ошибка при выдаче бана: ', error);
        });
}