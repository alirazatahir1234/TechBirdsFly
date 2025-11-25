"use client";

import { Card } from "@/components/ui/card";
import { TrendingUp, TrendingDown } from "lucide-react";

interface AnalyticsStatsCardProps {
  title: string;
  value: string;
  badge: string;
  isPositive: boolean;
  percentage: number;
}

export default function AnalyticsStatsCard({
  title,
  value,
  badge,
  isPositive,
  percentage,
}: AnalyticsStatsCardProps) {
  return (
    <Card className="p-6 bg-white border border-gray-200 shadow-sm">
      <div className="flex items-center justify-between mb-4">
        <p className="text-sm text-gray-600 font-medium">{title}</p>
        <div
          className={`flex items-center gap-1 px-2 py-1 rounded-md text-xs font-semibold ${
            isPositive
              ? "bg-green-50 text-green-700"
              : "bg-red-50 text-red-700"
          }`}
        >
          {isPositive ? (
            <TrendingUp className="w-3 h-3" />
          ) : (
            <TrendingDown className="w-3 h-3" />
          )}
          {isPositive ? "+" : ""}{percentage}%
        </div>
      </div>
      <h2 className="text-3xl font-bold text-gray-900 mb-2">{value}</h2>
      <p className="text-xs text-gray-500">{badge}</p>
    </Card>
  );
}