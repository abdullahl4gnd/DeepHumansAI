$(document).ready(function() {
    var form = $('#register-form');
    var submitBtn = form.find('button[type="submit"]');
    
    form.on('submit', function(e) {
        console.log('=== FORM SUBMIT TRIGGERED ===');
        
        var username = $('input[name="Input.UserName"]').val();
        var email = $('input[name="Input.Email"]').val();
        var password = $('input[name="Input.Password"]').val();
        var confirm = $('input[name="Input.ConfirmPassword"]').val();
        
        console.log('Username:', username);
        console.log('Email:', email);
        console.log('Password length:', password ? password.length : 0);
        console.log('Confirm length:', confirm ? confirm.length : 0);
        
        // Simple manual validation only
        if (!username || username.length < 1) {
            alert('Username is required');
            e.preventDefault();
            return false;
        }
        
        if (!email || email.indexOf('@') < 0) {
            alert('Valid email is required');
            e.preventDefault();
            return false;
        }
        
        if (!password || password.length < 10) {
            alert('Password must be at least 10 characters long');
            e.preventDefault();
            return false;
        }
        
        if (password !== confirm) {
            alert('Password and confirmation do not match');
            e.preventDefault();
            return false;
        }
        
        console.log('All validations passed, submitting...');
        submitBtn.prop('disabled', true).html('<i class="bi bi-hourglass-split"></i> Creating account...');
        // Let form submit naturally
    });
});

var toggleButtons = document.querySelectorAll('.toggle-password');
toggleButtons.forEach(function(btn) {
    btn.addEventListener('click', function() {
        var targetId = btn.getAttribute('data-target');
        var input = document.getElementById(targetId);
        if (!input) return;
        var isPassword = input.type === 'password';
        input.type = isPassword ? 'text' : 'password';
        btn.innerHTML = isPassword ? '<i class="bi bi-eye-slash"></i>' : '<i class="bi bi-eye"></i>';
    });
});

var passwordInput = document.getElementById('register-password');
var meterBar = document.getElementById('password-meter-bar');

function evaluateStrength(value) {
    var score = 0;
    var uniqueChars = new Set(value).size;
    
    if (value.length >= 10) score++;
    if (/[A-Z]/.test(value)) score++;
    if (/[a-z]/.test(value)) score++;
    if (/[0-9]/.test(value)) score++;
    if (/[^A-Za-z0-9]/.test(value)) score++;
    if (uniqueChars >= 4 && score === 5) score++;
    
    return Math.min(score, 5);
}

function updateMeter(score) {
    var widths = ['0%', '20%', '40%', '60%', '80%', '100%'];
    var colors = ['#e5e7eb', '#ef4444', '#f97316', '#f59e0b', '#10b981', '#22c55e'];
    meterBar.style.width = widths[score];
    meterBar.style.background = colors[score];
}

if (passwordInput) {
    passwordInput.addEventListener('input', function() {
        var val = passwordInput.value || '';
        var score = evaluateStrength(val);
        updateMeter(score);
    });
}
