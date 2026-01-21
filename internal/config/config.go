package config

import (
	"fmt"
	"net/url"
	"os"
	"strconv"
	"strings"

	"github.com/joho/godotenv"
)

type Config struct {
	WandererURL          string
	WandererAPIKey       string
	WandererMapSlug      string
	WandererCharID       int
	WandererHomeSystemID int
	TripwireURL          string
	TripwireUser         string
	TripwirePassword     string
	TripwireMaskID       string
}

// Load reads configuration from .env file and environment variables.
// Environment variables take precedence over .env file values.
func Load() (*Config, error) {
	// Try to load .env file, but don't fail if it doesn't exist (useful in Docker)
	_ = godotenv.Load()

	// Parse integer fields
	charID, err := parseIntEnv("WANDERER_CHAR_ID", true)
	if err != nil {
		return nil, err
	}

	homeSystemID, err := parseIntEnv("WANDERER_HOME_SYSTEM_ID", true)
	if err != nil {
		return nil, err
	}

	cfg := &Config{
		WandererURL:          strings.TrimSuffix(strings.TrimSpace(getEnv("WANDERER_URL", "")), "/"),
		WandererAPIKey:       strings.TrimSpace(getEnv("WANDERER_API_KEY", "")),
		WandererMapSlug:      strings.TrimSpace(getEnv("WANDERER_MAP_SLUG", "")),
		WandererCharID:       charID,
		WandererHomeSystemID: homeSystemID,
		TripwireURL:          strings.TrimSuffix(strings.TrimSpace(getEnv("TW_URL", "")), "/"),
		TripwireUser:         strings.TrimSpace(getEnv("TW_USER", "")),
		TripwirePassword:     strings.TrimSpace(getEnv("TW_PASSWORD", "")),
		TripwireMaskID:       strings.TrimSpace(getEnv("TW_MASK_ID", "")),
	}

	// Validate required fields
	if err := cfg.Validate(); err != nil {
		return nil, err
	}

	return cfg, nil
}

// getEnv retrieves an environment variable, returning a default if not set
func getEnv(key, defaultVal string) string {
	if value, exists := os.LookupEnv(key); exists {
		return value
	}
	return defaultVal
}

// parseIntEnv parses an integer from environment variable with validation
func parseIntEnv(key string, required bool) (int, error) {
	val := strings.TrimSpace(getEnv(key, ""))
	if val == "" {
		if required {
			return 0, fmt.Errorf("%s is required", key)
		}
		return 0, nil
	}

	intVal, err := strconv.Atoi(val)
	if err != nil {
		return 0, fmt.Errorf("%s must be a valid integer: %w", key, err)
	}

	if intVal <= 0 {
		return 0, fmt.Errorf("%s must be positive, got %d", key, intVal)
	}

	return intVal, nil
}

// Validate checks that all required configuration values are set
func (c *Config) Validate() error {
	if c.WandererURL == "" {
		return fmt.Errorf("WANDERER_URL is required")
	}

	if _, err := url.Parse(c.WandererURL); err != nil {
		return fmt.Errorf("invalid WANDERER_URL: %w", err)
	}

	if c.WandererAPIKey == "" {
		return fmt.Errorf("WANDERER_API_KEY is required")
	}

	if c.WandererMapSlug == "" {
		return fmt.Errorf("WANDERER_MAP_SLUG is required")
	}

	if c.TripwireURL == "" {
		return fmt.Errorf("TW_URL is required")
	}

	if _, err := url.Parse(c.TripwireURL); err != nil {
		return fmt.Errorf("invalid TW_URL: %w", err)
	}

	if c.TripwireUser == "" {
		return fmt.Errorf("TW_USER is required")
	}

	if c.TripwirePassword == "" {
		return fmt.Errorf("TW_PASSWORD is required")
	}

	if c.TripwireMaskID == "" {
		return fmt.Errorf("TW_MASK_ID is required")
	}

	return nil
}
