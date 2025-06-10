interface Config {
  apiBaseUrl: string;
}

let cachedConfig: Config | null = null;

export async function getConfig(): Promise<Config> {
  if (cachedConfig) {
    return cachedConfig;
  }

  try {
    const response = await fetch("/configuration", { 
      credentials: "include",
      headers: {
        'Content-Type': 'application/json'
      }
    });
    
    if (!response.ok) {
      throw new Error(`Failed to fetch config: ${response.status}`);
    }
    
    const config: Config = await response.json();
    cachedConfig = config;
    return config;
  } catch (error) {
    console.error("Failed to load configuration, using fallback:", error);
    // Fallback to current origin if config fails
    const fallbackConfig: Config = {
      apiBaseUrl: window.location.origin
    };
    cachedConfig = fallbackConfig;
    return fallbackConfig;
  }
}

export function clearConfigCache() {
  cachedConfig = null;
}