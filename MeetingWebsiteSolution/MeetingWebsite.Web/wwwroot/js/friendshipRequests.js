async function updateFriendshipRequests() {
    const friendshipRequestContainer = document.getElementById('friendship-requests-container');
    const requests = await getFriendshipRequests();

    requests.forEach(request => {
        friendshipRequestContainer.innerHTML += getFriendshipRequestTemplate(request.sender);
    });
}

async function getFriendshipRequests() {
    try {
        const response = await fetch('/api/friendship-requests');
        return await response.json();
    } catch (e) {}
}

document.addEventListener('DOMContentLoaded', async () => {
    await updateFriendshipRequests();
});