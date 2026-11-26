async function postJson(url, body) {
    let response;

    try {
        response = await fetch(`http://127.0.0.1:5080${url}`, {
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