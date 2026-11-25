const resendButton = document.getElementById("resend-button");

const durationSeconds = 60;
let timerId;
let deadline;

function updateCountdown() {
    const secondsLeft = Math.max(0,Math.ceil((deadline - Date.now()) / 1000));

    if (secondsLeft === 0) {
        clearInterval(timerId);
        resendButton.disabled = false;
        resendButton.textContent = "Отправить письмо повторно";
        return;
    }

    const minutes = String(Math.floor(secondsLeft / 60)).padStart(2, "0");
    const seconds = String(secondsLeft % 60).padStart(2, "0");

    resendButton.textContent = `Отправить повторно через ${minutes}:${seconds}`;
}

function startCountdown() {
    clearInterval(timerId);

    deadline = Date.now() + durationSeconds * 1000;
    resendButton.disabled = true;

    updateCountdown();
    timerId = setInterval(updateCountdown, 250);
}

resendButton.addEventListener("click", () => {
    startCountdown();
});

startCountdown();