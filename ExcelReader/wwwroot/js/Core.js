import { UPLOAD_IT_REQUEST_URL } from "./Config";

$(document).ready(() => {
    console.log("document is ready")
    var loadDataBttn = $("#loadTableData");
    let headers;

    let jwtToken = localStorage.getItem('jwtToken');
    if (jwtToken) {

        headers = {
            'Authorization': 'Bearer ' + jwtToken
        };
    }
    loadDataBttn.click(() => {
        $.ajax({
            url: UPLOAD_IT_REQUEST_URL,
            method: "GET",
            cache: false,
            async: true,
            headers: headers,
            success: (data) => {
                console.log("datashata: ", data)
                tableData = data.map(row => [
                    row.itRequest.requestId,
                    row.itRequest.author,
                    row.itRequest.type,
                    row.itRequest.subject,
                    row.itRequest.body,
                    row.itRequest.sourceFileId,
                    row.itRequest.status,
                    row.userFile.filename,
                    row.userFile.owner,
                    row.userFile.uploadDate
                ])

                tableshable.rows.add(tableData).draw();
                loadDataBttn.prop('disabled', true)



            },
            error: (err) => {
                console.log(err)
                window.alert(err.responseText)
            }
        })


    })



})