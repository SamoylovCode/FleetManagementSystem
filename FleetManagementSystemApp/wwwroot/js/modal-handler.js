//function loadModal(title, url, modalId = 'universalModal') { //URL - адрес контроллера, возвращающего PartialView
//    document.querySelector(`#${modalId} .modal-title`).innerText = title;

//    fetch(url) //HTTP GET-запрос на указанный URL; возвращает Promise, который завершится, когда придёт ответ
//        .then(r => r.text()) //Вызывается, когда сервер ответит (например, вернёт частичное представление); извлекает HTML как строку из ответа
//        .then(html => { //Вызывается, когда HTML успешно получен как текст, HTML теперь содержит всю разметку partial-представления (в виде строки)
//            document.getElementById(modalId + 'Body').innerHTML = html; //Вставка полученного HTML-кода (partial view)
//            new bootstrap.Modal(document.getElementById(modalId)).show();
//        })
//        .catch(err => console.error("Ошибка загрузки модального окна:", err));
//}

function loadModal(title, url, modalId = 'universalModal') {
    document.querySelector(`#${modalId} .modal-title`).innerText = title;

    fetch(url)
        .then(r => r.text())
        .then(html => {
            document.getElementById(`${modalId}Body`).innerHTML = html;

            const modalEl = document.getElementById(modalId);
            const modal = new bootstrap.Modal(modalEl);
            modal.show();

            const form = modalEl.querySelector('form');
            if (!form) return;

            form.addEventListener('submit', function (e) {
                e.preventDefault();
                const data = new FormData(form);

                fetch(form.action, {
                    method: 'POST',
                    body: data
                })
                    .then(async response => {
                        const contentType = response.headers.get("content-type") || "";
                        if (contentType.includes("application/json")) {
                            const json = await response.json();
                            if (json.success) {
                                modal.hide();
                                location.reload(); // или обновление части DOM
                            } else {
                                alert(json.error || "Ошибка при сохранении");
                            }
                        } else {
                            // Вернулся HTML с ошибками валидации
                            const html = await response.text();
                            document.getElementById(`${modalId}Body`).innerHTML = html;
                        }
                    })
                    .catch(err => {
                        console.error("Ошибка при отправке формы:", err);
                    });
            });
        })
        .catch(err => console.error("Ошибка загрузки модального окна:", err));
}

