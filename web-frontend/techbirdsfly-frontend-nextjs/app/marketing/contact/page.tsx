export default function ContactPage() {
  return (
    <div className="min-h-screen bg-linear-to-b from-gray-50 to-white py-12">
      <div className="max-w-2xl mx-auto px-4 sm:px-6 lg:px-8">
        <h1 className="text-4xl font-bold text-gray-900 mb-8 text-center">
          Get in Touch
        </h1>
        <div className="bg-white rounded-lg p-8 border border-gray-200">
          <form className="space-y-6">
            <div>
              <label className="block text-sm font-semibold text-gray-900 mb-2">
                Name
              </label>
              <input
                type="text"
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-600"
                placeholder="Your name"
              />
            </div>
            <div>
              <label className="block text-sm font-semibold text-gray-900 mb-2">
                Email
              </label>
              <input
                type="email"
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-600"
                placeholder="your@email.com"
              />
            </div>
            <div>
              <label className="block text-sm font-semibold text-gray-900 mb-2">
                Message
              </label>
              <textarea
                rows={5}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-600"
                placeholder="Your message..."
              ></textarea>
            </div>
            <button
              type="submit"
              className="w-full bg-purple-600 text-white py-2 rounded-lg font-semibold hover:bg-purple-700"
            >
              Send Message
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}
