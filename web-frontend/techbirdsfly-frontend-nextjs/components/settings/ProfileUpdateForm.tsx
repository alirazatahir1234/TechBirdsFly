"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useAuthStore } from "@/lib/store/authStore";
import { useProfileUpdate } from "@/lib/hooks/useProfileUpdate";
import { Eye, EyeOff } from "lucide-react";

export default function ProfileUpdateForm() {
  const { user } = useAuthStore();
  const { isLoading, updateProfile, changePassword } = useProfileUpdate();
  const [activeTab, setActiveTab] = useState<"profile" | "password">("profile");

  // Profile form state
  const [profileData, setProfileData] = useState({
    firstName: user?.firstName || "",
    lastName: user?.lastName || "",
    email: user?.email || "",
  });

  // Password form state
  const [showPasswords, setShowPasswords] = useState({
    current: false,
    new: false,
    confirm: false,
  });
  const [passwordData, setPasswordData] = useState({
    currentPassword: "",
    newPassword: "",
    confirmPassword: "",
  });

  const handleProfileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setProfileData((prev) => ({ ...prev, [name]: value }));
  };

  const handlePasswordChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setPasswordData((prev) => ({ ...prev, [name]: value }));
  };

  const handleProfileSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const success = await updateProfile(profileData);
    if (success) {
      setProfileData({
        firstName: user?.firstName || "",
        lastName: user?.lastName || "",
        email: user?.email || "",
      });
    }
  };

  const handlePasswordSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const success = await changePassword(passwordData);
    if (success) {
      setPasswordData({
        currentPassword: "",
        newPassword: "",
        confirmPassword: "",
      });
    }
  };

  return (
    <div className="bg-white rounded-lg shadow-sm border border-gray-100">
      {/* Tabs */}
      <div className="flex border-b border-gray-200">
        <button
          onClick={() => setActiveTab("profile")}
          className={`flex-1 py-4 px-6 font-medium text-center transition-colors ${
            activeTab === "profile"
              ? "text-purple-600 border-b-2 border-purple-600"
              : "text-gray-600 hover:text-gray-900"
          }`}
        >
          Update Profile
        </button>
        <button
          onClick={() => setActiveTab("password")}
          className={`flex-1 py-4 px-6 font-medium text-center transition-colors ${
            activeTab === "password"
              ? "text-purple-600 border-b-2 border-purple-600"
              : "text-gray-600 hover:text-gray-900"
          }`}
        >
          Change Password
        </button>
      </div>

      {/* Content */}
      <div className="p-6">
        {activeTab === "profile" && (
          <form onSubmit={handleProfileSubmit} className="space-y-6">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  First Name
                </label>
                <Input
                  type="text"
                  name="firstName"
                  value={profileData.firstName}
                  onChange={handleProfileChange}
                  placeholder="Enter first name"
                  className="bg-gray-50 border-gray-200"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Last Name
                </label>
                <Input
                  type="text"
                  name="lastName"
                  value={profileData.lastName}
                  onChange={handleProfileChange}
                  placeholder="Enter last name"
                  className="bg-gray-50 border-gray-200"
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Email Address
              </label>
              <Input
                type="email"
                name="email"
                value={profileData.email}
                onChange={handleProfileChange}
                placeholder="Enter email address"
                className="bg-gray-50 border-gray-200"
              />
            </div>

            <div className="flex justify-end gap-3 pt-4 border-t border-gray-200">
              <Button
                type="button"
                variant="outline"
                className="px-6 py-2"
                onClick={() =>
                  setProfileData({
                    firstName: user?.firstName || "",
                    lastName: user?.lastName || "",
                    email: user?.email || "",
                  })
                }
              >
                Cancel
              </Button>
              <Button
                type="submit"
                disabled={isLoading}
                className="px-6 py-2 bg-purple-600 hover:bg-purple-700 text-white"
              >
                {isLoading ? "Saving..." : "Save Changes"}
              </Button>
            </div>
          </form>
        )}

        {activeTab === "password" && (
          <form onSubmit={handlePasswordSubmit} className="space-y-6 max-w-md">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Current Password
              </label>
              <div className="relative">
                <Input
                  type={showPasswords.current ? "text" : "password"}
                  name="currentPassword"
                  value={passwordData.currentPassword}
                  onChange={handlePasswordChange}
                  placeholder="Enter current password"
                  className="bg-gray-50 border-gray-200 pr-10"
                />
                <button
                  type="button"
                  onClick={() =>
                    setShowPasswords((prev) => ({
                      ...prev,
                      current: !prev.current,
                    }))
                  }
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 hover:text-gray-700"
                >
                  {showPasswords.current ? (
                    <EyeOff className="w-4 h-4" />
                  ) : (
                    <Eye className="w-4 h-4" />
                  )}
                </button>
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                New Password
              </label>
              <div className="relative">
                <Input
                  type={showPasswords.new ? "text" : "password"}
                  name="newPassword"
                  value={passwordData.newPassword}
                  onChange={handlePasswordChange}
                  placeholder="Enter new password"
                  className="bg-gray-50 border-gray-200 pr-10"
                />
                <button
                  type="button"
                  onClick={() =>
                    setShowPasswords((prev) => ({
                      ...prev,
                      new: !prev.new,
                    }))
                  }
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 hover:text-gray-700"
                >
                  {showPasswords.new ? (
                    <EyeOff className="w-4 h-4" />
                  ) : (
                    <Eye className="w-4 h-4" />
                  )}
                </button>
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Confirm New Password
              </label>
              <div className="relative">
                <Input
                  type={showPasswords.confirm ? "text" : "password"}
                  name="confirmPassword"
                  value={passwordData.confirmPassword}
                  onChange={handlePasswordChange}
                  placeholder="Confirm new password"
                  className="bg-gray-50 border-gray-200 pr-10"
                />
                <button
                  type="button"
                  onClick={() =>
                    setShowPasswords((prev) => ({
                      ...prev,
                      confirm: !prev.confirm,
                    }))
                  }
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 hover:text-gray-700"
                >
                  {showPasswords.confirm ? (
                    <EyeOff className="w-4 h-4" />
                  ) : (
                    <Eye className="w-4 h-4" />
                  )}
                </button>
              </div>
            </div>

            <div className="flex justify-end gap-3 pt-4 border-t border-gray-200">
              <Button
                type="button"
                variant="outline"
                className="px-6 py-2"
                onClick={() =>
                  setPasswordData({
                    currentPassword: "",
                    newPassword: "",
                    confirmPassword: "",
                  })
                }
              >
                Cancel
              </Button>
              <Button
                type="submit"
                disabled={isLoading}
                className="px-6 py-2 bg-purple-600 hover:bg-purple-700 text-white"
              >
                {isLoading ? "Changing..." : "Change Password"}
              </Button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
}
