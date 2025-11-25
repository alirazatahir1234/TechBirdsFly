"use client";

import { TrendingUp } from "lucide-react";
import { Card } from "@/components/ui/card";
import { useState, useEffect } from "react";

export default function ActiveUsers() {
  const [chartData, setChartData] = useState<{ time: number; value: number }[]>([]);
  const [isLoaded, setIsLoaded] = useState(false);

  // Generate chart data only on client to prevent hydration mismatch
  useEffect(() => {
    const data = Array.from({ length: 12 }, (_, i) => ({
      time: i,
      value: Math.random() * 80 + 20
    }));
    setChartData(data);
    setIsLoaded(true);
  }, []);

  return (
    <Card className="p-6">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h2 className="text-lg font-semibold text-gray-900">Active users right now</h2>
          <p className="text-3xl font-bold text-purple-600 mt-2">300</p>
        </div>
        <div className="text-sm text-gray-500">
          Page views per minute
        </div>
      </div>
      
      {/* Chart Area - Only render after hydration */}
      {isLoaded && (
        <div className="h-48 bg-linear-to-br from-purple-400 to-purple-600 rounded-lg flex items-end justify-center p-4 mb-4">
          {/* Simple bar chart representation */}
          <div className="flex items-end gap-2 w-full max-w-md h-32">
            {chartData.map((point, i) => (
              <div
                key={i}
                className="bg-white/30 rounded-t flex-1"
                style={{
                  height: `${point.value}%`,
                }}
              />
            ))}
          </div>
          
          {/* Chart line overlay */}
          <svg className="absolute w-full max-w-md h-32" viewBox="0 0 300 120">
            <path
              d={chartData.map((point, i) => 
                `${i === 0 ? 'M' : 'L'} ${(i / (chartData.length - 1)) * 280 + 10} ${120 - (point.value * 1.2)}`
              ).join(' ')}
              fill="none"
              stroke="white"
              strokeWidth="2"
              opacity="0.8"
            />
          </svg>
        </div>
      )}
      
      {/* Fallback loading state for server render */}
      {!isLoaded && (
        <div className="h-48 bg-linear-to-br from-purple-400 to-purple-600 rounded-lg animate-pulse" />
      )}

      {/* Payout Info */}
      <div className="flex items-center gap-2 text-sm text-gray-600">
        <TrendingUp className="w-4 h-4" />
        <span>Upgrade your payout method in Setting</span>
      </div>
    </Card>
  );
}