function addUserToUserBar(sender) {
    // Get the image URL from the sender object or provide a default value
    var imageUrl = sender.imagePath;
    // Create a new div element with a unique ID
    var newDiv = document.createElement("div");
    // Set the class and style for the new div
    newDiv.className = "rounded-circle mx-1";
    newDiv.style.backgroundImage = "url('" + imageUrl + "')";
    newDiv.style.backgroundSize = "cover";
    newDiv.style.backgroundRepeat = "no-repeat";
    newDiv.style.height = "100px";
    newDiv.style.width = "100px";
    newDiv.id = sender.email;
    // Append the new div to the parent div
    document.getElementById("parentDiv").appendChild(newDiv);
}

function removeUserFromUserBar(divId) {
    var divToRemove = document.getElementById(divId);
    if (divToRemove) {
        // Remove the div if it exists
        divToRemove.parentNode.removeChild(divToRemove);
    } else {
        console.log("Div with ID " + divId + " not found.");
    }
}

function addVideoElement(sender) {
    // Create a video element
    var videoElement = document.createElement('video');
    videoElement.id = sender.email + 'video';
    videoElement.autoplay = true;
    videoElement.controls = true;
    videoElement.className = 'img-fluid w-100';
    videoElement.muted = false;
    // Get the remoteVideos div by its id
    var remoteVideosDiv = document.getElementById('remoteVideos');

    // Append the video element to the remoteVideos div
    remoteVideosDiv.appendChild(videoElement);
}

function removeVideoElement(id) {
    var remoteVideoElement = document.getElementById(id);
    if (remoteVideoElement) {
        remoteVideoElement.parentElement.removeChild(remoteVideoElement);
    }
}

function onSuccess() { };
function onError(error) {
    console.error(error);
};
