(function ($) {
    'use strict';

    const API_URL = 'https://localhost:7169/api/markets'; // Đổi lại Port của bạn nếu khác
    const API_STALL = 'https://localhost:7169/api/stalls'; // Thêm API Stalls để gọi tìm kiếm
    let loadedMarketId = 0;
    let originalMarketData = null; // Thêm biến lưu dữ liệu gốc để khi "Hủy" thì vẽ lại

    $(document).ready(function () {

        // 1. NGAY KHI VÀO TRANG: TỰ ĐỘNG GỌI API LẤY DANH SÁCH CHỢ
        loadMarketList();

        function loadMarketList() {
            $.ajax({
                url: API_URL, // Gọi đến cổng GET /api/markets
                type: 'GET',
                success: function (markets) {
                    let dropdown = $('#viewMarketId');
                    dropdown.empty();

                    if (markets.length === 0) {
                        dropdown.append('<option value="">-- Chưa có chợ nào trong hệ thống --</option>');
                        return;
                    }

                    dropdown.append('<option value="">-- Vui lòng chọn chợ --</option>');
                    markets.forEach(m => {
                        dropdown.append(`<option value="${m.marketId}">${m.marketName}</option>`);
                    });
                },
                error: function () {
                    $('#viewMarketId').html('<option value="">Lỗi tải danh sách chợ!</option>');
                }
            });
        }

        // 2. TẢI SƠ ĐỒ LÊN MÀN HÌNH THEO CHỢ ĐÃ CHỌN
        $('#btnLoadViewMap').click(function () {
            const marketId = $('#viewMarketId').val();
            if (!marketId) {
                Swal.fire('Chú ý', 'Vui lòng chọn một khu chợ từ danh sách!', 'warning');
                return;
            }

            Swal.fire({ title: 'Đang tải bản đồ...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); }});

            $.ajax({
                url: `${API_URL}/${marketId}/layout`,
                type: 'GET',
                success: function (market) {
                    Swal.close();
                    loadedMarketId = market.marketId;
                    originalMarketData = market; // Cất dữ liệu đi để dành

                    $('#displayMarketName').text(`SƠ ĐỒ: ${market.marketName}`);
                    $('#mapPlaceholder').hide();
                    $('#btnGoToEdit').fadeIn();
                    $('#legendCard').fadeIn();
                    $('#searchCard').fadeIn(); // HIỆN BỘ LỌC TÌM KIẾM

                    renderStaticMap(market);
                },
                error: function () {
                    Swal.fire('Lỗi', 'Không thể tải sơ đồ này', 'error');
                }
            });
        });

        // 3. NÚT CHUYỂN HƯỚNG SANG TRANG CẬP NHẬT (EDIT)
        $('#btnGoToEdit').click(function () {
            const selectedMarketId = $('#viewMarketId').val(); // Lấy ID chợ đang xem
            if (selectedMarketId) {
                window.location.href = `/MarketMap/Edit?id=${selectedMarketId}`;
            } else {
                Swal.fire('Thông báo', 'Vui lòng chọn chợ trước khi chỉnh sửa!', 'info');
            }
        });

        // 4. HÀM VẼ BẢN ĐỒ TĨNH
        function renderStaticMap(market) {
            const container = $('#mapContainer');
            container.empty();
            const scale = 20;

            market.zones.forEach((zone, index) => {
                // Khung Khu vực
                const zoneDiv = $(`
                    <div class="zone-box" id="zone-${index}"
                         style="left: 0px; top: 0px;
                                transform: translate(${zone.minX * scale}px, ${zone.minY * scale}px);
                                width: ${(zone.maxX - zone.minX) * scale}px;
                                height: ${(zone.maxY - zone.minY) * scale}px;
                                z-index: 1;">
                        <span class="zone-title">${zone.zoneName} (Nháy đúp)</span>
                    </div>
                `);
                container.append(zoneDiv);

                // Vẽ Sạp
                zone.stalls.forEach(stall => {
                    let bgColor = '#28a745';
                    if (stall.status === 'RENTED') bgColor = '#dc3545';
                    else if (stall.status === 'MAINTENANCE') bgColor = '#ffc107';

                    const stallDiv = $(`
                        <div class="stall-box stall-of-zone-${index}" data-id="${stall.stallId}"
                             style="display: none; left: 0px; top: 0px;
                                    transform: translate(${stall.posX * scale}px, ${stall.posY * scale}px);
                                    width: ${stall.width * scale}px;
                                    height: ${stall.height * scale}px;
                                    background-color: ${bgColor};
                                    z-index: 10;">
                            ${stall.stallCode}
                        </div>
                    `);

                     // 👉 TƯƠNG TÁC: Click xem Popup (Đóng băng kéo thả)
                    stallDiv.click(function() {
                         let ownerName = 'Chưa có người thuê';
                         let storeName = 'Chưa có tên (Trống)';
                         
                         if (stall.stallContracts && stall.stallContracts.length > 0) {
                             const active = stall.stallContracts.find(c => (c.status || '').toUpperCase() === 'ACTIVE');
                             if (active && active.vendor) {
                                 storeName = active.vendor.businessName || 'Chưa đăng ký';
                                 if (active.vendor.user) {
                                     ownerName = active.vendor.user.fullName || 'Chưa có thông tin';
                                 }
                             }
                         }

                         Swal.fire({
                             title: `Thông tin sạp ${stall.stallCode}`,
                             html: `<div style="text-align: left; font-size: 1.1em; line-height: 1.8;">
                                        👉 <b>Khu vực:</b> ${zone.zoneName} <br/>
                                        👤 <b>Tên Chủ sạp:</b> <span class="text-primary fw-bold">${ownerName}</span> <br/>
                                        🏢 <b>Tên Cửa hàng (Tên sạp):</b> <span class="text-primary fw-bold">${storeName}</span> <br/>
                                        📏 <b>Diện tích:</b> ${stall.areaM2} m² <br/>
                                        🏷️ <b>Loại mặt hàng:</b> ${stall.allowedBusinessType || 'Chưa đăng ký'} <br/>
                                        📌 <b>Trạng thái:</b> ${stall.status === 'RENTED' ? '<span class="text-danger fw-bold">Đã cho thuê</span>' : stall.status === 'MAINTENANCE' ? '<span class="text-warning fw-bold">Đang bảo trì</span>' : '<span class="text-success fw-bold">Đang trống</span>'}
                                    </div>`,
                             icon: 'info',
                             confirmButtonText: 'Đóng'
                         });
                    });


                    container.append(stallDiv);
                });

                // Nháy đúp ẩn/hiện sạp
                $(`#zone-${index}`).dblclick(function () {
                    $(`.stall-of-zone-${index}`).fadeToggle("fast");
                });
            });
        }

        // ==========================================
        // 5. LOGIC TRA CỨU SẠP TRÊN BẢN ĐỒ
        // ==========================================
        $('#btnSearch').click(function() {
            if (!loadedMarketId) return;

            const q = $('#txtSearchStall').val().trim();
            const status = $('#selStatus').val();

            if (!q && !status) {
                $('#btnResetSearch').click();
                return;
            }

            Swal.fire({ title: 'Đang dò tìm...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); }});

            $.ajax({
                url: `${API_STALL}/search/${loadedMarketId}?q=${encodeURIComponent(q)}&status=${encodeURIComponent(status)}`,
                type: 'GET',
                success: function(matchingStallIds) {
                    Swal.close();

                    if(!matchingStallIds || matchingStallIds.length === 0) {
                        Swal.fire('Thông báo', 'Không có sạp nào khớp với điều kiện tìm kiếm!', 'info');
                        $('#btnResetSearch').click();
                        return;
                    }

                    // BƯỚC 1: Tắt đèn sân khấu (Ẩn sạp, mờ Khu vực)
                    $('.stall-box').hide();
                    $('.zone-box').css({'opacity': '0.2', 'border': '1px dashed #ccc'});

                    // BƯỚC 2: Chiếu Spotlight vào các sạp tìm được
                    matchingStallIds.forEach(id => {
                        const el = $(`[data-id="${id}"]`);
                        if (el.length) {
                            el.show();
                            el.css({
                                'border': '3px solid #ffea00',
                                'box-shadow': '0 0 20px 8px rgba(255, 234, 0, 0.9)',
                                'z-index': '999',
                                'transform': el.css('transform') + ' scale(1.15)'
                            });

                            const classList = el.attr('class').split(/\s+/);
                            const zoneClass = classList.find(c => c.startsWith('stall-of-zone-'));
                            if(zoneClass) {
                                const index = zoneClass.replace('stall-of-zone-', '');
                                $(`#zone-${index}`).css({'opacity': '1', 'border': '2px solid #17a2b8'});
                            }
                        }
                    });
                },
                error: function(xhr) {
                    Swal.close();
                   Swal.fire('Lỗi API', xhr.responseJSON?.message || xhr.responseText || 'Chưa kết nối được với hệ thống tìm kiếm!', 'error');
                }
            });
        });

        // NÚT HỦY (Trả lại sơ đồ gốc)
        $('#btnResetSearch').click(function() {
            $('#txtSearchStall').val('');
            $('#selStatus').val('');

            if (originalMarketData) {
                renderStaticMap(originalMarketData); // Vẽ lại map sạch sẽ
            }
        });

        // Nhấn phím Enter để tìm kiếm luôn
        $('#txtSearchStall').keypress(function(e) {
            if(e.which == 13) $('#btnSearch').click();
        });
    });
})(jQuery);
