const roles = window.Roles;

document.getElementById('loadUsersBtn').addEventListener('click', async () => {
    try {
        const response = await fetch('/api/crew/user');
        const users = await response.json();
        const apiDataDiv = document.getElementById('apiData');
        apiDataDiv.innerHTML = '';

        for (const user of users) {
            const userDiv = createUserDiv(user);
            apiDataDiv.appendChild(userDiv);
            await handleUserBanAndWarn(user, userDiv);
        }
    } catch (error) {
        console.error('Ошибка: ', error);
    }
});

async function handleUserBanAndWarn(user, userDiv) {
    try {
        const banResponse = await fetch(`/api/crew/user/ban/${user.id}`);
        const isBanned = await banResponse.json();

        if (!isBanned) {
            const warnResponse = await fetch(`/api/crew/user/warn/${user.id}`);
            const warns = await warnResponse.json();
            addWarnButton(user.id, warns, userDiv);
            addBanButton(user.id, userDiv);

            if (roles.includes("Admin")) {
                addModerButton(user.id, userDiv);
            }
        } else {
            addUnbanButton(user.id, userDiv);
        }
    } catch (error) {
        console.error('Ошибка: ', error);
    }
}

function createUserDiv(user) {
    const userDiv = document.createElement('div');
    userDiv.setAttribute('id', user.id);
    userDiv.setAttribute('class', 'first');

    const userNameDiv = document.createElement('div');
    userNameDiv.textContent = user.userName;
    userDiv.appendChild(userNameDiv);

    return userDiv;
}

function addWarnButton(userId, warns, userDiv) {
    const userWarn = document.createElement('button');
    userWarn.textContent = `Warn (${warns}/3)`;
    userWarn.addEventListener('click', () => addWarnToUser(userId));
    userDiv.appendChild(userWarn);
}

function addBanButton(userId, userDiv) {
    const userBan = document.createElement('button');
    userBan.textContent = "Ban";
    userBan.addEventListener('click', () => addBanToUser(userId));
    userDiv.appendChild(userBan);
}

function addModerButton(userId, userDiv) {
    const userAddModer = document.createElement('button');
    userAddModer.textContent = "Made moder";
    userAddModer.addEventListener('click', () => madeModerFromUser(userId));
    userDiv.appendChild(userAddModer);
}

function addUnbanButton(userId, userDiv) {
    const userUnban = document.createElement('button');
    userUnban.textContent = "Unban";
    userUnban.addEventListener('click', () => unBanUser(userId));
    userDiv.appendChild(userUnban);
}

async function addWarnToUser(userId) {
    try {
        const response = await fetch(`/api/crew/user/warn/${userId}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json;charset=utf-8' }
        });
        const warns = await response.json();

        const userDiv = document.getElementById(userId);
        if (warns === 3) {
            clearUserButtons(userDiv);
            addUnbanButton(userId, userDiv);
        } else {
            userDiv.children[1].textContent = `Warn (${warns}/3)`;
        }
    } catch (error) {
        console.error('Ошибка при добавлении предупреждения: ', error);
    }
}

async function addBanToUser(userId) {
    try {
        const response = await fetch(`/api/crew/user/ban/${userId}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json;charset=utf-8' }
        });
        const ban = await response.json();

        if (ban) {
            const userDiv = document.getElementById(userId);
            clearUserButtons(userDiv);
            addUnbanButton(userId, userDiv);
        }
    } catch (error) {
        console.error('Ошибка при выдаче бана: ', error);
    }
}

async function unBanUser(userId) {
    try {
        const response = await fetch(`/api/crew/user/unban/${userId}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json;charset=utf-8' }
        });
        const ban = await response.json();

        if (ban) {
            const userDiv = document.getElementById(userId);
            clearUserButtons(userDiv);
            addWarnButton(userId, 0, userDiv);
            addBanButton(userId, userDiv);

            if (roles.includes("Admin")) {
                addModerButton(userId, userDiv);
            }
        }
    } catch (error) {
        console.error('Ошибка при снятии бана: ', error);
    }
}

function clearUserButtons(userDiv) {
    const buttons = userDiv.querySelectorAll('button');
    buttons.forEach(button => button.remove());
}