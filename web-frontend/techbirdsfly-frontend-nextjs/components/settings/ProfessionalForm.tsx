"use client";

import { useState } from "react";
import toast from "react-hot-toast";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";

interface ProfessionalFormProps {
  profile?: any;
}

export default function ProfessionalForm({ profile }: ProfessionalFormProps) {
  const [formData, setFormData] = useState({
    companyName: profile?.companyName || "",
    department: profile?.department || "",
    jobTitle: profile?.jobTitle || "",
    website: profile?.website || "",
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
      {/* Company & Department */}
      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="text-sm font-medium text-gray-700 block mb-1">Company Name</label>
          <Input
            name="companyName"
            placeholder="Enter company name"
            value={formData.companyName}
            onChange={handleChange}
            className="bg-gray-50 border-gray-200 text-gray-900 placeholder-gray-400"
          />
        </div>
        <div>
          <label className="text-sm font-medium text-gray-700 block mb-1">Department</label>
          <Input
            name="department"
            placeholder="Enter department"
            value={formData.department}
            onChange={handleChange}
            className="bg-gray-50 border-gray-200 text-gray-900 placeholder-gray-400"
          />
        </div>
      </div>

      {/* Job Title & Website */}
      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="text-sm font-medium text-gray-700 block mb-1">Job Title</label>
          <Input
            name="jobTitle"
            placeholder="Enter job title"
            value={formData.jobTitle}
            onChange={handleChange}
            className="bg-gray-50 border-gray-200 text-gray-900 placeholder-gray-400"
          />
        </div>
        <div>
          <label className="text-sm font-medium text-gray-700 block mb-1">Website</label>
          <Input
            name="website"
            type="url"
            placeholder="Enter website URL"
            value={formData.website}
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
          onClick={() => setFormData({ companyName: "", department: "", jobTitle: "", website: "" })}
        >
          Reset
        </Button>
      </div>
    </form>
  );
}
