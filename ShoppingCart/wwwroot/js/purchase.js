window.onload = function () {
}

function submitRating(productId, username, PurchaseDate) {

    var ID = productId.toString();
    var ID = ID.concat(PurchaseDate);
    var section = document.getElementById(ID);
    var ID1 = ID.concat(username);
    var rating = section.options[section.selectedIndex].value;
    var remarks = document.getElementById(ID1);
    if (rating == "0") {
        remarks.innerHTML = "You have not submitted any ratings for this purchase yet.";
    }
    else {
        let output = [];
        // Append all the filled whole stars
        for (var i = rating; i >= 1; i--)
            output.push('<i class="fa fa-star checked"></i>&nbsp;');

        // Fill the empty stars
        for (let i = (5 - rating); i >= 1; i--)
            output.push('<i class="fa fa-star"></i>&nbsp;');

        remarks.innerHTML = "<br>Your rating for this purchase is " + rating + ".<br>" + output.join('') + "<br><span class='sfont'>*If you would like to update your rating, please resubmit another rating.</span>";

    }
    let ajax = new XMLHttpRequest();

    ajax.open("POST", "/Home/SubmitRating");

    ajax.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");


    ajax.onreadystatechange = function () {
        if (this.readyState == XMLHttpRequest.DONE) {
            if (this.status == 200) {
                return this.responseText;
            }
        }
    }
    ajax.send("rating=" + rating + "&ProductId=" + productId + "&username=" + username + "&purchaseDate=" + PurchaseDate);

}