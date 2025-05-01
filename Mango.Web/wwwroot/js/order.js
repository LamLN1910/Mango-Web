var dataTable

$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("approved")) {
        loadDataTable("approved");
    }
    else if (url.includes("readyforpickup")) {
        loadDataTable("readyforpickup");
    }
    else if (url.includes("cancelled")) {
        loadDataTable("cancelled");
    }
    else {
        loadDataTable("all");
    }
});

function loadDataTable(status) {
    dataTable = $('#tblData').DataTable({
        order: [[0, 'desc']],
        ajax: {
            url: "/Order/getall?status=" + status,
        },
        columns: [
            { data: 'orderHeaderId', name: 'orderHeaderId', autoWidth: true },
            { data: 'email', name: 'Email', autoWidth: true },
            { data: 'name', name: 'Name', autoWidth: true },
            { data: 'phone', name: 'Phone', autoWidth: true },
            { data: 'status', name: 'status', autoWidth: true },
            { data: 'orderTotal', name: 'Total', autoWidth: true },
            {
                data: 'orderHeaderId',
                render: function (data) {
                    return ` <div class="w-75 btn-group">
                    <a href="/order/orderDetail?orderId=${data}" class="btn btn-primary mx-2" "><i class="bi bi-pencil-square"></i></a>
                    </div>`
                },
                width: "10%"
            }
        ]
    });
}