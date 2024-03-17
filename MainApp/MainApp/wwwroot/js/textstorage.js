if (window.sessionStorage){
    let elements = document.querySelectorAll('.session');

    for (let i = 0, length = elements.length; i < length; i++) {
        let name = elements[i].getAttribute('id');
   
        elements[i].value = sessionStorage.getItem(name) || elements[i].value;
   
        elements[i].onkeyup = function() {
            sessionStorage.setItem(name, elements[i].value);
        };
    }
}


function ValidCheck(){
    let name = document.getElementsByClassName('form__main-input');
    let error = document.getElementById('validerror');
    let button = document.getElementById('button');

    if(name[0].value === '')
    {
        if(document.getElementsByClassName('form__main-textarea').length === 0)
        {
            error.style.display = "flex";
            error.style.justifyContent = "center";
        }else{
            error.style.display = "inline-flex";
        }
        button.disabled = true;
    }else{
        error.style.display = "none";
        button.disabled = false;
    }
}


function RemoveStorage(){
    let elements = document.querySelectorAll('.session');
   
    for (let i = 0, length = elements.length; i < length; i++) {
        let name = elements[i].getAttribute('id');
        sessionStorage.removeItem(name);
    }
}