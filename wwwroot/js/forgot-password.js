const form = document.querySelector(".recovery-form");
const emailInput = document.getElementById("email");
const emailError = document.getElementById("email-error");

const formMessage = document.getElementById("form-message");
const formMessageText = document.getElementById("form-message-text");

const submitButton = document.getElementById("submit-button");
const submitText = document.getElementById("submit-text");
const submitSpinner = document.getElementById("submit-spinner");

let isSubmitting = false;

function showMessage(text, state) {
    formMessageText.textContent = text;
    formMessage.dataset.state = state;
    formMessage.hidden = false;
}

function showEmailError(text) {
    showMessage("Проверьте электронную почту", "error");

    emailInput.classList.add("error");
    emailError.textContent = text;

    emailInput.focus();
}

function clearErrors() {
    formMessage.hidden = true;
    emailInput.classList.remove("error");
    emailError.textContent = "";
}

function showLoading() {
    isSubmitting = true;

    showMessage("Отправляем письмо", "loading");

    emailInput.disabled = true;
    submitButton.disabled = true;

    submitSpinner.hidden = false;
    submitText.textContent = "Отправляем...";
}

form.addEventListener("submit", async (event) => {
    event.preventDefault();

    if (isSubmitting) {
        return;
    }

    clearErrors();

    const email = emailInput.value.trim();

    if (email === "") {
        showEmailError("Введите электронную почту.");
        return;
    }

    if (emailInput.validity.typeMismatch || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email))
    {
        showEmailError("Введите email в формате name@example.com.");
        return;
    }

    showLoading();

    try {
        await postJson("/api/auth/forgot-password", {
            email: email
        });

        sessionStorage.setItem("bugfix.resetEmail", email);

        location.assign("check-mail.html");
    } catch (error) {
        showMessage(error.message, "error");
    } finally {
        isSubmitting = false;

        emailInput.disabled = false;
        submitButton.disabled = false;

        submitSpinner.hidden = true;
        submitText.textContent = "Отправить ссылку";
    }
});