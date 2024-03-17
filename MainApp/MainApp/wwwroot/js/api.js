async function deletePart(id, table) {
    const response = await fetch("/api/part/remove/"+id+"&"+table, {
        method: "DELETE",
        headers: { "Accept": "application/json" }
    });
    if (response.ok) {
        const partId = await response.json();
        document.getElementById(partId).remove();
    }
    else {
        const error = await response.json();
        console.log(error.message);
    }
}

async function CheckExist(id, parentTable) {
    let title = document.getElementsByClassName("form__main-input");
    const response = await fetch("/api/check/title/"+id+"&"+parentTable+"&"+title[0].value, {
        method: "POST",
        headers: { "Accept": "application/json" }
    });
    if (response.ok) {
        const checking = await response.json();
        let error = document.getElementById('titleerror');
        let button = document.getElementById('button');

        if(checking)
        {
            error.style.display = "flex";
            error.style.justifyContent = "center";
            button.disabled = true;
        }
        else
        {
            error.style.display = "none";
            button.disabled = false;
        }
    }
    else {
        const error = await response.json();
        console.log(error.message);
    }

}

