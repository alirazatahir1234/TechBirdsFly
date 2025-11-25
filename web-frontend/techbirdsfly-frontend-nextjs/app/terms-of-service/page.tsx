export default function TermsOfService() {
  const currentDate = new Date().toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  });

  return (
    <main className="max-w-5xl mx-auto py-20 px-6 space-y-8">
      <section>
        <h1 className="text-4xl font-bold mb-2">Terms of Service</h1>
        <p className="text-gray-600">Last updated: {currentDate}</p>
      </section>

      <section className="space-y-4">
        <h2 className="text-2xl font-semibold">1. Introduction</h2>
        <p className="text-gray-700">
          By accessing and using TechBirdsFly, you agree to be bound by these Terms of Service. 
          If you do not agree to these terms, please do not use our platform.
        </p>
      </section>

      <section className="space-y-4">
        <h2 className="text-2xl font-semibold">2. User Accounts</h2>
        <p className="text-gray-700">
          You are responsible for maintaining the confidentiality of your account credentials and 
          for all activities that occur under your account. You agree to notify us immediately of 
          any unauthorized access.
        </p>
      </section>

      <section className="space-y-4">
        <h2 className="text-2xl font-semibold">3. Acceptable Use</h2>
        <p className="text-gray-700">You agree NOT to:</p>
        <ul className="list-disc pl-6 text-gray-700 space-y-2">
          <li>Use the platform for illegal or harmful purposes</li>
          <li>Create malware, viruses, or malicious code</li>
          <li>Attempt to hack or gain unauthorized access</li>
          <li>Spam or harass other users</li>
          <li>Create websites for phishing, fraud, or deception</li>
          <li>Violate intellectual property rights</li>
        </ul>
      </section>

      <section className="space-y-4">
        <h2 className="text-2xl font-semibold">4. Intellectual Property</h2>
        <p className="text-gray-700">
          <strong>Your Content:</strong> You retain all rights to websites and code you generate. 
          You can modify, export, and deploy them freely.
        </p>
        <p className="text-gray-700 mt-4">
          <strong>Our IP:</strong> TechBirdsFly platform, AI models, and branding are owned by us.
        </p>
      </section>

      <section className="space-y-4">
        <h2 className="text-2xl font-semibold">5. Payment & Billing</h2>
        <ul className="list-disc pl-6 text-gray-700 space-y-2">
          <li>Pricing is displayed in USD unless otherwise stated</li>
          <li>Subscriptions renew automatically unless cancelled</li>
          <li>Refunds are issued within 30 days of purchase if requested</li>
          <li>We may change pricing with 30 days notice</li>
        </ul>
      </section>

      <section className="space-y-4">
        <h2 className="text-2xl font-semibold">6. Limitation of Liability</h2>
        <p className="text-gray-700">
          To the fullest extent permitted by law, TechBirdsFly is not liable for indirect, 
          incidental, or consequential damages arising from your use of our platform.
        </p>
      </section>

      <section className="space-y-4">
        <h2 className="text-2xl font-semibold">7. Termination</h2>
        <p className="text-gray-700">
          We may terminate your account if you violate these Terms of Service or engage in 
          harmful behavior. You may cancel anytime by contacting support.
        </p>
      </section>

      <section className="space-y-4">
        <h2 className="text-2xl font-semibold">8. Changes to Terms</h2>
        <p className="text-gray-700">
          We may update these Terms of Service at any time. We'll notify you of material changes 
          via email. Continued use of the platform after updates constitutes acceptance.
        </p>
      </section>

      <section className="space-y-4">
        <h2 className="text-2xl font-semibold">9. Contact</h2>
        <p className="text-gray-700">
          For questions about these Terms, contact: <strong>legal@techbirdsfly.com</strong>
        </p>
      </section>
    </main>
  );
}
