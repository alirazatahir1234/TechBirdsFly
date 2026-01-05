import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  /* config options here */
  
  // Required for Docker/Cloud Run deployment
  output: "standalone",
  
  // Optimize images for production
  images: {
    unoptimized: process.env.NODE_ENV === 'development',
    remotePatterns: [
      {
        protocol: 'https',
        hostname: '**',
      },
    ],
  },
  
  // Environment variables exposed to the browser
  env: {
    NEXT_PUBLIC_API_URL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8000',
  },
};

export default nextConfig;
