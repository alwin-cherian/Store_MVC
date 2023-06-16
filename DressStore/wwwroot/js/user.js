$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/user/getall' },
        "columns": [
            { data: 'userName', "width":"20%" },
            { data: 'email', "width": "20%" },
            { data: 'phoneNumber', "width": "15%" },
            { data: '', "width": "10%" },
            { data: '', "width": "15%" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                    <a href = "/admin/product/upsert?id=${data}" class="btn btn-primary mx-2" > <i class="bi bi-pencil-square"></i> Edit</a >
                    </div>`
                },
                "width": "10%"
            }
        ]
    }); 
}

