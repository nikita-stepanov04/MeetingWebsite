const chatContainer = document.getElementById('chatContainer');
const messageInput = document.getElementById('messageInput');
const sendForm = document.getElementById('sendForm');
const scroll = document.getElementById('scroll');
const scrollHeight = document.getElementById('scroll-height');
const imageInputButton = document.getElementById('imageInputButton');
const imageInput = document.getElementById('imageInput');
const uploadImageForm = document.getElementById('uploadImageForm');

const chatUrl = `/wsChat`;
const uploadImageUrl = `/img/upload`;

const chatId = document.getElementById('chatIdInput').value;

const user1Id = document.getElementById('user1IdInput').value;
const user1ImageLink = document.getElementById('user1ImageLinkInput').value;
const user1Firstname = document.getElementById('user1FirstnameInput').value;
const user1Secondname = document.getElementById('user1SecondnameInput').value;

const user2ImageLink = document.getElementById('user2ImageLinkInput').value;
const user2Firstname = document.getElementById('user2FirstnameInput').value;
const user2Secondname = document.getElementById('user2SecondnameInput').value;

let _chatConnection;
const sentImagesPlaceholders = new Map();

async function startChat() {
    try {
        await _chatConnection.start();
    } catch (err) {
        console.log(err);
        setTimeout(startChat, 5000);
    }
}

async function buildChatConnection() {
    _chatConnection = new signalR.HubConnectionBuilder()
        .withUrl(`${chatUrl}?chatId=${encodeURIComponent(chatId)}`)
        .build();

    _chatConnection.on("ReceiveMessageAsync", async message => {
        await renderMessage(message, true);
        if (message.authorId != user1Id) {
            observer.observe(document.getElementById(`message-${message.messageId}`));
        }
        scrollToEnd(true);
    });

    _chatConnection.on("LoadChatAsync", async messages => {
        await renderMessages(messages)
        document.querySelectorAll('.tracking-message')
            .forEach(message => observer.observe(message));
        scrollToEnd()
    });

    _chatConnection.on("SetMessageAsReadAsync", messageId => {
        document.getElementById(`message-check-${messageId}`).classList.add('text-primary');
    });
}

function renderMessage(message, scroll) {
    if (message.text !== null) {
        renderTextMessage(message, scroll);
    } else if (message.imageLink !== null) {
        renderImageMessage(message, scroll);
    }
}

function renderMessages(messages) {
    for (const message of messages) {
        renderMessage(message, false)
    }
}

async function markMessageAsRead(messageBlockId) {
    const messageId = messageBlockId.split('-')[1];
    if (!isNaN(parseInt(messageId))) {
        await _chatConnection.invoke("SetMessageAsReadAsync", JSON.parse(messageId));
    }
}

function replacePlaceholder(containerId, placeholderId, scroll) {
    const imgContainer = document.getElementById(containerId)
    imgContainer.classList.remove('d-none');

    const placeholder = document.getElementById(placeholderId);
    chatContainer.replaceChild(imgContainer, placeholder);
    scrollToEnd(scroll)
}

async function sendMessage() {
    const message = {};
    message.text = messageInput.value;
    if (message.text) {
        await _chatConnection.invoke("SendMessageAsync", message);
        messageInput.value = '';
    }    
}

function renderTextMessage(message, scroll) {
    chatContainer.innerHTML += message.authorId == user1Id
        ? getMessageTemplate(user1ImageLink, user1Firstname, user1Secondname, message, false)
        : getMessageTemplate(user2ImageLink, user2Firstname, user2Secondname, message, true);
    scrollToEnd(scroll)
}

