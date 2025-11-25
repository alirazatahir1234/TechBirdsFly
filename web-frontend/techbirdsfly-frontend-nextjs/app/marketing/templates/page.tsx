export default function TemplatesPage() {
  return (
    <div className="min-h-screen bg-linear-to-b from-gray-50 to-white py-12">
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
        <h1 className="text-4xl font-bold text-gray-900 mb-8">Templates</h1>
        <div className="grid md:grid-cols-2 gap-8">
          {[
            "Portfolio",
            "E-commerce",
            "Blog",
            "SaaS",
            "Agency",
            "Landing Page",
          ].map((template) => (
            <div
              key={template}
              className="bg-white rounded-lg p-8 border border-gray-200 hover:shadow-lg transition"
            >
              <div className="bg-gray-200 h-40 rounded mb-4"></div>
              <h3 className="text-xl font-semibold text-gray-900">{template}</h3>
              <button className="mt-4 text-purple-600 font-semibold hover:text-purple-700">
                Use Template →
              </button>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
