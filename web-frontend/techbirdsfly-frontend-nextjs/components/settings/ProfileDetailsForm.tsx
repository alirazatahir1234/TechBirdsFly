"use client";

import { useState } from "react";
import toast from "react-hot-toast";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";

interface ProfileDetailsFormProps {
  user?: any;
  profile?: any;
}

export default function ProfileDetailsForm({ user, profile }: ProfileDetailsFormProps) {
  const [formData, setFormData] = useState({
    city: profile?.location?.split(",")[0] || "",
    country: profile?.location?.split(",")[1] || "",
    zipcode: profile?.preferences?.zipcode || "",
  });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    // Form submission handled by parent page component
  }

  return (
    <form onSubmit={onSubmit} className="space-y-4">
      {/* City & Country */}
      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="text-sm font-medium text-gray-700 block mb-1">City</label>
          <Input
            name="city"
            placeholder="Enter your city"
            value={formData.city}
            onChange={handleChange}
            className="bg-gray-50 border-gray-200 text-gray-900 placeholder-gray-400"
          />
        </div>
        <div>
          <label className="text-sm font-medium text-gray-700 block mb-1">Country</label>
          <Input
            name="country"
            placeholder="Enter country name"
            value={formData.country}
            onChange={handleChange}
            className="bg-gray-50 border-gray-200 text-gray-900 placeholder-gray-400"
          />
        </div>
      </div>

      {/* Zip Code */}
      <div>
        <label className="text-sm font-medium text-gray-700 block mb-1">Zip Code</label>
        <Input
          name="zipcode"
          placeholder="Enter zip code"
          value={formData.zipcode}
          onChange={handleChange}
          className="bg-gray-50 border-gray-200 text-gray-900 placeholder-gray-400"
        />
      </div>

      {/* Action Buttons */}
      <div className="flex justify-end gap-3 pt-4 border-t border-gray-200 mt-6">
        <Button
          type="button"
          variant="outline"
          className="px-6 py-2 border-gray-300 text-gray-700 hover:bg-gray-50"
          onClick={() => setFormData({ city: "", country: "", zipcode: "" })}
        >
          Reset
        </Button>
      </div>
    </form>
  );
}
