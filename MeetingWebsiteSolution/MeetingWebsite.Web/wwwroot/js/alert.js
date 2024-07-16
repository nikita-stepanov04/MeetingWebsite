document.addEventListener("DOMContentLoaded", () => {
    const alertMessage = document.getElementById("alertMessage").innerText;
    if (alertMessage !== "") {
        showAlert()
    }
});

function showAlert(message, type) {
    console.log(message, type)
    const alertContainer = document.getElementById('liveAlertPlaceholder');
    const alertBox = document.getElementById('alertBox');
    const alertMessage = document.getElementById("alertMessage");

    alertContainer.classList.remove('alert-hide');
    alertContainer.classList.add('alert-show');
    alertContainer.style.display = 'block';

    alertBox.classList.add(`alert-${type}`);
    alertMessage.textContent = message

    setTimeout(() =>  hideAlert(), 2000);
}

function hideAlert() {
    const alertContainer = document.getElementById('liveAlertPlaceholder');
    alertContainer.classList.remove('alert-show');
    alertContainer.classList.add('alert-hide');

    setTimeout(() => {
        alertContainer.style.display = 'none';
    }, 500);
}
