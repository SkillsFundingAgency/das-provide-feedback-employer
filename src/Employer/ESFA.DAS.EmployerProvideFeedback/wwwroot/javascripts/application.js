$(function() {

    $('fieldset.checkbox-limit').each(function() {

        var maxAnswers = $(this).data('max-answers') || 3,
            checkboxes = $(this).find('input:checkbox');

        toggleCheckboxesDisabled(checkboxes);

        checkboxes.on('change', function () {
            toggleCheckboxesDisabled(checkboxes);
        });

        function toggleCheckboxesDisabled(checkboxes) {
            checkedBoxes = checkboxes.filter(':checked');
            if (checkedBoxes.length === maxAnswers) {
                checkboxes.attr('disabled', 'disabled');
                checkedBoxes.removeAttr('disabled');
            } else {
                checkboxes.removeAttr('disabled');
            }
        }
    });
});