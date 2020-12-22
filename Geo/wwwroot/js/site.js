"use strict";

function loadFile(event, str) {
    if (event.target.files.length === 0) {
        document.getElementById(str).src = '../../img/no-image-min.png';
    }
    else {
        var reader = new FileReader();
        reader.onload = function () {
            var output = document.getElementById(str);
            output.src = reader.result;
        };
        reader.readAsDataURL(event.target.files[0]);
    }
}

$('.phone').mask('+7(000) 000-00-00', { placeholder: "+7(___) ___-__-__" });


$(function () {
    $('.datepicker').datetimepicker({
        locale: 'ru',
        format: "DD.MM.YYYY"
    });
});

$("#DateOpen").on("dp.change", function (e) {
    $('#DateClose').data("DateTimePicker").minDate(e.date);
});
$("#DateClose").on("dp.change", function (e) {
    $('#DateOpen').data("DateTimePicker").maxDate(e.date);
});

$(function () {
    $('[data-toggle="tooltip"]').tooltip()
});

function getDetails(id) {
    $.ajax({
        type: "POST",
        url: '/Home/Details',
        data: { id: id },
        cache: false,
        success: function (html) {
            $("#DetailsModal").html(html);
            $('#DetailsModal').modal('show');  // запускаем модальное окно
        }
    });
}