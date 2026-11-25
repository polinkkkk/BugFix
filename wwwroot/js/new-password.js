const form = document.querySelector(".password-form");

const passwordInput = document.getElementById("password");
const confirmPasswordInput = document.getElementById("password-confirm");

const passwordMessage = document.getElementById("password-error");
const confirmPasswordMessage = document.getElementById(
    "password-confirm-error"
);

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

function showFieldError(input, text) {
    const message = document.getElementById(`${input.id}-error`);

    input.classList.add("error");
    message.classList.add("error");
    message.textContent = text;
}

function clearErrors() {
    formMessage.hidden = true;

    passwordInput.classList.remove("error");
    confirmPasswordInput.classList.remove("error");

    passwordMessage.classList.remove("error");
    passwordMessage.textContent = "Не менее 8 символов.";

    confirmPasswordMessage.classList.remove("error");
    confirmPasswordMessage.textContent = "";
}

function showLoading() {
    isSubmitting = true;

    showMessage("Сохраняем новый пароль", "loading");

    passwordInput.disabled = true;
    confirmPasswordInput.disabled = true;
    submitButton.disabled = true;

    submitSpinner.hidden = false;
    submitText.textContent = "Сохраняем...";
}

form.addEventListener("submit", (event) => {
    event.preventDefault();

    if (isSubmitting) {
        return;
    }

    clearErrors();

    const password = passwordInput.value;
    const confirmPassword = confirmPasswordInput.value;

    if (password === "") {
        showFieldError(passwordInput, "Введите новый пароль.");
    } else if (password.length < 8) {
        showFieldError(
            passwordInput,
            "Используйте не менее 8 символов."
        );
    }

    if (confirmPassword === "") {
        showFieldError(confirmPasswordInput, "Повторите пароль.");
    } else if (confirmPassword !== password) {
        showFieldError(
            confirmPasswordInput,
            "Пароли не совпадают."
        );
    }

    const firstInvalidInput = form.querySelector("input.error");

    if (firstInvalidInput) {
        showMessage("Проверьте новый пароль", "error");
        firstInvalidInput.focus();
        return;
    }

    showLoading();
});