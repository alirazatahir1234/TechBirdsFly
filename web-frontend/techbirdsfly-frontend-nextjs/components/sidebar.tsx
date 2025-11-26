"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { Sparkles, Pencil, Upload, FolderOpen, Settings, Trash2 } from "lucide-react";

const menu = [
  { label: "Create", href: "/dashboard/create", icon: Sparkles },
  { label: "Projects", href: "/dashboard/projects", icon: FolderOpen },
  { label: "Editor", href: "/dashboard/editor", icon: Pencil },
  { label: "Export", href: "/dashboard/export", icon: Upload },
  { label: "Trash", href: "/dashboard/trash", icon: Trash2 },
];

export default function Sidebar() {
  const pathname = usePathname();

  return (
    <aside className="fixed left-0 top-0 h-screen w-64 border-r border-gray-200 bg-white dark:bg-neutral-950 dark:border-neutral-800 flex flex-col shadow-sm">
      {/* Logo */}
      <div className="p-6 border-b border-gray-200 dark:border-neutral-800">
        <div className="flex items-center gap-2">
          <div className="w-8 h-8 bg-purple-600 rounded-lg flex items-center justify-center">
            <Sparkles size={20} className="text-white" />
          </div>
          <h1 className="text-lg font-bold text-gray-900 dark:text-white">
            TechBirdsFly
          </h1>
        </div>
        <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
          AI Website Builder
        </p>
      </div>

      {/* Navigation */}
      <nav className="flex-1 p-4 space-y-2 overflow-y-auto">
        {menu.map(({ label, href, icon: Icon }) => {
          const isActive = pathname === href || pathname.startsWith(href + "/");

          return (
            <Link
              key={href}
              href={href}
              className={`flex items-center gap-3 px-4 py-3 rounded-lg text-sm font-medium transition-all ${
                isActive
                  ? "bg-purple-600 text-white shadow-md"
                  : "text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-neutral-800"
              }`}
            >
              <Icon size={18} />
              <span>{label}</span>
            </Link>
          );
        })}
      </nav>

      {/* Footer */}
      <div className="p-4 border-t border-gray-200 dark:border-neutral-800 space-y-2">
        <button className="w-full flex items-center gap-3 px-4 py-3 rounded-lg text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-neutral-800 transition-all">
          <Settings size={18} />
          <span>Settings</span>
        </button>
      </div>
    </aside>
  );
}
