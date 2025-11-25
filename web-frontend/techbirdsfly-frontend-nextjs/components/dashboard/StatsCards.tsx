"use client";

import { Users, MousePointer, DollarSign, Package } from "lucide-react";
import { Card } from "@/components/ui/card";

const statsData = [
  { 
    icon: Users, 
    label: "Users", 
    value: "35k", 
    iconColor: "bg-purple-100 text-purple-600" 
  },
  { 
    icon: MousePointer, 
    label: "Clicks", 
    value: "1m", 
    iconColor: "bg-green-100 text-green-600" 
  },
  { 
    icon: DollarSign, 
    label: "Sales", 
    value: "345$", 
    iconColor: "bg-red-100 text-red-600" 
  },
  { 
    icon: Package, 
    label: "Items", 
    value: "68", 
    iconColor: "bg-blue-100 text-blue-600" 
  },
];

export default function StatsCards() {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
      {statsData.map((stat, index) => (
        <Card key={index} className="p-6">
          <div className="flex items-center gap-4">
            <div className={`w-12 h-12 rounded-lg flex items-center justify-center ${stat.iconColor}`}>
              <stat.icon className="w-6 h-6" />
            </div>
            <div>
              <p className="text-sm text-gray-600">{stat.label}</p>
              <p className="text-2xl font-bold text-gray-900">{stat.value}</p>
            </div>
          </div>
        </Card>
      ))}
    </div>
  );
}