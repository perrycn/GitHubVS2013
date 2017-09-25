$(function () {

   var ajaxFormSubmit = function () {
        var $form = $(this); 

        var options = {
            url: $form.attr("action"),
            type: $form.attr("method"),
            data: $form.serialize()  
        };

        $.ajax(options).done(function (data) {
            var $target = $($form.attr("data-lib-target"));
            var $newHtml = $(data);
            $target.replaceWith($newHtml);
            $newHtml.effect("highlight");
        });

        return false;
    };

    var submitAutocompleteForm = function (event, ui) {

        var $input = $(this);
        $input.val(ui.item.label);

        var $form = $input.parents("form:first");
        $form.submit();
    }

    var createAutocomplete = function () {
        var $input = $(this);

        var options = {
            source: $input.attr("data-lib-autocomplete"),
            select: submitAutocompleteForm
        };

        $input.autocomplete(options);
    };

    var getPage = function () {
        var $a = $(this);

        var options = {
            url: $a.attr("href"),
            data: $("form").serialize(),
            type: "get"
        };

        $.ajax(options).done(function (data) {
            var target = $a.parents("div.pagedList").attr("data-lib-target");
            $(target).replaceWith(data);      
        });
        return false;
    }

    $("#expr_date").attr("value", function () {
        var d = new Date();
        var datestring = ("0" + d.getMonth()).slice(-2) + "/" + ("0" + d.getDate()).slice(-2) + "/" +
                          (d.getFullYear() + 1) + " " + ("0" + d.getHours()).slice(-2) + ":" +
                          ("0" + d.getMinutes()).slice(-2);
        return datestring;
    });

    $(".datepicker").datepicker();

    $("form[data-lib-ajax='true']").submit(ajaxFormSubmit);
    $("input[data-lib-autocomplete]").each(createAutocomplete);

    $(".body-content").on("click", ".pagedList a", getPage);

    $("#ddlISBN").change(function () {
        var isbn = $(this).val();
        $.getJSON("/Loans/GetCopy_NoListByISBN", { isbn: isbn },
            function (copy_noList) {
                var select = $("#ddlCopyNo");
                select.empty();
                select.append($('<option/>', {
                    value: 0,
                    text: "Select a Copy No."
                }));
                $.each(copy_noList, function (index, itemData) {
                    select.append($('<option/>', {
                        value: itemData.Value,
                        text: itemData.Text
                    }));
                });
            });  // .getJSON ending
    });  // .change ending
});