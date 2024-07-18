async function updateFriendshipRequests() {
    const requests = await getFriendshipRequests();

    for (const request of requests) {
        const user = request.sender;
        renderFriendshipRequest(user);
    }
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