import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import "./globals.css";
import { QueryProvider } from "@/lib/providers/QueryProvider";
import { SessionProvider } from "@/lib/providers/SessionProvider";
import { ToastProvider } from "@/lib/providers/ToastProvider";
import Footer from "@/components/Footer";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "TechBirdsFly - AI Website Generator",
  description: "Create stunning websites with AI-powered design generation",
  icons: {
    icon: [
      { url: "/images/techbirdsfly/techbirdsfly.svg", type: "image/svg+xml" },
      { url: "/images/techbirdsfly/techbirdsfly.png", sizes: "any" },
    ],
    shortcut: "/images/techbirdsfly/techbirdsfly.svg",
    apple: "/images/techbirdsfly/techbirdsfly.png",
  },
  openGraph: {
    title: "TechBirdsFly - AI Website Generator",
    description: "Create stunning websites with AI-powered design generation",
    images: [
      {
        url: "/images/techbirdsfly/techbirdsfly.svg",
        width: 512,
        height: 512,
        alt: "TechBirdsFly Logo",
      },
    ],
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <head>
        <link rel="icon" href="/favicon.svg" type="image/svg+xml" />
      </head>
      <body
        className={`${geistSans.variable} ${geistMono.variable} antialiased flex flex-col min-h-screen`}
      >
        <ToastProvider />
        <SessionProvider>
          <QueryProvider>
            <main className="flex-1">
              {children}
            </main>
          </QueryProvider>
          <Footer />
        </SessionProvider>
      </body>
    </html>
  );
}