function renderImageMessage(message, scroll) {
    let placeholderId;
    if (sentImagesPlaceholders.has(message.imageId)) {
        placeholderId = sentImagesPlaceholders.get(message.imageId);
        sentImagesPlaceholders.delete(message.imageId);
    } else {
        placeholderId = renderPlaceholder(message.authorId != user1Id);
    }
    chatContainer.innerHTML += message.authorId == user1Id
        ? getImageTemplate(user1ImageLink, user1Firstname, user1Secondname, message, false)
        : getImageTemplate(user2ImageLink, user2Firstname, user2Secondname, message, true);

    const imgContainer = document.getElementById(`message-${message.messageId}`);
    imgContainer.classList.add('d-none');

    const img = document.getElementById(`message-img-${message.messageId}`);
    img.onload = () => replacePlaceholder(`message-${message.messageId}`, placeholderId, scroll);
}

function renderPlaceholder(left) {
    const placeholderId = 'image-placeholder-' + Math.random();
    chatContainer.innerHTML += getImagePlaceholderTemplate(placeholderId, left);
    return `message-${placeholderId}`;
}

function scrollToEnd(smooth) {
    scroll.scrollTo({ top: scroll.scrollHeight, behavior: smooth ? 'smooth' : 'auto' });
}

async function sendImage() {
    const placeholderId = renderPlaceholder(false);
    scrollToEnd();

    const formData = new FormData(uploadImageForm);
    formData.append('chatId', chatId)

    await fetch(uploadImageUrl, {
        method: 'POST',
        body: formData
    }).then(async response => {
        if (response.ok) {
            const imgId = parseInt(await response.text());
            sentImagesPlaceholders.set(imgId, placeholderId);

            const message = {};
            message.imageId = imgId;
            await _chatConnection.invoke("SendMessageAsync", message);
        }
    })
}

function getMessageTemplate(imageLink, firstName, secondName, message, left) {
    const container = `
        <div class="main-block py-1 mb-3 text-wrap text-break flex-grow-1"
                style="background-color: #ebeced;">
            ${message.text}
        </div>
    `;
    return getBasicTemplate(
        imageLink,
        firstName,
        secondName,
        message.messageId,
        message.isRead,
        container,
        left
    );
}

function getImagePlaceholderTemplate(id, left) {
    const placeholder = `
        <div class="d-flex my-2 mb-3 ${left ? '' : 'ms-auto'}" id="${id}">               
            <div class="spinner-border text-secondary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div> 
        </div>
    `;
    return getBasicTemplate(
        left ? user2ImageLink : user1ImageLink,
        left ? user2Firstname : user1Firstname,
        left ? user2Secondname : user1Secondname,
        id,
        false,
        placeholder,
        left
    );
}

function getImageTemplate(imageLink, firstName, secondName, message, left) {
    const img = `
        <div class="d-flex ${left ? '' : 'ms-auto'}">
            <img src="${message.imageLink}" class="img-fluid mb-3" alt="img"
                id="message-img-${message.messageId}">
        </div>
    `;
    return getBasicTemplate(
        imageLink,
        firstName,
        secondName,
        message.messageId,
        message.isRead,
        img,
        left
    );
}

function getBasicTemplate(imageLink, firstName, secondName, messageId, isRead, entry, left) {
    return `
        <div class="row ${left ? `${isRead ? '' : 'tracking-message'}` : "justify-content-end"}" \
                id="message-${messageId}">
            <div class="col-6 target">
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
                    ${entry}
                    ${left 
                        ?  "" 
                        :  `<div class="ms-2 mb-3">
                                <i class="fa-solid fa-check ${isRead ? 'text-primary' : ''}"
                                    id="message-check-${messageId}"></i>
                            </div>`}
                </div>
            </div>
        </div>
    `;
}

const observerOptions = {
    threshold: 0.5
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
    await buildChatConnection();
    await startChat();

    sendForm.addEventListener("submit", (event) => {
        event.preventDefault();
        sendMessage();
    });

    imageInputButton.addEventListener("click", () => imageInput.click());

    imageInput.addEventListener("change", async () => {
        const image = imageInput.files[0];
        if (image) {
            await sendImage();
            imageInput.value = '';
        }
    });

    await _chatConnection.invoke("LoadChatAsync");
});