"use client";

import { useState } from "react";
import toast from "react-hot-toast";
import { Button } from "@/components/ui/button";

interface NotificationFormProps {
  profile?: any;
}

export default function NotificationForm({ profile }: NotificationFormProps) {
  const [formData, setFormData] = useState({
    enableAll: profile?.notificationsEnabled !== false,
    emailNotifications: profile?.emailNotifications !== false,
    pushNotifications: true,
    smsNotifications: false,
  });

  const handleToggle = (name: string, value: boolean) => {
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    // Form submission handled by parent page component
  }

  return (
    <form onSubmit={onSubmit} className="space-y-4">
      {/* Enable All Notifications */}
      <div className="flex items-center justify-between p-3 bg-gray-50 rounded-lg border border-gray-200">
        <label className="text-sm font-medium text-gray-700">Enable All Notifications</label>
        <input
          type="checkbox"
          checked={formData.enableAll}
          onChange={(e) => handleToggle("enableAll", e.target.checked)}
          className="w-4 h-4 rounded border-gray-300 cursor-pointer"
        />
      </div>

      {/* Email Notifications */}
      <div className="flex items-center justify-between p-3 bg-gray-50 rounded-lg border border-gray-200">
        <label className="text-sm font-medium text-gray-700">Email Notifications</label>
        <input
          type="checkbox"
          checked={formData.emailNotifications}
          onChange={(e) => handleToggle("emailNotifications", e.target.checked)}
          className="w-4 h-4 rounded border-gray-300 cursor-pointer"
          disabled={!formData.enableAll}
        />
      </div>

      {/* Push Notifications */}
      <div className="flex items-center justify-between p-3 bg-gray-50 rounded-lg border border-gray-200">
        <label className="text-sm font-medium text-gray-700">Push Notifications</label>
        <input
          type="checkbox"
          checked={formData.pushNotifications}
          onChange={(e) => handleToggle("pushNotifications", e.target.checked)}
          className="w-4 h-4 rounded border-gray-300 cursor-pointer"
          disabled={!formData.enableAll}
        />
      </div>

      {/* SMS Notifications */}
      <div className="flex items-center justify-between p-3 bg-gray-50 rounded-lg border border-gray-200">
        <label className="text-sm font-medium text-gray-700">SMS Notifications</label>
        <input
          type="checkbox"
          checked={formData.smsNotifications}
          onChange={(e) => handleToggle("smsNotifications", e.target.checked)}
          className="w-4 h-4 rounded border-gray-300 cursor-pointer"
          disabled={!formData.enableAll}
        />
      </div>

      {/* Action Buttons */}
      <div className="flex justify-end gap-3 pt-4 border-t border-gray-200 mt-6">
        <Button
          type="button"
          variant="outline"
          className="px-6 py-2 border-gray-300 text-gray-700 hover:bg-gray-50"
          onClick={() => setFormData({ enableAll: true, emailNotifications: true, pushNotifications: true, smsNotifications: false })}
        >
          Reset
        </Button>
      </div>
    </form>
  );
}
