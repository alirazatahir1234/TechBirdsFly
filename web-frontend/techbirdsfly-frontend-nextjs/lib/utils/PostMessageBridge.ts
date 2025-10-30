'use client';

type MessageHandler = (data: any) => void;

interface PostMessageBridgeOptions {
  timeout?: number;
  debug?: boolean;
}

export class PostMessageBridge {
  private listeners: Map<string, MessageHandler[]> = new Map();
  private iframe: HTMLIFrameElement | null = null;
  private debug: boolean;
  private messageId = 0;
  private pendingMessages: Map<number, (response: any) => void> = new Map();

  constructor(options: PostMessageBridgeOptions = {}) {
    this.debug = options.debug || false;

    window.addEventListener('message', (event) => {
      this.handleMessage(event);
    });
  }

  /**
   * Set the target iframe for communication
   */
  setIframe(iframe: HTMLIFrameElement) {
    this.iframe = iframe;
  }

  /**
   * Send a message to the iframe
   */
  send(type: string, data?: any): Promise<any> {
    return new Promise((resolve, reject) => {
      if (!this.iframe?.contentWindow) {
        reject(new Error('Iframe not set or not loaded'));
        return;
      }

      const id = this.messageId++;
      const message = { type, data, id };

      this.log('→ Sending to iframe:', message);

      // Store the resolver for this message
      this.pendingMessages.set(id, resolve);

      // Set timeout for response
      setTimeout(() => {
        if (this.pendingMessages.has(id)) {
          this.pendingMessages.delete(id);
          reject(new Error(`Message ${type} timed out`));
        }
      }, 5000);

      this.iframe.contentWindow!.postMessage(message, '*');
    });
  }

  /**
   * Subscribe to messages of a specific type
   */
  on(type: string, handler: MessageHandler) {
    if (!this.listeners.has(type)) {
      this.listeners.set(type, []);
    }
    this.listeners.get(type)!.push(handler);

    // Return unsubscribe function
    return () => {
      const handlers = this.listeners.get(type);
      if (handlers) {
        const index = handlers.indexOf(handler);
        if (index > -1) {
          handlers.splice(index, 1);
        }
      }
    };
  }

  /**
   * Subscribe to a message type once
   */
  once(type: string, handler: MessageHandler) {
    const unsubscribe = this.on(type, (data) => {
      handler(data);
      unsubscribe();
    });
    return unsubscribe;
  }

  /**
   * Handle incoming messages
   */
  private handleMessage = (event: MessageEvent) => {
    if (event.source !== this.iframe?.contentWindow && this.iframe) {
      return;
    }

    const { type, data, id } = event.data;

    this.log('← Received from iframe:', event.data);

    // Handle response to sent message
    if (id !== undefined && this.pendingMessages.has(id)) {
      const resolve = this.pendingMessages.get(id)!;
      this.pendingMessages.delete(id);
      resolve(data);
      return;
    }

    // Handle subscription messages
    const handlers = this.listeners.get(type);
    if (handlers) {
      handlers.forEach((handler) => handler(data));
    }
  };

  /**
   * Send a message and wait for response
   */
  async sendAndWait<T = any>(type: string, data?: any): Promise<T> {
    return this.send(type, data);
  }

  /**
   * Clear all listeners
   */
  clear() {
    this.listeners.clear();
    this.pendingMessages.clear();
  }

  /**
   * Logging helper
   */
  private log(...args: any[]) {
    if (this.debug) {
      console.log('[PostMessageBridge]', ...args);
    }
  }
}

/**
 * Create a shared instance of the bridge
 */
export const createPostMessageBridge = (options?: PostMessageBridgeOptions) => {
  return new PostMessageBridge(options);
};
