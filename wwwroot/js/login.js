import { authService } from './auth.js';

document.addEventListener('DOMContentLoaded', () => {
    const loginForm = document.getElementById('loginForm');
    if (!loginForm) return;

    if (authService.isAuthenticated()) {
        window.location.href = '/profile';
        return;
    }

    loginForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const usernameOrEmail = document.getElementById('login-username').value.trim();
        const password = document.getElementById('login-password').value;
        const errorElement = document.getElementById('loginError');

        if (errorElement) {
            errorElement.style.display = 'none';
            errorElement.textContent = '';
        }

        try {
            const result = await authService.login(usernameOrEmail, password);

            if (result.success) {
                window.location.href = '/profile';
            } else {
                if (errorElement) {
                    errorElement.textContent = result.message || 'Неверное имя пользователя или пароль';
                    errorElement.style.display = 'block';
                }
            }
        } catch (error) {
            console.error('Login error:', error);
            if (errorElement) {
                errorElement.textContent = 'Произошла ошибка при входе';
                errorElement.style.display = 'block';
            }
        }
    });
});