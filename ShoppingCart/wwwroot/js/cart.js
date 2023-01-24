window.onload = function () {
    QuantityControl();
}

function QuantityControl() {
    let quant = document.getElementsByClassName("InputQuant");

    for (var i = 0; i < quant.length; i++) {
        quant[i].addEventListener("change", Update);
    }
}

function Update(event) {
    Box = event.target;

    productid = Box.id;
 
    quantity = Box.value;

    if (Number(quantity) < 1) {
        return;
    }

    priceelm = document.getElementById('P0+' + productid);
    let price = Number(priceelm.value);

    oldpriceelm = document.getElementById('P1+' + productid);
    let oldprice = Number(oldpriceelm.value);

    totalelm = document.getElementById('total');
    let total = Number(totalelm.value);

    let newPrice = price * Number(quantity);
    
    oldpriceelm.value = newPrice;

    totalPrice = (total - oldprice) + newPrice;

    totalelm.value = totalPrice;

    UpdateAjax(productid, quantity);
}

function deletefromcart(productid, quantity, productname) {   
    priceelmx = document.getElementById('P0+' + productid);
    let pricex = Number(priceelmx.value);

    oldpriceelmx = document.getElementById('P1+' + productid);
    let oldpricex = Number(oldpriceelmx.value);

    totalelmx = document.getElementById('total');
    let totalx = Number(totalelmx.value);

    totalPricex = (totalx - oldpricex);

    totalelmx.value = totalPricex;

    document.getElementById(productname).remove();

    if (totalPricex == 0) {
        document.getElementById("cstatus").innerHTML = "Your cart is currently empty.";
    }

    UpdateAjax(productid, quantity);
}

function UpdateAjax(productid, quantity) {
    let ajax = new XMLHttpRequest();

    ajax.open("POST", "/Home/UpdateQuantity");
    ajax.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");

    ajax.onreadystatechange = function () {
        if (this.readyState == XMLHttpRequest.DONE) {
            if (this.status == 200) {
                return this.responseText;
            }
        }
    }

    ajax.send("productid=" + productid + "&quantity=" + quantity);
}
