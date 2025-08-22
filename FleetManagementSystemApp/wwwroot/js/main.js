function submitDelete(form) {
	$.ajax({
		url: form.action,
		type: form.method,
		data: $(form).serialize(),
		success: function () {
			// закрыть модалку
			$('#myModal').modal('hide');
			// перезагрузить список
			window.location.reload();
		},
		error: function (xhr) {
			showErrorToast("Ошибка: " + xhr.responseText);
		}
	});
	return false; // отменить обычный submit
}
function enableStep(stepNumber) {
    console.log('Переход к шагу:', stepNumber);

    // 1. Управление видимостью шагов
    document.querySelectorAll('.step-section').forEach(section => {
        section.classList.add('d-none');
        section.classList.remove('d-block');
    });
    document.getElementById(`step${stepNumber}`).classList.remove('d-none');
    document.getElementById(`step${stepNumber}`).classList.add('d-block');

    // 2. Обновляем навигацию - ПРАВИЛЬНО!
    document.querySelectorAll('.nav-link').forEach((link, index) => {
        const step = index + 1;
        const icon = link.querySelector('i');
        const label = link.querySelector('.small');

        // Сбрасываем состояние
        link.classList.remove('active');
        link.classList.add('disabled');

        if (icon) {
            // Все иконки делаем контурными и СЕРЫМИ
            icon.classList.remove('bi-1-circle-fill', 'bi-2-circle-fill', 'bi-3-circle-fill');
            icon.classList.add(`bi-${step}-circle`);
            icon.classList.remove('text-dark');
            icon.classList.add('text-muted');
        }

        if (label) {
            // Все подписи делаем серыми
            label.classList.remove('text-dark');
            label.classList.add('text-muted', 'text-body-tertiary');
        }
    });

    // 3. Активируем текущий и предыдущие шаги
    for (let i = 1; i <= stepNumber; i++) {
        const stepLink = document.querySelector(`.nav-link[href="#step${i}"]`);
        if (stepLink) {
            const icon = stepLink.querySelector('i');
            const label = stepLink.querySelector('.small');

            stepLink.classList.remove('disabled');

            if (icon) {
                // Для активного шага - заполненная иконка и ТЕМНЫЙ текст
                if (i === stepNumber) {
                    icon.classList.remove(`bi-${i}-circle`);
                    icon.classList.add(`bi-${i}-circle-fill`);
                    icon.classList.remove('text-muted');
                    icon.classList.add('text-dark');
                } else {
                    // Для пройденных шагов - контурная иконка, но ТЕМНЫЙ текст
                    icon.classList.remove('text-muted');
                    icon.classList.add('text-dark');
                }
            }

            if (label) {
                // Для всех активных и пройденных шагов - ТЕМНЫЙ текст
                label.classList.remove('text-muted', 'text-body-tertiary');
                label.classList.add('text-dark');
            }

            if (i === stepNumber) {
                stepLink.classList.add('active');
            }
        }
    }

    // Прокручиваем к верху формы
    window.scrollTo({ top: 0, behavior: 'smooth' });
}