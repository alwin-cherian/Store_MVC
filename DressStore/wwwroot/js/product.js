﻿$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/product/getall' },
        "columns": [
            { data: 'title', "width":"20%" },
            { data: 'description', "width": "20%" },
            { data: 'price', "width": "15%" },
            { data: 'category.id', "width": "10%" },
            { data: 'category.name', "width": "15%" },
            //{
            //    data: 'id',
            //    "render": function (data) {
            //        return `<div class="w-75 btn-group" role="group">
            //        <a href = "/admin/product/upsert?id=${data}" class="btn btn-primary mx-2" > <i class="bi bi-pencil-square"></i> Edit</a >
            //        <a class="btn btn-danger mx-2"><i class="bi bi-trash-fill"></i> Delete</a>
            //        </div>`
            //    },
            //    "width": "10%"
            //}
        ]
    });
}
