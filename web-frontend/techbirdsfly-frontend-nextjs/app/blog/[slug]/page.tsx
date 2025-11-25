import Link from 'next/link';
import { ArrowLeft, Calendar, Clock, Share2, Twitter, Linkedin, Facebook } from 'lucide-react';

interface Props {
  params: { slug: string };
}

const blogContent: Record<string, { title: string; date: string; content: string }> = {
  'how-techbirdsfly-works': {
    title: 'How TechBirdsFly Generates Websites with AI',
    date: 'November 20, 2025',
    content: `
      <h2>Introduction</h2>
      <p>
        TechBirdsFly uses advanced AI models to understand your requirements and instantly generate 
        complete websites, components, and code. Here's how the magic happens.
      </p>

      <h2>The AI Generation Pipeline</h2>
      <p>
        Our platform combines natural language processing, design algorithms, and code generation 
        to create websites in seconds.
      </p>

      <h3>Step 1: Understanding Your Input</h3>
      <p>
        When you describe your website, our AI analyzes the text to extract key requirements: 
        industry, target audience, design preferences, and functionality needs.
      </p>

      <h3>Step 2: Design Generation</h3>
      <p>
        Based on the analysis, our system generates a complete layout with proper spacing, 
        typography, and color schemes aligned with modern design principles.
      </p>

      <h3>Step 3: Component Creation</h3>
      <p>
        The design is then broken down into reusable React components with proper props, 
        state management, and accessibility features.
      </p>

      <h3>Step 4: Code Export</h3>
      <p>
        Finally, clean Next.js code is generated and ready for deployment. No cleanup needed!
      </p>

      <h2>What Makes TechBirdsFly Different</h2>
      <ul>
        <li><strong>Production-Ready Code:</strong> Generated code is clean, optimized, and follows best practices.</li>
        <li><strong>AI-Powered Customization:</strong> Describe changes in natural language and AI updates your design.</li>
        <li><strong>True Code Export:</strong> Get real React/Next.js code, not just exports.</li>
        <li><strong>Speed:</strong> Complete websites generated in seconds, not weeks.</li>
      </ul>
    `
  },
  'export-nextjs-code': {
    title: 'Exporting Next.js Code from AI',
    date: 'November 15, 2025',
    content: `
      <h2>Introduction</h2>
      <p>
        One of the key features of TechBirdsFly is the ability to export production-ready Next.js code. 
        Let's explore what makes our code exports special.
      </p>

      <h2>Export Features</h2>
      <h3>1. Clean, Readable Code</h3>
      <p>
        Our AI generates code that follows modern JavaScript and React best practices. 
        Every component is well-structured and easy to understand.
      </p>

      <h3>2. TypeScript Support</h3>
      <p>
        All generated code includes full TypeScript types for better developer experience 
        and fewer runtime errors.
      </p>

      <h3>3. Tailwind CSS Integration</h3>
      <p>
        Styling is handled with Tailwind CSS classes, making it easy to customize colors, 
        spacing, and responsive behavior.
      </p>

      <h3>4. Ready for Production</h3>
      <p>
        The generated code includes proper error handling, loading states, and accessibility features.
      </p>

      <h2>Getting Started with Exports</h2>
      <p>
        Simply click "Export" in your TechBirdsFly dashboard and choose your format. 
        The code will be ready to drop into your Next.js project immediately.
      </p>
    `
  },
  'future-of-ai-design': {
    title: 'The Future of AI-Powered Design',
    date: 'November 10, 2025',
    content: `
      <h2>The AI Design Revolution</h2>
      <p>
        AI is transforming how we design and build digital products. What once took weeks 
        can now be done in hours or minutes.
      </p>

      <h2>Emerging Trends</h2>
      <ul>
        <li>AI-powered design systems</li>
        <li>Automated accessibility compliance</li>
        <li>Intelligent performance optimization</li>
        <li>Natural language UI generation</li>
        <li>Real-time A/B testing with AI</li>
      </ul>

      <h2>The Human Element</h2>
      <p>
        While AI handles the heavy lifting, human designers will focus on strategy, 
        user research, and creative direction. The future is human + AI collaboration.
      </p>
    `
  },
  'responsive-design-ai': {
    title: 'Building Responsive Sites with AI',
    date: 'November 5, 2025',
    content: `
      <h2>Responsive Design at Scale</h2>
      <p>
        TechBirdsFly ensures every generated website is mobile-first and responsive across 
        all device sizes. Here's how we do it.
      </p>

      <h2>Our Approach</h2>
      <h3>Mobile-First Strategy</h3>
      <p>
        We start by designing for mobile, then scale up to tablets and desktops. 
        This ensures optimal user experience on all devices.
      </p>

      <h3>Breakpoint Optimization</h3>
      <p>
        Our AI uses intelligent breakpoints to adjust layouts, typography, and spacing 
        for different screen sizes.
      </p>

      <h3>Touch-Friendly Interactions</h3>
      <p>
        All generated components include touch-friendly button sizes and spacing, 
        ensuring excellent mobile UX.
      </p>
    `
  }
};

