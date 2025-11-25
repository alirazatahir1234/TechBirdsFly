import { Briefcase, MapPin, Calendar, Heart, TrendingUp, Zap, Award, Globe } from 'lucide-react';
import Link from 'next/link';

export default function CareersPage() {
  const jobs = [
    {
      title: "Senior .NET Developer",
      location: "Remote",
      type: "Full-time",
      description: "Build scalable backend services using .NET and Azure.",
      department: "Engineering"
    },
    {
      title: "Frontend Engineer (React / Next.js)",
      location: "Remote",
      type: "Full-time",
      description: "Create beautiful, responsive UI with React and Next.js.",
      department: "Engineering"
    },
    {
      title: "AI Prompt Engineer",
      location: "Remote",
      type: "Full-time",
      description: "Design and optimize AI prompts for website generation.",
      department: "AI/ML"
    },
    {
      title: "DevOps Engineer (Azure / Kubernetes)",
      location: "Remote",
      type: "Full-time",
      description: "Manage cloud infrastructure, deployment, and scaling.",
      department: "Infrastructure"
    },
    {
      title: "UI/UX Designer",
      location: "Remote",
      type: "Full-time",
      description: "Design intuitive interfaces for our AI builder platform.",
      department: "Design"
    },
    {
      title: "Content Strategist",
      location: "Remote",
      type: "Full-time",
      description: "Create marketing copy and technical documentation.",
      department: "Marketing"
    },
  ];

  const benefits = [
    { icon: Heart, title: "Health Insurance", desc: "Comprehensive medical, dental, and vision coverage" },
    { icon: TrendingUp, title: "Growth & Learning", desc: "Annual learning budget for courses and conferences" },
    { icon: Zap, title: "Flexible Hours", desc: "Work when you're most productive" },
    { icon: Award, title: "Stock Options", desc: "Share in our success with equity compensation" },
    { icon: Globe, title: "Remote First", desc: "Work from anywhere in the world" },
    { icon: Calendar, title: "Unlimited PTO", desc: "Flexible time off policy based on trust" },
  ];

  return (
    <main className="min-h-screen bg-white">
      {/* Hero Section */}
      <section className="bg-linear-to-br from-purple-900 to-blue-900 text-white py-20 px-4">
        <div className="max-w-5xl mx-auto text-center">
          <h1 className="text-5xl md:text-6xl font-bold mb-6">Join Our Team</h1>
          <p className="text-xl text-purple-100 max-w-3xl mx-auto">
            Help us revolutionize website creation. We're looking for talented engineers, designers, 
            and thinkers to join our remote-first team.
          </p>
        </div>
      </section>

      {/* Open Positions */}
      <section className="py-20 px-4">
        <div className="max-w-5xl mx-auto">
          <h2 className="text-4xl font-bold text-gray-900 mb-4 text-center">Open Positions</h2>
          <p className="text-xl text-gray-600 text-center max-w-2xl mx-auto mb-16">
            Explore our current opportunities
          </p>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-12">
            {jobs.map((job, i) => (
              <div 
                key={i} 
                className="group bg-white border border-gray-200 rounded-xl p-8 hover:shadow-xl hover:border-purple-200 transition-all"
              >
                <div className="flex items-start justify-between mb-4">
                  <div className="flex items-start gap-4">
                    <div className="bg-purple-100 p-3 rounded-lg group-hover:bg-purple-600 transition-all">
                      <Briefcase size={24} className="text-purple-600 group-hover:text-white transition-all" />
                    </div>
                    <div>
                      <h3 className="text-xl font-bold text-gray-900">{job.title}</h3>
                      <span className="inline-block bg-blue-100 text-blue-700 text-xs font-semibold px-3 py-1 rounded-full mt-2">
                        {job.department}
                      </span>
                    </div>
                  </div>
                </div>

                <div className="flex flex-wrap gap-3 mb-4 text-sm text-gray-600">
                  <div className="flex items-center gap-1">
                    <MapPin size={16} className="text-gray-400" />
                    {job.location}
                  </div>
                  <div className="flex items-center gap-1">
                    <Calendar size={16} className="text-gray-400" />
                    {job.type}
                  </div>
                </div>

                <p className="text-gray-600 mb-6">{job.description}</p>
                
                <Link
                  href="/contact"
                  className="inline-block text-purple-600 font-semibold hover:text-purple-700 group-hover:translate-x-1 transition-all"
                >
                  Apply Now →
                </Link>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Benefits Section */}
      <section className="bg-gray-50 py-20 px-4">
        <div className="max-w-5xl mx-auto">
          <h2 className="text-4xl font-bold text-gray-900 mb-4 text-center">Why Join TechBirdsFly?</h2>
          <p className="text-xl text-gray-600 text-center max-w-2xl mx-auto mb-16">
            We care about our team
          </p>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            {benefits.map((benefit, i) => {
              const Icon = benefit.icon;
              return (
                <div key={i} className="bg-white p-8 rounded-xl border border-gray-200 hover:shadow-lg transition-all text-center">
                  <Icon size={48} className="text-purple-600 mx-auto mb-4" />
                  <h3 className="text-lg font-bold text-gray-900 mb-2">{benefit.title}</h3>
                  <p className="text-gray-600">{benefit.desc}</p>
                </div>
              );
            })}
          </div>
        </div>
      </section>

      {/* Company Culture */}
      <section className="py-20 px-4">
        <div className="max-w-5xl mx-auto">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 items-center">
            <div>
              <h2 className="text-4xl font-bold text-gray-900 mb-6">Our Culture</h2>
              <div className="space-y-4 text-lg text-gray-600">
                <p>
                  We're a diverse, global team united by a mission to democratize AI-powered web development. 
                </p>
                <p>
                  We value innovation, collaboration, and continuous learning. Everyone's voice matters here.
                </p>
                <p>
                  Whether you're working on core infrastructure, cutting-edge AI features, or user-facing products, 
                  your work directly impacts thousands of developers worldwide.
                </p>
              </div>
            </div>
            <div className="bg-linear-to-br from-purple-100 to-blue-100 rounded-2xl h-96 flex items-center justify-center">
              <Zap size={140} className="text-purple-600 opacity-20" />
            </div>
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="bg-linear-to-r from-purple-600 to-blue-600 text-white py-16 px-4">
        <div className="max-w-4xl mx-auto text-center">
          <h2 className="text-3xl font-bold mb-4">Don't see a role that fits?</h2>
          <p className="text-lg text-purple-100 mb-8">
            We're always interested in talking to talented people. Reach out and let's chat!
          </p>
          <Link
            href="/contact"
            className="inline-block bg-white text-purple-600 font-semibold py-3 px-8 rounded-lg hover:shadow-lg transition-all"
          >
            Get in Touch
          </Link>
        </div>
      </section>
    </main>
  );
}
