export default function ForgotPasswordPage() {
  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      <div className="bg-white rounded-lg shadow-lg p-8 w-full max-w-md">
        <h1 className="text-2xl font-bold text-gray-900 mb-2 text-center">
          Reset Password
        </h1>
        <p className="text-gray-600 text-center mb-6">
          Enter your email to receive a password reset link
        </p>
        <form className="space-y-4">
          <div>
            <label className="block text-sm font-semibold text-gray-900 mb-2">
              Email Address
            </label>
            <input
              type="email"
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-600"
              placeholder="your@email.com"
            />
          </div>
          <button
            type="submit"
            className="w-full bg-purple-600 text-white py-2 rounded-lg font-semibold hover:bg-purple-700"
          >
            Send Reset Link
          </button>
          <p className="text-center text-gray-600">
            Remember your password?{" "}
            <a href="/auth/login" className="text-purple-600 font-semibold hover:text-purple-700">
              Log in
            </a>
          </p>
        </form>
      </div>
    </div>
  );
}
