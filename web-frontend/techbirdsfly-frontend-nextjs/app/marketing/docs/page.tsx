export default function DocsPage() {
  return (
    <div className="min-h-screen bg-linear-to-b from-gray-50 to-white py-12">
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
        <h1 className="text-4xl font-bold text-gray-900 mb-8">Documentation</h1>
        <div className="space-y-6">
          <section className="bg-white rounded-lg p-6 border border-gray-200">
            <h2 className="text-2xl font-semibold text-gray-900 mb-4">
              Getting Started
            </h2>
            <p className="text-gray-600">Learn the basics of TechBirdsFly...</p>
          </section>
          <section className="bg-white rounded-lg p-6 border border-gray-200">
            <h2 className="text-2xl font-semibold text-gray-900 mb-4">
              API Reference
            </h2>
            <p className="text-gray-600">Complete API documentation...</p>
          </section>
          <section className="bg-white rounded-lg p-6 border border-gray-200">
            <h2 className="text-2xl font-semibold text-gray-900 mb-4">
              Tutorials
            </h2>
            <p className="text-gray-600">Step-by-step guides and examples...</p>
          </section>
        </div>
      </div>
    </div>
  );
}
