const chatContainer = document.getElementById('chatContainer');
const messageInput = document.getElementById('messageInput');
const sendButton = document.getElementById('sendButton');

const chatUrl = 'https://localhost:5001/wsChat';
const chatId = document.getElementById('chatIdInput').value;

const user1Id = document.getElementById('user1IdInput').value;
const user1ImageLink = document.getElementById('user1ImageLinkInput').value;
const user1Firstname = document.getElementById('user1FirstnameInput').value;
const user1Secondname = document.getElementById('user1SecondnameInput').value;

const user2ImageLink = document.getElementById('user2ImageLinkInput').value;
const user2Firstname = document.getElementById('user2FirstnameInput').value;
const user2Secondname = document.getElementById('user2SecondnameInput').value;

let _connection;

async function start() {
    try {
        await _connection.start();
        console.log(`SignalR Connected to ${chatUrl}?chatId=${encodeURIComponent(chatId)}`);
    } catch (err) {
        console.log(err);
        setTimeout(start, 5000);
    }
}

async function buildConnection() {
    _connection = new signalR.HubConnectionBuilder()
        .withUrl(`${chatUrl}?chatId=${encodeURIComponent(chatId)}`)
        .build();

    _connection.on("ReceiveMessageAsync", message => {
        renderMessage(message);
        if (message.authorId != user1Id) {
            observer.observe(document.getElementById(`message-${message.messageId}`));
        }
    });

    _connection.on("LoadChatAsync", messages => {
        messages.forEach(message => renderMessage(message));
        document.querySelectorAll('.tracking-message')
            .forEach(message => observer.observe(message));
    });

    _connection.on("SetMessageAsReadAsync", messageId => {
        document.getElementById(`message-check-${messageId}`).classList.add('link-primary');
    });
}

async function markMessageAsRead(messageBlockId) {
    const messageId = messageBlockId.split('-')[1];
    await _connection.invoke("SetMessageAsReadAsync", JSON.parse(messageId));
}

async function sendMessage() {
    const message = messageInput.value;
    if (message) {
        await _connection.invoke("SendMessageAsync", message);
    }    
}

function renderMessage(message) {
    chatContainer.innerHTML += message.authorId == user1Id
        ? getMessageTemplate(user1ImageLink, user1Firstname, user1Secondname, message, false)
        : getMessageTemplate(user2ImageLink, user2Firstname, user2Secondname, message, true)
}

function getMessageTemplate(imageLink, firstName, secondName, message, left) {
    return `
        <div class="row ${left ? `${message.isRead ? '' : 'tracking-message'}` : "justify-content-end"}" \
                id="message-${message.messageId}">
            <div class="col-6">
                <div class="${left ? "" : "text-end"}">
                    <div class="mb-1 me-2">
                        <img src="${imageLink || '/img/avatar.png'}"
                             width="30" height="30"
                             class="d-inline-block align-text-top rounded-circle" />
                        <span class="fw-semibold align-middle ms-2">
                            ${firstName} ${secondName}                           
                        </span>
                    </div>
                </div>
                <div class="d-flex align-items-center">
                    <div class="main-block py-1 mb-3 text-wrap text-break flex-grow-1"
                         style="background-color: #ebeced;">
                        ${message.text}
                    </div>
                    ${left 
                        ?  "" 
                        :  `<div class="ms-2 mb-3">
                                <i class="fa-solid fa-check ${message.isRead ? 'link-primary' : ''}"
                                    id="message-check-${message.messageId}"></i>
                            </div>`}
                </div>
            </div>
        </div>
    `;
}

const observerOptions = {
    root: null,
    rootMargin: '0px',
    threshold: 1.0
};

const observer = new IntersectionObserver((entries, observer) => {
    entries.forEach(async entry => {
        if (entry.isIntersecting) {
            const messageId = entry.target.id;
            await markMessageAsRead(messageId);
            observer.unobserve(entry.target);
        }
    });
}, observerOptions);

document.addEventListener("DOMContentLoaded", async () => {
    await buildConnection();
    await start();
    sendButton.addEventListener("click", sendMessage);
    await _connection.invoke("LoadChatAsync");
});