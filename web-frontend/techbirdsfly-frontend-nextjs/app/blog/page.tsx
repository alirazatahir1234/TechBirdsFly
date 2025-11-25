import Link from 'next/link';
import { Calendar, ArrowRight, User, MessageCircle } from 'lucide-react';

export default function BlogPage() {
  const posts = [
    {
      title: "How TechBirdsFly Generates Websites with AI",
      slug: "how-techbirdsfly-works",
      description: "A deep dive into the AI that powers instant website creation. Learn about the cutting-edge technology behind every generated site.",
      date: "November 20, 2025",
      readTime: "5 min read",
      category: "Technology",
      author: "Ali Raza"
    },
    {
      title: "Exporting Next.js Code from AI",
      slug: "export-nextjs-code",
      description: "How TechBirdsFly generates clean, production-ready React/Next.js code that you can use immediately in your projects.",
      date: "November 15, 2025",
      readTime: "7 min read",
      category: "Development",
      author: "Ali Raza"
    },
    {
      title: "The Future of AI-Powered Design",
      slug: "future-of-ai-design",
      description: "Exploring the trends and possibilities in AI design automation. What does the future hold for web designers and developers?",
      date: "November 10, 2025",
      readTime: "6 min read",
      category: "Design",
      author: "Design Team"
    },
    {
      title: "Building Responsive Sites with AI",
      slug: "responsive-design-ai",
      description: "Learn how TechBirdsFly ensures mobile-first, responsive designs that look perfect on any device.",
      date: "November 5, 2025",
      readTime: "8 min read",
      category: "Best Practices",
      author: "Engineering Team"
    },
  ];

  return (
    <main className="min-h-screen bg-white">
      {/* Header */}
      <section className="bg-linear-to-br from-purple-900 to-blue-900 text-white py-20 px-4">
        <div className="max-w-5xl mx-auto text-center">
          <h1 className="text-5xl md:text-6xl font-bold mb-6">TechBirdsFly Blog</h1>
          <p className="text-xl text-purple-100 max-w-3xl mx-auto">
            Insights, tutorials, and updates on AI-powered website creation. Stay updated with the latest trends in web development.
          </p>
        </div>
      </section>

      {/* Blog Posts */}
      <section className="py-20 px-4">
        <div className="max-w-5xl mx-auto">
          <div className="grid gap-8">
            {posts.map(post => (
              <Link
                key={post.slug}
                href={`/blog/${post.slug}`}
                className="group block"
              >
                <div className="bg-white border border-gray-200 rounded-xl overflow-hidden hover:shadow-xl hover:border-purple-300 transition-all p-8">
                  <div className="flex items-start justify-between gap-6">
                    <div className="flex-1">
                      <div className="mb-4">
                        <span className="inline-block bg-purple-100 text-purple-700 text-xs font-semibold px-3 py-1 rounded-full">
                          {post.category}
                        </span>
                      </div>
                      
                      <h2 className="text-2xl md:text-3xl font-bold text-gray-900 group-hover:text-purple-600 transition mb-3 line-clamp-2">
                        {post.title}
                      </h2>
                      
                      <p className="text-gray-600 mb-6 leading-relaxed line-clamp-2">
                        {post.description}
                      </p>
                      
                      <div className="flex flex-wrap items-center gap-6 text-sm text-gray-500 mb-6">
                        <div className="flex items-center gap-2">
                          <Calendar size={16} className="text-gray-400" />
                          <span>{post.date}</span>
                        </div>
                        <div className="flex items-center gap-2">
                          <User size={16} className="text-gray-400" />
                          <span>{post.author}</span>
                        </div>
                        <div className="flex items-center gap-2">
                          <MessageCircle size={16} className="text-gray-400" />
                          <span>{post.readTime}</span>
                        </div>
                      </div>

                      <div className="flex items-center gap-2 text-purple-600 font-semibold group-hover:translate-x-1 transition">
                        Read More
                        <ArrowRight size={20} />
                      </div>
                    </div>
                  </div>
                </div>
              </Link>
            ))}
          </div>
        </div>
      </section>

      {/* Newsletter CTA */}
      <section className="bg-linear-to-r from-purple-600 to-blue-600 text-white py-16 px-4">
        <div className="max-w-3xl mx-auto text-center">
          <h2 className="text-3xl font-bold mb-4">Subscribe to Our Newsletter</h2>
          <p className="text-lg text-purple-100 mb-8">Get the latest articles delivered to your inbox every week.</p>
          <div className="flex gap-3">
            <input
              type="email"
              placeholder="Enter your email"
              className="flex-1 px-4 py-3 rounded-lg text-gray-900 focus:outline-none"
            />
            <button className="bg-white text-purple-600 font-semibold px-8 py-3 rounded-lg hover:shadow-lg transition-all">
              Subscribe
            </button>
          </div>
        </div>
      </section>
    </main>
  );
}
