"use client";

import { ProtectedRoute } from '@/components/auth/ProtectedRoute';
import DashboardLayout from "@/components/layout/DashboardLayout";
import ActiveUsers from "@/components/dashboard/ActiveUsers";
import EarningsCard from "@/components/dashboard/EarningsCard";
import StatsCards from "@/components/dashboard/StatsCards";
import SalesByAgeChart from "@/components/dashboard/SalesByAgeChart";
import ImpressionChart from "@/components/dashboard/ImpressionChart";

export default function DashboardPage() {
  return (
    <ProtectedRoute>
      <DashboardLayout title="Dashboard">
        <div className="space-y-6">
          {/* Top Row - Active Users and Earnings */}
          <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
            {/* Active Users Chart */}
            <div className="lg:col-span-8">
              <ActiveUsers />
            </div>

            {/* Earnings Card */}
            <div className="lg:col-span-4">
              <EarningsCard />
            </div>
          </div>

          {/* Stats Cards Row */}
          <StatsCards />

          {/* Bottom Row - Sales Chart and Impression */}
          <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
            {/* Sales by Age Chart */}
            <div className="lg:col-span-8">
              <SalesByAgeChart />
            </div>

            {/* Impression Chart */}
            <div className="lg:col-span-4">
              <ImpressionChart />
            </div>
          </div>
        </div>
      </DashboardLayout>
    </ProtectedRoute>
  );
}