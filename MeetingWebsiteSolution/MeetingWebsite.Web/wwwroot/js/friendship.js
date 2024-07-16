const baseUrl = 'https://localhost:5001';
const friendshipHubUrl = `${baseUrl}/friendships`;
let _connection;

let friendshipRequestCountInput;

async function start() {
    try {
        await _connection.start();
    } catch (err) {
        console.log(err);
        setTimeout(start, 5000);
    }
}

async function buildConnection() {
    _connection = new signalR.HubConnectionBuilder()
        .withUrl(friendshipHubUrl)
        .build();

    _connection.on('UpdateFriendshipRequestsCountAsync', count => {
        console.log(friendshipRequestCountInput)
        friendshipRequestCountInput.textContent = count
    });

    _connection.on('UpdateFriendshipStatusResponseAsync', async statusList => {
        await updateFriendshipButtonByStatus(statusList);
    })

    _connection.on('ShowErrorAsync', error => showAlert(error, 'danger'));
}

async function updateFriendshipButtonByStatus(statusList) {
    statusList.forEach(status => {
        const buttonParent = document.getElementById(`friendship-button-parent-${status.userId}`);

        if (buttonParent) {
            const elementId = `friendship-button-${status.userId}`;

            if (status.friendshipStatus === 'Friend') {
                buttonParent.innerHTML = getDeleteFromFriendsButtonTemplate(status.userId);
                document.getElementById(elementId).onclick = () => removeFromFriends(status.userId);
            } else if (status.friendshipStatus === 'NotAFriend') {
                buttonParent.innerHTML = getAddToFriendsButtonTemplate(status.userId);
                document.getElementById(elementId).onclick = () => addToFriends(status.userId);
            } else if (status.friendshipStatus === 'RequestSent') {
                buttonParent.innerHTML = getCancelFriendshipRequestButtonTemplate(status.userId);
                document.getElementById(elementId).onclick = () => cancelFriendshipRequest(status.userId);
            } else if (status.friendshipStatus === 'PendingAcceptance') {
                buttonParent.innerHTML = getAcceptRejectFriendshipRequestButtonTemplate(status.userId);
                document.getElementById(`friendship-button-${status.userId}-accept`).onclick =
                    () => acceptFriendshipRequest(status.userId);
                document.getElementById(`friendship-button-${status.userId}-reject`).onclick =
                    () => rejectFriendshipRequest(status.userId);
            }
        }
    });

    const friendshipRequestContainer = document.getElementById('friendship-requests-container');
    if (friendshipRequestContainer) {
        const status = statusList[0];
        if (status.friendshipStatus === 'PendingAcceptance'){
            const user = await getUser(status.userId);
            friendshipRequestContainer.innerHTML += getFriendshipRequestTemplate(user, true);
            document.getElementById(`friendship-button-${status.userId}-accept`).onclick =
                async () => {
                    await acceptFriendshipRequest(status.userId);
                    removeRequest(status.userId);
                };
            document.getElementById(`friendship-button-${status.userId}-reject`).onclick =
                async () => {
                    await rejectFriendshipRequest(status.userId);
                    removeRequest(status.userId);
                };
        }
        else if (status.friendshipStatus === 'NotAFriend') {
           removeRequest(status.userId);
        }
    }
}

async function getUser(userId) {
    try {
        const response = await fetch('/api/user/' + userId);
        return await response.json();
    } catch (e) {}
}

function removeRequest(requestId) {
    const requestNode = document.getElementById(`friendship-request-${requestId}`);
    if (requestNode) {
        requestNode.remove();
    }
}

async function updateFriendshipStatusRequest() {
    const userIds = Array.from(document.querySelectorAll('.friendship-placeholder'))
        .map(placeholder =>
            parseInt(placeholder.id.split('friendship-button-')[1]
        )
    );
    await _connection.invoke("UpdateFriendshipStatusRequestAsync", userIds);
}

