import { authService } from './auth.js';

const originalFetch = window.fetch;

window.fetch = async function (url, options = {}) {
    if (url.startsWith('/api/')) {
        return authService.makeAuthRequest(url, options);
    }

    return originalFetch(url, options);
};

export function fetchWithAuth(url, options) {
    return authService.makeAuthRequest(url, options);
}