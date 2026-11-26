const form = document.querySelector(".login-form");

const emailInput = document.getElementById("email");
const passwordInput = document.getElementById("password");

const formMessage = document.getElementById("form-message");
const formMessageText = document.getElementById("form-message-text");
const formMessageDescription = document.getElementById(
    "form-message-description"
);

const submitButton = document.getElementById("submit-button");
const submitText = document.getElementById("submit-text");
const submitSpinner = document.getElementById("submit-spinner");


const demoUser = {
    email: "demyan@example.com",
    password: "Password123!"
};

let isSubmitting = false;

function showMessage(text, state, description = "") {
    formMessageText.textContent = text;
    formMessage.dataset.state = state;

    formMessageDescription.textContent = description;
    formMessageDescription.hidden = description === "";

    formMessage.hidden = false;
}

function showFieldError(input, text) {
    const message = document.getElementById(`${input.id}-error`);

    input.classList.add("error");
    message.textContent = text;
}

function clearErrors() {
    formMessage.hidden = true;

    form.querySelectorAll("input").forEach((input) => {
        input.classList.remove("error");
    });

    form.querySelectorAll(".field-message").forEach((message) => {
        message.textContent = "";
    });
}

function showLoading() {
    isSubmitting = true;

    showMessage("Выполняется вход в аккаунт", "loading");

    emailInput.disabled = true;
    passwordInput.disabled = true;

    submitButton.disabled = true;
    submitSpinner.hidden = false;
    submitText.textContent = "Входим...";
}

form.addEventListener("submit", async (event) => {
    event.preventDefault();

    if (isSubmitting) {
        return;
    }

    clearErrors();

    const email = emailInput.value.trim();
    const password = passwordInput.value;

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
        showFieldError(passwordInput, "Введите пароль.");
    }

    const firstInvalidInput = form.querySelector("input.error");

    if (firstInvalidInput) {
        showMessage("Проверьте поля формы", "error");
        firstInvalidInput.focus();
        return;
    }

    if (
        email.toLowerCase() !== demoUser.email ||
        password !== demoUser.password
    ) {
        showMessage(
            "Неверная почта или пароль",
            "error",
            "Проверьте данные или восстановите доступ к аккаунту."
        );

        return;
    }

    showLoading();
});