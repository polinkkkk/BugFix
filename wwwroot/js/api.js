const apiUrl = "http://127.0.0.1:5080"

async function postJson(url, body) {
    let response;

    try {
        response = await fetch(`${apiUrl}${url}`, {
            method: "POST",
            credentials: "include",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(body)
        });
    } catch {
        throw new Error(
            "Не удалось связаться с сервером. Повторите попытку."
        );
    }

    const data = await response.json().catch(() => null);

    console.log("Адрес запроса:", response.url);
    console.log("HTTP-статус:", response.status);
    console.log("Ответ сервера:", data);

    if (!response.ok) {
        const errors = Object.values(data?.errors || {}).flat().join(" ");

        const message = response.status >= 500
            ? "Ошибка сервера. Повторите попытку позже."
            : errors || data?.title || "Не удалось выполнить запрос.";

        throw new Error(message);
    }

    return data;
}

async function getJson(path) {
    let response;

    try {
        response = await fetch(`${apiUrl}${path}`, {
            credentials: "include",
            cache: "no-store"
        });
    } catch {
        throw new Error(
            "Не удалось связаться с сервером. Повторите попытку."
        );
    }

    if (response.status === 401) {
        location.replace("login.html");
        throw new Error("Войдите в аккаунт.");
    }

    const data = await response.json().catch(() => null);

    if (!response.ok) {
        const errors = Object.values(data?.errors || {})
            .flat()
            .join(" ");

        throw new Error(
            response.status >= 500
                ? "Ошибка сервера. Повторите попытку позже."
                : errors || data?.title || "Не удалось выполнить запрос."
        );
    }

    return data;
}