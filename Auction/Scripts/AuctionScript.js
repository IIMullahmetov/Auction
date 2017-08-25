$.connection.hub.start().done(function () {
	//$.connection.AuctionHub.server.AuctionStart();
	var email = $("#email").text();
	$.connection.AuctionHub.server.Connect(email);
});
$.connection.AuctionHub.client.TimeReceiver = function (time) {
		$("#time").val(time);
}	

$.connection.AuctionHub.client.ReceiveInfo = function (price, email) {
	$("#price").val(price);
	$("#user").val(email);
}

$("#to_double").click(function () {
	$.connection.AuctionHub.server.ToDouble();
});

$("#to_offer").click(function () {
	var price = $("#price").val();
	var offer = $("#offer").val();
	if (offer - price >= 5) {
		$.connection.AuctionHub.server.ToOffer(offer);
	}
});

$("#to_connect").click(function () {
	$.connection.AuctionHub.server.AuctionStart();
});

$.connection.AuctionHub.client.AuctionEnd = function (price, email, time) {

}