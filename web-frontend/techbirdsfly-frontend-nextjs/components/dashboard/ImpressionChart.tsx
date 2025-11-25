"use client";

import { Card } from "@/components/ui/card";

const impressionData = [
  { day: "Mon", value: 40, active: false },
  { day: "Tue", value: 60, active: false },
  { day: "Wed", value: 100, active: true },
  { day: "Thu", value: 50, active: false },
];

export default function ImpressionChart() {
  const maxValue = Math.max(...impressionData.map(d => d.value));

  return (
    <Card className="p-6">
      <h3 className="text-lg font-semibold text-gray-900 mb-4">Impression</h3>
      
      <div className="space-y-4">
        {/* Chart Area */}
        <div className="flex items-end gap-3 h-32">
          {impressionData.map((item, index) => (
            <div key={index} className="flex-1 flex flex-col items-center">
              <div 
                className={`w-full rounded-t transition-colors ${
                  item.active ? 'bg-purple-600' : 'bg-purple-200'
                }`}
                style={{ 
                  height: `${(item.value / maxValue) * 100}%`,
                  minHeight: '8px'
                }}
              />
              <span className="text-xs text-gray-500 mt-2">{item.day}</span>
            </div>
          ))}
        </div>
        
        {/* Y-axis labels */}
        <div className="flex justify-between text-xs text-gray-500">
          <span>0</span>
          <span>10</span>
          <span>20</span>
        </div>
      </div>
    </Card>
  );
}