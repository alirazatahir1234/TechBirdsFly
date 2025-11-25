"use client";

import { useState } from "react";
import toast from "react-hot-toast";
import { Button } from "@/components/ui/button";

interface PreferencesFormProps {
  profile?: any;
}

export default function PreferencesForm({ profile }: PreferencesFormProps) {
  const [formData, setFormData] = useState({
    timezone: profile?.preferences?.timezone || "utc",
    theme: profile?.preferences?.theme || "light",
    language: profile?.preferences?.language || "english",
  });

  const handleChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    // Form submission handled by parent page component
  }

  return (
    <form onSubmit={onSubmit} className="space-y-4">
      {/* Timezone */}
      <div>
        <label className="text-sm font-medium text-gray-700 block mb-1">Timezone</label>
        <select
          name="timezone"
          value={formData.timezone}
          onChange={handleChange}
          className="w-full px-4 py-2 bg-gray-50 border border-gray-200 rounded-lg text-gray-900 focus:outline-none focus:ring-2 focus:ring-purple-500"
        >
          <option value="utc">UTC (Coordinated Universal Time)</option>
          <option value="pst">PST (Pacific Standard Time)</option>
          <option value="est">EST (Eastern Standard Time)</option>
          <option value="cst">CST (Central Standard Time)</option>
          <option value="gmt">GMT (Greenwich Mean Time)</option>
        </select>
      </div>

      {/* Theme */}
      <div>
        <label className="text-sm font-medium text-gray-700 block mb-1">Theme</label>
        <select
          name="theme"
          value={formData.theme}
          onChange={handleChange}
          className="w-full px-4 py-2 bg-gray-50 border border-gray-200 rounded-lg text-gray-900 focus:outline-none focus:ring-2 focus:ring-purple-500"
        >
          <option value="light">Light</option>
          <option value="dark">Dark</option>
          <option value="auto">Auto (System)</option>
        </select>
      </div>

      {/* Language */}
      <div>
        <label className="text-sm font-medium text-gray-700 block mb-1">Language</label>
        <select
          name="language"
          value={formData.language}
          onChange={handleChange}
          className="w-full px-4 py-2 bg-gray-50 border border-gray-200 rounded-lg text-gray-900 focus:outline-none focus:ring-2 focus:ring-purple-500"
        >
          <option value="english">English</option>
          <option value="spanish">Spanish</option>
          <option value="french">French</option>
          <option value="german">German</option>
          <option value="chinese">Chinese (Simplified)</option>
        </select>
      </div>

      {/* Action Buttons */}
      <div className="flex justify-end gap-3 pt-4 border-t border-gray-200 mt-6">
        <Button
          type="button"
          variant="outline"
          className="px-6 py-2 border-gray-300 text-gray-700 hover:bg-gray-50"
          onClick={() => setFormData({ timezone: "utc", theme: "light", language: "english" })}
        >
          Reset
        </Button>
      </div>
    </form>
  );
}
