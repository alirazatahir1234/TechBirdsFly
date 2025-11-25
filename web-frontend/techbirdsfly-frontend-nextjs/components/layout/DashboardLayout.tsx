"use client";

import { ReactNode, useState } from "react";
import { Menu, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import Sidebar from "./Sidebar";
import Topbar from "./Topbar";

interface DashboardLayoutProps {
  children: ReactNode;
  title?: string;
}

export default function DashboardLayout({ children, title }: DashboardLayoutProps) {
  const [sidebarOpen, setSidebarOpen] = useState(false);

  return (
    <div className="flex h-screen bg-gray-50">
      {/* Mobile sidebar overlay */}
      {sidebarOpen && (
        <div 
          className="fixed inset-0 bg-black bg-opacity-50 z-40 lg:hidden"
          onClick={() => setSidebarOpen(false)}
        />
      )}
      
      {/* Sidebar */}
      <div className={`
        fixed lg:static inset-y-0 left-0 z-50 transform lg:transform-none transition-transform duration-300 ease-in-out
        ${sidebarOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0'}
      `}>
        <Sidebar />
      </div>
      
      {/* Main Content */}
      <div className="flex-1 flex flex-col overflow-hidden w-full lg:w-auto">
        {/* Top Navigation */}
        <div className="lg:hidden">
          <div className="flex items-center justify-between p-4 bg-white border-b border-gray-200">
            <Button
              variant="ghost"
              size="sm"
              onClick={() => setSidebarOpen(!sidebarOpen)}
              className="text-gray-600 hover:text-gray-900"
            >
              {sidebarOpen ? <X className="w-6 h-6" /> : <Menu className="w-6 h-6" />}
            </Button>
            <h1 className="text-xl font-bold text-gray-900">{title || "Dashboard"}</h1>
            <div className="w-10" /> {/* Spacer */}
          </div>
        </div>
        
        <div className="hidden lg:block">
          <Topbar title={title} />
        </div>
        
        {/* Page Content */}
        <main className="flex-1 overflow-auto p-4 lg:p-6">
          {children}
        </main>
      </div>
    </div>
  );
}