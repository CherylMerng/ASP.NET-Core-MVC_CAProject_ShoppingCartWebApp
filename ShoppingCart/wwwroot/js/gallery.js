window.onload = function () {
    setup();
}

function setup() {

    let elem = document.getElementsByClassName("add-to-cart");

    for (var i = 0; i < elem.length; i++) {
        elem[i].addEventListener("click", updateCart)
    }
}

function displayresults(status) {
    elem = document.getElementById("total");
    elem.innerHTML = Number(elem.innerHTML) + 1;
}

function updateCart(event) {
    elem = event.target;
    product = elem.id;

    let xhr = new XMLHttpRequest();
    xhr.open("POST", "/Home/clickAddtoCart");
    xhr.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");

    xhr.onreadystatechange = function () {
        if (this.readyState == XMLHttpRequest.DONE) {
            if (this.status == 200) {
                displayresults(JSON.parse(this.responseText));
            }
        }
    }

    xhr.send("product=" + product);
}