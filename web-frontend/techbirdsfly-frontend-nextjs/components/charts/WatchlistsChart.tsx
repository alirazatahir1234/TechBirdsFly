"use client";

import { Card } from "@/components/ui/card";

const watchlistData = [
  { date: "May 5", green: 150, orange: 100 },
  { date: "May 6", green: 180, orange: 120 },
  { date: "May 7", green: 340, orange: 200 },
  { date: "May 8", green: 320, orange: 180 },
  { date: "May 9", green: 280, orange: 220 },
  { date: "May 10", green: 380, orange: 250 },
  { date: "May 11", green: 360, orange: 240 },
  { date: "May 12", green: 290, orange: 210 },
  { date: "May 13", green: 320, orange: 180 },
  { date: "May 14", green: 250, orange: 150 },
  { date: "May 15", green: 200, orange: 120 },
];

export default function WatchlistsChart() {
  const maxValue = 400;
  const width = 700;
  const height = 280;
  const padding = 50;
  const graphWidth = width - padding * 2;
  const graphHeight = height - padding * 2;

  const scaleX = (index: number) =>
    (index / (watchlistData.length - 1)) * graphWidth + padding;
  const scaleY = (value: number) =>
    height - padding - (value / maxValue) * graphHeight;

  // Green line path
  const greenPath = watchlistData
    .map((d, i) => `${i === 0 ? "M" : "L"} ${scaleX(i)} ${scaleY(d.green)}`)
    .join(" ");

  const greenAreaPath =
    greenPath +
    ` L ${scaleX(watchlistData.length - 1)} ${height - padding} L ${scaleX(0)} ${height - padding} Z`;

  // Orange line path
  const orangePath = watchlistData
    .map((d, i) => `${i === 0 ? "M" : "L"} ${scaleX(i)} ${scaleY(d.orange)}`)
    .join(" ");

  const orangeAreaPath =
    orangePath +
    ` L ${scaleX(watchlistData.length - 1)} ${height - padding} L ${scaleX(0)} ${height - padding} Z`;

  return (
    <Card className="p-6 bg-white border border-gray-200 shadow-sm">
      <div className="flex items-center justify-between mb-6">
        <h3 className="text-lg font-semibold text-gray-900">Watchlists</h3>
        <div className="flex gap-6">
          <button className="text-xs text-gray-600 hover:text-gray-900 font-medium">
            Day
          </button>
          <button className="text-xs text-gray-600 hover:text-gray-900 font-medium">
            Week
          </button>
          <button className="text-xs text-gray-600 hover:text-gray-900 font-medium">
            Month
          </button>
        </div>
      </div>

      {/* Chart */}
      <svg
        width="100%"
        height={height}
        viewBox={`0 0 ${width} ${height}`}
        className="w-full"
      >
        {/* Grid lines */}
        {[0, 100, 200, 300, 400].map((gridValue, i) => (
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
        {[0, 100, 200, 300, 400].map((gridValue, i) => (
          <text
            key={`label-${i}`}
            x={padding - 10}
            y={scaleY(gridValue) + 4}
            textAnchor="end"
            className="text-xs fill-gray-500"
          >
            {gridValue}
          </text>
        ))}

        {/* Green area fill */}
        <path d={greenAreaPath} fill="rgba(34, 197, 94, 0.1)" stroke="none" />

        {/* Orange area fill */}
        <path d={orangeAreaPath} fill="rgba(245, 158, 11, 0.1)" stroke="none" />

        {/* Green line */}
        <path
          d={greenPath}
          fill="none"
          stroke="#22c55e"
          strokeWidth="2.5"
          strokeLinecap="round"
          strokeLinejoin="round"
        />

        {/* Orange line */}
        <path
          d={orangePath}
          fill="none"
          stroke="#f59e0b"
          strokeWidth="2.5"
          strokeLinecap="round"
          strokeLinejoin="round"
        />

        {/* Green data points */}
        {watchlistData.map((d, i) => (
          <circle
            key={`green-${i}`}
            cx={scaleX(i)}
            cy={scaleY(d.green)}
            r="3"
            fill="#22c55e"
          />
        ))}

        {/* Orange data points */}
        {watchlistData.map((d, i) => (
          <circle
            key={`orange-${i}`}
            cx={scaleX(i)}
            cy={scaleY(d.orange)}
            r="3"
            fill="#f59e0b"
          />
        ))}

        {/* Peak labels */}
        {/* Green peak (May 10) */}
        <rect
          x={scaleX(5) - 18}
          y={scaleY(380) - 28}
          width="36"
          height="18"
          rx="3"
          fill="#22c55e"
        />
        <text
          x={scaleX(5)}
          y={scaleY(380) - 12}
          textAnchor="middle"
          className="text-xs font-semibold fill-white"
        >
          380
        </text>

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
        {[0, 2, 4, 6, 8, 10].map((i) => (
          <text
            key={`x-label-${i}`}
            x={scaleX(i)}
            y={height - padding + 20}
            textAnchor="middle"
            className="text-xs fill-gray-500"
          >
            {watchlistData[i]?.date.split(" ")[1]}
          </text>
        ))}
      </svg>

      {/* Legend */}
      <div className="flex gap-8 mt-6 px-4">
        <div className="flex items-center gap-2">
          <div className="w-3 h-3 bg-green-500 rounded-full"></div>
          <span className="text-sm text-gray-600">Primary Series</span>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-3 h-3 bg-orange-500 rounded-full"></div>
          <span className="text-sm text-gray-600">Secondary Series</span>
        </div>
      </div>
    </Card>
  );
}