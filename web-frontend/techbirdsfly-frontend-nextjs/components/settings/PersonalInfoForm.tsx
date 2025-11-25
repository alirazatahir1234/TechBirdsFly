"use client";

import { useState } from "react";
import toast from "react-hot-toast";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";

interface PersonalInfoFormProps {
  user?: any;
}

export default function PersonalInfoForm({ user }: PersonalInfoFormProps) {
  const [formData, setFormData] = useState({
    firstName: user?.firstName || "",
    lastName: user?.lastName || "",
    email: user?.email || "",
    username: user?.username || "",
    phone: user?.phone || "",
    bio: user?.bio || "",
  });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    // Form submission handled by parent page component
  }

  return (
    <form onSubmit={onSubmit} className="space-y-4">
      {/* Full Name & Last Name */}
      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="text-sm font-medium text-gray-700 block mb-1">Full Name</label>
          <Input
            name="firstName"
            placeholder="Enter first name"
            value={formData.firstName}
            onChange={handleChange}
            className="bg-gray-50 border-gray-200 text-gray-900 placeholder-gray-400"
          />
        </div>
        <div>
          <label className="text-sm font-medium text-gray-700 block mb-1">Last Name</label>
          <Input
            name="lastName"
            placeholder="Enter last name"
            value={formData.lastName}
            onChange={handleChange}
            className="bg-gray-50 border-gray-200 text-gray-900 placeholder-gray-400"
          />
        </div>
      </div>

      {/* Email & Username */}
      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="text-sm font-medium text-gray-700 block mb-1">Email Address</label>
          <Input
            name="email"
            type="email"
            placeholder="Enter email address"
            value={formData.email}
            onChange={handleChange}
            className="bg-gray-50 border-gray-200 text-gray-900 placeholder-gray-400"
          />
        </div>
        <div>
          <label className="text-sm font-medium text-gray-700 block mb-1">Username</label>
          <Input
            name="username"
            placeholder="Enter username"
            value={formData.username}
            onChange={handleChange}
            className="bg-gray-50 border-gray-200 text-gray-900 placeholder-gray-400"
          />
        </div>
      </div>

      {/* Phone & Bio */}
      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="text-sm font-medium text-gray-700 block mb-1">Phone No</label>
          <Input
            name="phone"
            placeholder="Enter phone number"
            value={formData.phone}
            onChange={handleChange}
            className="bg-gray-50 border-gray-200 text-gray-900 placeholder-gray-400"
          />
        </div>
        <div>
          <label className="text-sm font-medium text-gray-700 block mb-1">Bio</label>
          <Input
            name="bio"
            placeholder="Write a short bio"
            value={formData.bio}
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
          onClick={() => setFormData({ firstName: "", lastName: "", email: "", username: "", phone: "", bio: "" })}
        >
          Reset
        </Button>
      </div>
    </form>
  );
}
