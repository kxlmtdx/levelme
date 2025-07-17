import { authService } from './auth.js';

document.addEventListener('DOMContentLoaded', () => {
    const registerForm = document.getElementById('registerForm');
    if (!registerForm) return;

    registerForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        clearErrors();

        const username = document.getElementById('username').value.trim();
        const email = document.getElementById('email').value.trim();
        const password = document.getElementById('password').value;
        const confirmPassword = document.getElementById('confirm-password').value;

        if (password !== confirmPassword) {
            showError('confirm-password', 'Пароли не совпадают');
            return;
        }

        try {
            const response = await fetch('/api/auth/register', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    username,
                    email,
                    password,
                    confirmPassword,
                }),
            });

            const data = await response.json();

            if (!response.ok) {
                if (data.errors) {
                    for (const [field, message] of Object.entries(data.errors)) {
                        showError(field, message);
                    }
                } else {
                    showGeneralError(data.message || 'Ошибка регистрации');
                }
                return;
            }

            const loginResult = await authService.login(username, password);

            if (loginResult.success) {
                window.location.href = '/profile';
            } else {
                showGeneralError('Регистрация прошла успешно, но вход не удался. Пожалуйста, войдите вручную.');
            }
        } catch (error) {
            console.error('Registration error:', error);
            showGeneralError('Произошла ошибка при регистрации');
        }
    });

    function clearErrors() {
        document.querySelectorAll('.error-message').forEach(el => {
            el.style.display = 'none';
            el.textContent = '';
        });
    }

    function showError(fieldId, message) {
        const errorElement = document.getElementById(`${fieldId}Error`);
        if (errorElement) {
            errorElement.textContent = message;
            errorElement.style.display = 'block';
        }
    }

    function showGeneralError(message) {
        const errorElement = document.getElementById('generalError') || createGeneralErrorElement();
        errorElement.textContent = message;
        errorElement.style.display = 'block';
    }

    function createGeneralErrorElement() {
        const errorElement = document.createElement('div');
        errorElement.id = 'generalError';
        errorElement.className = 'error-message';
        errorElement.style.color = 'red';
        errorElement.style.display = 'none';
        errorElement.style.marginBottom = '15px';
        registerForm.prepend(errorElement);
        return errorElement;
    }
});