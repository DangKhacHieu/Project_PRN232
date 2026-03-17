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
                html += '<tr>';
                html += `<td>#${item.invoiceId}</td>`;

                // Cập nhật lại logic hiển thị ngày tháng theo DTO mới
                let dateDisplay = item.month && item.year
                    ? `Tháng ${item.month}/${item.year}`
                    : new Date(item.createdAt).toLocaleDateString();
                html += `<td>${dateDisplay}</td>`;

                html += `<td>${item.totalAmount.toLocaleString()} đ</td>`;

                // TRẠNG THÁI: DB của bạn đang lưu chữ IN HOA ("PAID", "UNPAID")
                if (item.status === 'PAID') {
                    html += '<td><span class="badge bg-success">Đã thanh toán</span></td>';
                } else {
                    html += '<td><span class="badge bg-danger">Chưa thanh toán</span></td>';
                }

                // NÚT HÀNH ĐỘNG
                html += '<td>';
                if (item.status === 'PAID') {
                    // Thêm tham số 'this' vào hàm downloadReceipt
                    html += `<button class="btn btn-sm btn-info text-white" onclick="downloadReceipt(${item.invoiceId}, this)">
                                <i class="fas fa-download"></i> Tải Biên lai
                            </button>`;
                } else {
                    html += `<button class="btn btn-sm btn-primary" onclick="showPaymentQR(${item.invoiceId})"><i class="fas fa-qrcode"></i> Thanh toán</button>`;
                }
                html += '</td>';
                html += '</tr>';
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

    fetch(`${API_BASE_URL}/${invoiceId}/export-pdf`, {
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