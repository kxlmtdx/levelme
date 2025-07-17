class AuthService {
  constructor() {
    this.tokenKey = 'accessToken';
    this.refreshTokenKey = 'refreshToken';
  }

  setTokens(token, refreshToken) {
    localStorage.setItem(this.tokenKey, token);
    localStorage.setItem(this.refreshTokenKey, refreshToken);
  }

  getToken() {
    return localStorage.getItem(this.tokenKey);
  }

  getRefreshToken() {
    return localStorage.getItem(this.refreshTokenKey);
  }

  isAuthenticated() {
    return !!this.getToken();
  }

  clearTokens() {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.refreshTokenKey);
  }

  async makeAuthRequest(url, options = {}) {
    const token = this.getToken();
    const refreshToken = this.getRefreshToken();

    const headers = {
      'Content-Type': 'application/json',
      ...options.headers,
    };

    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    let response = await fetch(url, {
      ...options,
      headers,
    });

    if (response.status === 401 && refreshToken) {
      try {
        const newTokens = await this.refreshToken();
        if (newTokens) {
          headers['Authorization'] = `Bearer ${newTokens.token}`;
          response = await fetch(url, {
            ...options,
            headers,
          });
        } else {
          this.clearTokens();
          window.location.href = '/login';
          return null;
        }
      } catch (error) {
        console.error('Token refresh failed:', error);
        this.clearTokens();
        window.location.href = '/login';
        return null;
      }
    }

    return response;
    }

  async refreshToken() {
    const token = this.getToken();
    const refreshToken = this.getRefreshToken();

    if (!token || !refreshToken) {
      return null;
    }

    try {
      const response = await fetch('/api/auth/refresh', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          token,
          refreshToken,
        }),
      });

      if (response.ok) {
        const { token: newToken, refreshToken: newRefreshToken } = await response.json();
        this.setTokens(newToken, newRefreshToken);
        return { token: newToken, refreshToken: newRefreshToken };
      } else {
        this.clearTokens();
        return null;
      }
    } catch (error) {
      console.error('Refresh token error:', error);
      this.clearTokens();
      return null;
    }
  }

  async login(username, password) {
    try {
      const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          username,
          password,
        }),
      });

      const data = await response.json();

      if (response.ok) {
        this.setTokens(data.token, data.refreshToken);
        return { success: true };
      } else {
        return { success: false, message: data.message || 'Login failed' };
      }
    } catch (error) {
      console.error('Login error:', error);
      return { success: false, message: 'Network error' };
    }
  }

  async logout() {
    try {
      await this.makeAuthRequest('/api/auth/logout', {
        method: 'POST',
      });
    } finally {
      this.clearTokens();
      window.location.href = '/login';
    }
  }
}

export const authService = new AuthService();