export default function BlogArticlePage({ params }: Props) {
  const post = blogContent[params.slug];

  if (!post) {
    return (
      <main className="min-h-screen bg-white">
        <div className="max-w-4xl mx-auto py-20 px-4">
          <Link href="/blog" className="flex items-center gap-2 text-purple-600 hover:text-purple-700 mb-10 font-semibold">
            <ArrowLeft size={20} />
            Back to Blog
          </Link>
          <h1 className="text-4xl font-bold">Article Not Found</h1>
          <p className="text-gray-600 mt-4">The article you're looking for doesn't exist.</p>
        </div>
      </main>
    );
  }

  return (
    <main className="min-h-screen bg-white">
      {/* Header */}
      <section className="bg-linear-to-br from-purple-900 to-blue-900 text-white py-20 px-4">
        <div className="max-w-4xl mx-auto">
          <Link href="/blog" className="flex items-center gap-2 text-purple-100 hover:text-white mb-8 font-semibold">
            <ArrowLeft size={20} />
            Back to Blog
          </Link>
          <h1 className="text-5xl font-bold mb-6">{post.title}</h1>
          <div className="flex flex-wrap items-center gap-8 text-purple-100">
            <div className="flex items-center gap-2">
              <Calendar size={20} />
              <span className="text-lg">{post.date}</span>
            </div>
            <div className="flex items-center gap-2">
              <Clock size={20} />
              <span className="text-lg">5 min read</span>
            </div>
          </div>
        </div>
      </section>

      {/* Content */}
      <section className="py-20 px-4">
        <article className="max-w-4xl mx-auto">
          <div 
            className="prose prose-lg max-w-none space-y-8 text-gray-700
              prose-h2:text-3xl prose-h2:font-bold prose-h2:text-gray-900 prose-h2:mt-8 prose-h2:mb-4
              prose-h3:text-xl prose-h3:font-bold prose-h3:text-gray-900 prose-h3:mt-6 prose-h3:mb-3
              prose-p:text-lg prose-p:leading-relaxed prose-p:text-gray-600
              prose-ul:space-y-2 prose-li:text-lg prose-li:text-gray-600
              prose-strong:font-semibold prose-strong:text-gray-900"
            dangerouslySetInnerHTML={{ __html: post.content }}
          />
        </article>
      </section>

      {/* Share Section */}
      <section className="bg-gray-50 border-t border-gray-200 py-16 px-4">
        <div className="max-w-4xl mx-auto">
          <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between">
            <div>
              <h3 className="text-2xl font-bold text-gray-900 mb-2">Share This Article</h3>
              <p className="text-gray-600">Help spread the word about TechBirdsFly</p>
            </div>
            <div className="flex gap-4 mt-6 sm:mt-0">
              <a href="#" className="flex items-center gap-2 px-6 py-3 bg-white border border-gray-200 rounded-lg hover:bg-gray-50 transition-all font-semibold text-gray-900">
                <Twitter size={20} className="text-blue-400" />
                Twitter
              </a>
              <a href="#" className="flex items-center gap-2 px-6 py-3 bg-white border border-gray-200 rounded-lg hover:bg-gray-50 transition-all font-semibold text-gray-900">
                <Linkedin size={20} className="text-blue-600" />
                LinkedIn
              </a>
              <a href="#" className="flex items-center gap-2 px-6 py-3 bg-white border border-gray-200 rounded-lg hover:bg-gray-50 transition-all font-semibold text-gray-900">
                <Facebook size={20} className="text-blue-700" />
                Facebook
              </a>
            </div>
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="bg-linear-to-r from-purple-600 to-blue-600 text-white py-16 px-4">
        <div className="max-w-4xl mx-auto text-center">
          <h2 className="text-3xl font-bold mb-4">Ready to build with TechBirdsFly?</h2>
          <p className="text-lg text-purple-100 mb-8">Create beautiful websites in minutes with AI.</p>
          <Link href="/contact" className="inline-block bg-white text-purple-600 font-semibold py-3 px-8 rounded-lg hover:shadow-lg transition-all">
            Get Started Free
          </Link>
        </div>
      </section>
    </main>
  );
}
