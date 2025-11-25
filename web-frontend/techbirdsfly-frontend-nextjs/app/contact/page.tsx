'use client';

import { useState } from 'react';
import { Mail, MapPin, Phone, Send, CheckCircle } from 'lucide-react';

export default function ContactPage() {
  const [formData, setFormData] = useState({
    name: '',
    email: '',
    subject: '',
    message: ''
  });

  const [submitted, setSubmitted] = useState(false);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    // TODO: Connect to backend API
    setSubmitted(true);
    setTimeout(() => {
      setFormData({ name: '', email: '', subject: '', message: '' });
      setSubmitted(false);
    }, 3000);
  };

  return (
    <main className="min-h-screen bg-linear-to-b from-purple-50 via-white to-blue-50">
      {/* Header */}
      <section className="bg-linear-to-br from-purple-900 to-blue-900 text-white py-16 px-4">
        <div className="max-w-5xl mx-auto text-center">
          <h1 className="text-5xl font-bold mb-4">Get in Touch</h1>
          <p className="text-xl text-purple-100">We'd love to hear from you. Send us a message!</p>
        </div>
      </section>

      {/* Content */}
      <section className="py-20 px-4">
        <div className="max-w-5xl mx-auto">
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-12">
            {/* Contact Info */}
            <div className="space-y-8">
              <div>
                <h2 className="text-2xl font-bold text-gray-900 mb-8">Contact Information</h2>
              </div>

              {[
                { icon: Mail, title: "Email", detail: "hello@techbirdsfly.com" },
                { icon: MapPin, title: "Location", detail: "San Francisco, CA" },
                { icon: Phone, title: "Phone", detail: "+1 (555) 123-4567" },
              ].map((item, i) => {
                const Icon = item.icon;
                return (
                  <div key={i} className="flex gap-4">
                    <div className="shrink-0">
                      <Icon size={28} className="text-purple-600 mt-1" />
                    </div>
                    <div>
                      <h3 className="font-semibold text-gray-900">{item.title}</h3>
                      <p className="text-gray-600">{item.detail}</p>
                    </div>
                  </div>
                );
              })}

              <div className="pt-8 border-t border-gray-200">
                <h3 className="font-semibold text-gray-900 mb-4">Business Hours</h3>
                <p className="text-gray-600">Monday - Friday: 9AM - 6PM PST</p>
                <p className="text-gray-600">Saturday: 10AM - 2PM PST</p>
                <p className="text-gray-600">Sunday: Closed</p>
              </div>
            </div>

            {/* Contact Form */}
            <div className="lg:col-span-2">
              <div className="bg-white rounded-2xl shadow-xl p-10 border border-gray-100">
                {submitted && (
                  <div className="mb-6 bg-green-50 border border-green-200 rounded-xl p-4 flex items-center gap-3">
                    <CheckCircle size={24} className="text-green-600" />
                    <div>
                      <h3 className="font-semibold text-green-900">Message Sent!</h3>
                      <p className="text-sm text-green-800">We'll get back to you as soon as possible.</p>
                    </div>
                  </div>
                )}

                <form onSubmit={handleSubmit} className="space-y-6">
                  {/* Name */}
                  <div>
                    <label htmlFor="name" className="block text-sm font-semibold text-gray-900 mb-2">
                      Full Name
                    </label>
                    <input
                      type="text"
                      id="name"
                      name="name"
                      value={formData.name}
                      onChange={handleChange}
                      required
                      className="w-full px-4 py-3 rounded-lg border border-gray-300 focus:border-purple-600 focus:outline-none focus:ring-2 focus:ring-purple-100 transition-all"
                      placeholder="John Doe"
                    />
                  </div>

                  {/* Email */}
                  <div>
                    <label htmlFor="email" className="block text-sm font-semibold text-gray-900 mb-2">
                      Email Address
                    </label>
                    <input
                      type="email"
                      id="email"
                      name="email"
                      value={formData.email}
                      onChange={handleChange}
                      required
                      className="w-full px-4 py-3 rounded-lg border border-gray-300 focus:border-purple-600 focus:outline-none focus:ring-2 focus:ring-purple-100 transition-all"
                      placeholder="john@example.com"
                    />
                  </div>

                  {/* Subject */}
                  <div>
                    <label htmlFor="subject" className="block text-sm font-semibold text-gray-900 mb-2">
                      Subject
                    </label>
                    <input
                      type="text"
                      id="subject"
                      name="subject"
                      value={formData.subject}
                      onChange={handleChange}
                      required
                      className="w-full px-4 py-3 rounded-lg border border-gray-300 focus:border-purple-600 focus:outline-none focus:ring-2 focus:ring-purple-100 transition-all"
                      placeholder="How can we help?"
                    />
                  </div>

                  {/* Message */}
                  <div>
                    <label htmlFor="message" className="block text-sm font-semibold text-gray-900 mb-2">
                      Message
                    </label>
                    <textarea
                      id="message"
                      name="message"
                      value={formData.message}
                      onChange={handleChange}
                      required
                      rows={6}
                      className="w-full px-4 py-3 rounded-lg border border-gray-300 focus:border-purple-600 focus:outline-none focus:ring-2 focus:ring-purple-100 transition-all resize-none"
                      placeholder="Tell us more about your inquiry..."
                    />
                  </div>

                  {/* Submit Button */}
                  <button
                    type="submit"
                    className="w-full bg-linear-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700 text-white font-semibold py-3 px-6 rounded-lg transition-all flex items-center justify-center gap-2 group"
                  >
                    <Send size={20} className="group-hover:translate-x-1 transition-transform" />
                    Send Message
                  </button>
                </form>

                <p className="text-xs text-gray-500 text-center mt-6">
                  We typically respond within 24 hours during business days.
                </p>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="bg-linear-to-r from-purple-600 to-blue-600 text-white py-16 px-4 mt-20">
        <div className="max-w-4xl mx-auto text-center">
          <h2 className="text-3xl font-bold mb-4">Can't wait? Start building now!</h2>
          <p className="text-lg text-purple-100 mb-8">Try TechBirdsFly free for 14 days. No credit card required.</p>
          <button className="bg-white text-purple-600 font-semibold py-3 px-8 rounded-lg hover:shadow-lg transition-all">
            Start Free Trial
          </button>
        </div>
      </section>
    </main>
  );
}
