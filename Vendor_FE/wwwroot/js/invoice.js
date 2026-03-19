// wwwroot/js/invoice.js

const API_BASE_URL = 'https://localhost:7169/api/Invoices';

$(document).ready(function () {
    loadInvoices();
});

function loadInvoices() {
    var token = sessionStorage.getItem('vendorToken');

    $.ajax({
        url: API_BASE_URL,
        type: 'GET',
        headers: { 'Authorization': 'Bearer ' + token },
        success: function (data) {
            var html = '';
            $.each(data, function (index, item) {
                html += '<tr class="border-bottom">';
                html += `<td class="py-3 px-4 fw-bold text-start">#HD-${item.invoiceId.toString().padStart(6, '0')}</td>`;

                let dateDisplay = item.month && item.year
                    ? `${item.month}/${item.year}`
                    : new Date(item.createdAt).toLocaleDateString();
                html += `<td class="py-3 text-muted">${dateDisplay}</td>`;

                html += `<td class="py-3 fw-bold text-end ${item.status === 'PAID' ? 'text-success' : 'text-danger'}">${item.totalAmount.toLocaleString()} đ</td>`;

                if (item.status === 'PAID') {
                    html += `<td class="py-3"><span class="badge bg-success px-3 py-2 rounded-pill shadow-sm"><i class="fa-solid fa-check me-1"></i> Đã thanh toán</span></td>`;
                } else {
                    html += `<td class="py-3"><span class="badge bg-warning text-dark px-3 py-2 rounded-pill shadow-sm"><i class="fa-solid fa-clock me-1"></i> Chưa thanh toán</span></td>`;
                }

                html += '<td class="py-3 px-4 text-end">';
                html += `<a href="/Invoice/Details/${item.invoiceId}" class="btn btn-sm btn-outline-primary rounded-pill px-3 me-2 hover-lift"><i class="fa-solid fa-eye me-1"></i>Chi tiết</a>`;
                
                if (item.status === 'PAID') {
                    html += `<button class="btn btn-sm btn-outline-secondary rounded-pill px-3 hover-lift" onclick="downloadReceipt(${item.invoiceId}, this)"><i class="fa-solid fa-file-pdf me-1"></i>Tải PDF</button>`;
                } else {
                    html += `<button class="btn btn-sm btn-success rounded-pill px-3 hover-lift" onclick="showPaymentQR(${item.invoiceId})"><i class="fa-solid fa-qrcode me-1"></i>Thanh toán</button>`;
                }
                html += '</td></tr>';
            });
            $('#invoiceTableBody').html(html);
        },
        error: function (err) {
            console.error('Lỗi lấy danh sách hóa đơn:', err);
            alert("Không thể tải dữ liệu hóa đơn. Vui lòng thử lại!");
        }
    });
}

function showPaymentQR(invoiceId) {
    var token = sessionStorage.getItem('vendorToken');
    $('#qrImage').hide();
    $('#qrLoading').show();
    $('#paymentModal').modal('show');

    $.ajax({
        url: `${API_BASE_URL}/${invoiceId}/generate-qr`,
        type: 'GET',
        headers: { 'Authorization': 'Bearer ' + token },
        success: function (res) {
            $('#qrImage').attr('src', res.qrUrl).show();
            $('#qrLoading').hide();
        },
        error: function () {
            alert('Có lỗi khi tạo mã QR. Vui lòng thử lại sau!');
            $('#paymentModal').modal('hide');
        }
    });
}

function downloadReceipt(invoiceId) {
    var token = sessionStorage.getItem('vendorToken');

    fetch(`/Invoice/ExportPdf?id=${invoiceId}`, {
        method: 'GET',
        headers: { 'Authorization': 'Bearer ' + token }
    })
        .then(res => {
            if (!res.ok) throw new Error("Lỗi khi tải file");
            return res.blob();
        })
        .then(blob => {
            var url = window.URL.createObjectURL(blob);
            var a = document.createElement('a');
            a.href = url;
            a.download = `BienLai_${invoiceId}.pdf`;
            document.body.appendChild(a);
            a.click();
            a.remove();
            window.URL.revokeObjectURL(url); 
        })
        .catch(err => {
            console.error(err);
            alert("Có lỗi xảy ra khi tải biên lai.");
        });
}