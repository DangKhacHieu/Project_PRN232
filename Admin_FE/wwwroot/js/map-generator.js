const API_URL = 'https://localhost:7169/api/markets';
let changedStalls = {};
let zoneCounter = 1;

$(document).ready(function () {

    // ==========================================
    // 1. GIAO DIỆN: THÊM & XÓA KHU VỰC
    // ==========================================
    $('#btnAddZone').click(function () {
        zoneCounter++;
        const newZoneHtml = `
                <div class="zone-form border p-3 mb-3 bg-light rounded">
                    <h5 class="zone-title-label">Khu ${zoneCounter}</h5>
                    <div class="form-group mb-1"><label>Tên khu:</label><input type="text" class="form-control z-name" value="Khu Mới ${zoneCounter}" /></div>
                    <div class="form-group mb-1"><label>Tiền tố mã:</label><input type="text" class="form-control z-prefix" value="KM" /></div>
                    <div class="row">
                        <div class="col-6 form-group mb-1"><label>Tổng sạp:</label><input type="number" class="form-control z-num" value="8" /></div>
                        <div class="col-6 form-group mb-1"><label>Số cột:</label><input type="number" class="form-control z-col" value="4" /></div>
                    </div>
                    <div class="row">
                        <div class="col-4 form-group mb-1"><label>Rộng(m):</label><input type="number" class="form-control z-w" value="3" /></div>
                        <div class="col-4 form-group mb-1"><label>Dài(m):</label><input type="number" class="form-control z-h" value="3" /></div>
                        <div class="col-4 form-group mb-1"><label>Lối đi(m):</label><input type="number" class="form-control z-gap" value="1" /></div>
                    </div>
                    <button type="button" class="btn btn-danger btn-sm w-100 mt-2 btn-remove-zone">Xóa Khu Này</button>
                </div>
            `;
        $('#zonesContainer').append(newZoneHtml);
    });

    $(document).on('click', '.btn-remove-zone', function () {
        $(this).closest('.zone-form').remove();
    });

    $(document).on('input', '.z-name', function () {
        const name = $(this).val() || '';
        const cleanName = name.toUpperCase().replace(/\bKHU\b/g, "").trim();
        const prefix = cleanName.split(/\s+/).map(word => word.charAt(0) || '').join('');
        $(this).closest('.zone-form').find('.z-prefix').val(prefix || 'K');
    });

    // ==========================================
    // 2. TẠO MỚI SƠ ĐỒ CHỢ
    // ==========================================
    $('#btnGenerate').click(function () {
        let zonesArray = [];

        $('.zone-form').each(function () {
            zonesArray.push({
                zoneName: $(this).find('.z-name').val() || "Khu Mới",
                zonePrefix: $(this).find('.z-prefix').val() || "K",
                numberOfStalls: parseInt($(this).find('.z-num').val()) || 1,
                columns: parseInt($(this).find('.z-col').val()) || 1,
                stallWidth: parseFloat($(this).find('.z-w').val()) || 1,
                stallHeight: parseFloat($(this).find('.z-h').val()) || 1,
                stallGap: parseFloat($(this).find('.z-gap').val()) || 0
            });
        });

        const requestData = {
            marketName: $('#marketName').val().trim(),
            zoneGap: parseFloat($('#zoneGap').val()) || 0,
            zones: zonesArray
        };

        if (!requestData.marketName) {
            Swal.fire('Thiếu thông tin!', 'Vui lòng nhập Tên chợ!', 'warning');
            return;
        }

        // Hiện loading mượt mà
        Swal.fire({ title: 'Đang xử lý...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });

        $.ajax({
            url: `${API_URL}/generate`,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(requestData),
            success: function (response) {
                // Đã xóa bỏ đoạn (ID Chợ: ${response.marketId}) 
                Swal.fire({
                    title: 'Thành công!',
                    text: 'Tạo sơ đồ thành công! Hãy nháy đúp vào từng Khu để xem sạp.',
                    icon: 'success'
                });

                renderMap(response.marketData);
                $('#btnSavePositions').show();
            },
            error: function (err) {
                if (err.responseJSON && err.responseJSON.errors) {
                    let errorHtml = "<ul style='text-align: left;'>";
                    let errors = err.responseJSON.errors;
                    for (let field in errors) {
                        errorHtml += `<li><b>${field}</b>: ${errors[field].join(', ')}</li>`;
                    }
                    errorHtml += "</ul>";

                    Swal.fire({ title: 'Lỗi Dữ Liệu', html: errorHtml, icon: 'error' });
                } else if (err.responseText) {
                    Swal.fire('Lỗi Server', err.responseText, 'error');
                } else {
                    Swal.fire('Lỗi', 'Sai định dạng dữ liệu gửi lên.', 'error');
                    console.error(err);
                }
            }
        });
    });

    // ==========================================
    // 3. HÀM VẼ SƠ ĐỒ LÊN MÀN HÌNH
    // ==========================================
    function renderMap(market) {
        const container = $('#mapContainer');
        container.empty();
        changedStalls = {};
        const scale = 20;

        market.zones.forEach((zone, index) => {
            const zoneDiv = $(`
                    <div class="zone-box draggable-zone" id="zone-${index}" 
                        data-x="${zone.minX * scale}" 
                        data-y="${zone.minY * scale}"
                        style="
                            left: 0px; top: 0px; 
                            transform: translate(${zone.minX * scale}px, ${zone.minY * scale}px);
                            width: ${(zone.maxX - zone.minX) * scale}px; 
                            height: ${(zone.maxY - zone.minY) * scale}px;
                            cursor: grab; z-index: 1;">
                        <span class="zone-title" style="pointer-events: none;">${zone.zoneName} (Nháy đúp)</span>
                    </div>
                `);
            container.append(zoneDiv);

            zone.stalls.forEach(stall => {
                const stallDiv = $(`
                        <div class="stall-box draggable stall-of-zone-${index}" 
                            data-id="${stall.stallId}"
                            data-x="${stall.posX * scale}" 
                            data-y="${stall.posY * scale}"
                            style="
                                display: none; left: 0px; top: 0px;
                                transform: translate(${stall.posX * scale}px, ${stall.posY * scale}px);
                                width: ${stall.width * scale}px; 
                                height: ${stall.height * scale}px;
                                z-index: 10;">
                            ${stall.stallCode}
                        </div>
                    `);
                container.append(stallDiv);
            });

            $(`#zone-${index}`).dblclick(function () {
                $(`.stall-of-zone-${index}`).fadeToggle("fast");
            });
        });

        initDraggable(scale);
    }

    // ==========================================
    // 4. KÍCH HOẠT KÉO THẢ (Sạp và Khu vực)
    // ==========================================
    function initDraggable(scale) {
        interact('.draggable').draggable({
            listeners: {
                move(event) {
                    const target = event.target;
                    const x = (parseFloat(target.getAttribute('data-x')) || 0) + event.dx;
                    const y = (parseFloat(target.getAttribute('data-y')) || 0) + event.dy;

                    target.style.transform = `translate(${x}px, ${y}px)`;
                    target.setAttribute('data-x', x);
                    target.setAttribute('data-y', y);

                    changedStalls[target.getAttribute('data-id')] = {
                        stallId: parseInt(target.getAttribute('data-id')),
                        posX: x / scale,
                        posY: y / scale
                    };
                }
            }
        });

        interact('.draggable-zone').draggable({
            listeners: {
                move(event) {
                    const target = event.target;
                    const x = (parseFloat(target.getAttribute('data-x')) || 0) + event.dx;
                    const y = (parseFloat(target.getAttribute('data-y')) || 0) + event.dy;

                    target.style.transform = `translate(${x}px, ${y}px)`;
                    target.setAttribute('data-x', x);
                    target.setAttribute('data-y', y);

                    const zoneIndex = target.id.split('-')[1];

                    $(`.stall-of-zone-${zoneIndex}`).each(function () {
                        const stallX = (parseFloat(this.getAttribute('data-x')) || 0) + event.dx;
                        const stallY = (parseFloat(this.getAttribute('data-y')) || 0) + event.dy;

                        this.style.transform = `translate(${stallX}px, ${stallY}px)`;
                        this.setAttribute('data-x', stallX);
                        this.setAttribute('data-y', stallY);

                        const stallDbId = this.getAttribute('data-id');
                        changedStalls[stallDbId] = {
                            stallId: parseInt(stallDbId),
                            posX: stallX / scale,
                            posY: stallY / scale
                        };
                    });
                }
            }
        });
    }

    // ==========================================
    // 5. LƯU TỌA ĐỘ MỚI XUỐNG DATABASE
    // ==========================================
    $('#btnSavePositions').click(function () {
        const updateList = Object.values(changedStalls);
        if (updateList.length === 0) {
            Swal.fire('Chú ý!', 'Bạn chưa di chuyển sạp nào cả!', 'info');
            return;
        }

        Swal.fire({ title: 'Đang lưu...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });

        $.ajax({
            url: `${API_URL}/stalls/update-positions`,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(updateList),
            success: function () {
                Swal.fire('Thành công!', 'Đã lưu vị trí mới vào Database!', 'success');
                changedStalls = {};
            },
            error: function (err) {
                Swal.fire('Lỗi', 'Lỗi khi lưu vị trí. Vui lòng check Console.', 'error');
                console.error(err);
            }
        });
    });

    // ==========================================
    // 6. TẢI LẠI SƠ ĐỒ ĐÃ LƯU
    // ==========================================
    $('#btnLoadMap').click(function () {
        const marketId = $('#loadMarketId').val();

        if (!marketId || marketId <= 0) {
            Swal.fire('Thiếu thông tin!', 'Vui lòng nhập ID chợ hợp lệ!', 'warning');
            return;
        }

        Swal.fire({ title: 'Đang tải...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });

        $.ajax({
            url: `${API_URL}/${marketId}/layout`,
            type: 'GET',
            success: function (marketData) {
                Swal.fire('Thành công!', `Đã tải sơ đồ: ${marketData.marketName}`, 'success');
                $('#marketName').val(marketData.marketName);
                renderMap(marketData);
                $('#btnSavePositions').show();
            },
            error: function (err) {
                if (err.status === 404) {
                    Swal.fire('Không tìm thấy!', `Không có chợ nào có tên = ${marketName} trong hệ thống!`, 'warning');
                } else {
                    Swal.fire('Lỗi', 'Lỗi khi tải sơ đồ. Hãy check console.', 'error');
                    console.error(err);
                }
            }
        });
    });

    // ==========================================
    // 7. CLICK CHUỘT PHẢI VÀO SẠP ĐỂ SỬA / XÓA
    // ==========================================
    $(document).on('contextmenu', '.stall-box', function (e) {
        e.preventDefault(); // Chặn cái menu chuột phải mặc định xấu xí của trình duyệt web

        const stallDiv = $(this);
        const stallId = stallDiv.attr('data-id');
        const currentCode = stallDiv.text().trim();

        // Bật popup cực đẹp của SweetAlert2
        Swal.fire({
            title: `Quản lý Sạp`,
            html: `
                    <div style="text-align: left;">
                        <label>Mã sạp (Tên sạp):</label>
                        <input id="swal-stallCode" class="swal2-input" style="margin-top: 0;" value="${currentCode}" disabled>
                        
                        <label class="mt-3">Trạng thái:</label>
                        <select id="swal-status" class="swal2-input" style="margin-top: 0;" disabled>
                            <option value="VACANT">🟢 Đang trống</option>
                            <option value="RENTED">🔴 Đã cho thuê</option>
                            <option value="MAINTENANCE">🟡 Bảo trì</option>
                        </select>
                    </div>
                `,
            showCancelButton: true,
            showDenyButton: true,
            confirmButtonText: '💾 Lưu thông tin',
            denyButtonText: '🗑️ Xóa sạp này',
            cancelButtonText: 'Hủy',
            confirmButtonColor: '#28a745',
            denyButtonColor: '#dc3545',
            preConfirm: () => {
                return {
                    stallCode: document.getElementById('swal-stallCode').value,
                    status: document.getElementById('swal-status').value
                }
            }
        }).then((result) => {
            // NẾU BẤM NÚT LƯU (CẬP NHẬT)
            if (result.isConfirmed) {
                const newData = result.value;

                // Gửi API PUT xuống C#
                $.ajax({
                    url: `${API_URL}/stalls/${stallId}`,
                    type: 'PUT',
                    contentType: 'application/json',
                    // Tạm thời gửi Width/Height mặc định vì giao diện đang tập trung sửa Tên & Trạng thái
                    data: JSON.stringify({
                        stallCode: newData.stallCode,
                        width: 3,
                        height: 3,
                        status: newData.status,
                        allowedBusinessType: "Tự do"
                    }),
                    success: function () {
                        Swal.fire('Đã lưu!', 'Cập nhật sạp thành công.', 'success');
                        // Đổi tên sạp trực tiếp trên giao diện mà không cần load lại trang
                        stallDiv.text(newData.stallCode);

                        // Đổi màu sạp theo trạng thái cho trực quan
                        if (newData.status === 'RENTED') stallDiv.css('background-color', '#dc3545'); // Đỏ
                        else if (newData.status === 'MAINTENANCE') stallDiv.css('background-color', '#ffc107'); // Vàng
                        else stallDiv.css('background-color', '#28a745'); // Xanh lá
                    },
                    error: function (err) {
                        Swal.fire('Lỗi!', 'Không thể cập nhật sạp.', 'error');
                        console.error(err);
                    }
                });
            }
            // NẾU BẤM NÚT XÓA
            else if (result.isDenied) {
                Swal.fire({
                    title: 'Bạn chắc chắn chứ?',
                    text: `Hành động này sẽ xóa vĩnh viễn sạp ${currentCode}!`,
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonText: 'Đồng ý Xóa',
                    confirmButtonColor: '#dc3545'
                }).then((confirmDelete) => {
                    if (confirmDelete.isConfirmed) {
                        // Gửi API DELETE xuống C#
                        $.ajax({
                            url: `${API_URL}/stalls/${stallId}`,
                            type: 'DELETE',
                            success: function () {
                                Swal.fire('Đã xóa!', 'Sạp đã bay màu khỏi sơ đồ.', 'success');
                                // Gỡ cái sạp đó ra khỏi màn hình HTML luôn
                                stallDiv.fadeOut(300, function () { $(this).remove(); });
                            },
                            error: function (err) {
                                Swal.fire('Lỗi!', 'Không thể xóa sạp này.', 'error');
                                console.error(err);
                            }
                        });
                    }
                });
            }
        });
    });

});