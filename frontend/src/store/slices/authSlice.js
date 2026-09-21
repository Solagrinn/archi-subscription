import { createSlice } from '@reduxjs/toolkit';

const authSlice = createSlice({
  name: 'auth',
  initialState: {
    currentCustomer: null,
  },
  reducers: {
    loginAsCustomer: (state, action) => {
      state.currentCustomer = action.payload;
    },
    logout: (state) => {
      state.currentCustomer = null;
    },
  },
});

export const { loginAsCustomer, logout } = authSlice.actions;
export default authSlice.reducer;

