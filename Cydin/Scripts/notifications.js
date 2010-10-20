$(document).ready(function() {
	$("#notification-change-button").click (function() {
		$("#notification-summary").hide ();
		$("#notification-selector").show ();
	});
	$("#notification-done-button").click (function() {
		$("#notification-summary").show ();
		$("#notification-selector").hide ();
		refreshSummary ();
	});
	$("#notification-selector").find ("input").each (function(i) {
		$(this).click (function () {
			send (this);
		});
	});
	refreshSummary ();
});
    
function refreshSummary ()
{
	var hasNots = false;
	var nlist = $("#notification-summary-list");
	nlist.html ("");
	$("#notification-selector").find ("input").each (function(i) {
		if ($(this).attr("checked") != "") {
			if (hasNots)
    			nlist.append (", ");
			nlist.append ("<b>" + $(this).attr("nname") + "</b>");
			hasNots = true;
		}
	});
	if (hasNots)
		nlist.prepend ("You are subscribed to the following notifications: ");
	else
		nlist.prepend ("You dont't have notification subscriptions ");
}

function send (ob)
{
	var postUrl = $("#notification-summary").attr("postUrl");

	ob.disabled = true;
	$.post (postUrl, {notif: ob.id, value: ob.checked}, function(xml) {
		ob.disabled = false;
	});
}
