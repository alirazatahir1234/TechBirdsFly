'use client';

import { ProtectedRoute } from "@/components/auth/ProtectedRoute";
import Sidebar from "@/components/sidebar";
import "../globals.css";

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <ProtectedRoute>
      <div className="flex h-screen bg-gray-50 dark:bg-neutral-950">
        <Sidebar />
        <main className="ml-64 w-full overflow-y-auto">
          <div className="p-10 max-w-7xl mx-auto">{children}</div>
        </main>
      </div>
    </ProtectedRoute>
  );
}
