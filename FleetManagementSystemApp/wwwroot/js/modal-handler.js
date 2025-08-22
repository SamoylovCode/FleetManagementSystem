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

