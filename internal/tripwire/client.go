package tripwire

import (
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"time"

	"github.com/craig-jarvis/tripwire2wanderer/internal/config"
	"github.com/craig-jarvis/tripwire2wanderer/internal/data"
)

type Client struct {
	baseURL  string
	user     string
	password string
	maskID   string
	client   *http.Client
}

// New creates a new Tripwire API client
func New(cfg *config.Config) *Client {
	return &Client{
		baseURL:  cfg.TripwireURL,
		user:     cfg.TripwireUser,
		password: cfg.TripwirePassword,
		maskID:   cfg.TripwireMaskID,
		client: &http.Client{
			Timeout: 30 * time.Second,
		},
	}
}

// GetWormholes retrieves all wormholes from Tripwire
func (c *Client) GetWormholes() ([]data.TripwireWormhole, error) {
	url := fmt.Sprintf("%s?q=/wormholes&maskID=%s", c.baseURL, c.maskID)
	body, err := c.makeRequest(url)
	if err != nil {
		return nil, err
	}

	var wormholes []data.TripwireWormhole
	if err := json.Unmarshal(body, &wormholes); err != nil {
		return nil, fmt.Errorf("failed to parse wormholes response: %w", err)
	}

	return wormholes, nil
}

// GetSignatures retrieves all signatures from Tripwire
func (c *Client) GetSignatures() ([]data.TripwireSignature, error) {
	url := fmt.Sprintf("%s?q=/signatures&maskID=%s", c.baseURL, c.maskID)
	body, err := c.makeRequest(url)
	if err != nil {
		return nil, err
	}

	var signatures []data.TripwireSignature
	if err := json.Unmarshal(body, &signatures); err != nil {
		return nil, fmt.Errorf("failed to parse signatures response: %w", err)
	}

	return signatures, nil
}

// makeRequest makes an HTTP GET request with basic authentication
func (c *Client) makeRequest(url string) ([]byte, error) {
	req, err := http.NewRequest("GET", url, nil)
	if err != nil {
		return nil, fmt.Errorf("failed to create request: %w", err)
	}

	// Set basic authentication
	req.SetBasicAuth(c.user, c.password)

	resp, err := c.client.Do(req)
	if err != nil {
		return nil, fmt.Errorf("request failed: %w", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		bodyBytes, _ := io.ReadAll(resp.Body)
		return nil, fmt.Errorf("API returned status %d: %s", resp.StatusCode, string(bodyBytes))
	}

	body, err := io.ReadAll(resp.Body)
	if err != nil {
		return nil, fmt.Errorf("failed to read response body: %w", err)
	}

	return body, nil
}
