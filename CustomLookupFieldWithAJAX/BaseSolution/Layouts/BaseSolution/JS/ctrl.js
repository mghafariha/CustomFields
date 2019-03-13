$(document).ready(function () {
    item = {
        "__metadata": {
            "type": "SP.Data.list3ListItem"
        },
        "Title": "Name"
    };
    $.ajax({
        url: fullurl + "/_api/web/lists/getbytitle('list3')/items",
        type: "POST",
        contentType: "application/json;odata=verbose",
        data: JSON.stringify(item),
        headers: {
            "Accept": "application/json;odata=verbose",
            "X-RequestDigest": $("#__REQUESTDIGEST").val()
        },
        success: function (data) {
            alert('Success');
        },
        error: function (data) {
            alert(Error);
        }
    });

})