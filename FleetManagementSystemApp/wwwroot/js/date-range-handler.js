window.initDateRangeFields = function () {
    $('input[data-fieldtype="DateRange"]').each(function () {
        if (!$(this).data('daterangepicker')) {
            $(this).daterangepicker({
                opens: 'left',
                drops: 'down',
                locale: {
                    format: 'DD.MM.YYYY',
                    separator: ' - ',
                    applyLabel: 'Применить',
                    cancelLabel: 'Отмена',
                    daysOfWeek: ['Вс', 'Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб'],
                    monthNames: ['Январь', 'Февраль', 'Март', 'Апрель', 'Май', 'Июнь',
                        'Июль', 'Август', 'Сентябрь', 'Октябрь', 'Ноябрь', 'Декабрь'],
                    firstDay: 1
                },
                autoUpdateInput: true
            });
        }
    });
}

window.initDateFields = function () {
    $('input[data-fieldtype="Date"]').each(function () {
        if (!$(this).data('daterangepicker')) {
            $(this).daterangepicker({
                singleDatePicker: true,
                showDropdowns: true,
                autoUpdateInput: true,
                locale: { format: 'DD.MM.YYYY' }
            });
        }
    });
};