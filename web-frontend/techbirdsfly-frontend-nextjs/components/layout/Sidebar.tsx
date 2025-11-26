"use client";

import { useState } from "react";
import Link from "next/link";
import { 
  Sparkles,
  Wand2,
  Folder,
  Image,
  Download,
  Settings,
  ChevronDown,
  LogOut
} from "lucide-react";
import { AppLogoIcon } from "@/components/AppLogo";

interface SidebarItem {
  icon: any;
  label: string;
  href?: string;
  active?: boolean;
  hasSubmenu?: boolean;
  description?: string;
  isPrimary?: boolean;
}

/**
 * Base44-Style Sidebar for AI Website Builder
 * Minimal, AI-focused, 7 core items max
 * 
 * Flow:
 * 1. Generate (AI-powered website generation)
 * 2. Create Website (Custom prompt entry)
 * 3. Editor (Edit generated website)
 * 4. Projects (Manage all generated sites)
 * 5. Media (AI Images + Uploads)
 * 6. Export (HTML/React/Next.js)
 * 7. Settings (Profile, Billing, API)
 */
const sidebarItems: SidebarItem[] = [
  { 
    icon: Sparkles, 
    label: "Generator", 
    href: "/dashboard/generator", 
    active: false,
    isPrimary: true,
    description: "Create new website"
  },
  { 
    icon: Wand2, 
    label: "Editor", 
    href: "/dashboard/editor", 
    active: false,
    description: "Edit & customize"
  },
  { 
    icon: Folder, 
    label: "Projects", 
    href: "/dashboard/projects", 
    active: false,
    description: "Your websites"
  },
  { 
    icon: Image, 
    label: "Media", 
    href: "/dashboard/media", 
    active: false,
    description: "AI images & uploads"
  },
  { 
    icon: Download, 
    label: "Export", 
    href: "/dashboard/export", 
    active: false,
    description: "HTML/React/Next.js"
  },
  { 
    icon: Settings, 
    label: "Settings", 
    href: "/dashboard/settings", 
    active: false,
    description: "Profile & Billing"
  },
];

export default function Sidebar() {
  return (
    <aside className="w-64 h-screen bg-white border-r border-gray-200 flex flex-col">
      {/* Logo Section */}
      <div className="p-6 border-b border-gray-200">
        <div className="flex items-center gap-3">
          <AppLogoIcon size="md" />
          <div>
            <h2 className="font-bold text-gray-900">TechBirdsFly</h2>
            <p className="text-xs text-gray-500">AI Website Builder</p>
          </div>
        </div>
      </div>

      {/* Navigation - Minimal AI-First Design */}
      <nav className="flex-1 px-4 py-8">
        <ul className="space-y-1">
          {sidebarItems.map((item, index) => {
            const Icon = item.icon;
            
            return (
              <li key={index}>
                {item.href ? (
                  <Link href={item.href} className="block">
                    <div className={`
                      flex items-start gap-3 px-4 py-3 rounded-lg transition-all duration-200 group
                      ${item.isPrimary 
                        ? 'bg-linear-to-r from-purple-600 to-indigo-600 text-white shadow-md hover:shadow-lg hover:from-purple-700 hover:to-indigo-700' 
                        : 'text-gray-700 hover:bg-gray-50'
                      }
                    `}>
                      <Icon className={`w-5 h-5 shrink-0 mt-0.5 ${
                        item.isPrimary ? 'text-white' : 'text-gray-600 group-hover:text-purple-600'
                      }`} />
                      <div className="flex-1 min-w-0">
                        <p className={`text-sm font-semibold ${
                          item.isPrimary ? 'text-white' : 'text-gray-900'
                        }`}>
                          {item.label}
                        </p>
                        {item.description && (
                          <p className={`text-xs mt-0.5 ${
                            item.isPrimary ? 'text-purple-100' : 'text-gray-500'
                          }`}>
                            {item.description}
                          </p>
                        )}
                      </div>
                    </div>
                  </Link>
                ) : (
                  <div className={`
                    flex items-start gap-3 px-4 py-3 rounded-lg
                    text-gray-700 cursor-pointer hover:bg-gray-50
                  `}>
                    <Icon className="w-5 h-5 shrink-0 mt-0.5 text-gray-600" />
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-semibold text-gray-900">{item.label}</p>
                      {item.description && (
                        <p className="text-xs mt-0.5 text-gray-500">{item.description}</p>
                      )}
                    </div>
                  </div>
                )}
              </li>
            );
          })}
        </ul>
      </nav>

      {/* Footer - Divider + Logout */}
      <div className="border-t border-gray-200 px-4 py-4">
        <button className="w-full flex items-center gap-2 px-3 py-2 rounded-lg text-gray-600 hover:bg-red-50 hover:text-red-600 transition-colors text-sm font-medium">
          <LogOut className="w-4 h-4" />
          Logout
        </button>
      </div>
    </aside>
  );
}