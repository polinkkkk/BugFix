export class ApiError extends Error {
    constructor(message, status = 0, errors = {}) {
        super(message);
        this.status = status;
        this.errors = errors;
    }
}

export async function postJson(path, body) {
    let response;

    try {
        response = await fetch(path, {
            method: "POST",
            credentials: "same-origin",
            headers: {
                "Content-Type": "application/json",
                "Accept": "application/json"
            },
            body: JSON.stringify(body)
        });
    } catch {
        throw new ApiError(
            "Не удалось связаться с сервером. Проверьте подключение и повторите попытку."
        );
    }

    let data = null;

    try {
        const text = await response.text();
        data = text ? JSON.parse(text) : null;
    } catch {
        throw new ApiError(
            "Сервер вернул некорректный ответ. Повторите попытку позже.",
            response.status
        );
    }

    if (!response.ok) {
        const message = response.status >= 500
            ? "Ошибка сервера. Повторите попытку позже."
            : data?.title || "Не удалось выполнить запрос.";

        throw new ApiError(
            message,
            response.status,
            data?.errors || {}
        );
    }

    return data;
}