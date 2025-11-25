"use client";

import { Card } from "@/components/ui/card";

const ageGroups = [
  { label: "35 to 40", value: 85 },
  { label: "30 to 35", value: 92 }, 
  { label: "25 to 30", value: 78 },
  { label: "20 to 25", value: 65 },
  { label: "15 to 20", value: 45 },
  { label: "10 to 15", value: 25 }
];

// Sample data points for the smooth curve
const chartPoints = [
  { x: 10, y: 75 }, { x: 20, y: 45 }, { x: 30, y: 55 }, { x: 40, y: 85 }, 
  { x: 50, y: 92 }, { x: 60, y: 88 }, { x: 70, y: 78 }, { x: 80, y: 65 }, 
  { x: 90, y: 45 }, { x: 100, y: 55 }, { x: 200, y: 75 }, { x: 300, y: 60 },
  { x: 400, y: 45 }, { x: 500, y: 35 }
];

export default function SalesByAgeChart() {
  // Generate smooth curve path
  const generatePath = () => {
    let path = `M 10,${120 - chartPoints[0].y}`;
    
    for (let i = 0; i < chartPoints.length - 1; i++) {
      const current = chartPoints[i];
      const next = chartPoints[i + 1];
      const cpx = (current.x + next.x) / 2;
      
      path += ` Q ${cpx},${120 - current.y} ${next.x},${120 - next.y}`;
    }
    
    return path;
  };

  return (
    <Card className="p-6">
      <div className="flex items-center justify-between mb-6">
        <h3 className="text-lg font-semibold text-gray-900">Sales by Age</h3>
        <div className="flex items-center gap-2 text-sm text-purple-600">
          <span className="w-3 h-3 bg-purple-600 rounded-full"></span>
          <span>Sales</span>
        </div>
      </div>

      <div className="flex">
        {/* Y-axis labels */}
        <div className="flex flex-col justify-between text-xs text-gray-500 mr-4 h-48">
          {ageGroups.map((group, index) => (
            <span key={index} className="text-right w-16">{group.label}</span>
          ))}
        </div>

        {/* Chart area */}
        <div className="flex-1 relative">
          <svg className="w-full h-48" viewBox="0 0 520 120">
            {/* Grid lines */}
            {[0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 200, 300, 400, 500].map(x => (
              <line 
                key={x} 
                x1={x} 
                y1="0" 
                x2={x} 
                y2="120" 
                stroke="#f3f4f6" 
                strokeWidth="1"
              />
            ))}
            
            {/* Main chart line */}
            <path
              d={generatePath()}
              fill="none"
              stroke="#7c3aed"
              strokeWidth="3"
              strokeLinecap="round"
            />
            
            {/* Data points */}
            {chartPoints.map((point, index) => (
              <circle
                key={index}
                cx={point.x}
                cy={120 - point.y}
                r="4"
                fill="#7c3aed"
              />
            ))}
          </svg>
          
          {/* X-axis labels */}
          <div className="flex justify-between text-xs text-gray-500 mt-2">
            <span>10</span>
            <span>20</span>
            <span>30</span>
            <span>40</span>
            <span>50</span>
            <span>60</span>
            <span>70</span>
            <span>80</span>
            <span>90</span>
            <span>100</span>
            <span>200</span>
            <span>300</span>
            <span>400</span>
            <span>500</span>
          </div>
        </div>
      </div>
    </Card>
  );
}