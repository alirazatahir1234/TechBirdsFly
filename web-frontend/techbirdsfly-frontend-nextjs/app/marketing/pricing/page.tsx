export default function PricingPage() {
  const tiers = [
    {
      name: "Starter",
      price: "$29",
      description: "Perfect for individuals",
      features: [
        "Up to 5 websites",
        "AI Generator",
        "Basic templates",
        "Community support",
      ],
    },
    {
      name: "Pro",
      price: "$79",
      description: "For growing teams",
      features: [
        "Unlimited websites",
        "Advanced AI features",
        "Premium templates",
        "Priority support",
        "Team collaboration",
      ],
      highlighted: true,
    },
    {
      name: "Enterprise",
      price: "Custom",
      description: "For large organizations",
      features: [
        "Everything in Pro",
        "Custom integrations",
        "Dedicated support",
        "SLA guarantee",
      ],
    },
  ];

  return (
    <div className="min-h-screen bg-linear-to-b from-gray-50 to-white py-12">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <h1 className="text-4xl font-bold text-gray-900 text-center mb-12">
          Simple, Transparent Pricing
        </h1>
        <div className="grid md:grid-cols-3 gap-8">
          {tiers.map((tier) => (
            <div
              key={tier.name}
              className={`rounded-lg p-8 ${
                tier.highlighted
                  ? "bg-purple-600 text-white ring-2 ring-purple-600 scale-105"
                  : "bg-white border border-gray-200"
              }`}
            >
              <h3 className="text-2xl font-bold mb-2">{tier.name}</h3>
              <p
                className={tier.highlighted ? "text-purple-100" : "text-gray-600"}
              >
                {tier.description}
              </p>
              <p className="text-4xl font-bold my-6">{tier.price}</p>
              <ul className="space-y-3 mb-8">
                {tier.features.map((feature) => (
                  <li key={feature} className="flex items-center">
                    <span className="mr-3">✓</span>
                    {feature}
                  </li>
                ))}
              </ul>
              <button
                className={`w-full py-2 rounded-lg font-semibold ${
                  tier.highlighted
                    ? "bg-white text-purple-600 hover:bg-gray-100"
                    : "bg-purple-600 text-white hover:bg-purple-700"
                }`}
              >
                Get Started
              </button>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
