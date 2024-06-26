document.getElementById('loadUsersBtn').addEventListener('click', function () {
    var roles = window.Roles;

    fetch('/api/crew/user')
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

                return fetch(`/api/crew/user/ban/${user.id}`)
                    .then(response => response.json())
                    .then(ban => {
                        if (!ban) {
                            return fetch(`/api/crew/user/warn/${user.id}`)
                                .then(response => response.json())
                                .then(warns => {
                                    const userWarn = document.createElement('button');
                                    userWarn.textContent = `Warn (${warns}/3)`;
                                    userWarn.addEventListener('click', () => {
                                        addWarnToUser(user.id);
                                    });
                                    userDiv.appendChild(userWarn);

                                    const userBan = document.createElement('button');
                                    userBan.textContent = "Ban";
                                    userBan.addEventListener('click', () => {
                                        addBanToUser(user.id);
                                    });
                                    userDiv.appendChild(userBan);

                                    if (roles.includes("Admin")) {
                                        const userAddModer = document.createElement('button');
                                        userAddModer.textContent = "Made moder";
                                        userAddModer.addEventListener('click', () => {
                                            madeModerFromUser(user.id);
                                        });
                                        userDiv.appendChild(userAddModer);
                                    }
                                });
                        } else {
                            const userBan = document.createElement('button');
                            userBan.textContent = "Unban";
                            userBan.addEventListener('click', () => {
                                unBanUser(user.id);
                            });
                            userDiv.appendChild(userBan);
                        }
                    });
            })).catch(error => console.error('������: ', error));
        }).catch(error => console.error('������: ', error));
});

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
            console.error('������ ��� ���������� ���������.: ', error);
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
            console.error('������ ��� ������ ����: ', error);
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
            console.error('������ ��� ������ ����: ', error);
        });
}