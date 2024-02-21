
function SawNotification(id) {

    document.getElementById("Notification_" + id).remove();

    notifCountInner = document.getElementById("notifCountInner");
    notifCountInner.innerText = notifCountInner.innerText - 1;

    let notifCount = document.getElementById("notifCount");
    notifCount.innerText = notifCount.innerText - 1;

    if (notifCount.innerText == 0)
        notifCount.innerText = "";

    $.ajax({
        type: "POST",
        url: "/Home/SawNotification",
        data: {
            Id: id,
        },
        success: function (result) {
            // Handle the result here
            console.log(result);
        }
    });
}
