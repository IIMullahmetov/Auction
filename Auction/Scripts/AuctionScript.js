$.connection.hub.start().done(function () {
	//$.connection.AuctionHub.server.AuctionStart();
	var email = $("#email").text();
	$.connection.AuctionHub.server.Connect(email);
});
$.connection.AuctionHub.client.TimeReceiver = function (time) {
	$("#time").text("Оставшееся время: " + time);
}	

$.connection.AuctionHub.client.ReceiveInfo = function (price, email, product) {
	$("#price").text("Текущая  цена: " + price);
	$("#user").text("Покупатель: " + email);
	$("#product").text("Название товара: " + product);
}

$("#to_offer").click(function () {
	var price = $("#price").val();
	var offer = $("#offer").val();
	$("#offer").val("");
	if (offer - price > 4) {
		$.connection.AuctionHub.server.ToOffer(offer);
	} else {
		alert("Предложение должно быть больше стоимости на 5")
	}
});

$("#start_a").click(function () {
	$.connection.AuctionHub.server.AuctionStart("ProductA").done(function (info) {
		alert(info);
	});
});

$("#start_b").click(function () {
	$.connection.AuctionHub.server.AuctionStart("ProductB").done(function (info) {
		alert(info);
	});
});

$.connection.AuctionHub.client.AuctionEnd = function (info) {
	alert(info);
	$("#price").val(100);
	$("#user").val("");
	$("#product").val("");
}

$.connection.AuctionHub.client.DisableButtons = function () {
	$("#start_a").prop("disabled", true);
	$("#start_b").prop("disabled", true);
}

$.connection.AuctionHub.client.EnableButtons = function () {
	$("#start_a").prop("disabled", false);
	$("#start_b").prop("disabled", false);
}