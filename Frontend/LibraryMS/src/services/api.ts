import axios from 'axios';

const api = axios.create({
    baseURL: 'https://localhost:7261/api/', // Replace with your backend API URL (e.g., http://localhost:5000/api)
    headers: {
        'Content-Type': 'application/json',
    },
});

api.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('access_token');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

async function refreshToken() {
    try {
        // Get the refresh token from localStorage or wherever it's stored
        const refreshToken = localStorage.getItem('refresh_token');

        if (!refreshToken) {
            throw new Error('No refresh token available');
        }

        // Make the POST request to the refresh endpoint
        const response = await api.post('/Auth/refresh', {
            refreshToken: refreshToken
        });

        // Extract new access token and refresh token from response
        const { accessToken, refreshToken: newRefreshToken } = response.data;

        // Store the new tokens in localStorage (or your preferred storage)
        localStorage.setItem('access_token', accessToken);
        localStorage.setItem('refresh_token', newRefreshToken);

        console.log('Tokens refreshed successfully');
        return { accessToken, newRefreshToken };
    } catch (error) {
        console.error('Token refresh failed:', error.response?.data?.Error || error.message);
        // Handle error (e.g., redirect to login page if refresh fails)
        throw error;
    }
}

api.interceptors.response.use(
    (response) => response,
    async (error) => {
        console.log(error);
        const originalRequest = error.config;
        // Check if the error is 401 and the request hasn't been retried yet
        if (error.response?.status === 401 && !originalRequest._retry) {
            originalRequest._retry = true;

            try {
                // Call the refresh token function
                const { accessToken } = await refreshToken();

                // Update the Authorization header with the new access token
                originalRequest.headers.Authorization = `Bearer ${accessToken}`;

                // Retry the original request
                return api(originalRequest);
            } catch (refreshError) {
                // Handle refresh failure (e.g., redirect to login)
                console.error('Refresh token failed:', refreshError);
                // Optionally, clear tokens and redirect to login
                localStorage.removeItem('access_token');
                localStorage.removeItem('refresh_token');
                window.location.href = '/login';
                return Promise.reject(refreshError);
            }
        }

        return Promise.reject(error);
    }
);

export default api;