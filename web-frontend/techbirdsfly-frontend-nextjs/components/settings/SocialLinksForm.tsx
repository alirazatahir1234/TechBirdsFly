"use client";

import { useState } from "react";
import toast from "react-hot-toast";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";

interface SocialLinksFormProps {
  profile?: any;
}

export default function SocialLinksForm({ profile }: SocialLinksFormProps) {
  const [formData, setFormData] = useState({
    facebook: profile?.socialMediaLinks?.facebook || "",
    instagram: profile?.socialMediaLinks?.instagram || "",
    twitter: profile?.socialMediaLinks?.twitter || "",
    linkedin: profile?.socialMediaLinks?.linkedin || "",
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
      {/* Facebook & Instagram */}
      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="text-sm font-medium text-gray-700 block mb-1">Facebook</label>
          <Input
            name="facebook"
            placeholder="https://facebook.com/username"
            value={formData.facebook}
            onChange={handleChange}
            className="bg-gray-50 border-gray-200 text-gray-900 placeholder-gray-400"
          />
        </div>
        <div>
          <label className="text-sm font-medium text-gray-700 block mb-1">Instagram</label>
          <Input
            name="instagram"
            placeholder="https://instagram.com/username"
            value={formData.instagram}
            onChange={handleChange}
            className="bg-gray-50 border-gray-200 text-gray-900 placeholder-gray-400"
          />
        </div>
      </div>

      {/* Twitter & LinkedIn */}
      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="text-sm font-medium text-gray-700 block mb-1">Twitter/X</label>
          <Input
            name="twitter"
            placeholder="https://twitter.com/username"
            value={formData.twitter}
            onChange={handleChange}
            className="bg-gray-50 border-gray-200 text-gray-900 placeholder-gray-400"
          />
        </div>
        <div>
          <label className="text-sm font-medium text-gray-700 block mb-1">LinkedIn</label>
          <Input
            name="linkedin"
            placeholder="https://linkedin.com/in/username"
            value={formData.linkedin}
            onChange={handleChange}
            className="bg-gray-50 border-gray-200 text-gray-900 placeholder-gray-400"
          />
        </div>
      </div>

      {/* Action Buttons */}
      <div className="flex justify-end gap-3 pt-4 border-t border-gray-200 mt-6">
        <Button
          type="button"
          variant="outline"
          className="px-6 py-2 border-gray-300 text-gray-700 hover:bg-gray-50"
          onClick={() => setFormData({ facebook: "", instagram: "", twitter: "", linkedin: "" })}
        >
          Reset
        </Button>
      </div>
    </form>
  );
}
