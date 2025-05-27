function loadModal(title, url, modalId = 'universalModal') { //URL - адрес контроллера, возвращающего PartialView
    document.querySelector(`#${modalId} .modal-title`).innerText = title;

    fetch(url) //HTTP GET-запрос на указанный URL; возвращает Promise, который завершится, когда придёт ответ
        .then(r => r.text()) //Вызывается, когда сервер ответит (например, вернёт частичное представление); извлекает HTML как строку из ответа
        .then(html => { //Вызывается, когда HTML успешно получен как текст, HTML теперь содержит всю разметку partial-представления (в виде строки)
            document.getElementById(modalId + 'Body').innerHTML = html; //Вставка полученного HTML-кода (partial view)
            new bootstrap.Modal(document.getElementById(modalId)).show();
        })
        .catch(err => console.error("Ошибка загрузки модального окна:", err));
}