function getAddToFriendsButtonTemplate(id) {
    return `
        <button class="btn btn-sm btn-primary add-to-friends" id="friendship-button-${id}">
            <i class="fa-solid fa-user-plus"></i>
        </button>
    `;
}

function getDeleteFromFriendsButtonTemplate(id) {
    return `
        <button class="btn btn-sm btn-danger remove-from-friends" id="friendship-button-${id}">
            <i class="fa-solid fa-user-minus"></i>
        </button>
    `;
}

function getCancelFriendshipRequestButtonTemplate(id) {
    return `
        <button class="btn btn-sm btn-secondary cancel-friendship-request" id="friendship-button-${id}">
            <i class="fa-solid fa-xmark"></i>
            Cancel request
        </button>
    `;
}

function getAcceptRejectFriendshipRequestButtonTemplate(id, noSign) {
    return `
        <div class="d-flex" id="friendship-button-${id}">
            <div class="d-grid flex-grow-1">
                <button class="btn btn-link link-success link-underline
                        link-underline-opacity-0 ps-2 pe-1 pt-0 accept-friendship-request"
                        id="friendship-button-${id}-accept">
                    <i class="fa-solid fa-circle-check fa-xl"></i>
                    ${noSign ? '' : 'Accept'}
                </button>
            </div>
            <div class="d-grid flex-grow-1">
                <button class="btn btn-link link-danger link-underline 
                        link-underline-opacity-0 pt-0 reject-friendship-request"
                        id="friendship-button-${id}-reject">
                    <i class="fa-solid fa-circle-xmark fa-xl"></i>
                    ${noSign ? '' : 'Reject'}
                </button>
            </div>
        </div>
    `;
}

function getFriendshipRequestTemplate(sender) {
    return `
        <div class="row main-block py-1 px-0 mb-3" 
                style="background-color: #ebeced"
                id="friendship-request-${sender.userId}">
            <div class="col overflow-auto">
                <div class="navbar-brand d-flex">
                    <img src="${sender.imageLink ?? "/img/avatar.png"}"
                         width="40" height="40"
                         class="d-inline-block align-text-top object-fit-cover rounded-circle mt-2">
                    <div class="d-grid">
                        <div class="ms-2 fs-5 overflow-ellipsis">
                            ${sender.firstname} ${sender.secondname}
                        </div>
                        <div class="ms-2 fs-6 text-body-secondary">                            
                            ${getFullYears(sender.birthday)} y.o
                        </div>
                    </div>
                </div>
            </div>
            <div class="col-auto d-flex mt-3 px-0">
                ${getAcceptRejectFriendshipRequestButtonTemplate(sender.userId, true)}
            </div>
        </div>
    `;
}

function getFullYears(dateString) {
    const dateParts = dateString.split('-');
    const year = parseInt(dateParts[0]);
    const month = parseInt(dateParts[1]);
    const day = parseInt(dateParts[2]);

    const dateTime = new Date(year, month - 1, day, 0, 0, 0);
    let yearsDifference = new Date().getFullYear() - year;

    if (new Date() < new Date(dateTime).setFullYear(new Date().getFullYear() + yearsDifference)) {
        yearsDifference--;
    }

    return yearsDifference;
}

async function addToFriends(id) {
    await _connection.invoke("SendFriendshipRequestAsync", id);
}

async function removeFromFriends(id) {
    await _connection.invoke("DeleteFromFriendsAsync", id);
}

async function acceptFriendshipRequest(id) {
    await _connection.invoke("AcceptFriendshipRequestAsync", id);
}

async function rejectFriendshipRequest(id) {
    await _connection.invoke("RejectFriendshipRequestAsync", id);
}

async function cancelFriendshipRequest(id) {
    await _connection.invoke("CancelFriendshipRequestAsync", id);
}

document.addEventListener("DOMContentLoaded", async () => {
    friendshipRequestCountInput = document.getElementById('friendship-requests-count');

    await buildConnection();
    await start();
    await updateFriendshipStatusRequest();
})