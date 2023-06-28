var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/order/getall' },
        "columns": [
            { data: 'id', "width":"7%" },
            { data: 'firstName', "width": "12%" },
            { data: 'phoneNumber', "width": "12%" },
            { data: 'applicationUser.email', "width": "15%" },
            { data: 'orderStatus', "width": "12%" },
            { data: 'orderTotal', "width": "12%" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                    <a href = "/admin/order/details?orderId=${data}" class="btn btn-primary mx-2" > <i class="bi bi-pencil-square"></i></a >
                    <a href = "/admin/order/generatepdf?orderId=${data}" class="btn btn-primary mx-2" > <i class="bi bi-file-earmark-arrow-down-fill"></i></a >
                    </div>`
                },
                "width": "25%"
            }
            
        ]
    });
}
