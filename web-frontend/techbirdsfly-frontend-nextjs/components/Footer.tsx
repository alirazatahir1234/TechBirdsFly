'use client';

import Link from 'next/link';
import { Mail, MapPin, Linkedin, Github, Twitter } from 'lucide-react';

export default function Footer() {
  return (
    <footer className="bg-gray-900 text-gray-300 py-16 px-4">
      <div className="max-w-6xl mx-auto">
        <div className="grid grid-cols-1 md:grid-cols-5 gap-12 mb-12">
          {/* Brand */}
          <div>
            <h3 className="font-bold text-white mb-4 text-lg">TechBirdsFly</h3>
            <p className="text-sm leading-relaxed">
              AI-powered website builder for developers, designers, and entrepreneurs.
            </p>
          </div>

          {/* Product */}
          <div>
            <h4 className="font-semibold text-white mb-4">Product</h4>
            <ul className="space-y-2 text-sm">
              <li><Link href="/marketing" className="hover:text-white transition">Features</Link></li>
              <li><Link href="/marketing" className="hover:text-white transition">Pricing</Link></li>
              <li><Link href="/marketing" className="hover:text-white transition">Templates</Link></li>
              <li><Link href="/blog" className="hover:text-white transition">Blog</Link></li>
            </ul>
          </div>

          {/* Company */}
          <div>
            <h4 className="font-semibold text-white mb-4">Company</h4>
            <ul className="space-y-2 text-sm">
              <li><Link href="/about" className="hover:text-white transition">About</Link></li>
              <li><Link href="/careers" className="hover:text-white transition">Careers</Link></li>
              <li><Link href="/contact" className="hover:text-white transition">Contact</Link></li>
            </ul>
          </div>

          {/* Legal */}
          <div>
            <h4 className="font-semibold text-white mb-4">Legal</h4>
            <ul className="space-y-2 text-sm">
              <li><Link href="/privacy-policy" className="hover:text-white transition">Privacy Policy</Link></li>
              <li><Link href="/terms-of-service" className="hover:text-white transition">Terms of Service</Link></li>
              <li><Link href="/cookie-policy" className="hover:text-white transition">Cookie Policy</Link></li>
            </ul>
          </div>

          {/* Contact */}
          <div>
            <h4 className="font-semibold text-white mb-4">Get in Touch</h4>
            <ul className="space-y-3 text-sm">
              <li className="flex items-center gap-2">
                <Mail size={16} />
                <span>support@techbirdsfly.com</span>
              </li>
              <li className="flex items-center gap-2">
                <MapPin size={16} />
                <span>Dubai, UAE</span>
              </li>
              <li className="flex items-center gap-3 mt-4">
                <a href="#" className="hover:text-white transition">
                  <Linkedin size={18} />
                </a>
                <a href="#" className="hover:text-white transition">
                  <Github size={18} />
                </a>
                <a href="#" className="hover:text-white transition">
                  <Twitter size={18} />
                </a>
              </li>
            </ul>
          </div>
        </div>

        <div className="border-t border-gray-800 pt-8 text-sm">
          <div className="flex flex-col md:flex-row justify-between items-center text-gray-400">
            <p>&copy; 2025 TechBirdsFly. All rights reserved.</p>
            <div className="flex gap-6 mt-4 md:mt-0 text-sm">
              <span>Made with 💜 by TechBirdsFly Team</span>
            </div>
          </div>
        </div>
      </div>
    </footer>
  );
}
