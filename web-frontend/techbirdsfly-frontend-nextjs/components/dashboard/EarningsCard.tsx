"use client";

import { ChevronRight } from "lucide-react";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";

const earningsItems = [
  { 
    icon: "🎮", 
    name: "Bento 3D Kit", 
    type: "Illustration", 
    color: "bg-purple-100" 
  },
  { 
    icon: "🎮", 
    name: "Bento 3D Kit", 
    type: "Coded Template", 
    color: "bg-blue-100" 
  },
  { 
    icon: "🎮", 
    name: "Bento 3D Kit", 
    type: "Illustration", 
    color: "bg-red-100" 
  },
];

export default function EarningsCard() {
  return (
    <div className="space-y-6">
      {/* Earnings This Month */}
      <Card className="p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-2">Your earning this month</h3>
        <p className="text-3xl font-bold text-purple-600 mb-4">735.2$</p>
        <p className="text-sm text-gray-600 mb-4">
          Update your payout method in Setting
        </p>
        <Button 
          variant="ghost" 
          className="text-purple-600 hover:text-purple-700 p-0 h-auto font-medium"
        >
          Withdraw All Earnings
        </Button>
      </Card>

      {/* Earnings by Item */}
      <Card className="p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Earnings by item</h3>
        <div className="space-y-4">
          {earningsItems.map((item, index) => (
            <div key={index} className="flex items-center gap-3">
              <div className={`w-10 h-10 rounded-lg flex items-center justify-center text-lg ${item.color}`}>
                {item.icon}
              </div>
              <div className="flex-1">
                <p className="font-medium text-gray-900">{item.name}</p>
                <p className="text-sm text-gray-500">{item.type}</p>
              </div>
              <ChevronRight className="w-4 h-4 text-gray-400" />
            </div>
          ))}
        </div>
      </Card>
    </div>
  );
}