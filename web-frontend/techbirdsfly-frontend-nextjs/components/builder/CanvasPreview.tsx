'use client';

import React, { useEffect, useRef, useCallback } from 'react';
import { WebsiteComponent } from '@/lib/store/builderStore';
import { PostMessageBridge } from '@/lib/utils/PostMessageBridge';

interface CanvasPreviewProps {
  components: WebsiteComponent[];
  iframeRef: React.RefObject<HTMLIFrameElement | null>;
}

export const CanvasPreview: React.FC<CanvasPreviewProps> = ({ components, iframeRef }) => {
  const bridgeRef = useRef<PostMessageBridge | null>(null);

  // Initialize iframe and postMessage bridge
  useEffect(() => {
    if (!iframeRef.current) return;

    const iframe = iframeRef.current;
    const iframeDoc = iframe.contentDocument || iframe.contentWindow?.document;

    if (!iframeDoc) return;

    // Initialize bridge
    if (!bridgeRef.current) {
      bridgeRef.current = new PostMessageBridge({ debug: false });
      bridgeRef.current.setIframe(iframe);
    }

    const bridge = bridgeRef.current;

    // Create HTML content
    const html = `
      <!DOCTYPE html>
      <html>
        <head>
          <meta charset="UTF-8">
          <meta name="viewport" content="width=device-width, initial-scale=1.0">
          <script src="https://cdn.tailwindcss.com"></script>
          <style>
            body { margin: 0; padding: 16px; background: white; font-family: system-ui, -apple-system, sans-serif; }
            .component { position: relative; margin-bottom: 20px; }
            .component-wrapper { display: flex; justify-content: center; }
          </style>
        </head>
        <body>
          <div id="root"></div>
          <script>
            const state = {
              components: [],
              updateCount: 0
            };

            // Listen for messages from parent
            window.addEventListener('message', (event) => {
              const { type, data, id } = event.data;

              switch(type) {
                case 'UPDATE_COMPONENTS':
                  state.components = data;
                  state.updateCount++;
                  renderComponents(data);
                  // Send acknowledgment
                  window.parent.postMessage({ type: 'COMPONENTS_UPDATED', id, data: { count: state.updateCount } }, '*');
                  break;

                case 'GET_HTML':
                  const html = document.documentElement.outerHTML;
                  window.parent.postMessage({ type: 'HTML_CONTENT', id, data: html }, '*');
                  break;

                case 'GET_DOM':
                  const dom = document.getElementById('root')?.innerHTML;
                  window.parent.postMessage({ type: 'DOM_CONTENT', id, data: dom }, '*');
                  break;

                case 'PING':
                  window.parent.postMessage({ type: 'PONG', id }, '*');
                  break;
              }
            });

            function renderComponents(components) {
              const root = document.getElementById('root');
              if (!root) return;

              root.innerHTML = components
                .map(comp => \`<div class="component-wrapper">\${renderComponent(comp)}</div>\`)
                .join('');
            }

            function renderComponent(comp) {
              const templates = {
                hero: \`
                  <div class="w-full h-64 bg-gradient-to-r from-blue-600 to-purple-600 rounded-lg flex items-center justify-center text-white shadow-lg">
                    <div class="text-center">
                      <h1 class="text-4xl font-bold mb-4">\${comp.properties?.title || 'Welcome'}</h1>
                      <p class="text-lg opacity-90">\${comp.properties?.subtitle || 'Your website here'}</p>
                    </div>
                  </div>
                \`,
                section: \`
                  <div class="w-full h-48 bg-slate-100 rounded-lg p-4 flex items-center justify-center border-2 border-slate-200 shadow">
                    <div class="text-center">
                      <h2 class="text-2xl font-bold mb-2">\${comp.properties?.title || 'Section'}</h2>
                      <p class="text-gray-600">\${comp.properties?.content || 'Content goes here'}</p>
                    </div>
                  </div>
                \`,
                footer: \`
                  <div class="w-full h-24 bg-slate-900 rounded-lg flex items-center justify-between px-4 text-white shadow-lg">
                    <div>&copy; 2025 TechBirdsFly</div>
                    <div class="space-x-4">
                      <a href="#" class="hover:opacity-75">About</a>
                      <a href="#" class="hover:opacity-75">Contact</a>
                    </div>
                  </div>
                \`,
                navbar: \`
                  <nav class="w-full h-16 bg-slate-800 rounded-lg flex items-center justify-between px-4 text-white border-b-2 border-slate-700 shadow-lg">
                    <div class="font-bold text-lg">Logo</div>
                    <div class="space-x-4">
                      <a href="#" class="hover:opacity-75">Home</a>
                      <a href="#" class="hover:opacity-75">Services</a>
                      <a href="#" class="hover:opacity-75">Contact</a>
                    </div>
                  </nav>
                \`,
                card: \`
                  <div class="w-64 h-80 bg-white rounded-lg shadow-lg p-4 flex flex-col justify-between border">
                    <div>
                      <h3 class="text-xl font-bold mb-2">\${comp.properties?.title || 'Card'}</h3>
                      <p class="text-gray-600">\${comp.properties?.description || 'Card content'}</p>
                    </div>
                    <button class="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 transition">
                      \${comp.properties?.cta || 'Learn More'}
                    </button>
                  </div>
                \`,
                cta: \`
                  <div class="w-full h-32 bg-gradient-to-r from-green-500 to-emerald-500 rounded-lg flex items-center justify-center text-white font-bold shadow-lg">
                    <div class="text-center">
                      <p class="text-2xl mb-4">\${comp.properties?.headline || 'Ready to get started?'}</p>
                      <button class="bg-white text-green-600 px-6 py-3 rounded-lg font-bold hover:bg-gray-100 transition">
                        \${comp.properties?.buttonText || 'Get Started'}
                      </button>
                    </div>
                  </div>
                \`,
                testimonial: \`
                  <div class="w-full h-40 bg-slate-100 rounded-lg p-4 text-gray-800 italic border-l-4 border-blue-500 shadow">
                    <p class="mb-4">"\${comp.properties?.quote || 'This is amazing!'}"</p>
                    <div class="font-bold">- \${comp.properties?.author || 'Customer'}</div>
                  </div>
                \`,
                faq: \`
                  <div class="w-full bg-slate-100 rounded-lg p-4 shadow">
                    <h3 class="text-xl font-bold mb-4">FAQ</h3>
                    <div class="space-y-4">
                      <details class="cursor-pointer">
                        <summary class="font-semibold">Question 1?</summary>
                        <p class="mt-2 text-gray-600">Answer to question 1</p>
                      </details>
                    </div>
                  </div>
                \`,
              };

              return templates[comp.type] || templates.section;
            }

            // Notify parent that preview is ready
            window.parent.postMessage({ type: 'PREVIEW_READY' }, '*');
          </script>
        </body>
      </html>
    `;

    iframeDoc.open();
    iframeDoc.write(html);
    iframeDoc.close();

    // Wait for iframe to be ready
    const handleReady = () => {
      bridge.send('UPDATE_COMPONENTS', components).catch(console.error);
    };

    bridge.once('PREVIEW_READY', handleReady);

    return () => {
      bridge.clear();
    };
  }, [components, iframeRef]);

  return (
    <iframe
      ref={iframeRef}
      className="w-full h-full border-0"
      sandbox={{
        allowSameOrigin: true,
        allowScripts: true,
      } as any}
      title="Website Preview"
    />
  );
};
