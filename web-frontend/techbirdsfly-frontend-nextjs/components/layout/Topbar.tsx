"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { Search, Bell, Settings, LogOut, User, Loader } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useAuthStore } from "@/lib/store/authStore";
import toast from "react-hot-toast";
import { AppLogoIcon } from "@/components/AppLogo";

interface TopbarProps {
  title?: string;
}

export default function Topbar({ title = "Dashboard" }: TopbarProps) {
  const [menuOpen, setMenuOpen] = useState(false);
  const [isLoggingOut, setIsLoggingOut] = useState(false);
  const router = useRouter();
  const { user, logout } = useAuthStore();

  const getInitial = () => {
    if (user?.firstName && user?.lastName) {
      return (user.firstName[0] + user.lastName[0]).toUpperCase();
    }
    if (user?.firstName) {
      return user.firstName[0].toUpperCase();
    }
    return "U";
  };

  const handleLogout = async () => {
    setIsLoggingOut(true);
    try {
      console.log("🚪 Starting logout...");
      logout();
      console.log("✅ Logout successful, clearing store");
      
      toast.success("Logged out successfully!", {
        duration: 2000,
      });
      
      console.log("📍 Redirecting to main website...");
      
      // Give toast time to show and ensure logout is complete
      await new Promise(resolve => setTimeout(resolve, 500));
      
      router.push("/marketing");
      console.log("🚀 Pushed to marketing route");
    } catch (error) {
      console.error("❌ Logout error:", error);
      toast.error("Error logging out: " + (error instanceof Error ? error.message : "Unknown error"), {
        duration: 3000,
      });
      setIsLoggingOut(false);
    }
  };

  return (
    <header className="bg-white border-b border-gray-200 px-6 py-4">
      <div className="flex items-center justify-between">
        {/* Logo and Page Title */}
        <div className="flex items-center gap-3">
          <AppLogoIcon size="sm" />
          <h1 className="text-2xl font-bold text-gray-900">{title}</h1>
        </div>

        {/* Search and Actions */}
        <div className="flex items-center gap-4">
          {/* Search Input */}
          <div className="relative">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-gray-400" />
            <Input
              type="text"
              placeholder="Search anything here..."
              className="pl-10 pr-4 py-2 w-80 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent"
            />
          </div>

          {/* Action Buttons */}
          <div className="flex items-center gap-2">
            <Button
              variant="ghost"
              size="sm"
              className="text-gray-600 hover:text-gray-900 hover:bg-gray-100"
            >
              <Bell className="w-5 h-5" />
            </Button>
            <Button
              variant="ghost"
              size="sm"
              className="text-gray-600 hover:text-gray-900 hover:bg-gray-100"
            >
              <Settings className="w-5 h-5" />
            </Button>
          </div>

          {/* User Profile Card with Dropdown */}
          <div className="relative">
            <button
              onClick={() => setMenuOpen(!menuOpen)}
              className="flex items-center gap-3 px-3 py-2 rounded-lg hover:bg-gray-50 transition-colors disabled:opacity-50"
              disabled={isLoggingOut}
            >
              <div className="w-10 h-10 bg-purple-600 rounded-full flex items-center justify-center text-white font-semibold text-sm">
                {getInitial()}
              </div>
              <div className="text-left hidden sm:block">
                <p className="text-sm font-medium text-gray-900">
                  {user?.firstName && user?.lastName
                    ? `${user.firstName} ${user.lastName}`
                    : user?.firstName
                    ? user.firstName
                    : "Guest User"}
                </p>
                <p className="text-xs text-gray-500 truncate max-w-[150px]">
                  {user?.email || "No email"}
                </p>
              </div>
            </button>

            {/* Dropdown Menu */}
            {menuOpen && !isLoggingOut && (
              <div className="absolute right-0 top-full mt-2 bg-white text-gray-900 rounded-lg shadow-xl p-2 w-56 z-50 border border-gray-200">
                {/* User Info */}
                <div className="px-3 py-2 border-b border-gray-200 mb-2">
                  <p className="text-sm font-semibold text-gray-900">
                    {user?.firstName && user?.lastName
                      ? `${user.firstName} ${user.lastName}`
                      : user?.firstName
                      ? user.firstName
                      : "Guest User"}
                  </p>
                  <p className="text-xs text-gray-500 truncate">
                    {user?.email || "No email"}
                  </p>
                </div>

                {/* Menu Items */}
                <Link href="/settings">
                  <button
                    onClick={() => setMenuOpen(false)}
                    className="w-full text-left py-2 px-3 hover:bg-gray-100 rounded flex items-center gap-2 text-sm text-gray-700 hover:text-gray-900"
                  >
                    <User className="w-4 h-4" />
                    Profile Settings
                  </button>
                </Link>

                <button
                  onClick={handleLogout}
                  disabled={isLoggingOut}
                  className="w-full text-left py-2 px-3 hover:bg-red-50 rounded flex items-center gap-2 text-sm text-red-600 hover:text-red-700 mt-1 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {isLoggingOut ? (
                    <Loader className="w-4 h-4 animate-spin" />
                  ) : (
                    <LogOut className="w-4 h-4" />
                  )}
                  {isLoggingOut ? "Logging out..." : "Logout"}
                </button>
              </div>
            )}

            {/* Loading State */}
            {isLoggingOut && (
              <div className="absolute right-0 top-full mt-2 bg-white text-gray-900 rounded-lg shadow-xl p-4 w-56 z-50 border border-gray-200 flex items-center justify-center gap-2">
                <Loader className="w-4 h-4 animate-spin text-purple-600" />
                <span className="text-sm text-gray-600">Logging out...</span>
              </div>
            )}
          </div>
        </div>
      </div>
    </header>
  );
}