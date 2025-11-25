export default function FeaturesPage() {
  return (
    <div className="min-h-screen bg-linear-to-b from-gray-50 to-white">
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <h1 className="text-4xl font-bold text-gray-900 mb-8">Features</h1>
        <div className="grid md:grid-cols-2 gap-8">
          <div className="bg-white p-6 rounded-lg border border-gray-200">
            <h3 className="text-xl font-semibold text-gray-900 mb-2">AI Generator</h3>
            <p className="text-gray-600">Create stunning websites with AI in seconds</p>
          </div>
          <div className="bg-white p-6 rounded-lg border border-gray-200">
            <h3 className="text-xl font-semibold text-gray-900 mb-2">Code Export</h3>
            <p className="text-gray-600">Export clean, production-ready code</p>
          </div>
          <div className="bg-white p-6 rounded-lg border border-gray-200">
            <h3 className="text-xl font-semibold text-gray-900 mb-2">Templates</h3>
            <p className="text-gray-600">Use pre-built templates as starting points</p>
          </div>
          <div className="bg-white p-6 rounded-lg border border-gray-200">
            <h3 className="text-xl font-semibold text-gray-900 mb-2">Collaboration</h3>
            <p className="text-gray-600">Work together with your team in real-time</p>
          </div>
        </div>
      </div>
    </div>
  );
}
