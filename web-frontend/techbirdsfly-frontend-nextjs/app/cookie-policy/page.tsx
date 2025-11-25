export default function CookiePolicy() {
  const currentDate = new Date().toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  });

  return (
    <main className="max-w-5xl mx-auto py-20 px-6 space-y-8">
      <section>
        <h1 className="text-4xl font-bold mb-2">Cookie Policy</h1>
        <p className="text-gray-600">Last updated: {currentDate}</p>
      </section>

      <section className="space-y-4">
        <h2 className="text-2xl font-semibold">1. What Are Cookies?</h2>
        <p className="text-gray-700">
          Cookies are small pieces of text data stored in your browser to remember information 
          about you and improve your experience on TechBirdsFly.
        </p>
      </section>

      <section className="space-y-4">
        <h2 className="text-2xl font-semibold">2. Types of Cookies We Use</h2>

        <div className="space-y-4">
          <div>
            <h3 className="font-semibold mb-2">Essential Cookies</h3>
            <p className="text-gray-700">
              Required for authentication, session management, and security. These cannot be disabled.
            </p>
            <ul className="list-disc pl-6 text-gray-700 space-y-1 mt-2">
              <li>Session tokens</li>
              <li>CSRF protection tokens</li>
              <li>User authentication cookies</li>
            </ul>
          </div>

          <div>
            <h3 className="font-semibold mb-2">Analytics Cookies</h3>
            <p className="text-gray-700">
              Used to understand how users interact with our platform to improve features and UX.
            </p>
            <ul className="list-disc pl-6 text-gray-700 space-y-1 mt-2">
              <li>Google Analytics</li>
              <li>Mixpanel tracking</li>
              <li>Usage analytics</li>
            </ul>
          </div>

          <div>
            <h3 className="font-semibold mb-2">Preference Cookies</h3>
            <p className="text-gray-700">
              Remember your preferences like theme, language, and settings.
            </p>
            <ul className="list-disc pl-6 text-gray-700 space-y-1 mt-2">
              <li>Dark/light mode preference</li>
              <li>Language selection</li>
              <li>Sidebar collapse state</li>
            </ul>
          </div>

          <div>
            <h3 className="font-semibold mb-2">Marketing Cookies</h3>
            <p className="text-gray-700">
              Help us show you relevant ads and measure campaign effectiveness (only with consent).
            </p>
            <ul className="list-disc pl-6 text-gray-700 space-y-1 mt-2">
              <li>Facebook Pixel</li>
              <li>LinkedIn conversion tracking</li>
              <li>Google Ads tracking</li>
            </ul>
          </div>
        </div>
      </section>

      <section className="space-y-4">
        <h2 className="text-2xl font-semibold">3. Third-Party Cookies</h2>
        <p className="text-gray-700">
          We use services that may set their own cookies:
        </p>
        <ul className="list-disc pl-6 text-gray-700 space-y-2">
          <li><strong>Stripe:</strong> For payment processing</li>
          <li><strong>Google Analytics:</strong> For usage tracking</li>
          <li><strong>Intercom:</strong> For customer support chat</li>
          <li><strong>Mixpanel:</strong> For product analytics</li>
        </ul>
      </section>

      <section className="space-y-4">
        <h2 className="text-2xl font-semibold">4. Managing Cookies</h2>
        <p className="text-gray-700">
          You can control cookies through your browser settings:
        </p>
        <ul className="list-disc pl-6 text-gray-700 space-y-2">
          <li><strong>Chrome:</strong> Settings → Privacy and security → Cookies and other site data</li>
          <li><strong>Firefox:</strong> Preferences → Privacy & Security → Cookies and Site Data</li>
          <li><strong>Safari:</strong> Preferences → Privacy → Manage Website Data</li>
          <li><strong>Edge:</strong> Settings → Privacy, search, and services → Cookies and other site data</li>
        </ul>
      </section>

      <section className="space-y-4">
        <h2 className="text-2xl font-semibold">5. Cookie Consent</h2>
        <p className="text-gray-700">
          When you first visit TechBirdsFly, we'll ask for your consent to non-essential cookies. 
          You can change your preferences anytime in your account settings.
        </p>
      </section>

      <section className="space-y-4">
        <h2 className="text-2xl font-semibold">6. Cookie Duration</h2>
        <ul className="list-disc pl-6 text-gray-700 space-y-2">
          <li><strong>Session Cookies:</strong> Deleted when you close your browser</li>
          <li><strong>Persistent Cookies:</strong> Can last from days to years</li>
          <li><strong>Authentication Cookies:</strong> Remain until you log out</li>
        </ul>
      </section>

      <section className="space-y-4">
        <h2 className="text-2xl font-semibold">7. Contact Us</h2>
        <p className="text-gray-700">
          For questions about our use of cookies, contact: <strong>privacy@techbirdsfly.com</strong>
        </p>
      </section>
    </main>
  );
}
