"use client";

import { Card } from "@/components/ui/card";

const countriesData = [
  { name: "Pakistan", flag: "🇵🇰", visitors: 2840 },
  { name: "Germany", flag: "🇩🇪", visitors: 1950 },
  { name: "United State", flag: "🇺🇸", visitors: 1240 },
  { name: "Spain", flag: "🇪🇸", visitors: 890 },
];

export default function TopCountries() {
  const maxVisitors = Math.max(...countriesData.map((c) => c.visitors));

  return (
    <Card className="p-6 bg-white border border-gray-200 shadow-sm">
      <h3 className="text-lg font-semibold text-gray-900 mb-6">Top Countries</h3>

      <div className="space-y-6">
        {countriesData.map((country) => (
          <div key={country.name} className="flex items-center gap-3">
            {/* Flag */}
            <div className="text-2xl">{country.flag}</div>

            {/* Country name and bar */}
            <div className="flex-1">
              <div className="flex items-center justify-between mb-1">
                <p className="text-sm font-medium text-gray-900">
                  {country.name}
                </p>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2 overflow-hidden">
                <div
                  className="h-full bg-linear-to-r from-purple-500 to-purple-600 rounded-full"
                  style={{
                    width: `${(country.visitors / maxVisitors) * 100}%`,
                  }}
                ></div>
              </div>
            </div>

            {/* Visitor count */}
            <div className="text-sm font-semibold text-gray-900 min-w-12 text-right">
              {country.visitors.toLocaleString()}
            </div>
          </div>
        ))}
      </div>
    </Card>
  );
}