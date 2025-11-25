"use client";

import { Card } from "@/components/ui/card";
import { ChevronDown } from "lucide-react";

const chartData = [
  { day: 10, sales: 4000 },
  { day: 11, sales: 5500 },
  { day: 12, sales: 4200 },
  { day: 13, sales: 5800 },
  { day: 14, sales: 4900 },
  { day: 15, sales: 6200 },
  { day: 16, sales: 5100 },
  { day: 17, sales: 8234, isPeak: true },
  { day: 18, sales: 6100 },
  { day: 19, sales: 5400 },
  { day: 20, sales: 5800 },
  { day: 21, sales: 5200 },
  { day: 22, sales: 6100 },
  { day: 23, sales: 4900 },
  { day: 24, sales: 5600 },
  { day: 25, sales: 4800 },
];

export default function SalesFunnelChart() {
  // Create SVG path for smooth curve
  const maxValue = Math.max(...chartData.map((d) => d.sales));
  const width = 600;
  const height = 250;
  const padding = 40;
  const graphWidth = width - padding * 2;
  const graphHeight = height - padding * 2;

  // Scale functions
  const scaleX = (index: number) =>
    (index / (chartData.length - 1)) * graphWidth + padding;
  const scaleY = (value: number) =>
    height - padding - (value / maxValue) * graphHeight;

  // Generate SVG path
  const pathData = chartData
    .map((d, i) => {
      const x = scaleX(i);
      const y = scaleY(d.sales);
      return `${i === 0 ? "M" : "L"} ${x} ${y}`;
    })
    .join(" ");

  // Generate area fill path
  const areaPathData =
    pathData +
    ` L ${scaleX(chartData.length - 1)} ${height - padding} L ${scaleX(0)} ${height - padding} Z`;

  return (
    <Card className="p-6 bg-white border border-gray-200 shadow-sm">
      <div className="flex items-center justify-between mb-6">
        <h3 className="text-lg font-semibold text-gray-900">Sales Funnel</h3>
        <button className="flex items-center gap-1 text-sm text-purple-600 hover:text-purple-700 font-medium">
          This Month
          <ChevronDown className="w-4 h-4" />
        </button>
      </div>

      {/* Chart */}
      <svg
        width="100%"
        height={height}
        viewBox={`0 0 ${width} ${height}`}
        className="w-full"
      >
        {/* Grid lines */}
        {[0, 2000, 4000, 6000, 8000].map((gridValue, i) => (
          <line
            key={`grid-${i}`}
            x1={padding}
            y1={scaleY(gridValue)}
            x2={width - padding}
            y2={scaleY(gridValue)}
            stroke="#f3f4f6"
            strokeWidth="1"
          />
        ))}

        {/* Y-axis labels */}
        {[0, 2000, 4000, 6000, 8000].map((gridValue, i) => (
          <text
            key={`label-${i}`}
            x={padding - 10}
            y={scaleY(gridValue) + 4}
            textAnchor="end"
            className="text-xs fill-gray-500"
          >
            {gridValue / 1000}k
          </text>
        ))}

        {/* Area fill */}
        <path d={areaPathData} fill="rgba(168, 85, 247, 0.1)" stroke="none" />

        {/* Line */}
        <path
          d={pathData}
          fill="none"
          stroke="#a855f7"
          strokeWidth="3"
          strokeLinecap="round"
          strokeLinejoin="round"
        />

        {/* Data points */}
        {chartData.map((d, i) => (
          <g key={`point-${i}`}>
            {d.isPeak && (
              <>
                <circle
                  cx={scaleX(i)}
                  cy={scaleY(d.sales)}
                  r="6"
                  fill="#a855f7"
                />
                {/* Peak label */}
                <rect
                  x={scaleX(i) - 20}
                  y={scaleY(d.sales) - 28}
                  width="40"
                  height="20"
                  rx="4"
                  fill="#a855f7"
                />
                <text
                  x={scaleX(i)}
                  y={scaleY(d.sales) - 12}
                  textAnchor="middle"
                  className="text-xs font-semibold fill-white"
                >
                  08.5k
                </text>
              </>
            )}
          </g>
        ))}

        {/* X-axis */}
        <line
          x1={padding}
          y1={height - padding}
          x2={width - padding}
          y2={height - padding}
          stroke="#e5e7eb"
          strokeWidth="1"
        />

        {/* X-axis labels */}
        {[10, 13, 16, 19, 22, 25].map((day, i) => {
          const index = chartData.findIndex((d) => d.day === day);
          if (index === -1) return null;
          return (
            <text
              key={`x-label-${i}`}
              x={scaleX(index)}
              y={height - padding + 20}
              textAnchor="middle"
              className="text-xs fill-gray-500"
            >
              {day}
            </text>
          );
        })}
      </svg>
    </Card>
  );
}