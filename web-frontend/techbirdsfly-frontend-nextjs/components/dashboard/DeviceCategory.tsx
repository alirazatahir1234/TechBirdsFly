"use client";

import { Card } from "@/components/ui/card";
import { Smartphone, Monitor, Tablet, Tv } from "lucide-react";

const deviceData = [
  { name: "Mobile", value: 9650, percentage: 45 },
  { name: "Desktop", value: 2340, percentage: 22 },
  { name: "Tablet", value: 1240, percentage: 15 },
  { name: "TV", value: 980, percentage: 12 },
];

const deviceIcons = {
  Mobile: Smartphone,
  Desktop: Monitor,
  Tablet: Tablet,
  TV: Tv,
};

const deviceColors = {
  Mobile: "bg-purple-100 text-purple-600",
  Desktop: "bg-blue-100 text-blue-600",
  Tablet: "bg-green-100 text-green-600",
  TV: "bg-orange-100 text-orange-600",
};

export default function DeviceCategory() {
  return (
    <Card className="p-6 bg-white border border-gray-200 shadow-sm">
      <h3 className="text-lg font-semibold text-gray-900 mb-6">Device Category</h3>

      <div className="space-y-4">
        {deviceData.map((device) => {
          const Icon = deviceIcons[device.name as keyof typeof deviceIcons];
          const colorClass =
            deviceColors[device.name as keyof typeof deviceColors];

          return (
            <div key={device.name} className="flex items-center gap-4">
              <div className={`p-2 rounded-lg ${colorClass}`}>
                <Icon className="w-5 h-5" />
              </div>
              <div className="flex-1">
                <div className="flex items-center justify-between mb-1">
                  <p className="text-sm font-medium text-gray-900">
                    {device.name}
                  </p>
                  <span className="text-sm font-semibold text-gray-900">
                    {device.percentage}%
                  </span>
                </div>
                <div className="w-full bg-gray-200 rounded-full h-1.5 overflow-hidden">
                  <div
                    className={`h-full rounded-full ${
                      device.name === "Mobile"
                        ? "bg-purple-600"
                        : device.name === "Desktop"
                          ? "bg-blue-600"
                          : device.name === "Tablet"
                            ? "bg-green-600"
                            : "bg-orange-600"
                    }`}
                    style={{ width: `${device.percentage}%` }}
                  ></div>
                </div>
              </div>
            </div>
          );
        })}
      </div>
    </Card>
  );
}