const form = document.querySelector(".form");

const nameInput = document.getElementById("name");
const emailInput = document.getElementById("email");
const passwordInput = document.getElementById("password");
const confirmPasswordInput = document.getElementById("confirm-password");
const agreementInput = document.getElementById("agreement");

const formMessage = document.getElementById("form-message");
const formMessageText = document.getElementById("form-message-text");

const submitButton = document.getElementById("submit-button");
const submitText = document.getElementById("submit-text");
const submitSpinner = document.getElementById("submit-spinner");


const registeredEmails = ["postavte5@example.com"];

let isSubmitting = false;

function showMessage(text, state) {
    formMessageText.textContent = text;
    formMessage.dataset.state = state;
    formMessage.hidden = false;
}

function showFieldError(input, text) {
    const message = document.getElementById(`${input.id}-error`);

    input.classList.add("error");
    message.textContent = text;
    message.classList.add("error");
}

function clearErrors() {
    formMessage.hidden = true;

    form.querySelectorAll("input").forEach((input) => {
        input.classList.remove("error");
    });

    form.querySelectorAll(".field-message").forEach((message) => {
        message.textContent = "";
        message.classList.remove("error");
    });

    document.getElementById("password-error").textContent =
        "Не менее 8 символов.";

    document.getElementById("agreement-error").textContent = "";
}

function showLoading() {
    isSubmitting = true;

    showMessage("Идет создание аккаунта", "loading");

    form.querySelectorAll("input").forEach((input) => {
        input.disabled = true;
    });

    submitButton.disabled = true;
    submitSpinner.hidden = false;
    submitText.textContent = "Создаём аккаунт...";
}

form.addEventListener("submit", (event) => {
    event.preventDefault();

    if (isSubmitting) {
        return;
    }

    clearErrors();

    const name = nameInput.value.trim();
    const email = emailInput.value.trim();
    const password = passwordInput.value;
    const confirmPassword = confirmPasswordInput.value;

    const hasEmptyFields =
        name === "" ||
        email === "" ||
        password === "" ||
        confirmPassword === "" ||
        !agreementInput.checked;

    if (name === "") {
        showFieldError(nameInput, "Введите имя.");
    }

    if (email === "") {
        showFieldError(emailInput, "Введите электронную почту.");
    } else if (
        emailInput.validity.typeMismatch ||
        !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)
    ) {
        showFieldError(
            emailInput,
            "Введите email в формате name@example.com."
        );
    }

    if (password === "") {
        showFieldError(passwordInput, "Придумайте пароль.");
    } else if (password.length < 8) {
        showFieldError(
            passwordInput,
            "Используйте не менее 8 символов."
        );
    }

    if (confirmPassword === "") {
        showFieldError(confirmPasswordInput, "Повторите пароль.");
    } else if (confirmPassword !== password) {
        showFieldError(confirmPasswordInput, "Пароли не совпадают.");
    }

    if (!agreementInput.checked) {
        agreementInput.classList.add("error");

        document.getElementById("agreement-error").textContent =
            "Примите условия использования, чтобы продолжить.";
    }

    const firstInvalidInput = form.querySelector("input.error");

    if (firstInvalidInput) {
        showMessage(
            hasEmptyFields
                ? "Заполните обязательные поля"
                : "Проверьте введённые данные",
            "error"
        );

        firstInvalidInput.focus();
        return;
    }

    if (registeredEmails.includes(email.toLowerCase())) {
        showMessage(
            "Аккаунт с такой почтой уже существует",
            "error"
        );

        showFieldError(
            emailInput,
            "Эта почта уже используется. Войдите или восстановите пароль."
        );

        emailInput.focus();
        return;
    }

    showLoading();
});