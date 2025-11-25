export default function PrivacyPolicy() {
  const currentDate = new Date().toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  });

  return (
    <main className="max-w-5xl mx-auto py-20 px-6 space-y-8">
      <section>
        <h1 className="text-4xl font-bold mb-2">Privacy Policy</h1>
        <p className="text-gray-600">Last updated: {currentDate}</p>
      </section>

      <section className="space-y-4">
        <p className="text-gray-700 leading-relaxed">
          TechBirdsFly ("we", "our", "us") is committed to protecting your privacy. 
          This Privacy Policy explains what data we collect, how we use it, and your rights as a user of our platform.
        </p>
      </section>

      <section className="space-y-4">
        <h2 className="text-2xl font-semibold">1. Data We Collect</h2>
        <p className="text-gray-700">We collect the following types of data:</p>
        <ul className="list-disc pl-6 text-gray-700 space-y-2">
          <li><strong>Account Information:</strong> Name, email, password (hashed), phone number</li>
          <li><strong>Usage Analytics:</strong> Pages visited, features used, time spent, device type</li>
          <li><strong>Generated Content:</strong> Websites, prompts, components you create (stored in your account)</li>
          <li><strong>Billing Information:</strong> Via Stripe (we don't store credit card details)</li>
          <li><strong>Communication:</strong> Emails, support tickets, feedback you provide</li>
        </ul>
      </section>

      <section className="space-y-4">
        <h2 className="text-2xl font-semibold">2. How We Use Your Data</h2>
        <ul className="list-disc pl-6 text-gray-700 space-y-2">
          <li>To provide and improve our platform</li>
          <li>To process payments and send invoices</li>
          <li>To send important account notifications</li>
          <li>To analyze usage patterns and improve features</li>
          <li>To comply with legal obligations</li>
          <li>To prevent fraud and security issues</li>
        </ul>
      </section>

      <section className="space-y-4">
        <h2 className="text-2xl font-semibold">3. Data Sharing</h2>
        <p className="text-gray-700">
          We do NOT sell your data. We may share data with:
        </p>
        <ul className="list-disc pl-6 text-gray-700 space-y-2">
          <li><strong>Service Providers:</strong> Payment processors (Stripe), hosting providers, analytics tools</li>
          <li><strong>Legal Requirements:</strong> When required by law enforcement or court orders</li>
        </ul>
      </section>

      <section className="space-y-4">
        <h2 className="text-2xl font-semibold">4. Your Rights</h2>
        <p className="text-gray-700">You have the right to:</p>
        <ul className="list-disc pl-6 text-gray-700 space-y-2">
          <li>Request access to your personal data</li>
          <li>Request correction of inaccurate data</li>
          <li>Request deletion of your account and data</li>
          <li>Export your data in a standard format</li>
          <li>Opt-out of marketing communications</li>
        </ul>
      </section>

      <section className="space-y-4">
        <h2 className="text-2xl font-semibold">5. Data Security</h2>
        <p className="text-gray-700">
          We use industry-standard encryption (TLS/SSL) to protect data in transit. 
          Your passwords are hashed and stored securely. Access to your data is restricted to 
          authorized personnel only.
        </p>
      </section>

      <section className="space-y-4">
        <h2 className="text-2xl font-semibold">6. Contact Us</h2>
        <p className="text-gray-700">
          For privacy concerns or to exercise your rights, contact us at: <strong>privacy@techbirdsfly.com</strong>
        </p>
      </section>
    </main>
  );
}
