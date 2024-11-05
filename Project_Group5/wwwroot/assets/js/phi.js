document.querySelector('form').addEventListener('submit', function (event) {
    event.preventDefault();

    var name = document.getElementById('name').value;
    var phone = document.getElementById('phone').value;
    var email = document.getElementById('email').value;
    var date = document.getElementById('date').value;

    var namePattern = /^[\p{L}\s]{3,}$/gu; // Chỉ chấp nhận ký tự chữ và khoảng trắng, tối thiểu 3 ký tự
    var phonePattern = /^0[0-9]{8,10}$/; // Bắt đầu bằng 0, theo sau là từ 8 đến 10 chữ số

    if (!name) {
        document.getElementById('nameError').textContent = 'Bạn chưa điền họ tên.';
        return;
    } else if (!namePattern.test(name)) {
        document.getElementById('nameError').textContent = 'Họ tên từ 3 kí tự, không có số.';
        return;
    } else {
        document.getElementById('nameError').textContent = '';
    }

    if (!phone) {
        document.getElementById('phoneError').textContent = 'Bạn chưa điền số điện thoại.';
        return;
    } else if (!phonePattern.test(phone)) {
        document.getElementById('phoneError').textContent = 'Số điện thoại phải bắt đầu bằng 0 và có từ 9 đến 11 chữ số.';
        return;
    } else {
        document.getElementById('phoneError').textContent = '';
    }

    if (!email) {
        document.getElementById('emailError').textContent = 'Bạn chưa điền email.';
        return;
    } else if (!email.includes('@')) {
        document.getElementById('emailError').textContent = 'Email phải chứa ký tự "@".';
        return;
    } else {
        document.getElementById('emailError').textContent = '';
    }
    if (!date) {
        document.getElementById('dateError').textContent = 'Bạn chưa chọn ngày tháng năm.';
        return;
    } else {
        document.getElementById('dateError').textContent = '';
    }

});