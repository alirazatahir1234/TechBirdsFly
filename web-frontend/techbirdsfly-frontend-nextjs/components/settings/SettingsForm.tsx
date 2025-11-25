"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card } from "@/components/ui/card";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

const settingsSchema = z.object({
  fullName: z.string().min(1, "Full name is required"),
  lastName: z.string().min(1, "Last name is required"),
  email: z.string().email("Invalid email address"),
  username: z.string().min(3, "Username must be at least 3 characters"),
  phone: z.string().optional(),
  city: z.string().optional(),
  country: z.string().optional(),
  zip: z.string().optional(),
  textFormat: z.string().optional(),
  bio: z.string().optional(),
  timezone: z.string().optional(),
});

type SettingsFormValues = z.infer<typeof settingsSchema>;

export default function SettingsForm() {
  const [isSaving, setIsSaving] = useState(false);

  const form = useForm<SettingsFormValues>({
    resolver: zodResolver(settingsSchema),
    defaultValues: {
      fullName: "",
      lastName: "",
      email: "",
      username: "",
      phone: "",
      city: "",
      country: "",
      zip: "",
      textFormat: "normal",
      bio: "",
      timezone: "pacific",
    },
  });

  async function onSubmit(values: SettingsFormValues) {
    setIsSaving(true);
    try {
      console.log("Settings saved:", values);
      // API call would go here
      await new Promise((resolve) => setTimeout(resolve, 1000));
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <Card className="bg-white p-6 shadow-sm border-0">
      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
          {/* Personal Information Section */}
          <div>
            <h3 className="text-lg font-semibold text-gray-900 mb-6">
              Personal Information
            </h3>

            {/* Full Name & Last Name */}
            <div className="grid grid-cols-2 gap-4 mb-4">
              <FormField
                control={form.control}
                name="fullName"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel className="text-sm font-medium text-gray-700">
                      Full Name
                    </FormLabel>
                    <FormControl>
                      <Input
                        placeholder="Enter first name"
                        className="bg-gray-50 border-gray-200 text-gray-900"
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="lastName"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel className="text-sm font-medium text-gray-700">
                      Last Name
                    </FormLabel>
                    <FormControl>
                      <Input
                        placeholder="Enter last name"
                        className="bg-gray-50 border-gray-200 text-gray-900"
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            {/* Email & Username */}
            <div className="grid grid-cols-2 gap-4 mb-4">
              <FormField
                control={form.control}
                name="email"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel className="text-sm font-medium text-gray-700">
                      Email Address
                    </FormLabel>
                    <FormControl>
                      <Input
                        type="email"
                        placeholder="Enter email address"
                        className="bg-gray-50 border-gray-200 text-gray-900"
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="username"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel className="text-sm font-medium text-gray-700">
                      Username
                    </FormLabel>
                    <FormControl>
                      <Input
                        placeholder="Enter user name"
                        className="bg-gray-50 border-gray-200 text-gray-900"
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            {/* Phone & City */}
            <div className="grid grid-cols-2 gap-4 mb-4">
              <FormField
                control={form.control}
                name="phone"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel className="text-sm font-medium text-gray-700">
                      Phone No
                    </FormLabel>
                    <FormControl>
                      <Input
                        placeholder="Enter phone no"
                        className="bg-gray-50 border-gray-200 text-gray-900"
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="city"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel className="text-sm font-medium text-gray-700">
                      City
                    </FormLabel>
                    <FormControl>
                      <Input
                        placeholder="Enter your city"
                        className="bg-gray-50 border-gray-200 text-gray-900"
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            {/* Country & Zip Code */}
            <div className="grid grid-cols-2 gap-4 mb-4">
              <FormField
                control={form.control}
                name="country"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel className="text-sm font-medium text-gray-700">
                      Country Name
                    </FormLabel>
                    <FormControl>
                      <Input
                        placeholder="Enter country name"
                        className="bg-gray-50 border-gray-200 text-gray-900"
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="zip"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel className="text-sm font-medium text-gray-700">
                      Zip Code
                    </FormLabel>
                    <FormControl>
                      <Input
                        placeholder="Enter zip code"
                        className="bg-gray-50 border-gray-200 text-gray-900"
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            {/* Text Format Dropdown */}
            <div className="mb-4">
              <FormField
                control={form.control}
                name="textFormat"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel className="text-sm font-medium text-gray-700">
                      Text Format
                    </FormLabel>
                    <Select value={field.value} onValueChange={field.onChange}>
                      <FormControl>
                        <SelectTrigger className="bg-gray-50 border-gray-200 text-gray-900">
                          <SelectValue placeholder="Select text format" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        <SelectItem value="normal">Normal text</SelectItem>
                        <SelectItem value="rich">Rich text</SelectItem>
                        <SelectItem value="markdown">Markdown</SelectItem>
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            {/* Bio Text Area */}
            <div className="mb-6">
              <FormField
                control={form.control}
                name="bio"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel className="text-sm font-medium text-gray-700">
                      Bio (Write a short introduction)
                    </FormLabel>
                    <FormControl>
                      <textarea
                        placeholder="Lorem ipsum, in graphical and textual context, refers to filler text that is placed in a document or visual presentation. Lorem ipsum is derived from the Latin 'dolorem ipsum' 'roughly translated as 'pain itself'."
                        className="w-full px-4 py-3 bg-gray-50 border border-gray-200 rounded-lg text-gray-900 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent resize-none"
                        rows={4}
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            {/* Timezone Dropdown */}
            <div className="mb-6">
              <FormField
                control={form.control}
                name="timezone"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel className="text-sm font-medium text-gray-700">
                      Timezone
                    </FormLabel>
                    <Select value={field.value} onValueChange={field.onChange}>
                      <FormControl>
                        <SelectTrigger className="bg-gray-50 border-gray-200 text-gray-900">
                          <SelectValue placeholder="Select timezone" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        <SelectItem value="pacific">Pacific Standard Time</SelectItem>
                        <SelectItem value="mountain">Mountain Standard Time</SelectItem>
                        <SelectItem value="central">Central Standard Time</SelectItem>
                        <SelectItem value="eastern">Eastern Standard Time</SelectItem>
                        <SelectItem value="gmt">GMT</SelectItem>
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>
          </div>

          {/* Action Buttons */}
          <div className="flex justify-end gap-3 pt-4 border-t border-gray-200">
            <Button
              type="button"
              variant="outline"
              className="px-6 py-2 border-gray-300 text-gray-700 hover:bg-gray-50"
              onClick={() => form.reset()}
            >
              Cancel
            </Button>
            <Button
              type="submit"
              disabled={isSaving}
              className="px-6 py-2 bg-purple-600 hover:bg-purple-700 text-white"
            >
              {isSaving ? "Saving..." : "Save"}
            </Button>
          </div>
        </form>
      </Form>
    </Card>
  );
}
