import { Zap, Users, Target, Lightbulb, Code, Rocket } from 'lucide-react';

export default function AboutPage() {
  const values = [
    { icon: Lightbulb, title: "Innovation", description: "Push boundaries with cutting-edge AI technology" },
    { icon: Users, title: "Community", description: "Build together with developers worldwide" },
    { icon: Code, title: "Quality", description: "Production-ready code, every single time" },
    { icon: Rocket, title: "Speed", description: "Create in minutes, not weeks" },
  ];

  return (
    <main className="min-h-screen bg-white">
      {/* Hero Section */}
      <section className="bg-linear-to-br from-purple-50 via-white to-blue-50 py-20 px-4 md:py-28">
        <div className="max-w-5xl mx-auto text-center">
          <h1 className="text-5xl md:text-6xl font-bold text-gray-900 mb-6">
            About TechBirdsFly
          </h1>
          <p className="text-xl text-gray-600 max-w-3xl mx-auto leading-relaxed">
            We're revolutionizing how websites are built by combining advanced AI, scalable cloud architecture, 
            and modern engineering to help developers, agencies, and businesses create stunning digital products in minutes.
          </p>
        </div>
      </section>

      {/* Mission Section */}
      <section className="py-20 px-4">
        <div className="max-w-5xl mx-auto">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-12 items-center">
            <div>
              <h2 className="text-4xl font-bold text-gray-900 mb-6">Our Mission</h2>
              <p className="text-lg text-gray-600 mb-6 leading-relaxed">
                To make website creation faster, smarter, and accessible to everyone — from 
                enterprise developers to solo founders building their dreams.
              </p>
              <p className="text-lg text-gray-600 leading-relaxed">
                We believe great design and code shouldn't take weeks. With TechBirdsFly, 
                it takes minutes.
              </p>
            </div>
            <div className="bg-linear-to-br from-purple-100 to-blue-100 rounded-2xl h-80 flex items-center justify-center">
              <Target size={120} className="text-purple-600 opacity-20" />
            </div>
          </div>
        </div>
      </section>

      {/* Core Values */}
      <section className="py-20 px-4 bg-gray-50">
        <div className="max-w-5xl mx-auto">
          <h2 className="text-4xl font-bold text-gray-900 mb-4 text-center">Our Core Values</h2>
          <p className="text-lg text-gray-600 text-center max-w-2xl mx-auto mb-16">
            What drives us every single day
          </p>
          
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
            {values.map((value, i) => {
              const Icon = value.icon;
              return (
                <div key={i} className="bg-white p-8 rounded-xl border border-gray-200 hover:shadow-lg transition-all text-center">
                  <Icon size={48} className="text-purple-600 mx-auto mb-4" />
                  <h3 className="text-xl font-bold text-gray-900 mb-3">{value.title}</h3>
                  <p className="text-gray-600">{value.description}</p>
                </div>
              );
            })}
          </div>
        </div>
      </section>

      {/* What We Do */}
      <section className="py-20 px-4">
        <div className="max-w-5xl mx-auto">
          <h2 className="text-4xl font-bold text-gray-900 mb-4 text-center">What We Do</h2>
          <p className="text-lg text-gray-600 text-center max-w-2xl mx-auto mb-16">
            Powerful tools for modern web development
          </p>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            {[
              { icon: Zap, title: "AI Website Generation", desc: "Create complete websites from simple descriptions" },
              { icon: Code, title: "Clean Code Export", desc: "Get production-ready React & Next.js code instantly" },
              { icon: Lightbulb, title: "Smart Components", desc: "Reusable, customizable UI components" },
              { icon: Rocket, title: "One-Click Deploy", desc: "Deploy to Vercel, Netlify, or your servers" },
            ].map((item, i) => {
              const Icon = item.icon;
              return (
                <div key={i} className="bg-linear-to-br from-purple-50 to-blue-50 p-8 rounded-xl border border-purple-100">
                  <Icon size={40} className="text-purple-600 mb-4" />
                  <h3 className="text-xl font-bold text-gray-900 mb-2">{item.title}</h3>
                  <p className="text-gray-600">{item.desc}</p>
                </div>
              );
            })}
          </div>
        </div>
      </section>

      {/* Founder Section */}
      <section className="py-20 px-4 bg-linear-to-br from-purple-900 to-blue-900 text-white">
        <div className="max-w-4xl mx-auto text-center">
          <h2 className="text-4xl font-bold mb-6">Meet the Founder</h2>
          <div className="bg-white bg-opacity-10 backdrop-blur-sm p-12 rounded-2xl border border-white border-opacity-20">
            <div className="w-24 h-24 bg-linear-to-br from-purple-400 to-blue-400 rounded-full mx-auto mb-6"></div>
            <h3 className="text-3xl font-bold mb-2">Ali Raza Tahir</h3>
            <p className="text-purple-200 mb-6 text-lg">Senior Software Engineer & Founder</p>
            <p className="text-lg leading-relaxed max-w-2xl mx-auto">
              With expertise in .NET, Azure, microservices architecture, and AI-driven development, 
              Ali leads TechBirdsFly with a vision to democratize website creation. 
              His passion for clean code and innovative solutions drives every feature we build.
            </p>
          </div>
        </div>
      </section>
    </main>
  );
}
